using Godot;
using MechDefenseHalo.Settings;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Scripts.Settings
{
    /// <summary>
    /// Simple validation script to test the settings system
    /// Can be attached to any node for testing
    /// </summary>
    public partial class SettingsValidator : Node
    {
        public override void _Ready()
        {
            GD.Print("=== Settings System Validation ===");
            
            // Wait a frame for autoloads to initialize
            CallDeferred(nameof(ValidateSettings));
        }
        
        private void ValidateSettings()
        {
            // Check SettingsManager
            if (SettingsManager.Instance == null)
            {
                GD.PrintErr("❌ SettingsManager not found! Check autoload configuration.");
                return;
            }
            GD.Print("✅ SettingsManager initialized");
            
            // Check EventBus
            if (EventBus.Instance == null)
            {
                GD.PrintErr("❌ EventBus not found!");
                return;
            }
            GD.Print("✅ EventBus initialized");
            
            // Check settings loaded
            var settings = SettingsManager.Instance.CurrentSettings;
            if (settings == null)
            {
                GD.PrintErr("❌ CurrentSettings is null!");
                return;
            }
            GD.Print("✅ CurrentSettings loaded");
            
            // Validate each settings category
            ValidateGraphicsSettings(settings.Graphics);
            ValidateAudioSettings(settings.Audio);
            ValidateControlSettings(settings.Controls);
            ValidateGameplaySettings(settings.Gameplay);
            
            // Test event system
            TestEventSystem();
            
            // Test save/load
            TestPersistence();
            
            GD.Print("=== Validation Complete ===");
        }
        
        private void ValidateGraphicsSettings(GraphicsSettingsData graphics)
        {
            if (graphics == null)
            {
                GD.PrintErr("❌ Graphics settings is null!");
                return;
            }
            
            GD.Print($"✅ Graphics Settings:");
            GD.Print($"   - Resolution: {graphics.ResolutionWidth}x{graphics.ResolutionHeight}");
            GD.Print($"   - Fullscreen: {graphics.Fullscreen}");
            GD.Print($"   - VSync: {graphics.VSync}");
            GD.Print($"   - Quality: {graphics.QualityLevel}");
            GD.Print($"   - Target FPS: {graphics.TargetFPS}");
        }
        
        private void ValidateAudioSettings(AudioSettingsData audio)
        {
            if (audio == null)
            {
                GD.PrintErr("❌ Audio settings is null!");
                return;
            }
            
            GD.Print($"✅ Audio Settings:");
            GD.Print($"   - Master: {audio.MasterVolume * 100:F0}%");
            GD.Print($"   - Music: {audio.MusicVolume * 100:F0}%");
            GD.Print($"   - SFX: {audio.SFXVolume * 100:F0}%");
            GD.Print($"   - UI: {audio.UIVolume * 100:F0}%");
            GD.Print($"   - Muted: {audio.MuteMaster}");
        }
        
        private void ValidateControlSettings(ControlSettingsData controls)
        {
            if (controls == null)
            {
                GD.PrintErr("❌ Control settings is null!");
                return;
            }
            
            GD.Print($"✅ Control Settings:");
            GD.Print($"   - Mouse Sensitivity: {controls.MouseSensitivity}");
            GD.Print($"   - Invert Y: {controls.InvertY}");
            GD.Print($"   - Key Bindings: {controls.KeyBindings?.Count ?? 0} actions");
            
            // Validate default bindings exist
            if (ControlSettingsApplier.DefaultKeyBindings.Count > 0)
            {
                GD.Print($"   - Default Bindings: {ControlSettingsApplier.DefaultKeyBindings.Count} actions");
            }
        }
        
        private void ValidateGameplaySettings(GameplaySettingsData gameplay)
        {
            if (gameplay == null)
            {
                GD.PrintErr("❌ Gameplay settings is null!");
                return;
            }
            
            GD.Print($"✅ Gameplay Settings:");
            GD.Print($"   - Auto Pickup: {gameplay.AutoPickupItems}");
            GD.Print($"   - Damage Numbers: {gameplay.ShowDamageNumbers}");
            GD.Print($"   - Screen Shake: {gameplay.ScreenShake}");
            GD.Print($"   - Shake Intensity: {gameplay.ScreenShakeIntensity}");
            GD.Print($"   - Language: {gameplay.Language}");
        }
        
        private void TestEventSystem()
        {
            GD.Print("\n--- Testing Event System ---");
            
            bool eventReceived = false;
            
            EventBus.On("test_settings_event", (data) =>
            {
                eventReceived = true;
                GD.Print($"✅ Event received with data: {data}");
            });
            
            EventBus.Emit("test_settings_event", "Test Data");
            
            if (eventReceived)
            {
                GD.Print("✅ Event system working");
            }
            else
            {
                GD.PrintErr("❌ Event system failed");
            }
            
            EventBus.Off("test_settings_event", null);
        }
        
        private void TestPersistence()
        {
            GD.Print("\n--- Testing Persistence ---");
            
            var settingsPath = OS.GetUserDataDir() + "/settings.cfg";
            GD.Print($"Settings file location: {settingsPath}");
            
            if (FileAccess.FileExists(settingsPath))
            {
                GD.Print("✅ Settings file exists");
            }
            else
            {
                GD.Print("⚠️ Settings file doesn't exist yet (will be created on first save)");
            }
            
            // Test save
            try
            {
                SettingsManager.Instance.SaveSettings();
                GD.Print("✅ Settings saved successfully");
                
                if (FileAccess.FileExists(settingsPath))
                {
                    GD.Print("✅ Settings file created");
                }
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"❌ Failed to save settings: {e.Message}");
            }
        }
        
        /// <summary>
        /// Test method that can be called from console or debug menu
        /// </summary>
        public void TestModifySettings()
        {
            GD.Print("\n--- Testing Settings Modification ---");
            
            var settings = SettingsManager.Instance.CurrentSettings;
            
            // Modify some settings
            settings.Audio.MasterVolume = 0.75f;
            settings.Graphics.QualityLevel = QualityPreset.Medium;
            settings.Gameplay.ShowDamageNumbers = false;
            
            GD.Print("Settings modified");
            
            // Apply
            SettingsManager.Instance.ApplyAllSettings();
            GD.Print("✅ Settings applied");
            
            // Save
            SettingsManager.Instance.SaveSettings();
            GD.Print("✅ Settings saved");
        }
        
        /// <summary>
        /// Test reset to defaults
        /// </summary>
        public void TestResetToDefaults()
        {
            GD.Print("\n--- Testing Reset to Defaults ---");
            
            SettingsManager.Instance.ResetToDefaults();
            GD.Print("✅ Settings reset to defaults");
            
            var settings = SettingsManager.Instance.CurrentSettings;
            GD.Print($"Graphics Quality: {settings.Graphics.QualityLevel}");
            GD.Print($"Audio Master: {settings.Audio.MasterVolume * 100:F0}%");
        }
    }
}
