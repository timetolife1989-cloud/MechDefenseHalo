using Godot;
using System;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.UI.Settings
{
    /// <summary>
    /// Accessibility settings component
    /// Manages colorblind modes, subtitles, UI scaling, and other accessibility features
    /// </summary>
    public partial class AccessibilitySettings : Control
    {
        #region Export Variables
        
        [Export] private OptionButton colorblindModeOption;
        [Export] private CheckButton subtitlesCheck;
        [Export] private HSlider uiScaleSlider;
        [Export] private Label uiScaleLabel;
        [Export] private CheckButton highContrastCheck;
        [Export] private CheckButton screenReaderCheck;
        [Export] private CheckButton reducedMotionCheck;
        [Export] private HSlider textSizeSlider;
        [Export] private Label textSizeLabel;
        [Export] private CheckButton autoPauseCheck;
        
        #endregion
        
        #region Private Fields
        
        private AccessibilitySettingsData currentSettings;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            SetupDropdowns();
            ConnectSignals();
            InitializeSettings();
        }
        
        #endregion
        
        #region Public Methods
        
        public void LoadSettings()
        {
            var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
            if (settingsManager == null)
            {
                GD.PrintErr("SettingsManager not available!");
                return;
            }
            
            // Load from our local settings data
            if (currentSettings == null)
            {
                currentSettings = new AccessibilitySettingsData();
            }
            
            // Colorblind mode
            if (colorblindModeOption != null)
            {
                colorblindModeOption.Selected = (int)currentSettings.ColorblindMode;
            }
            
            // Subtitles
            if (subtitlesCheck != null)
            {
                subtitlesCheck.ButtonPressed = currentSettings.SubtitlesEnabled;
            }
            
            // UI scale
            if (uiScaleSlider != null)
            {
                uiScaleSlider.Value = currentSettings.UIScale;
                UpdateUIScaleLabel(currentSettings.UIScale);
            }
            
            // High contrast
            if (highContrastCheck != null)
            {
                highContrastCheck.ButtonPressed = currentSettings.HighContrastMode;
            }
            
            // Screen reader
            if (screenReaderCheck != null)
            {
                screenReaderCheck.ButtonPressed = currentSettings.ScreenReaderEnabled;
            }
            
            // Reduced motion
            if (reducedMotionCheck != null)
            {
                reducedMotionCheck.ButtonPressed = currentSettings.ReducedMotion;
            }
            
            // Text size
            if (textSizeSlider != null)
            {
                textSizeSlider.Value = currentSettings.TextSize;
                UpdateTextSizeLabel(currentSettings.TextSize);
            }
            
            // Auto pause
            if (autoPauseCheck != null)
            {
                autoPauseCheck.ButtonPressed = currentSettings.AutoPauseOnFocusLoss;
            }
        }
        
        public void ApplySettings()
        {
            if (currentSettings == null)
            {
                GD.PrintErr("Accessibility settings not initialized!");
                return;
            }
            
            // Colorblind mode
            if (colorblindModeOption != null)
            {
                currentSettings.ColorblindMode = (ColorblindMode)colorblindModeOption.Selected;
                ApplyColorblindMode(currentSettings.ColorblindMode);
            }
            
            // Subtitles
            if (subtitlesCheck != null)
            {
                currentSettings.SubtitlesEnabled = subtitlesCheck.ButtonPressed;
            }
            
            // UI scale
            if (uiScaleSlider != null)
            {
                currentSettings.UIScale = (float)uiScaleSlider.Value;
                ApplyUIScale(currentSettings.UIScale);
            }
            
            // High contrast
            if (highContrastCheck != null)
            {
                currentSettings.HighContrastMode = highContrastCheck.ButtonPressed;
                ApplyHighContrast(currentSettings.HighContrastMode);
            }
            
            // Screen reader
            if (screenReaderCheck != null)
            {
                currentSettings.ScreenReaderEnabled = screenReaderCheck.ButtonPressed;
            }
            
            // Reduced motion
            if (reducedMotionCheck != null)
            {
                currentSettings.ReducedMotion = reducedMotionCheck.ButtonPressed;
            }
            
            // Text size
            if (textSizeSlider != null)
            {
                currentSettings.TextSize = (float)textSizeSlider.Value;
                ApplyTextSize(currentSettings.TextSize);
            }
            
            // Auto pause
            if (autoPauseCheck != null)
            {
                currentSettings.AutoPauseOnFocusLoss = autoPauseCheck.ButtonPressed;
            }
            
            GD.Print("Accessibility settings applied");
        }
        
        public void ResetToDefaults()
        {
            var defaults = new AccessibilitySettingsData();
            
            if (colorblindModeOption != null)
            {
                colorblindModeOption.Selected = (int)defaults.ColorblindMode;
            }
            
            if (subtitlesCheck != null)
            {
                subtitlesCheck.ButtonPressed = defaults.SubtitlesEnabled;
            }
            
            if (uiScaleSlider != null)
            {
                uiScaleSlider.Value = defaults.UIScale;
                UpdateUIScaleLabel(defaults.UIScale);
            }
            
            if (highContrastCheck != null)
            {
                highContrastCheck.ButtonPressed = defaults.HighContrastMode;
            }
            
            if (screenReaderCheck != null)
            {
                screenReaderCheck.ButtonPressed = defaults.ScreenReaderEnabled;
            }
            
            if (reducedMotionCheck != null)
            {
                reducedMotionCheck.ButtonPressed = defaults.ReducedMotion;
            }
            
            if (textSizeSlider != null)
            {
                textSizeSlider.Value = defaults.TextSize;
                UpdateTextSizeLabel(defaults.TextSize);
            }
            
            if (autoPauseCheck != null)
            {
                autoPauseCheck.ButtonPressed = defaults.AutoPauseOnFocusLoss;
            }
            
            currentSettings = defaults;
            
            GD.Print("Accessibility settings reset to defaults");
        }
        
        #endregion
        
        #region Private Methods
        
        private void InitializeSettings()
        {
            currentSettings = new AccessibilitySettingsData();
            LoadSettings();
        }
        
        private void SetupDropdowns()
        {
            // Setup colorblind mode dropdown
            if (colorblindModeOption != null)
            {
                colorblindModeOption.Clear();
                colorblindModeOption.AddItem("None");
                colorblindModeOption.AddItem("Protanopia (Red-Blind)");
                colorblindModeOption.AddItem("Deuteranopia (Green-Blind)");
                colorblindModeOption.AddItem("Tritanopia (Blue-Blind)");
                colorblindModeOption.AddItem("Monochromacy");
            }
        }
        
        private void ConnectSignals()
        {
            if (uiScaleSlider != null)
            {
                uiScaleSlider.ValueChanged += (value) => UpdateUIScaleLabel((float)value);
            }
            
            if (textSizeSlider != null)
            {
                textSizeSlider.ValueChanged += (value) => UpdateTextSizeLabel((float)value);
            }
        }
        
        private void UpdateUIScaleLabel(float scale)
        {
            if (uiScaleLabel != null)
            {
                uiScaleLabel.Text = $"{(scale * 100):F0}%";
            }
        }
        
        private void UpdateTextSizeLabel(float size)
        {
            if (textSizeLabel != null)
            {
                textSizeLabel.Text = $"{(size * 100):F0}%";
            }
        }
        
        // Apply methods that would affect the game
        private void ApplyColorblindMode(ColorblindMode mode)
        {
            // In a real implementation, this would apply shader effects or color adjustments
            // For now, we'll just log it
            GD.Print($"Colorblind mode set to: {mode}");
            
            // TODO: Apply colorblind shader or color palette adjustments
            // This would typically involve setting a global shader parameter or
            // adjusting the color palette of UI elements and game visuals
        }
        
        private void ApplyUIScale(float scale)
        {
            // Scale the UI root or canvas
            var root = GetTree().Root;
            if (root != null)
            {
                root.ContentScaleFactor = scale;
            }
            
            GD.Print($"UI scale set to: {scale}");
        }
        
        private void ApplyHighContrast(bool enabled)
        {
            // In a real implementation, this would apply high contrast theme
            GD.Print($"High contrast mode: {(enabled ? "enabled" : "disabled")}");
            
            // TODO: Switch to high contrast theme
            // This would typically involve loading a different theme resource
        }
        
        private void ApplyTextSize(float size)
        {
            // In a real implementation, this would adjust base font sizes
            GD.Print($"Text size set to: {size}");
            
            // TODO: Adjust theme font sizes globally
            // This would typically involve modifying the theme's default font sizes
        }
        
        #endregion
    }
    
    #region Accessibility Data Structures
    
    /// <summary>
    /// Colorblind mode options
    /// </summary>
    public enum ColorblindMode
    {
        None = 0,
        Protanopia = 1,      // Red-blind
        Deuteranopia = 2,    // Green-blind
        Tritanopia = 3,      // Blue-blind
        Monochromacy = 4     // Complete color blindness
    }
    
    /// <summary>
    /// Accessibility settings data container
    /// </summary>
    [Serializable]
    public class AccessibilitySettingsData
    {
        public ColorblindMode ColorblindMode { get; set; } = ColorblindMode.None;
        public bool SubtitlesEnabled { get; set; } = false;
        public float UIScale { get; set; } = 1.0f;
        public bool HighContrastMode { get; set; } = false;
        public bool ScreenReaderEnabled { get; set; } = false;
        public bool ReducedMotion { get; set; } = false;
        public float TextSize { get; set; } = 1.0f;
        public bool AutoPauseOnFocusLoss { get; set; } = true;
    }
    
    #endregion
}
