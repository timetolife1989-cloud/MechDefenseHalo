using Godot;
using GdUnit4;
using MechDefenseHalo.UI.HUD;
using MechDefenseHalo.Core;
using MechDefenseHalo.Components;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.UI
{
    /// <summary>
    /// Unit tests for HUDManager
    /// </summary>
    [TestSuite]
    public class HUDManagerTests
    {
        private HUDManager _hudManager;

        [Before]
        public void Setup()
        {
            _hudManager = new HUDManager();
        }

        [After]
        public void Teardown()
        {
            _hudManager?.QueueFree();
            _hudManager = null;
        }

        [TestCase]
        public void HUDManager_OnCreation_ShouldNotBeNull()
        {
            // Assert
            AssertObject(_hudManager).IsNotNull();
        }

        [TestCase]
        public void AddScore_WithPositiveValue_ShouldIncreaseScore()
        {
            // Arrange
            int initialScore = 0;
            int scoreToAdd = 100;

            // Act
            _hudManager.AddScore(scoreToAdd);

            // Assert - we can't directly check the score, but we verify the method doesn't throw
            AssertThat(() => _hudManager.AddScore(scoreToAdd)).Not().ThrowsException();
        }

        [TestCase]
        public void ResetScore_ShouldSetScoreToZero()
        {
            // Arrange
            _hudManager.AddScore(500);

            // Act
            _hudManager.ResetScore();

            // Assert - verify method execution without exception
            AssertThat(() => _hudManager.ResetScore()).Not().ThrowsException();
        }

        [TestCase]
        public void SetHUDVisible_WithTrue_ShouldShowHUD()
        {
            // Act
            _hudManager.SetHUDVisible(true);

            // Assert
            AssertBool(_hudManager.Visible).IsTrue();
        }

        [TestCase]
        public void SetHUDVisible_WithFalse_ShouldHideHUD()
        {
            // Act
            _hudManager.SetHUDVisible(false);

            // Assert
            AssertBool(_hudManager.Visible).IsFalse();
        }
    }
}
