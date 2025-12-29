using Godot;
using MechDefenseHalo.Settings;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Main settings menu UI with tabbed interface
    /// 
    /// REQUIRED SCENE STRUCTURE (create manually in Godot):
    /// 
    /// Control (SettingsMenu) - Script: SettingsMenuUI.cs
    /// ├─ Panel (Background)
    /// │  ├─ Label (Title) - text: "SETTINGS"
    /// │  ├─ TabContainer (SettingsTabs)
    /// │  │  ├─ GraphicsTab (Graphics) - Scene: GraphicsTab.tscn
    /// │  │  ├─ AudioTab (Audio) - Scene: AudioTab.tscn
    /// │  │  ├─ ControlsTab (Controls) - Scene: ControlsTab.tscn
    /// │  │  └─ GameplayTab (Gameplay) - Scene: GameplayTab.tscn
    /// │  └─ HBoxContainer (ButtonContainer)
    /// │     ├─ Button (ApplyButton) - text: "Apply"
    /// │     ├─ Button (ResetButton) - text: "Reset to Default"
    /// │     └─ Button (CloseButton) - text: "Close"
    /// </summary>
    public partial class SettingsMenuUI : Control
    {
        #region Export Variables
        
        [Export] public TabContainer SettingsTabs { get; set; }
        [Export] public Button ApplyButton { get; set; }
        [Export] public Button ResetButton { get; set; }
        [Export] public Button CloseButton { get; set; }
        [Export] public ConfirmationDialog ResetConfirmDialog { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private SettingsManager _settingsManager;
        private GraphicsTabUI _graphicsTab;
        private AudioTabUI _audioTab;
        private ControlsTabUI _controlsTab;
        private GameplayTabUI _gameplayTab;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            _settingsManager = SettingsManager.Instance;
            
            if (_settingsManager == null)
            {
                GD.PrintErr("SettingsManager not found!");
                return;
            }
            
            // Get tab references
            if (SettingsTabs != null)
            {
                _graphicsTab = SettingsTabs.GetNodeOrNull<GraphicsTabUI>("Graphics");
                _audioTab = SettingsTabs.GetNodeOrNull<AudioTabUI>("Audio");
                _controlsTab = SettingsTabs.GetNodeOrNull<ControlsTabUI>("Controls");
                _gameplayTab = SettingsTabs.GetNodeOrNull<GameplayTabUI>("Gameplay");
            }
            
            // Connect button signals
            if (ApplyButton != null)
                ApplyButton.Pressed += OnApplyPressed;
            
            if (ResetButton != null)
                ResetButton.Pressed += OnResetPressed;
            
            if (CloseButton != null)
                CloseButton.Pressed += OnClosePressed;
            
            if (ResetConfirmDialog != null)
            {
                ResetConfirmDialog.Confirmed += OnResetConfirmed;
            }
            
            // Load current settings into UI
            RefreshUI();
            
            // Listen for settings events
            EventBus.On("settings_saved", OnSettingsSaved);
            EventBus.On("settings_reset", OnSettingsReset);
            
            Hide(); // Start hidden
        }
        
        public override void _ExitTree()
        {
            EventBus.Off("settings_saved", OnSettingsSaved);
            EventBus.Off("settings_reset", OnSettingsReset);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Show the settings menu
        /// </summary>
        public void ShowSettings()
        {
            RefreshUI();
            Show();
        }
        
        /// <summary>
        /// Hide the settings menu
        /// </summary>
        public void HideSettings()
        {
            Hide();
        }
        
        /// <summary>
        /// Refresh all tabs with current settings
        /// </summary>
        public void RefreshUI()
        {
            if (_settingsManager == null || _settingsManager.CurrentSettings == null)
                return;
            
            _graphicsTab?.LoadSettings(_settingsManager.CurrentSettings.Graphics);
            _audioTab?.LoadSettings(_settingsManager.CurrentSettings.Audio);
            _controlsTab?.LoadSettings(_settingsManager.CurrentSettings.Controls);
            _gameplayTab?.LoadSettings(_settingsManager.CurrentSettings.Gameplay);
        }
        
        #endregion
        
        #region Private Methods
        
        private void OnApplyPressed()
        {
            // Collect settings from all tabs
            if (_graphicsTab != null)
                _graphicsTab.SaveToSettings(_settingsManager.CurrentSettings.Graphics);
            
            if (_audioTab != null)
                _audioTab.SaveToSettings(_settingsManager.CurrentSettings.Audio);
            
            if (_controlsTab != null)
                _controlsTab.SaveToSettings(_settingsManager.CurrentSettings.Controls);
            
            if (_gameplayTab != null)
                _gameplayTab.SaveToSettings(_settingsManager.CurrentSettings.Gameplay);
            
            // Apply and save
            _settingsManager.ApplyAllSettings();
            _settingsManager.SaveSettings();
            
            GD.Print("Settings applied and saved");
        }
        
        private void OnResetPressed()
        {
            if (ResetConfirmDialog != null)
            {
                ResetConfirmDialog.DialogText = "Are you sure you want to reset all settings to default values?";
                ResetConfirmDialog.Show();
            }
            else
            {
                OnResetConfirmed();
            }
        }
        
        private void OnResetConfirmed()
        {
            _settingsManager?.ResetToDefaults();
            RefreshUI();
        }
        
        private void OnClosePressed()
        {
            HideSettings();
        }
        
        private void OnSettingsSaved(object data)
        {
            GD.Print("Settings saved event received");
        }
        
        private void OnSettingsReset(object data)
        {
            RefreshUI();
        }
        
        #endregion
    }
}
