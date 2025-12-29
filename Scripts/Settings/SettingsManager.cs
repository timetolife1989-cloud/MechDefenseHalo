using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Settings
{
    /// <summary>
    /// Central settings manager for persistent game settings
    /// Handles loading, saving, and applying all settings
    /// </summary>
    public partial class SettingsManager : Node
    {
        #region Singleton
        
        private static SettingsManager _instance;
        
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("SettingsManager accessed before initialization!");
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Constants
        
        private const string SETTINGS_FILE = "settings.cfg";
        private const string SECTION_GRAPHICS = "graphics";
        private const string SECTION_AUDIO = "audio";
        private const string SECTION_CONTROLS = "controls";
        private const string SECTION_GAMEPLAY = "gameplay";
        
        #endregion
        
        #region Properties
        
        private GameSettings _currentSettings;
        
        public GameSettings CurrentSettings => _currentSettings;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple SettingsManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }
            
            _instance = this;
            
            LoadSettings();
            ApplyAllSettings();
            
            GD.Print("SettingsManager initialized");
        }
        
        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Load settings from disk
        /// </summary>
        public void LoadSettings()
        {
            string settingsPath = GetSettingsPath();
            
            if (!FileAccess.FileExists(settingsPath))
            {
                GD.Print("No saved settings found, creating defaults");
                _currentSettings = CreateDefaultSettings();
                SaveSettings();
                return;
            }
            
            var config = new ConfigFile();
            Error err = config.Load(settingsPath);
            
            if (err != Error.Ok)
            {
                GD.PrintErr($"Failed to load settings: {err}");
                _currentSettings = CreateDefaultSettings();
                return;
            }
            
            _currentSettings = new GameSettings
            {
                Graphics = LoadGraphicsSettings(config),
                Audio = LoadAudioSettings(config),
                Controls = LoadControlSettings(config),
                Gameplay = LoadGameplaySettings(config)
            };
            
            GD.Print("Settings loaded successfully");
        }
        
        /// <summary>
        /// Save current settings to disk
        /// </summary>
        public void SaveSettings()
        {
            var config = new ConfigFile();
            
            SaveGraphicsSettings(config);
            SaveAudioSettings(config);
            SaveControlSettings(config);
            SaveGameplaySettings(config);
            
            Error err = config.Save(GetSettingsPath());
            
            if (err != Error.Ok)
            {
                GD.PrintErr($"Failed to save settings: {err}");
            }
            else
            {
                GD.Print("Settings saved successfully");
                EventBus.Emit("settings_saved");
            }
        }
        
        /// <summary>
        /// Apply all settings immediately
        /// </summary>
        public void ApplyAllSettings()
        {
            if (_currentSettings == null)
            {
                GD.PrintErr("Cannot apply settings - currentSettings is null");
                return;
            }
            
            GraphicsSettingsApplier.Apply(_currentSettings.Graphics);
            AudioSettingsApplier.Apply(_currentSettings.Audio);
            ControlSettingsApplier.Apply(_currentSettings.Controls);
            GameplaySettingsApplier.Apply(_currentSettings.Gameplay);
            
            EventBus.Emit("settings_applied");
        }
        
        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            _currentSettings = CreateDefaultSettings();
            ApplyAllSettings();
            SaveSettings();
            
            GD.Print("Settings reset to defaults");
            EventBus.Emit("settings_reset");
        }
        
        #endregion
        
        #region Private Methods - Load
        
        private GraphicsSettingsData LoadGraphicsSettings(ConfigFile config)
        {
            var settings = new GraphicsSettingsData
            {
                ResolutionWidth = (int)config.GetValue(SECTION_GRAPHICS, "resolution_width", 1920),
                ResolutionHeight = (int)config.GetValue(SECTION_GRAPHICS, "resolution_height", 1080),
                Fullscreen = (bool)config.GetValue(SECTION_GRAPHICS, "fullscreen", true),
                VSync = (bool)config.GetValue(SECTION_GRAPHICS, "vsync", true),
                TargetFPS = (int)config.GetValue(SECTION_GRAPHICS, "target_fps", 60),
                QualityLevel = (QualityPreset)(int)config.GetValue(SECTION_GRAPHICS, "quality_level", (int)QualityPreset.High),
                ShadowQuality = (int)config.GetValue(SECTION_GRAPHICS, "shadow_quality", 2),
                ParticleQuality = (int)config.GetValue(SECTION_GRAPHICS, "particle_quality", 2),
                Bloom = (bool)config.GetValue(SECTION_GRAPHICS, "bloom", true),
                MotionBlur = (bool)config.GetValue(SECTION_GRAPHICS, "motion_blur", false),
                RenderScale = (float)config.GetValue(SECTION_GRAPHICS, "render_scale", 1.0f)
            };
            
            return settings;
        }
        
        private AudioSettingsData LoadAudioSettings(ConfigFile config)
        {
            var settings = new AudioSettingsData
            {
                MasterVolume = (float)config.GetValue(SECTION_AUDIO, "master_volume", 1.0f),
                MusicVolume = (float)config.GetValue(SECTION_AUDIO, "music_volume", 0.8f),
                SFXVolume = (float)config.GetValue(SECTION_AUDIO, "sfx_volume", 1.0f),
                UIVolume = (float)config.GetValue(SECTION_AUDIO, "ui_volume", 0.9f),
                MuteMaster = (bool)config.GetValue(SECTION_AUDIO, "mute_master", false)
            };
            
            return settings;
        }
        
        private ControlSettingsData LoadControlSettings(ConfigFile config)
        {
            var settings = new ControlSettingsData
            {
                MouseSensitivity = (float)config.GetValue(SECTION_CONTROLS, "mouse_sensitivity", 1.0f),
                InvertY = (bool)config.GetValue(SECTION_CONTROLS, "invert_y", false),
                ControllerSensitivity = (float)config.GetValue(SECTION_CONTROLS, "controller_sensitivity", 1.0f),
                ControllerDeadzone = (int)config.GetValue(SECTION_CONTROLS, "controller_deadzone", 15)
            };
            
            // Load key bindings
            settings.KeyBindings = new Dictionary<string, int>(ControlSettingsApplier.DefaultKeyBindings);
            
            foreach (var key in ControlSettingsApplier.DefaultKeyBindings.Keys)
            {
                int value = (int)config.GetValue(SECTION_CONTROLS, $"key_{key}", ControlSettingsApplier.DefaultKeyBindings[key]);
                settings.KeyBindings[key] = value;
            }
            
            return settings;
        }
        
        private GameplaySettingsData LoadGameplaySettings(ConfigFile config)
        {
            var settings = new GameplaySettingsData
            {
                AutoPickupItems = (bool)config.GetValue(SECTION_GAMEPLAY, "auto_pickup", true),
                ShowDamageNumbers = (bool)config.GetValue(SECTION_GAMEPLAY, "damage_numbers", true),
                ScreenShake = (bool)config.GetValue(SECTION_GAMEPLAY, "screen_shake", true),
                ScreenShakeIntensity = (float)config.GetValue(SECTION_GAMEPLAY, "shake_intensity", 1.0f),
                ShowFPSCounter = (bool)config.GetValue(SECTION_GAMEPLAY, "show_fps", false),
                ShowPing = (bool)config.GetValue(SECTION_GAMEPLAY, "show_ping", false),
                Language = (string)config.GetValue(SECTION_GAMEPLAY, "language", "en")
            };
            
            return settings;
        }
        
        #endregion
        
        #region Private Methods - Save
        
        private void SaveGraphicsSettings(ConfigFile config)
        {
            var settings = _currentSettings.Graphics;
            
            config.SetValue(SECTION_GRAPHICS, "resolution_width", settings.ResolutionWidth);
            config.SetValue(SECTION_GRAPHICS, "resolution_height", settings.ResolutionHeight);
            config.SetValue(SECTION_GRAPHICS, "fullscreen", settings.Fullscreen);
            config.SetValue(SECTION_GRAPHICS, "vsync", settings.VSync);
            config.SetValue(SECTION_GRAPHICS, "target_fps", settings.TargetFPS);
            config.SetValue(SECTION_GRAPHICS, "quality_level", (int)settings.QualityLevel);
            config.SetValue(SECTION_GRAPHICS, "shadow_quality", settings.ShadowQuality);
            config.SetValue(SECTION_GRAPHICS, "particle_quality", settings.ParticleQuality);
            config.SetValue(SECTION_GRAPHICS, "bloom", settings.Bloom);
            config.SetValue(SECTION_GRAPHICS, "motion_blur", settings.MotionBlur);
            config.SetValue(SECTION_GRAPHICS, "render_scale", settings.RenderScale);
        }
        
        private void SaveAudioSettings(ConfigFile config)
        {
            var settings = _currentSettings.Audio;
            
            config.SetValue(SECTION_AUDIO, "master_volume", settings.MasterVolume);
            config.SetValue(SECTION_AUDIO, "music_volume", settings.MusicVolume);
            config.SetValue(SECTION_AUDIO, "sfx_volume", settings.SFXVolume);
            config.SetValue(SECTION_AUDIO, "ui_volume", settings.UIVolume);
            config.SetValue(SECTION_AUDIO, "mute_master", settings.MuteMaster);
        }
        
        private void SaveControlSettings(ConfigFile config)
        {
            var settings = _currentSettings.Controls;
            
            config.SetValue(SECTION_CONTROLS, "mouse_sensitivity", settings.MouseSensitivity);
            config.SetValue(SECTION_CONTROLS, "invert_y", settings.InvertY);
            config.SetValue(SECTION_CONTROLS, "controller_sensitivity", settings.ControllerSensitivity);
            config.SetValue(SECTION_CONTROLS, "controller_deadzone", settings.ControllerDeadzone);
            
            // Save key bindings
            foreach (var binding in settings.KeyBindings)
            {
                config.SetValue(SECTION_CONTROLS, $"key_{binding.Key}", binding.Value);
            }
        }
        
        private void SaveGameplaySettings(ConfigFile config)
        {
            var settings = _currentSettings.Gameplay;
            
            config.SetValue(SECTION_GAMEPLAY, "auto_pickup", settings.AutoPickupItems);
            config.SetValue(SECTION_GAMEPLAY, "damage_numbers", settings.ShowDamageNumbers);
            config.SetValue(SECTION_GAMEPLAY, "screen_shake", settings.ScreenShake);
            config.SetValue(SECTION_GAMEPLAY, "shake_intensity", settings.ScreenShakeIntensity);
            config.SetValue(SECTION_GAMEPLAY, "show_fps", settings.ShowFPSCounter);
            config.SetValue(SECTION_GAMEPLAY, "show_ping", settings.ShowPing);
            config.SetValue(SECTION_GAMEPLAY, "language", settings.Language);
        }
        
        #endregion
        
        #region Helper Methods
        
        private GameSettings CreateDefaultSettings()
        {
            return new GameSettings
            {
                Graphics = new GraphicsSettingsData(),
                Audio = new AudioSettingsData(),
                Controls = new ControlSettingsData(),
                Gameplay = new GameplaySettingsData()
            };
        }
        
        private string GetSettingsPath()
        {
            return OS.GetUserDataDir() + "/" + SETTINGS_FILE;
        }
        
        #endregion
    }
}
