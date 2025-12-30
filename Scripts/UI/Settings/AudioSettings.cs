using Godot;
using System;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.UI.Settings
{
    /// <summary>
    /// Audio settings component
    /// Manages volume controls for master, music, SFX, and UI audio
    /// </summary>
    public partial class AudioSettings : Control
    {
        #region Export Variables
        
        [Export] private HSlider masterVolumeSlider;
        [Export] private Label masterVolumeLabel;
        [Export] private CheckButton muteMasterCheck;
        
        [Export] private HSlider musicVolumeSlider;
        [Export] private Label musicVolumeLabel;
        
        [Export] private HSlider sfxVolumeSlider;
        [Export] private Label sfxVolumeLabel;
        
        [Export] private HSlider uiVolumeSlider;
        [Export] private Label uiVolumeLabel;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
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
            
            var audio = settingsManager.CurrentSettings.Audio;
            
            // Master volume
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.Value = audio.MasterVolume;
                UpdateVolumeLabel(masterVolumeLabel, audio.MasterVolume);
            }
            
            if (muteMasterCheck != null)
            {
                muteMasterCheck.ButtonPressed = audio.MuteMaster;
            }
            
            // Music volume
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.Value = audio.MusicVolume;
                UpdateVolumeLabel(musicVolumeLabel, audio.MusicVolume);
            }
            
            // SFX volume
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.Value = audio.SFXVolume;
                UpdateVolumeLabel(sfxVolumeLabel, audio.SFXVolume);
            }
            
            // UI volume
            if (uiVolumeSlider != null)
            {
                uiVolumeSlider.Value = audio.UIVolume;
                UpdateVolumeLabel(uiVolumeLabel, audio.UIVolume);
            }
        }
        
        public void ApplySettings()
        {
            var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
            if (settingsManager == null || settingsManager.CurrentSettings == null)
            {
                GD.PrintErr("SettingsManager not available!");
                return;
            }
            
            var audio = settingsManager.CurrentSettings.Audio;
            
            // Master volume
            if (masterVolumeSlider != null)
            {
                audio.MasterVolume = (float)masterVolumeSlider.Value;
            }
            
            if (muteMasterCheck != null)
            {
                audio.MuteMaster = muteMasterCheck.ButtonPressed;
            }
            
            // Music volume
            if (musicVolumeSlider != null)
            {
                audio.MusicVolume = (float)musicVolumeSlider.Value;
            }
            
            // SFX volume
            if (sfxVolumeSlider != null)
            {
                audio.SFXVolume = (float)sfxVolumeSlider.Value;
            }
            
            // UI volume
            if (uiVolumeSlider != null)
            {
                audio.UIVolume = (float)uiVolumeSlider.Value;
            }
            
            // Apply audio settings through the applier
            AudioSettingsApplier.Apply(audio);
            
            GD.Print("Audio settings applied");
        }
        
        public void ResetToDefaults()
        {
            var defaults = new AudioSettingsData();
            
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.Value = defaults.MasterVolume;
                UpdateVolumeLabel(masterVolumeLabel, defaults.MasterVolume);
            }
            
            if (muteMasterCheck != null)
            {
                muteMasterCheck.ButtonPressed = defaults.MuteMaster;
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.Value = defaults.MusicVolume;
                UpdateVolumeLabel(musicVolumeLabel, defaults.MusicVolume);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.Value = defaults.SFXVolume;
                UpdateVolumeLabel(sfxVolumeLabel, defaults.SFXVolume);
            }
            
            if (uiVolumeSlider != null)
            {
                uiVolumeSlider.Value = defaults.UIVolume;
                UpdateVolumeLabel(uiVolumeLabel, defaults.UIVolume);
            }
            
            GD.Print("Audio settings reset to defaults");
        }
        
        #endregion
        
        #region Private Methods
        
        private void ConnectSignals()
        {
            // Real-time preview for all sliders
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.ValueChanged += (value) =>
                {
                    UpdateVolumeLabel(masterVolumeLabel, (float)value);
                    PreviewMasterVolume((float)value);
                };
            }
            
            if (muteMasterCheck != null)
            {
                muteMasterCheck.Toggled += (pressed) =>
                {
                    PreviewMasterMute(pressed);
                };
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.ValueChanged += (value) =>
                {
                    UpdateVolumeLabel(musicVolumeLabel, (float)value);
                    PreviewMusicVolume((float)value);
                };
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.ValueChanged += (value) =>
                {
                    UpdateVolumeLabel(sfxVolumeLabel, (float)value);
                    PreviewSFXVolume((float)value);
                };
            }
            
            if (uiVolumeSlider != null)
            {
                uiVolumeSlider.ValueChanged += (value) =>
                {
                    UpdateVolumeLabel(uiVolumeLabel, (float)value);
                    PreviewUIVolume((float)value);
                };
            }
        }
        
        private void UpdateVolumeLabel(Label label, float volume)
        {
            if (label != null)
            {
                label.Text = $"{(volume * 100):F0}%";
            }
        }
        
        // Real-time preview methods
        private void PreviewMasterVolume(float volume)
        {
            int busIdx = AudioServer.GetBusIndex("Master");
            if (busIdx >= 0)
            {
                AudioServer.SetBusVolumeDb(busIdx, volume > 0 ? Mathf.LinearToDb(volume) : -80f);
            }
        }
        
        private void PreviewMasterMute(bool mute)
        {
            int busIdx = AudioServer.GetBusIndex("Master");
            if (busIdx >= 0)
            {
                AudioServer.SetBusMute(busIdx, mute);
            }
        }
        
        private void PreviewMusicVolume(float volume)
        {
            int busIdx = AudioServer.GetBusIndex("Music");
            if (busIdx >= 0)
            {
                AudioServer.SetBusVolumeDb(busIdx, volume > 0 ? Mathf.LinearToDb(volume) : -80f);
            }
        }
        
        private void PreviewSFXVolume(float volume)
        {
            int busIdx = AudioServer.GetBusIndex("SFX");
            if (busIdx >= 0)
            {
                AudioServer.SetBusVolumeDb(busIdx, volume > 0 ? Mathf.LinearToDb(volume) : -80f);
            }
        }
        
        private void PreviewUIVolume(float volume)
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
