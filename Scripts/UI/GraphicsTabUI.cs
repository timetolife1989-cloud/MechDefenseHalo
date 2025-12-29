using Godot;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Graphics settings tab UI
    /// </summary>
    public partial class GraphicsTabUI : Control
    {
        #region Export Variables
        
        [Export] public OptionButton ResolutionDropdown { get; set; }
        [Export] public CheckBox FullscreenCheckbox { get; set; }
        [Export] public CheckBox VSyncCheckbox { get; set; }
        [Export] public OptionButton QualityPresetDropdown { get; set; }
        [Export] public HSlider RenderScaleSlider { get; set; }
        [Export] public Label RenderScaleLabel { get; set; }
        [Export] public SpinBox TargetFPSSpinBox { get; set; }
        [Export] public HSlider ShadowQualitySlider { get; set; }
        [Export] public Label ShadowQualityLabel { get; set; }
        [Export] public CheckBox BloomCheckbox { get; set; }
        [Export] public CheckBox MotionBlurCheckbox { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private GraphicsSettingsData _currentSettings;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            SetupResolutionDropdown();
            SetupQualityDropdown();
            ConnectSignals();
        }
        
        #endregion
        
        #region Public Methods
        
        public void LoadSettings(GraphicsSettingsData settings)
        {
            _currentSettings = settings;
            
            if (FullscreenCheckbox != null)
                FullscreenCheckbox.ButtonPressed = settings.Fullscreen;
            
            if (VSyncCheckbox != null)
                VSyncCheckbox.ButtonPressed = settings.VSync;
            
            if (QualityPresetDropdown != null)
                QualityPresetDropdown.Selected = (int)settings.QualityLevel;
            
            if (RenderScaleSlider != null)
            {
                RenderScaleSlider.Value = settings.RenderScale;
                UpdateRenderScaleLabel(settings.RenderScale);
            }
            
            if (TargetFPSSpinBox != null)
                TargetFPSSpinBox.Value = settings.TargetFPS;
            
            if (ShadowQualitySlider != null)
            {
                ShadowQualitySlider.Value = settings.ShadowQuality;
                UpdateShadowQualityLabel(settings.ShadowQuality);
            }
            
            if (BloomCheckbox != null)
                BloomCheckbox.ButtonPressed = settings.Bloom;
            
            if (MotionBlurCheckbox != null)
                MotionBlurCheckbox.ButtonPressed = settings.MotionBlur;
            
            // Update resolution dropdown
            UpdateResolutionDropdown(settings.ResolutionWidth, settings.ResolutionHeight);
        }
        
        public void SaveToSettings(GraphicsSettingsData settings)
        {
            if (FullscreenCheckbox != null)
                settings.Fullscreen = FullscreenCheckbox.ButtonPressed;
            
            if (VSyncCheckbox != null)
                settings.VSync = VSyncCheckbox.ButtonPressed;
            
            if (QualityPresetDropdown != null)
                settings.QualityLevel = (QualityPreset)QualityPresetDropdown.Selected;
            
            if (RenderScaleSlider != null)
                settings.RenderScale = (float)RenderScaleSlider.Value;
            
            if (TargetFPSSpinBox != null)
                settings.TargetFPS = (int)TargetFPSSpinBox.Value;
            
            if (ShadowQualitySlider != null)
                settings.ShadowQuality = (int)ShadowQualitySlider.Value;
            
            if (BloomCheckbox != null)
                settings.Bloom = BloomCheckbox.ButtonPressed;
            
            if (MotionBlurCheckbox != null)
                settings.MotionBlur = MotionBlurCheckbox.ButtonPressed;
            
            // Get resolution from dropdown
            if (ResolutionDropdown != null)
            {
                string resText = ResolutionDropdown.GetItemText(ResolutionDropdown.Selected);
                ParseResolution(resText, out settings.ResolutionWidth, out settings.ResolutionHeight);
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void SetupResolutionDropdown()
        {
            if (ResolutionDropdown == null) return;
            
            ResolutionDropdown.Clear();
            ResolutionDropdown.AddItem("1280x720");
            ResolutionDropdown.AddItem("1366x768");
            ResolutionDropdown.AddItem("1600x900");
            ResolutionDropdown.AddItem("1920x1080");
            ResolutionDropdown.AddItem("2560x1440");
            ResolutionDropdown.AddItem("3840x2160");
        }
        
        private void SetupQualityDropdown()
        {
            if (QualityPresetDropdown == null) return;
            
            QualityPresetDropdown.Clear();
            QualityPresetDropdown.AddItem("Low");
            QualityPresetDropdown.AddItem("Medium");
            QualityPresetDropdown.AddItem("High");
            QualityPresetDropdown.AddItem("Ultra");
        }
        
        private void ConnectSignals()
        {
            if (RenderScaleSlider != null)
                RenderScaleSlider.ValueChanged += (value) => UpdateRenderScaleLabel(value);
            
            if (ShadowQualitySlider != null)
                ShadowQualitySlider.ValueChanged += (value) => UpdateShadowQualityLabel((int)value);
        }
        
        private void UpdateRenderScaleLabel(double value)
        {
            if (RenderScaleLabel != null)
                RenderScaleLabel.Text = $"{value:F2}x";
        }
        
        private void UpdateShadowQualityLabel(int value)
        {
            if (ShadowQualityLabel != null)
            {
                string qualityText = value switch
                {
                    0 => "Off",
                    1 => "Low",
                    2 => "Medium",
                    3 => "High",
                    _ => "Unknown"
                };
                ShadowQualityLabel.Text = qualityText;
            }
        }
        
        private void UpdateResolutionDropdown(int width, int height)
        {
            if (ResolutionDropdown == null) return;
            
            string targetRes = $"{width}x{height}";
            
            for (int i = 0; i < ResolutionDropdown.ItemCount; i++)
            {
                if (ResolutionDropdown.GetItemText(i) == targetRes)
                {
                    ResolutionDropdown.Selected = i;
                    return;
                }
            }
            
            // Default to 1920x1080 if not found
            ResolutionDropdown.Selected = 3;
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
