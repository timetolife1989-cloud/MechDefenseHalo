using Godot;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Audio settings tab UI
    /// </summary>
    public partial class AudioTabUI : Control
    {
        #region Export Variables
        
        [Export] public HSlider MasterVolumeSlider { get; set; }
        [Export] public Label MasterVolumeLabel { get; set; }
        [Export] public CheckBox MuteMasterCheckbox { get; set; }
        
        [Export] public HSlider MusicVolumeSlider { get; set; }
        [Export] public Label MusicVolumeLabel { get; set; }
        
        [Export] public HSlider SFXVolumeSlider { get; set; }
        [Export] public Label SFXVolumeLabel { get; set; }
        
        [Export] public HSlider UIVolumeSlider { get; set; }
        [Export] public Label UIVolumeLabel { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private AudioSettingsData _currentSettings;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            ConnectSignals();
        }
        
        #endregion
        
        #region Public Methods
        
        public void LoadSettings(AudioSettingsData settings)
        {
            _currentSettings = settings;
            
            if (MasterVolumeSlider != null)
            {
                MasterVolumeSlider.Value = settings.MasterVolume;
                UpdateVolumeLabel(MasterVolumeLabel, settings.MasterVolume);
            }
            
            if (MuteMasterCheckbox != null)
                MuteMasterCheckbox.ButtonPressed = settings.MuteMaster;
            
            if (MusicVolumeSlider != null)
            {
                MusicVolumeSlider.Value = settings.MusicVolume;
                UpdateVolumeLabel(MusicVolumeLabel, settings.MusicVolume);
            }
            
            if (SFXVolumeSlider != null)
            {
                SFXVolumeSlider.Value = settings.SFXVolume;
                UpdateVolumeLabel(SFXVolumeLabel, settings.SFXVolume);
            }
            
            if (UIVolumeSlider != null)
            {
                UIVolumeSlider.Value = settings.UIVolume;
                UpdateVolumeLabel(UIVolumeLabel, settings.UIVolume);
            }
        }
        
        public void SaveToSettings(AudioSettingsData settings)
        {
            if (MasterVolumeSlider != null)
                settings.MasterVolume = (float)MasterVolumeSlider.Value;
            
            if (MuteMasterCheckbox != null)
                settings.MuteMaster = MuteMasterCheckbox.ButtonPressed;
            
            if (MusicVolumeSlider != null)
                settings.MusicVolume = (float)MusicVolumeSlider.Value;
            
            if (SFXVolumeSlider != null)
                settings.SFXVolume = (float)SFXVolumeSlider.Value;
            
            if (UIVolumeSlider != null)
                settings.UIVolume = (float)UIVolumeSlider.Value;
        }
        
        #endregion
        
        #region Private Methods
        
        private void ConnectSignals()
        {
            if (MasterVolumeSlider != null)
                MasterVolumeSlider.ValueChanged += (value) => 
                {
                    UpdateVolumeLabel(MasterVolumeLabel, value);
                    // Apply in real-time for preview
                    ApplyMasterVolume((float)value);
                };
            
            if (MuteMasterCheckbox != null)
                MuteMasterCheckbox.Toggled += (pressed) => 
                {
                    ApplyMasterMute(pressed);
                };
            
            if (MusicVolumeSlider != null)
                MusicVolumeSlider.ValueChanged += (value) => 
                {
                    UpdateVolumeLabel(MusicVolumeLabel, value);
                    ApplyMusicVolume((float)value);
                };
            
            if (SFXVolumeSlider != null)
                SFXVolumeSlider.ValueChanged += (value) => 
                {
                    UpdateVolumeLabel(SFXVolumeLabel, value);
                    ApplySFXVolume((float)value);
                };
            
            if (UIVolumeSlider != null)
                UIVolumeSlider.ValueChanged += (value) => 
                {
                    UpdateVolumeLabel(UIVolumeLabel, value);
                    ApplyUIVolume((float)value);
                };
        }
        
        private void UpdateVolumeLabel(Label label, double value)
        {
            if (label != null)
                label.Text = $"{(value * 100):F0}%";
        }
        
        // Real-time preview methods
        private void ApplyMasterVolume(float volume)
        {
            int busIdx = AudioServer.GetBusIndex("Master");
            if (busIdx >= 0)
            {
                AudioServer.SetBusVolumeDb(busIdx, volume > 0 ? Mathf.LinearToDb(volume) : -80f);
            }
        }
        
        private void ApplyMasterMute(bool mute)
        {
            int busIdx = AudioServer.GetBusIndex("Master");
            if (busIdx >= 0)
            {
                AudioServer.SetBusMute(busIdx, mute);
            }
        }
        
        private void ApplyMusicVolume(float volume)
        {
            int busIdx = AudioServer.GetBusIndex("Music");
            if (busIdx >= 0)
            {
                AudioServer.SetBusVolumeDb(busIdx, volume > 0 ? Mathf.LinearToDb(volume) : -80f);
            }
        }
        
        private void ApplySFXVolume(float volume)
        {
            int busIdx = AudioServer.GetBusIndex("SFX");
            if (busIdx >= 0)
            {
                AudioServer.SetBusVolumeDb(busIdx, volume > 0 ? Mathf.LinearToDb(volume) : -80f);
            }
        }
        
        private void ApplyUIVolume(float volume)
        {
            int busIdx = AudioServer.GetBusIndex("UI");
            if (busIdx >= 0)
            {
                AudioServer.SetBusVolumeDb(busIdx, volume > 0 ? Mathf.LinearToDb(volume) : -80f);
            }
        }
        
        #endregion
    }
}
