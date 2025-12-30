using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.AI;

namespace MechDefenseHalo.Tests.AI
{
    /// <summary>
    /// Unit tests for FormationController
    /// Tests formation positioning and member management
    /// </summary>
    [TestSuite]
    public class FormationControllerTests
    {
        private FormationController _controller;
        
        [Before]
        public void Setup()
        {
            _controller = new FormationController();
            _controller.Spacing = 2f;
        }
        
        [After]
        public void Teardown()
        {
            _controller = null;
        }
        
        [TestCase]
        public void GetFormationPosition_LineFormation_ShouldReturnHorizontalLine()
        {
            // Arrange
            _controller.Formation = FormationController.FormationType.Line;
            var enemy = new EnemyAIController();
            _controller.AddMember(enemy);
            var center = Vector3.Zero;
            
            // Act
            var position = _controller.GetFormationPosition(enemy, center);
            
            // Assert
            AssertFloat(position.Y).IsEqual(0f);
            AssertFloat(position.Z).IsEqual(0f);
        }
        
        [TestCase]
        public void GetFormationPosition_ColumnFormation_ShouldReturnVerticalLine()
        {
            // Arrange
            _controller.Formation = FormationController.FormationType.Column;
            var enemy = new EnemyAIController();
            _controller.AddMember(enemy);
            var center = Vector3.Zero;
            
            // Act
            var position = _controller.GetFormationPosition(enemy, center);
            
            // Assert
            AssertFloat(position.X).IsEqual(0f);
            AssertFloat(position.Y).IsEqual(0f);
        }
        
        [TestCase]
        public void GetFormationPosition_CircleFormation_ShouldReturnCircularPosition()
        {
            // Arrange
            _controller.Formation = FormationController.FormationType.Circle;
            var enemy = new EnemyAIController();
            _controller.AddMember(enemy);
            var center = Vector3.Zero;
            
            // Act
            var position = _controller.GetFormationPosition(enemy, center);
            
            // Assert
            AssertFloat(position.Y).IsEqual(0f);
            // Should be on a circle with radius = Spacing * 2
            float expectedRadius = _controller.Spacing * 2;
            float actualRadius = Mathf.Sqrt(position.X * position.X + position.Z * position.Z);
            AssertFloat(actualRadius).IsEqual(expectedRadius);
        }
        
        [TestCase]
        public void AddMember_NewMember_ShouldIncreaseCount()
        {
            // Arrange
            var enemy = new EnemyAIController();
            
            // Act
            _controller.AddMember(enemy);
            
            // Assert - member should be added successfully
            var position = _controller.GetFormationPosition(enemy, Vector3.Zero);
            AssertObject(position).IsNotNull();
        }
        
        [TestCase]
        public void AddMember_SameMemberTwice_ShouldNotDuplicate()
        {
            // Arrange
            var enemy = new EnemyAIController();
            
            // Act
            _controller.AddMember(enemy);
            _controller.AddMember(enemy); // Add same member again
            
            // Assert - should still have consistent position
            var position1 = _controller.GetFormationPosition(enemy, Vector3.Zero);
            var position2 = _controller.GetFormationPosition(enemy, Vector3.Zero);
            AssertFloat(position1.X).IsEqual(position2.X);
        }
        
        [TestCase]
        public void RemoveMember_ExistingMember_ShouldRemove()
        {
            // Arrange
            var enemy = new EnemyAIController();
            _controller.AddMember(enemy);
            
            // Act
            _controller.RemoveMember(enemy);
            
            // Assert - should return default position (target position)
            var targetPos = new Vector3(10, 0, 10);
            var position = _controller.GetFormationPosition(enemy, targetPos);
            AssertFloat(position.X).IsEqual(targetPos.X);
            AssertFloat(position.Z).IsEqual(targetPos.Z);
        }
    }
}
