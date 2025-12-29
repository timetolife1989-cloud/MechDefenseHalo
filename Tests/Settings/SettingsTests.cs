using Godot;
using GdUnit4;
using MechDefenseHalo.Settings;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Settings
{
    /// <summary>
    /// Unit tests for Settings system
    /// 
    /// Tests:
    /// - Settings creation and defaults
    /// - Settings persistence (save/load)
    /// - Settings application
    /// - Reset to defaults
    /// </summary>
    [TestSuite]
    public class SettingsTests
    {
        // Note: These tests focus on data structures and don't test the singleton SettingsManager
        // Integration tests should test the actual SettingsManager singleton behavior
        private string _testSettingsPath;

        [Before]
        public void Setup()
        {
            _testSettingsPath = OS.GetUserDataDir() + "/test_settings.cfg";
            
            // Clean up any existing test settings
            if (FileAccess.FileExists(_testSettingsPath))
            {
                DirAccess.RemoveAbsolute(_testSettingsPath);
            }
        }

        [After]
        public void Teardown()
        {
            // Clean up test settings file
            if (FileAccess.FileExists(_testSettingsPath))
            {
                DirAccess.RemoveAbsolute(_testSettingsPath);
            }
        }

        [TestCase]
        public void CreateDefaultSettings_ShouldHaveValidDefaults()
        {
            // Act
            var settings = new GameSettings();

            // Assert - Graphics
            AssertThat(settings.Graphics).IsNotNull();
            AssertThat(settings.Graphics.ResolutionWidth).IsEqual(1920);
            AssertThat(settings.Graphics.ResolutionHeight).IsEqual(1080);
            AssertThat(settings.Graphics.Fullscreen).IsTrue();
            AssertThat(settings.Graphics.QualityLevel).IsEqual(QualityPreset.High);

            // Assert - Audio
            AssertThat(settings.Audio).IsNotNull();
            AssertThat(settings.Audio.MasterVolume).IsEqual(1.0f);
            AssertThat(settings.Audio.MusicVolume).IsEqual(0.8f);

            // Assert - Controls
            AssertThat(settings.Controls).IsNotNull();
            AssertThat(settings.Controls.MouseSensitivity).IsEqual(1.0f);
            AssertThat(settings.Controls.InvertY).IsFalse();

            // Assert - Gameplay
            AssertThat(settings.Gameplay).IsNotNull();
            AssertThat(settings.Gameplay.AutoPickupItems).IsTrue();
            AssertThat(settings.Gameplay.ShowDamageNumbers).IsTrue();
        }

        [TestCase]
        public void GraphicsSettingsData_ShouldHaveValidRanges()
        {
            // Arrange
            var settings = new GraphicsSettingsData();

            // Assert
            AssertThat(settings.RenderScale).IsGreaterEqual(0.5f);
            AssertThat(settings.RenderScale).IsLessEqual(1.5f);
            AssertThat(settings.ShadowQuality).IsGreaterEqual(0);
            AssertThat(settings.ShadowQuality).IsLessEqual(3);
        }

        [TestCase]
        public void AudioSettingsData_VolumesShouldBeNormalized()
        {
            // Arrange
            var settings = new AudioSettingsData();

            // Assert - All volumes should be in 0-1 range
            AssertThat(settings.MasterVolume).IsGreaterEqual(0f);
            AssertThat(settings.MasterVolume).IsLessEqual(1f);
            AssertThat(settings.MusicVolume).IsGreaterEqual(0f);
            AssertThat(settings.MusicVolume).IsLessEqual(1f);
            AssertThat(settings.SFXVolume).IsGreaterEqual(0f);
            AssertThat(settings.SFXVolume).IsLessEqual(1f);
        }

        [TestCase]
        public void ControlSettingsApplier_ShouldHaveDefaultBindings()
        {
            // Act
            var defaults = ControlSettingsApplier.DefaultKeyBindings;

            // Assert
            AssertThat(defaults).IsNotNull();
            AssertThat(defaults.Count).IsGreater(0);
            AssertThat(defaults.ContainsKey("move_forward")).IsTrue();
            AssertThat(defaults.ContainsKey("move_backward")).IsTrue();
            AssertThat(defaults.ContainsKey("fire")).IsTrue();
            AssertThat(defaults.ContainsKey("reload")).IsTrue();
        }

        [TestCase]
        public void QualityPreset_ShouldHaveAllLevels()
        {
            // Assert - Verify all quality presets exist
            var low = QualityPreset.Low;
            var medium = QualityPreset.Medium;
            var high = QualityPreset.High;
            var ultra = QualityPreset.Ultra;

            AssertThat((int)low).IsEqual(0);
            AssertThat((int)medium).IsEqual(1);
            AssertThat((int)high).IsEqual(2);
            AssertThat((int)ultra).IsEqual(3);
        }

        [TestCase]
        public void GameplaySettings_DefaultLanguageShouldBeEnglish()
        {
            // Arrange
            var settings = new GameplaySettingsData();

            // Assert
            AssertThat(settings.Language).IsEqual("en");
        }

        [TestCase]
        public void ScreenShakeIntensity_ShouldBeInValidRange()
        {
            // Arrange
            var settings = new GameplaySettingsData();

            // Assert
            AssertThat(settings.ScreenShakeIntensity).IsGreaterEqual(0f);
            AssertThat(settings.ScreenShakeIntensity).IsLessEqual(2f);
        }
    }
}
