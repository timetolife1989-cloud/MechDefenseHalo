using Godot;
using GdUnit4;
using MechDefenseHalo.Hangar;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Hangar
{
    /// <summary>
    /// Unit tests for ModelRotator
    /// </summary>
    [TestSuite]
    public class ModelRotatorTests
    {
        private ModelRotator _rotator;
        private Node3D _testModel;

        [Before]
        public void Setup()
        {
            _rotator = new ModelRotator();
            _testModel = new Node3D();
        }

        [After]
        public void Teardown()
        {
            _testModel?.QueueFree();
            _rotator = null;
            _testModel = null;
        }

        [TestCase]
        public void SetTarget_ShouldSetTargetModel()
        {
            // Act
            _rotator.SetTarget(_testModel);

            // Assert - No exception should be thrown
            AssertObject(_testModel).IsNotNull();
        }

        [TestCase]
        public void RotateModel_WithoutTarget_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _rotator.RotateModel(new Vector2(10, 0)))
                .Not().ThrowsException();
        }

        [TestCase]
        public void RotateModel_WithTarget_ShouldUpdateRotation()
        {
            // Arrange
            _rotator.SetTarget(_testModel);
            var initialRotation = _testModel.RotationDegrees;

            // Act
            _rotator.RotateModel(new Vector2(50, 0));

            // Assert - Rotation should have changed
            AssertThat(_testModel.RotationDegrees.Y).IsNotEqual(initialRotation.Y);
        }

        [TestCase]
        public void ResetRotation_ShouldResetToZero()
        {
            // Arrange
            _rotator.SetTarget(_testModel);
            _rotator.RotateModel(new Vector2(50, 25));

            // Act
            _rotator.ResetRotation();

            // Assert
            AssertFloat(_testModel.RotationDegrees.Y).IsEqual(0f);
            AssertFloat(_testModel.RotationDegrees.X).IsEqual(0f);
        }

        [TestCase]
        public void RotateModel_VerticalRotation_ShouldBeClamped()
        {
            // Arrange
            _rotator.SetTarget(_testModel);

            // Act - Try to rotate beyond limits
            _rotator.RotateModel(new Vector2(0, 1000));

            // Assert - Should be clamped between -80 and 80
            AssertFloat(_testModel.RotationDegrees.X).IsLessEqual(80f);
            AssertFloat(_testModel.RotationDegrees.X).IsGreaterEqual(-80f);
        }
    }
}
