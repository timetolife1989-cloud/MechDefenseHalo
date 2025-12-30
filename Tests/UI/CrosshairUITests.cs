using Godot;
using GdUnit4;
using MechDefenseHalo.UI.HUD;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.UI
{
    /// <summary>
    /// Unit tests for CrosshairUI
    /// </summary>
    [TestSuite]
    public class CrosshairUITests
    {
        private CrosshairUI _crosshair;

        [Before]
        public void Setup()
        {
            _crosshair = new CrosshairUI();
        }

        [After]
        public void Teardown()
        {
            _crosshair?.QueueFree();
            _crosshair = null;
        }

        [TestCase]
        public void CrosshairUI_OnCreation_ShouldNotBeNull()
        {
            // Assert
            AssertObject(_crosshair).IsNotNull();
        }

        [TestCase]
        public void ShowHitMarker_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _crosshair.ShowHitMarker()).Not().ThrowsException();
        }

        [TestCase]
        public void SetSpread_WithValidValue_ShouldNotThrow()
        {
            // Arrange
            float spread = 20f;

            // Act & Assert
            AssertThat(() => _crosshair.SetSpread(spread)).Not().ThrowsException();
        }

        [TestCase]
        public void IncreaseSpread_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _crosshair.IncreaseSpread(5f)).Not().ThrowsException();
        }

        [TestCase]
        public void ResetSpread_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _crosshair.ResetSpread()).Not().ThrowsException();
        }

        [TestCase]
        public void SetColor_WithValidColor_ShouldNotThrow()
        {
            // Arrange
            Color color = Colors.Red;

            // Act & Assert
            AssertThat(() => _crosshair.SetColor(color)).Not().ThrowsException();
        }

        [TestCase]
        public void SetCrosshairVisible_WithTrue_ShouldShowCrosshair()
        {
            // Act
            _crosshair.SetCrosshairVisible(true);

            // Assert
            AssertBool(_crosshair.Visible).IsTrue();
        }

        [TestCase]
        public void SetCrosshairVisible_WithFalse_ShouldHideCrosshair()
        {
            // Act
            _crosshair.SetCrosshairVisible(false);

            // Assert
            AssertBool(_crosshair.Visible).IsFalse();
        }
    }
}
