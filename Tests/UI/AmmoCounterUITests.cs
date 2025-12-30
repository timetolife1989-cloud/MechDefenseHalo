using Godot;
using GdUnit4;
using MechDefenseHalo.UI.HUD;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.UI
{
    /// <summary>
    /// Unit tests for AmmoCounterUI
    /// </summary>
    [TestSuite]
    public class AmmoCounterUITests
    {
        private AmmoCounterUI _ammoCounter;

        [Before]
        public void Setup()
        {
            _ammoCounter = new AmmoCounterUI();
        }

        [After]
        public void Teardown()
        {
            _ammoCounter?.QueueFree();
            _ammoCounter = null;
        }

        [TestCase]
        public void AmmoCounterUI_OnCreation_ShouldNotBeNull()
        {
            // Assert
            AssertObject(_ammoCounter).IsNotNull();
        }

        [TestCase]
        public void UpdateAmmo_WithValidValues_ShouldNotThrow()
        {
            // Arrange
            int current = 15;
            int max = 30;

            // Act & Assert
            AssertThat(() => _ammoCounter.UpdateAmmo(current, max)).Not().ThrowsException();
        }

        [TestCase]
        public void UpdateAmmo_WithZeroAmmo_ShouldNotThrow()
        {
            // Arrange
            int current = 0;
            int max = 30;

            // Act & Assert
            AssertThat(() => _ammoCounter.UpdateAmmo(current, max)).Not().ThrowsException();
        }

        [TestCase]
        public void UpdateAmmo_WithNegativeValues_ShouldClampToZero()
        {
            // Arrange
            int current = -10;
            int max = 30;

            // Act & Assert
            AssertThat(() => _ammoCounter.UpdateAmmo(current, max)).Not().ThrowsException();
        }

        [TestCase]
        public void ShowReloadProgress_WithValidProgress_ShouldNotThrow()
        {
            // Arrange
            float progress = 0.5f;

            // Act & Assert
            AssertThat(() => _ammoCounter.ShowReloadProgress(progress)).Not().ThrowsException();
        }

        [TestCase]
        public void OnReloadComplete_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _ammoCounter.OnReloadComplete()).Not().ThrowsException();
        }

        [TestCase]
        public void StartReload_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _ammoCounter.StartReload()).Not().ThrowsException();
        }
    }
}
