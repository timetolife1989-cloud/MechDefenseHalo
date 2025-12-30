using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Camera;

namespace MechDefenseHalo.Tests.Camera
{
    /// <summary>
    /// Unit tests for FollowCamera
    /// </summary>
    [TestSuite]
    public class FollowCameraTests
    {
        #region Initialization Tests

        [TestCase]
        public void FollowCamera_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var camera = AutoFree(new FollowCamera());

            // Assert
            AssertObject(camera.Offset).IsEqual(new Vector3(0, 5, 10));
            AssertFloat(camera.FollowSpeed).IsEqual(5f);
            AssertFloat(camera.RotationSpeed).IsEqual(3f);
            AssertBool(camera.EnableCollision).IsTrue();
            AssertFloat(camera.CollisionMargin).IsEqual(0.5f);
        }

        [TestCase]
        public void Ready_WithTarget_InitializesPosition()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());
            var target = AutoFree(new Node3D());
            target.GlobalPosition = new Vector3(10, 0, 0);
            camera.Target = target;

            // Act
            camera._Ready();

            // Assert - camera should be positioned at target + offset
            AssertFloat(camera.GlobalPosition.X).IsGreater(0);
        }

        [TestCase]
        public void Ready_WithNoTarget_DoesNotCrash()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());
            camera.Target = null;

            // Act & Assert (should not throw)
            camera._Ready();
        }

        #endregion

        #region Target Management Tests

        [TestCase]
        public void SetTarget_UpdatesTarget()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());
            var target = AutoFree(new Node3D());

            // Act
            camera.SetTarget(target);

            // Assert
            AssertObject(camera.Target).IsEqual(target);
        }

        [TestCase]
        public void SetTarget_WithNullTarget_Succeeds()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());

            // Act & Assert (should not throw)
            camera.SetTarget(null);
            AssertObject(camera.Target).IsNull();
        }

        #endregion

        #region Offset Tests

        [TestCase]
        public void SetOffset_UpdatesOffset()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());
            var newOffset = new Vector3(1, 2, 3);

            // Act
            camera.SetOffset(newOffset);

            // Assert
            AssertObject(camera.GetOffset()).IsEqual(newOffset);
        }

        [TestCase]
        public void GetOffset_ReturnsCurrentOffset()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());
            var expectedOffset = new Vector3(5, 10, 15);
            camera.Offset = expectedOffset;

            // Act
            var result = camera.GetOffset();

            // Assert
            AssertObject(result).IsEqual(expectedOffset);
        }

        #endregion

        #region Follow Behavior Tests

        [TestCase]
        public void Process_WithNoTarget_DoesNotCrash()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());
            camera.Target = null;

            // Act & Assert (should not throw)
            camera._Process(0.016);
        }

        [TestCase]
        public void Process_WithTarget_UpdatesPosition()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());
            var target = AutoFree(new Node3D());
            var root = AutoFree(new Node3D());
            root.AddChild(camera);
            root.AddChild(target);
            
            target.GlobalPosition = new Vector3(10, 0, 0);
            camera.GlobalPosition = new Vector3(0, 0, 0);
            camera.Target = target;
            camera.FollowSpeed = 1.0f;

            // Act
            camera._Process(1.0); // Process for 1 second

            // Assert - camera should have moved towards target
            AssertFloat(camera.GlobalPosition.X).IsGreater(0);
        }

        [TestCase]
        public void SnapToTarget_InstantlyMovesCamera()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());
            var target = AutoFree(new Node3D());
            var root = AutoFree(new Node3D());
            root.AddChild(camera);
            root.AddChild(target);
            
            target.GlobalPosition = new Vector3(100, 0, 0);
            camera.GlobalPosition = new Vector3(0, 0, 0);
            camera.Target = target;
            camera.Offset = Vector3.Zero;

            // Act
            camera.SnapToTarget();

            // Assert - camera should be at target position
            AssertFloat(camera.GlobalPosition.X).IsEqual(100f, 0.1f);
        }

        [TestCase]
        public void SnapToTarget_WithNoTarget_DoesNotCrash()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());
            camera.Target = null;

            // Act & Assert (should not throw)
            camera.SnapToTarget();
        }

        #endregion

        #region Collision Tests

        [TestCase]
        public void FollowCamera_CollisionEnabled_IsTrue()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());

            // Act & Assert
            AssertBool(camera.EnableCollision).IsTrue();
        }

        [TestCase]
        public void FollowCamera_CanDisableCollision()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());

            // Act
            camera.EnableCollision = false;

            // Assert
            AssertBool(camera.EnableCollision).IsFalse();
        }

        [TestCase]
        public void Process_WithCollisionDisabled_StillFollows()
        {
            // Arrange
            var camera = AutoFree(new FollowCamera());
            var target = AutoFree(new Node3D());
            var root = AutoFree(new Node3D());
            root.AddChild(camera);
            root.AddChild(target);
            
            camera.EnableCollision = false;
            target.GlobalPosition = new Vector3(10, 0, 0);
            camera.GlobalPosition = new Vector3(0, 0, 0);
            camera.Target = target;
            camera.FollowSpeed = 1.0f;

            // Act
            camera._Process(1.0);

            // Assert - camera should still move
            AssertFloat(camera.GlobalPosition.X).IsGreater(0);
        }

        #endregion
    }
}
