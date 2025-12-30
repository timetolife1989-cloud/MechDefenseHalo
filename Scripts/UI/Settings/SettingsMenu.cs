using Godot;
using System;
using MechDefenseHalo.Settings;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.UI.Settings
{
    /// <summary>
    /// Main settings menu controller that manages all settings tabs
    /// Coordinates between UI components and SettingsManager
    /// </summary>
    public partial class SettingsMenu : Control
    {
        #region Export Variables
        
        [Export] private TabContainer tabContainer;
        [Export] private GraphicsSettings graphicsSettings;
        [Export] private AudioSettings audioSettings;
        [Export] private ControlSettings controlSettings;
        [Export] private AccessibilitySettings accessibilitySettings;
        
        [Export] private Button applyButton;
        [Export] private Button cancelButton;
        [Export] private Button defaultsButton;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            ConnectSignals();
            LoadSettings();
            
            GD.Print("SettingsMenu initialized");
        }
        
        #endregion
        
        #region Private Methods
        
        private void ConnectSignals()
        {
            if (applyButton != null)
                applyButton.Pressed += OnApplyPressed;
            
            if (cancelButton != null)
                cancelButton.Pressed += OnCancelPressed;
            
            if (defaultsButton != null)
                defaultsButton.Pressed += OnDefaultsPressed;
        }
        
        private void LoadSettings()
        {
            var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
            if (settingsManager == null)
            {
                GD.PrintErr("SettingsManager not available!");
                return;
            }
            
            graphicsSettings?.LoadSettings();
            audioSettings?.LoadSettings();
            controlSettings?.LoadSettings();
            accessibilitySettings?.LoadSettings();
            
            GD.Print("Settings loaded into UI");
        }
        
        private void OnApplyPressed()
        {
            graphicsSettings?.ApplySettings();
            audioSettings?.ApplySettings();
            controlSettings?.ApplySettings();
            accessibilitySettings?.ApplySettings();
            
            // Save through SettingsManager
            var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
            if (settingsManager != null)
            {
                settingsManager.SaveSettings();
            }
            
            GD.Print("Settings applied and saved");
            
            // Optionally hide menu after applying
            // Hide();
        }
        
        private void OnCancelPressed()
        {
            // Reload settings to discard changes
            LoadSettings();
            Hide();
        }
        
        private void OnDefaultsPressed()
        {
            graphicsSettings?.ResetToDefaults();
            audioSettings?.ResetToDefaults();
            controlSettings?.ResetToDefaults();
            accessibilitySettings?.ResetToDefaults();
            
            GD.Print("Settings reset to defaults");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Show the settings menu
        /// </summary>
        public void ShowSettings()
        {
            LoadSettings();
            Show();
        }
        
        /// <summary>
        /// Hide the settings menu
        /// </summary>
        public void HideSettings()
        {
            Hide();
        }
        
        #endregion
    }
}
