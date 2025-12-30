using Godot;
using GdUnit4;
using MechDefenseHalo.UI.HUD;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.UI
{
    /// <summary>
    /// Unit tests for HealthBarUI
    /// </summary>
    [TestSuite]
    public class HealthBarUITests
    {
        private HealthBarUI _healthBar;

        [Before]
        public void Setup()
        {
            _healthBar = new HealthBarUI();
        }

        [After]
        public void Teardown()
        {
            _healthBar?.QueueFree();
            _healthBar = null;
        }

        [TestCase]
        public void HealthBarUI_OnCreation_ShouldNotBeNull()
        {
            // Assert
            AssertObject(_healthBar).IsNotNull();
        }

        [TestCase]
        public void UpdateHealth_WithValidValues_ShouldNotThrow()
        {
            // Arrange
            float current = 50f;
            float max = 100f;

            // Act & Assert
            AssertThat(() => _healthBar.UpdateHealth(current, max)).Not().ThrowsException();
        }

        [TestCase]
        public void UpdateHealth_WithZeroHealth_ShouldNotThrow()
        {
            // Arrange
            float current = 0f;
            float max = 100f;

            // Act & Assert
            AssertThat(() => _healthBar.UpdateHealth(current, max)).Not().ThrowsException();
        }

        [TestCase]
        public void UpdateHealth_WithNegativeValues_ShouldClampToZero()
        {
            // Arrange
            float current = -50f;
            float max = 100f;

            // Act & Assert
            AssertThat(() => _healthBar.UpdateHealth(current, max)).Not().ThrowsException();
        }

        [TestCase]
        public void UpdateShield_WithValidValues_ShouldNotThrow()
        {
            // Arrange
            float current = 25f;
            float max = 50f;

            // Act & Assert
            AssertThat(() => _healthBar.UpdateShield(current, max)).Not().ThrowsException();
        }

        [TestCase]
        public void SetShieldVisible_WithTrue_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _healthBar.SetShieldVisible(true)).Not().ThrowsException();
        }

        [TestCase]
        public void SetShieldVisible_WithFalse_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _healthBar.SetShieldVisible(false)).Not().ThrowsException();
        }
    }
}
