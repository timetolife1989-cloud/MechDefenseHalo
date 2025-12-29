using Godot;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Gameplay settings tab UI
    /// </summary>
    public partial class GameplayTabUI : Control
    {
        #region Export Variables
        
        [Export] public CheckBox AutoPickupCheckbox { get; set; }
        [Export] public CheckBox DamageNumbersCheckbox { get; set; }
        [Export] public CheckBox ScreenShakeCheckbox { get; set; }
        [Export] public HSlider ShakeIntensitySlider { get; set; }
        [Export] public Label ShakeIntensityLabel { get; set; }
        [Export] public CheckBox ShowFPSCheckbox { get; set; }
        [Export] public CheckBox ShowPingCheckbox { get; set; }
        [Export] public OptionButton LanguageDropdown { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private GameplaySettingsData _currentSettings;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            SetupLanguageDropdown();
            ConnectSignals();
        }
        
        #endregion
        
        #region Public Methods
        
        public void LoadSettings(GameplaySettingsData settings)
        {
            _currentSettings = settings;
            
            if (AutoPickupCheckbox != null)
                AutoPickupCheckbox.ButtonPressed = settings.AutoPickupItems;
            
            if (DamageNumbersCheckbox != null)
                DamageNumbersCheckbox.ButtonPressed = settings.ShowDamageNumbers;
            
            if (ScreenShakeCheckbox != null)
                ScreenShakeCheckbox.ButtonPressed = settings.ScreenShake;
            
            if (ShakeIntensitySlider != null)
            {
                ShakeIntensitySlider.Value = settings.ScreenShakeIntensity;
                UpdateShakeIntensityLabel(settings.ScreenShakeIntensity);
            }
            
            if (ShowFPSCheckbox != null)
                ShowFPSCheckbox.ButtonPressed = settings.ShowFPSCounter;
            
            if (ShowPingCheckbox != null)
                ShowPingCheckbox.ButtonPressed = settings.ShowPing;
            
            if (LanguageDropdown != null)
                SetLanguageDropdown(settings.Language);
        }
        
        public void SaveToSettings(GameplaySettingsData settings)
        {
            if (AutoPickupCheckbox != null)
                settings.AutoPickupItems = AutoPickupCheckbox.ButtonPressed;
            
            if (DamageNumbersCheckbox != null)
                settings.ShowDamageNumbers = DamageNumbersCheckbox.ButtonPressed;
            
            if (ScreenShakeCheckbox != null)
                settings.ScreenShake = ScreenShakeCheckbox.ButtonPressed;
            
            if (ShakeIntensitySlider != null)
                settings.ScreenShakeIntensity = (float)ShakeIntensitySlider.Value;
            
            if (ShowFPSCheckbox != null)
                settings.ShowFPSCounter = ShowFPSCheckbox.ButtonPressed;
            
            if (ShowPingCheckbox != null)
                settings.ShowPing = ShowPingCheckbox.ButtonPressed;
            
            if (LanguageDropdown != null)
                settings.Language = GetLanguageCode();
        }
        
        #endregion
        
        #region Private Methods
        
        private void SetupLanguageDropdown()
        {
            if (LanguageDropdown == null) return;
            
            LanguageDropdown.Clear();
            LanguageDropdown.AddItem("English", 0);
            LanguageDropdown.AddItem("Español", 1);
            LanguageDropdown.AddItem("Français", 2);
            LanguageDropdown.AddItem("Deutsch", 3);
            LanguageDropdown.AddItem("日本語", 4);
            LanguageDropdown.AddItem("中文", 5);
        }
        
        private void ConnectSignals()
        {
            if (ShakeIntensitySlider != null)
                ShakeIntensitySlider.ValueChanged += (value) => UpdateShakeIntensityLabel(value);
        }
        
        private void UpdateShakeIntensityLabel(double value)
        {
            if (ShakeIntensityLabel != null)
                ShakeIntensityLabel.Text = $"{(value * 100):F0}%";
        }
        
        private void SetLanguageDropdown(string languageCode)
        {
            if (LanguageDropdown == null) return;
            
            int index = languageCode switch
            {
                "en" => 0,
                "es" => 1,
                "fr" => 2,
                "de" => 3,
                "ja" => 4,
                "zh" => 5,
                _ => 0
            };
            
            LanguageDropdown.Selected = index;
        }
        
        private string GetLanguageCode()
        {
            if (LanguageDropdown == null) return "en";
            
            return LanguageDropdown.Selected switch
            {
                0 => "en",
                1 => "es",
                2 => "fr",
                3 => "de",
                4 => "ja",
                5 => "zh",
                _ => "en"
            };
        }
        
        #endregion
    }
}
