using Godot;
using System;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.UI.Settings
{
    /// <summary>
    /// Graphics settings component
    /// Manages display, quality, and performance settings
    /// </summary>
    public partial class GraphicsSettings : Control
    {
        #region Export Variables
        
        [Export] private OptionButton resolutionOption;
        [Export] private OptionButton windowModeOption;
        [Export] private CheckButton vsyncCheck;
        [Export] private HSlider fpsLimitSlider;
        [Export] private Label fpsLimitLabel;
        [Export] private OptionButton qualityPresetOption;
        [Export] private HSlider renderScaleSlider;
        [Export] private Label renderScaleLabel;
        [Export] private CheckButton bloomCheck;
        [Export] private CheckButton motionBlurCheck;
        [Export] private HSlider shadowQualitySlider;
        [Export] private Label shadowQualityLabel;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            SetupDropdowns();
            ConnectSignals();
        }
        
        #endregion
        
        #region Public Methods
        
        public void LoadSettings()
        {
            var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
            if (settingsManager == null || settingsManager.CurrentSettings == null)
            {
                GD.PrintErr("SettingsManager not available!");
                return;
            }
            
            var graphics = settingsManager.CurrentSettings.Graphics;
            
            // Window mode
            if (windowModeOption != null)
            {
                windowModeOption.Selected = graphics.Fullscreen ? 0 : 1;
            }
            
            // VSync
            if (vsyncCheck != null)
            {
                vsyncCheck.ButtonPressed = graphics.VSync;
            }
            
            // FPS limit
            if (fpsLimitSlider != null)
            {
                fpsLimitSlider.Value = graphics.TargetFPS;
                UpdateFpsLimitLabel(graphics.TargetFPS);
            }
            
            // Quality preset
            if (qualityPresetOption != null)
            {
                qualityPresetOption.Selected = (int)graphics.QualityLevel;
            }
            
            // Render scale
            if (renderScaleSlider != null)
            {
                renderScaleSlider.Value = graphics.RenderScale;
                UpdateRenderScaleLabel(graphics.RenderScale);
            }
            
            // Effects
            if (bloomCheck != null)
            {
                bloomCheck.ButtonPressed = graphics.Bloom;
            }
            
            if (motionBlurCheck != null)
            {
                motionBlurCheck.ButtonPressed = graphics.MotionBlur;
            }
            
            // Shadow quality
            if (shadowQualitySlider != null)
            {
                shadowQualitySlider.Value = graphics.ShadowQuality;
                UpdateShadowQualityLabel(graphics.ShadowQuality);
            }
            
            // Resolution
            UpdateResolutionDropdown(graphics.ResolutionWidth, graphics.ResolutionHeight);
        }
        
        public void ApplySettings()
        {
            var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
            if (settingsManager == null || settingsManager.CurrentSettings == null)
            {
                GD.PrintErr("SettingsManager not available!");
                return;
            }
            
            var graphics = settingsManager.CurrentSettings.Graphics;
            
            // Window mode
            if (windowModeOption != null)
            {
                graphics.Fullscreen = windowModeOption.Selected == 0;
                var mode = graphics.Fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
                DisplayServer.WindowSetMode(mode);
            }
            
            // VSync
            if (vsyncCheck != null)
            {
                graphics.VSync = vsyncCheck.ButtonPressed;
                var vsyncMode = graphics.VSync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled;
                DisplayServer.WindowSetVsyncMode(vsyncMode);
            }
            
            // FPS limit
            if (fpsLimitSlider != null)
            {
                graphics.TargetFPS = (int)fpsLimitSlider.Value;
                Engine.MaxFps = graphics.TargetFPS;
            }
            
            // Quality preset
            if (qualityPresetOption != null)
            {
                graphics.QualityLevel = (QualityPreset)qualityPresetOption.Selected;
            }
            
            // Render scale
            if (renderScaleSlider != null)
            {
                graphics.RenderScale = (float)renderScaleSlider.Value;
            }
            
            // Effects
            if (bloomCheck != null)
            {
                graphics.Bloom = bloomCheck.ButtonPressed;
            }
            
            if (motionBlurCheck != null)
            {
                graphics.MotionBlur = motionBlurCheck.ButtonPressed;
            }
            
            // Shadow quality
            if (shadowQualitySlider != null)
            {
                graphics.ShadowQuality = (int)shadowQualitySlider.Value;
            }
            
            // Resolution
            if (resolutionOption != null)
            {
                string resText = resolutionOption.GetItemText(resolutionOption.Selected);
                ParseResolution(resText, out int width, out int height);
                graphics.ResolutionWidth = width;
                graphics.ResolutionHeight = height;
                DisplayServer.WindowSetSize(new Vector2I(width, height));
            }
            
            // Apply all graphics settings through the applier
            GraphicsSettingsApplier.Apply(graphics);
            
            GD.Print("Graphics settings applied");
        }
        
        public void ResetToDefaults()
        {
            var defaults = new GraphicsSettingsData();
            
            if (windowModeOption != null)
                windowModeOption.Selected = defaults.Fullscreen ? 0 : 1;
            
            if (vsyncCheck != null)
                vsyncCheck.ButtonPressed = defaults.VSync;
            
            if (fpsLimitSlider != null)
            {
                fpsLimitSlider.Value = defaults.TargetFPS;
                UpdateFpsLimitLabel(defaults.TargetFPS);
            }
            
            if (qualityPresetOption != null)
                qualityPresetOption.Selected = (int)defaults.QualityLevel;
            
            if (renderScaleSlider != null)
            {
                renderScaleSlider.Value = defaults.RenderScale;
                UpdateRenderScaleLabel(defaults.RenderScale);
            }
            
            if (bloomCheck != null)
                bloomCheck.ButtonPressed = defaults.Bloom;
            
            if (motionBlurCheck != null)
                motionBlurCheck.ButtonPressed = defaults.MotionBlur;
            
            if (shadowQualitySlider != null)
            {
                shadowQualitySlider.Value = defaults.ShadowQuality;
                UpdateShadowQualityLabel(defaults.ShadowQuality);
            }
            
            UpdateResolutionDropdown(defaults.ResolutionWidth, defaults.ResolutionHeight);
            
            GD.Print("Graphics settings reset to defaults");
        }
        
        #endregion
        
        #region Private Methods
        
        private void SetupDropdowns()
        {
            // Setup resolution dropdown
            if (resolutionOption != null)
            {
                resolutionOption.Clear();
                resolutionOption.AddItem("1280x720");
                resolutionOption.AddItem("1366x768");
                resolutionOption.AddItem("1600x900");
                resolutionOption.AddItem("1920x1080");
                resolutionOption.AddItem("2560x1440");
                resolutionOption.AddItem("3840x2160");
            }
            
            // Setup window mode dropdown
            if (windowModeOption != null)
            {
                windowModeOption.Clear();
                windowModeOption.AddItem("Fullscreen");
                windowModeOption.AddItem("Windowed");
            }
            
            // Setup quality preset dropdown
            if (qualityPresetOption != null)
            {
                qualityPresetOption.Clear();
                qualityPresetOption.AddItem("Low");
                qualityPresetOption.AddItem("Medium");
                qualityPresetOption.AddItem("High");
                qualityPresetOption.AddItem("Ultra");
            }
        }
        
        private void ConnectSignals()
        {
            if (fpsLimitSlider != null)
                fpsLimitSlider.ValueChanged += (value) => UpdateFpsLimitLabel((int)value);
            
            if (renderScaleSlider != null)
                renderScaleSlider.ValueChanged += (value) => UpdateRenderScaleLabel((float)value);
            
            if (shadowQualitySlider != null)
                shadowQualitySlider.ValueChanged += (value) => UpdateShadowQualityLabel((int)value);
        }
        
        private void UpdateFpsLimitLabel(int fps)
        {
            if (fpsLimitLabel != null)
                fpsLimitLabel.Text = fps == 0 ? "Unlimited" : $"{fps} FPS";
        }
        
        private void UpdateRenderScaleLabel(float scale)
        {
            if (renderScaleLabel != null)
                renderScaleLabel.Text = $"{scale:F2}x";
        }
        
        private void UpdateShadowQualityLabel(int quality)
        {
            if (shadowQualityLabel != null)
            {
                shadowQualityLabel.Text = quality switch
                {
                    0 => "Off",
                    1 => "Low",
                    2 => "Medium",
                    3 => "High",
                    _ => "Unknown"
                };
            }
        }
        
        private void UpdateResolutionDropdown(int width, int height)
        {
            if (resolutionOption == null) return;
            
            string targetRes = $"{width}x{height}";
            
            for (int i = 0; i < resolutionOption.ItemCount; i++)
            {
                if (resolutionOption.GetItemText(i) == targetRes)
                {
                    resolutionOption.Selected = i;
                    return;
                }
            }
            
            // Default to 1920x1080 if not found
            resolutionOption.Selected = 3;
        }
        
        private void ParseResolution(string resText, out int width, out int height)
        {
            string[] parts = resText.Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out width) && int.TryParse(parts[1], out height))
            {
                return;
            }
            
            // Default
            width = 1920;
            height = 1080;
        }
        
        #endregion
    }
}
