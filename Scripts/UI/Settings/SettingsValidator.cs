using Godot;
using System;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.UI.Settings
{
    /// <summary>
    /// Settings validator for the UI.Settings namespace
    /// Validates settings values and ensures they are within acceptable ranges
    /// </summary>
    public static class SettingsValidator
    {
        #region Graphics Validation
        
        public static bool ValidateResolution(int width, int height)
        {
            // Minimum resolution: 1280x720
            // Maximum resolution: 7680x4320 (8K)
            if (width < 1280 || height < 720)
            {
                GD.PrintErr($"Resolution too small: {width}x{height}. Minimum is 1280x720");
                return false;
            }
            
            if (width > 7680 || height > 4320)
            {
                GD.PrintErr($"Resolution too large: {width}x{height}. Maximum is 7680x4320");
                return false;
            }
            
            return true;
        }
        
        public static int ClampFPS(int fps)
        {
            // FPS range: 30-240, or 0 for unlimited
            if (fps == 0) return 0; // Unlimited
            
            if (fps < 30)
            {
                GD.Print($"FPS too low ({fps}), clamping to 30");
                return 30;
            }
            
            if (fps > 240)
            {
                GD.Print($"FPS too high ({fps}), clamping to 240");
                return 240;
            }
            
            return fps;
        }
        
        public static float ClampRenderScale(float scale)
        {
            // Render scale range: 0.5 - 2.0
            if (scale < 0.5f)
            {
                GD.Print($"Render scale too low ({scale}), clamping to 0.5");
                return 0.5f;
            }
            
            if (scale > 2.0f)
            {
                GD.Print($"Render scale too high ({scale}), clamping to 2.0");
                return 2.0f;
            }
            
            return scale;
        }
        
        public static int ClampShadowQuality(int quality)
        {
            // Shadow quality range: 0-3
            return Mathf.Clamp(quality, 0, 3);
        }
        
        #endregion
        
        #region Audio Validation
        
        public static float ClampVolume(float volume)
        {
            // Volume range: 0.0 - 1.0
            return Mathf.Clamp(volume, 0.0f, 1.0f);
        }
        
        public static bool ValidateAudioBus(string busName)
        {
            int busIdx = AudioServer.GetBusIndex(busName);
            if (busIdx < 0)
            {
                GD.PrintErr($"Audio bus '{busName}' not found!");
                return false;
            }
            return true;
        }
        
        #endregion
        
        #region Control Validation
        
        public static float ClampSensitivity(float sensitivity)
        {
            // Sensitivity range: 0.1 - 5.0
            if (sensitivity < 0.1f)
            {
                GD.Print($"Sensitivity too low ({sensitivity}), clamping to 0.1");
                return 0.1f;
            }
            
            if (sensitivity > 5.0f)
            {
                GD.Print($"Sensitivity too high ({sensitivity}), clamping to 5.0");
                return 5.0f;
            }
            
            return sensitivity;
        }
        
        public static int ClampDeadzone(int deadzone)
        {
            // Deadzone range: 0-50%
            return Mathf.Clamp(deadzone, 0, 50);
        }
        
        public static bool ValidateKeyBinding(int keycode)
        {
            // Check if the keycode is valid
            if (keycode < 0)
            {
                GD.PrintErr($"Invalid keycode: {keycode}");
                return false;
            }
            
            // Disallow certain keys (e.g., system keys)
            Key key = (Key)keycode;
            
            // Don't allow binding to Escape or system keys
            if (key == Key.Escape || key == Key.Print || key == Key.Sysreq)
            {
                GD.PrintErr($"Cannot bind to system key: {key}");
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Accessibility Validation
        
        public static float ClampUIScale(float scale)
        {
            // UI scale range: 0.5 - 2.0
            if (scale < 0.5f)
            {
                GD.Print($"UI scale too low ({scale}), clamping to 0.5");
                return 0.5f;
            }
            
            if (scale > 2.0f)
            {
                GD.Print($"UI scale too high ({scale}), clamping to 2.0");
                return 2.0f;
            }
            
            return scale;
        }
        
        public static float ClampTextSize(float size)
        {
            // Text size range: 0.5 - 2.0
            if (size < 0.5f)
            {
                GD.Print($"Text size too low ({size}), clamping to 0.5");
                return 0.5f;
            }
            
            if (size > 2.0f)
            {
                GD.Print($"Text size too high ({size}), clamping to 2.0");
                return 2.0f;
            }
            
            return size;
        }
        
        #endregion
        
        #region General Validation
        
        /// <summary>
        /// Validate all graphics settings
        /// </summary>
        public static GraphicsSettingsData ValidateGraphicsSettings(GraphicsSettingsData settings)
        {
            if (settings == null)
            {
                GD.PrintErr("Graphics settings is null, creating defaults");
                return new GraphicsSettingsData();
            }
            
            // Validate and clamp values
            if (!ValidateResolution(settings.ResolutionWidth, settings.ResolutionHeight))
            {
                settings.ResolutionWidth = 1920;
                settings.ResolutionHeight = 1080;
            }
            
            settings.TargetFPS = ClampFPS(settings.TargetFPS);
            settings.RenderScale = ClampRenderScale(settings.RenderScale);
            settings.ShadowQuality = ClampShadowQuality(settings.ShadowQuality);
            settings.ParticleQuality = ClampShadowQuality(settings.ParticleQuality);
            
            return settings;
        }
        
        /// <summary>
        /// Validate all audio settings
        /// </summary>
        public static AudioSettingsData ValidateAudioSettings(AudioSettingsData settings)
        {
            if (settings == null)
            {
                GD.PrintErr("Audio settings is null, creating defaults");
                return new AudioSettingsData();
            }
            
            // Validate and clamp values
            settings.MasterVolume = ClampVolume(settings.MasterVolume);
            settings.MusicVolume = ClampVolume(settings.MusicVolume);
            settings.SFXVolume = ClampVolume(settings.SFXVolume);
            settings.UIVolume = ClampVolume(settings.UIVolume);
            
            return settings;
        }
        
        /// <summary>
        /// Validate all control settings
        /// </summary>
        public static ControlSettingsData ValidateControlSettings(ControlSettingsData settings)
        {
            if (settings == null)
            {
                GD.PrintErr("Control settings is null, creating defaults");
                return new ControlSettingsData();
            }
            
            // Validate and clamp values
            settings.MouseSensitivity = ClampSensitivity(settings.MouseSensitivity);
            settings.ControllerSensitivity = ClampSensitivity(settings.ControllerSensitivity);
            settings.ControllerDeadzone = ClampDeadzone(settings.ControllerDeadzone);
            
            // Validate key bindings
            if (settings.KeyBindings != null)
            {
                var invalidBindings = new System.Collections.Generic.List<string>();
                
                foreach (var binding in settings.KeyBindings)
                {
                    if (!ValidateKeyBinding(binding.Value))
                    {
                        invalidBindings.Add(binding.Key);
                    }
                }
                
                // Reset invalid bindings to defaults
                foreach (var key in invalidBindings)
                {
                    if (ControlSettingsApplier.DefaultKeyBindings.ContainsKey(key))
                    {
                        settings.KeyBindings[key] = ControlSettingsApplier.DefaultKeyBindings[key];
                        GD.Print($"Reset invalid key binding for '{key}' to default");
                    }
                }
            }
            
            return settings;
        }
        
        /// <summary>
        /// Validate accessibility settings
        /// </summary>
        public static AccessibilitySettingsData ValidateAccessibilitySettings(AccessibilitySettingsData settings)
        {
            if (settings == null)
            {
                GD.PrintErr("Accessibility settings is null, creating defaults");
                return new AccessibilitySettingsData();
            }
            
            // Validate and clamp values
            settings.UIScale = ClampUIScale(settings.UIScale);
            settings.TextSize = ClampTextSize(settings.TextSize);
            
            return settings;
        }
        
        #endregion
    }
}
