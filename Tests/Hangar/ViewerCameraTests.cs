using Godot;
using GdUnit4;
using MechDefenseHalo.Hangar;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Hangar
{
    /// <summary>
    /// Unit tests for ViewerCamera
    /// </summary>
    [TestSuite]
    public class ViewerCameraTests
    {
        private ViewerCamera _camera;

        [Before]
        public void Setup()
        {
            _camera = new ViewerCamera();
        }

        [After]
        public void Teardown()
        {
            _camera = null;
        }

        [TestCase]
        public void SetDistance_WithinRange_ShouldSetDistance()
        {
            // Act
            _camera.SetDistance(5f);

            // Assert - Position.Z should be set to the distance
            AssertFloat(_camera.Position.Z).IsEqual(5f);
        }

        [TestCase]
        public void SetDistance_BelowMin_ShouldClampToMin()
        {
            // Act
            _camera.SetDistance(1f); // Below minimum of 2

            // Assert - Should be clamped to minimum
            AssertFloat(_camera.Position.Z).IsGreaterEqual(2f);
        }

        [TestCase]
        public void SetDistance_AboveMax_ShouldClampToMax()
        {
            // Act
            _camera.SetDistance(100f); // Above maximum of 20

            // Assert - Should be clamped to maximum
            AssertFloat(_camera.Position.Z).IsLessEqual(20f);
        }

        [TestCase]
        public void ZoomIn_ShouldDecreaseDistance()
        {
            // Arrange
            _camera.SetDistance(10f);
            var initialDistance = _camera.Position.Z;

            // Act
            _camera.ZoomIn();

            // Assert - Distance should decrease
            AssertFloat(_camera.Position.Z).IsLess(initialDistance);
        }

        [TestCase]
        public void ZoomOut_ShouldIncreaseDistance()
        {
            // Arrange
            _camera.SetDistance(10f);
            var initialDistance = _camera.Position.Z;

            // Act
            _camera.ZoomOut();

            // Assert - Distance should increase
            AssertFloat(_camera.Position.Z).IsGreater(initialDistance);
        }

        [TestCase]
        public void ZoomIn_AtMinDistance_ShouldStayAtMin()
        {
            // Arrange
            _camera.SetDistance(2f); // At minimum

            // Act
            _camera.ZoomIn();
            _camera.ZoomIn();

            // Assert - Should stay at minimum
            AssertFloat(_camera.Position.Z).IsGreaterEqual(2f);
        }

        [TestCase]
        public void ZoomOut_AtMaxDistance_ShouldStayAtMax()
        {
            // Arrange
            _camera.SetDistance(20f); // At maximum

            // Act
            _camera.ZoomOut();
            _camera.ZoomOut();

            // Assert - Should stay at maximum
            AssertFloat(_camera.Position.Z).IsLessEqual(20f);
        }
    }
}
