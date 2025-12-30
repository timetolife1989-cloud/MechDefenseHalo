using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Camera;

namespace MechDefenseHalo.Tests.Camera
{
    /// <summary>
    /// Unit tests for CameraController
    /// </summary>
    [TestSuite]
    public class CameraControllerTests
    {
        #region Initialization Tests

        [TestCase]
        public void CameraController_OnReady_InitializesShake()
        {
            // Arrange
            var camera = AutoFree(new CameraController());
            var root = AutoFree(new Node3D());
            root.AddChild(camera);

            // Act
            camera._Ready();

            // Assert
            AssertInt(camera.GetChildCount()).IsGreater(0);
        }

        [TestCase]
        public void CameraController_DefaultOffset_IsCorrect()
        {
            // Arrange & Act
            var camera = AutoFree(new CameraController());

            // Assert
            AssertObject(camera.Offset).IsEqual(new Vector3(0, 5, 10));
        }

        [TestCase]
        public void CameraController_DefaultFollowSpeed_Is5()
        {
            // Arrange & Act
            var camera = AutoFree(new CameraController());

            // Assert
            AssertFloat(camera.FollowSpeed).IsEqual(5f);
        }

        #endregion

        #region Shake Tests

        [TestCase]
        public void Shake_WithValidParameters_StartsShake()
        {
            // Arrange
            var camera = AutoFree(new CameraController());
            var root = AutoFree(new Node3D());
            root.AddChild(camera);
            camera._Ready();

            // Act & Assert (should not throw)
            camera.Shake(1.0f, 0.5f);
        }

        #endregion

        #region Target Following Tests

        [TestCase]
        public void Process_WithNoTarget_DoesNotCrash()
        {
            // Arrange
            var camera = AutoFree(new CameraController());
            camera.Target = null;

            // Act & Assert (should not throw)
            camera._Process(0.016);
        }

        [TestCase]
        public void Process_WithTarget_UpdatesPosition()
        {
            // Arrange
            var camera = AutoFree(new CameraController());
            var target = AutoFree(new Node3D());
            var root = AutoFree(new Node3D());
            root.AddChild(camera);
            root.AddChild(target);
            
            target.GlobalPosition = new Vector3(10, 0, 0);
            camera.GlobalPosition = new Vector3(0, 0, 0);
            camera.Target = target;
            camera.FollowSpeed = 1.0f;

            // Act
            camera._Process(1.0); // 1 second with speed 1.0 should move it all the way

            // Assert - camera should have moved towards target + offset
            AssertFloat(camera.GlobalPosition.X).IsGreater(0);
        }

        #endregion
    }
}
