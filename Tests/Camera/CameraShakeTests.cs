using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Camera;

namespace MechDefenseHalo.Tests.Camera
{
    /// <summary>
    /// Unit tests for CameraShake
    /// </summary>
    [TestSuite]
    public class CameraShakeTests
    {
        #region Initialization Tests

        [TestCase]
        public void CameraShake_CanBeCreated()
        {
            // Arrange & Act
            var shake = AutoFree(new CameraShake());

            // Assert
            AssertObject(shake).IsNotNull();
        }

        #endregion

        #region Shake Functionality Tests

        [TestCase]
        public void StartShake_WithValidParameters_SetsInternalState()
        {
            // Arrange
            var shake = AutoFree(new CameraShake());
            float intensity = 2.0f;
            float duration = 1.0f;

            // Act
            shake.StartShake(intensity, duration);

            // Assert - should not throw, state is private but we can verify behavior
            shake._Process(0.016);
        }

        [TestCase]
        public void Process_WithNoActiveShake_DoesNotEmitSignal()
        {
            // Arrange
            var shake = AutoFree(new CameraShake());
            bool signalEmitted = false;
            shake.ShakeUpdated += (offset) => signalEmitted = true;

            // Act
            shake._Process(0.016);

            // Assert
            AssertBool(signalEmitted).IsFalse();
        }

        [TestCase]
        public void Process_WithActiveShake_EmitsSignal()
        {
            // Arrange
            var shake = AutoFree(new CameraShake());
            bool signalEmitted = false;
            shake.ShakeUpdated += (offset) => signalEmitted = true;
            shake.StartShake(1.0f, 1.0f);

            // Act
            shake._Process(0.016);

            // Assert
            AssertBool(signalEmitted).IsTrue();
        }

        [TestCase]
        public void Process_ShakeDecaysOverTime()
        {
            // Arrange
            var shake = AutoFree(new CameraShake());
            Vector3 firstOffset = Vector3.Zero;
            int emitCount = 0;
            
            shake.ShakeUpdated += (offset) => 
            {
                if (emitCount == 0) firstOffset = offset;
                emitCount++;
            };
            
            shake.StartShake(1.0f, 1.0f);

            // Act - process enough times to complete the 1.0s shake (1.0s / 0.016s ~= 63 iterations)
            int requiredIterations = (int)(1.0f / 0.016f) + 10; // Add buffer for safety
            for (int i = 0; i < requiredIterations; i++)
            {
                shake._Process(0.016);
            }

            // Assert - shake should have completed (signal stops being emitted)
            int finalEmitCount = emitCount;
            shake._Process(0.016);
            AssertInt(emitCount).IsEqual(finalEmitCount); // No new emissions
        }

        #endregion
    }
}
