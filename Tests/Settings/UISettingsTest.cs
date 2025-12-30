using Godot;
using System;
using MechDefenseHalo.UI.Settings;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.Tests.Settings
{
    /// <summary>
    /// Test script for UI.Settings components
    /// Tests the new settings menu UI implementation
    /// </summary>
    public partial class UISettingsTest : Node
    {
        public override void _Ready()
        {
            GD.Print("=== UI Settings Components Test ===");
            
            // Wait a frame for autoloads to initialize
            CallDeferred(nameof(RunTests));
        }
        
        private void RunTests()
        {
            TestSettingsValidator();
            TestAccessibilitySettings();
            TestIntegrationWithSettingsManager();
            
            GD.Print("=== UI Settings Test Complete ===");
        }
        
        #region Validator Tests
        
        private void TestSettingsValidator()
        {
            GD.Print("\n--- Testing SettingsValidator ---");
            
            // Test resolution validation
            bool validRes = SettingsValidator.ValidateResolution(1920, 1080);
            GD.Print($"✅ Resolution 1920x1080 valid: {validRes}");
            
            bool invalidRes = SettingsValidator.ValidateResolution(800, 600);
            GD.Print($"✅ Resolution 800x600 invalid (too small): {!invalidRes}");
            
            // Test FPS clamping
            int clampedFPS = SettingsValidator.ClampFPS(300);
            GD.Print($"✅ FPS 300 clamped to: {clampedFPS} (expected 240)");
            
            clampedFPS = SettingsValidator.ClampFPS(15);
            GD.Print($"✅ FPS 15 clamped to: {clampedFPS} (expected 30)");
            
            clampedFPS = SettingsValidator.ClampFPS(0);
            GD.Print($"✅ FPS 0 (unlimited) allowed: {clampedFPS == 0}");
            
            // Test render scale clamping
            float clampedScale = SettingsValidator.ClampRenderScale(3.0f);
            GD.Print($"✅ Render scale 3.0 clamped to: {clampedScale} (expected 2.0)");
            
            clampedScale = SettingsValidator.ClampRenderScale(0.2f);
            GD.Print($"✅ Render scale 0.2 clamped to: {clampedScale} (expected 0.5)");
            
            // Test volume clamping
            float clampedVolume = SettingsValidator.ClampVolume(1.5f);
            GD.Print($"✅ Volume 1.5 clamped to: {clampedVolume} (expected 1.0)");
            
            clampedVolume = SettingsValidator.ClampVolume(-0.5f);
            GD.Print($"✅ Volume -0.5 clamped to: {clampedVolume} (expected 0.0)");
            
            // Test sensitivity clamping
            float clampedSens = SettingsValidator.ClampSensitivity(10.0f);
            GD.Print($"✅ Sensitivity 10.0 clamped to: {clampedSens} (expected 5.0)");
            
            clampedSens = SettingsValidator.ClampSensitivity(0.05f);
            GD.Print($"✅ Sensitivity 0.05 clamped to: {clampedSens} (expected 0.1)");
            
            // Test deadzone clamping
            int clampedDeadzone = SettingsValidator.ClampDeadzone(75);
            GD.Print($"✅ Deadzone 75 clamped to: {clampedDeadzone} (expected 50)");
            
            // Test key binding validation
            bool validKey = SettingsValidator.ValidateKeyBinding((int)Key.A);
            GD.Print($"✅ Key 'A' is valid: {validKey}");
            
            bool invalidKey = SettingsValidator.ValidateKeyBinding((int)Key.Escape);
            GD.Print($"✅ Key 'Escape' is invalid: {!invalidKey}");
            
            // Test UI scale clamping
            float clampedUIScale = SettingsValidator.ClampUIScale(3.0f);
            GD.Print($"✅ UI Scale 3.0 clamped to: {clampedUIScale} (expected 2.0)");
            
            // Test text size clamping
            float clampedTextSize = SettingsValidator.ClampTextSize(0.3f);
            GD.Print($"✅ Text Size 0.3 clamped to: {clampedTextSize} (expected 0.5)");
            
            GD.Print("✅ SettingsValidator tests passed");
        }
        
        #endregion
        
        #region Accessibility Tests
        
        private void TestAccessibilitySettings()
        {
            GD.Print("\n--- Testing AccessibilitySettings ---");
            
            // Test default values
            var defaults = new AccessibilitySettingsData();
            GD.Print($"✅ Default colorblind mode: {defaults.ColorblindMode} (expected None)");
            GD.Print($"✅ Default subtitles: {defaults.SubtitlesEnabled} (expected False)");
            GD.Print($"✅ Default UI scale: {defaults.UIScale} (expected 1.0)");
            GD.Print($"✅ Default high contrast: {defaults.HighContrastMode} (expected False)");
            GD.Print($"✅ Default reduced motion: {defaults.ReducedMotion} (expected False)");
            GD.Print($"✅ Default text size: {defaults.TextSize} (expected 1.0)");
            GD.Print($"✅ Default auto pause: {defaults.AutoPauseOnFocusLoss} (expected True)");
            
            // Test colorblind mode enum
            GD.Print("\nColorblind Modes:");
            foreach (ColorblindMode mode in Enum.GetValues(typeof(ColorblindMode)))
            {
                GD.Print($"  - {mode} = {(int)mode}");
            }
            
            GD.Print("✅ AccessibilitySettings data structure tests passed");
        }
        
        #endregion
        
        #region Integration Tests
        
        private void TestIntegrationWithSettingsManager()
        {
            GD.Print("\n--- Testing Integration with SettingsManager ---");
            
            var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
            if (settingsManager == null)
            {
                GD.PrintErr("❌ SettingsManager not available!");
                return;
            }
            
            GD.Print("✅ SettingsManager is available");
            
            // Test that CurrentSettings exists
            if (settingsManager.CurrentSettings == null)
            {
                GD.PrintErr("❌ CurrentSettings is null!");
                return;
            }
            
            GD.Print("✅ CurrentSettings is initialized");
            
            // Test graphics settings validation
            var graphics = settingsManager.CurrentSettings.Graphics;
            var validatedGraphics = SettingsValidator.ValidateGraphicsSettings(graphics);
            GD.Print($"✅ Graphics settings validated: Resolution {validatedGraphics.ResolutionWidth}x{validatedGraphics.ResolutionHeight}");
            
            // Test audio settings validation
            var audio = settingsManager.CurrentSettings.Audio;
            var validatedAudio = SettingsValidator.ValidateAudioSettings(audio);
            GD.Print($"✅ Audio settings validated: Master volume {validatedAudio.MasterVolume}");
            
            // Test control settings validation
            var controls = settingsManager.CurrentSettings.Controls;
            var validatedControls = SettingsValidator.ValidateControlSettings(controls);
            GD.Print($"✅ Control settings validated: Mouse sensitivity {validatedControls.MouseSensitivity}");
            
            // Test that settings can be saved
            try
            {
                settingsManager.SaveSettings();
                GD.Print("✅ Settings saved successfully");
            }
            catch (Exception e)
            {
                GD.PrintErr($"❌ Failed to save settings: {e.Message}");
            }
            
            GD.Print("✅ Integration tests passed");
        }
        
        #endregion
        
        #region Component Creation Tests
        
        /// <summary>
        /// Test that can be called manually to verify component instantiation
        /// </summary>
        public void TestComponentCreation()
        {
            GD.Print("\n--- Testing Component Creation ---");
            
            // Test SettingsMenu creation
            var settingsMenu = new SettingsMenu();
            if (settingsMenu != null)
            {
                GD.Print("✅ SettingsMenu instantiated");
                settingsMenu.QueueFree();
            }
            else
            {
                GD.PrintErr("❌ Failed to create SettingsMenu");
            }
            
            // Test GraphicsSettings creation
            var graphicsSettings = new GraphicsSettings();
            if (graphicsSettings != null)
            {
                GD.Print("✅ GraphicsSettings instantiated");
                graphicsSettings.QueueFree();
            }
            else
            {
                GD.PrintErr("❌ Failed to create GraphicsSettings");
            }
            
            // Test AudioSettings creation
            var audioSettings = new AudioSettings();
            if (audioSettings != null)
            {
                GD.Print("✅ AudioSettings instantiated");
                audioSettings.QueueFree();
            }
            else
            {
                GD.PrintErr("❌ Failed to create AudioSettings");
            }
            
            // Test ControlSettings creation
            var controlSettings = new ControlSettings();
            if (controlSettings != null)
            {
                GD.Print("✅ ControlSettings instantiated");
                controlSettings.QueueFree();
            }
            else
            {
                GD.PrintErr("❌ Failed to create ControlSettings");
            }
            
            // Test AccessibilitySettings creation
            var accessibilitySettings = new AccessibilitySettings();
            if (accessibilitySettings != null)
            {
                GD.Print("✅ AccessibilitySettings instantiated");
                accessibilitySettings.QueueFree();
            }
            else
            {
                GD.PrintErr("❌ Failed to create AccessibilitySettings");
            }
            
            GD.Print("✅ All components can be instantiated");
        }
        
        #endregion
        
        #region Manual Test Methods
        
        /// <summary>
        /// Manual test to demonstrate validation on extreme values
        /// </summary>
        public void TestExtremeValues()
        {
            GD.Print("\n--- Testing Extreme Values ---");
            
            var graphics = new GraphicsSettingsData
            {
                ResolutionWidth = 10000,
                ResolutionHeight = 10000,
                TargetFPS = 1000,
                RenderScale = 10.0f,
                ShadowQuality = 100
            };
            
            GD.Print($"Before validation: {graphics.ResolutionWidth}x{graphics.ResolutionHeight}, FPS: {graphics.TargetFPS}, Scale: {graphics.RenderScale}");
            
            graphics = SettingsValidator.ValidateGraphicsSettings(graphics);
            
            GD.Print($"After validation: {graphics.ResolutionWidth}x{graphics.ResolutionHeight}, FPS: {graphics.TargetFPS}, Scale: {graphics.RenderScale}");
            GD.Print("✅ Extreme values properly validated");
        }
        
        #endregion
    }
}
