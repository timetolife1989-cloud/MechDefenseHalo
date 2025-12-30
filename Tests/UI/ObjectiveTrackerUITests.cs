using Godot;
using GdUnit4;
using MechDefenseHalo.UI.HUD;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.UI
{
    /// <summary>
    /// Unit tests for ObjectiveTrackerUI
    /// </summary>
    [TestSuite]
    public class ObjectiveTrackerUITests
    {
        private ObjectiveTrackerUI _objectiveTracker;

        [Before]
        public void Setup()
        {
            _objectiveTracker = new ObjectiveTrackerUI();
        }

        [After]
        public void Teardown()
        {
            _objectiveTracker?.QueueFree();
            _objectiveTracker = null;
        }

        [TestCase]
        public void ObjectiveTrackerUI_OnCreation_ShouldNotBeNull()
        {
            // Assert
            AssertObject(_objectiveTracker).IsNotNull();
        }

        [TestCase]
        public void AddObjective_WithValidData_ShouldNotThrow()
        {
            // Arrange
            string id = "obj_1";
            string description = "Defeat 10 enemies";

            // Act & Assert
            AssertThat(() => _objectiveTracker.AddObjective(id, description)).Not().ThrowsException();
        }

        [TestCase]
        public void AddObjective_WithOptionalFlag_ShouldNotThrow()
        {
            // Arrange
            string id = "obj_2";
            string description = "Find secret area";
            bool isOptional = true;

            // Act & Assert
            AssertThat(() => _objectiveTracker.AddObjective(id, description, isOptional)).Not().ThrowsException();
        }

        [TestCase]
        public void UpdateObjective_WithValidProgress_ShouldNotThrow()
        {
            // Arrange
            string id = "obj_1";
            int current = 5;
            int total = 10;

            // Act & Assert
            AssertThat(() => _objectiveTracker.UpdateObjective(id, current, total)).Not().ThrowsException();
        }

        [TestCase]
        public void CompleteObjective_WithValidId_ShouldNotThrow()
        {
            // Arrange
            string id = "obj_1";

            // Act & Assert
            AssertThat(() => _objectiveTracker.CompleteObjective(id)).Not().ThrowsException();
        }

        [TestCase]
        public void FailObjective_WithValidId_ShouldNotThrow()
        {
            // Arrange
            string id = "obj_1";

            // Act & Assert
            AssertThat(() => _objectiveTracker.FailObjective(id)).Not().ThrowsException();
        }

        [TestCase]
        public void RemoveObjective_WithValidId_ShouldNotThrow()
        {
            // Arrange
            string id = "obj_1";

            // Act & Assert
            AssertThat(() => _objectiveTracker.RemoveObjective(id)).Not().ThrowsException();
        }

        [TestCase]
        public void ClearAllObjectives_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _objectiveTracker.ClearAllObjectives()).Not().ThrowsException();
        }
    }
}
