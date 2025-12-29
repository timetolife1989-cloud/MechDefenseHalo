using Godot;
using System;

namespace MechDefenseHalo.Audio
{
    /// <summary>
    /// Audio settings manager with volume control and persistence.
    /// 
    /// USAGE:
    /// AudioSettings.Instance.SetMasterVolume(0.8f);
    /// AudioSettings.Instance.SetMusicVolume(0.6f);
    /// AudioSettings.Instance.SaveSettings();
    /// 
    /// FEATURES:
    /// - Master, Music, and SFX volume controls
    /// - Persistent settings (saved to user://audio_settings.cfg)
    /// - Audio bus integration
    /// - Volume normalization (0-1 range)
    /// </summary>
    public partial class AudioSettings : Node
    {
        public static AudioSettings Instance { get; private set; }

        #region Constants

        private const string SETTINGS_FILE = "user://audio_settings.cfg";
        private const string SECTION_AUDIO = "audio";
        private const string KEY_MASTER_VOLUME = "master_volume";
        private const string KEY_MUSIC_VOLUME = "music_volume";
        private const string KEY_SFX_VOLUME = "sfx_volume";

        #endregion

        #region Properties

        private float _masterVolume = 1.0f;
        private float _musicVolume = 0.7f;
        private float _sfxVolume = 1.0f;

        public float MasterVolume => _masterVolume;
        public float MusicVolume => _musicVolume;
        public float SFXVolume => _sfxVolume;

        #endregion

        public override void _Ready()
        {
            Instance = this;
            LoadSettings();
            ApplySettings();
            GD.Print("AudioSettings initialized");
        }

        #region Public Methods

        /// <summary>
        /// Set master volume (0-1)
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp(volume, 0f, 1f);
            ApplyMasterVolume();
        }

        /// <summary>
        /// Set music volume (0-1)
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp(volume, 0f, 1f);
            ApplyMusicVolume();
        }

        /// <summary>
        /// Set SFX volume (0-1)
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp(volume, 0f, 1f);
            ApplySFXVolume();
        }

        /// <summary>
        /// Save current settings to disk
        /// </summary>
        public void SaveSettings()
        {
            var config = new ConfigFile();
            
            config.SetValue(SECTION_AUDIO, KEY_MASTER_VOLUME, _masterVolume);
            config.SetValue(SECTION_AUDIO, KEY_MUSIC_VOLUME, _musicVolume);
            config.SetValue(SECTION_AUDIO, KEY_SFX_VOLUME, _sfxVolume);
            
            Error err = config.Save(SETTINGS_FILE);
            if (err != Error.Ok)
            {
                GD.PrintErr($"Failed to save audio settings: {err}");
            }
            else
            {
                GD.Print("Audio settings saved successfully");
            }
        }

        /// <summary>
        /// Load settings from disk
        /// </summary>
        public void LoadSettings()
        {
            var config = new ConfigFile();
            Error err = config.Load(SETTINGS_FILE);
            
            if (err != Error.Ok)
            {
                GD.Print($"No saved audio settings found, using defaults");
                return;
            }
            
            _masterVolume = (float)config.GetValue(SECTION_AUDIO, KEY_MASTER_VOLUME, 1.0f);
            _musicVolume = (float)config.GetValue(SECTION_AUDIO, KEY_MUSIC_VOLUME, 0.7f);
            _sfxVolume = (float)config.GetValue(SECTION_AUDIO, KEY_SFX_VOLUME, 1.0f);
            
            GD.Print("Audio settings loaded successfully");
        }

        /// <summary>
        /// Reset settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            _masterVolume = 1.0f;
            _musicVolume = 0.7f;
            _sfxVolume = 1.0f;
            ApplySettings();
            SaveSettings();
        }

        #endregion

        #region Private Methods

        private void ApplySettings()
        {
            ApplyMasterVolume();
            ApplyMusicVolume();
            ApplySFXVolume();
        }

        private void ApplyMasterVolume()
        {
            int busIdx = AudioServer.GetBusIndex("Master");
            if (busIdx >= 0)
            {
                float volumeDb = LinearToDb(_masterVolume);
                AudioServer.SetBusVolumeDb(busIdx, volumeDb);
            }
        }

        private void ApplyMusicVolume()
        {
            int busIdx = AudioServer.GetBusIndex("Music");
            if (busIdx >= 0)
            {
                float volumeDb = LinearToDb(_musicVolume);
                AudioServer.SetBusVolumeDb(busIdx, volumeDb);
            }
        }

        private void ApplySFXVolume()
        {
            int busIdx = AudioServer.GetBusIndex("SFX");
            if (busIdx >= 0)
            {
                float volumeDb = LinearToDb(_sfxVolume);
                AudioServer.SetBusVolumeDb(busIdx, volumeDb);
            }
        }

        /// <summary>
        /// Convert linear volume (0-1) to decibels
        /// </summary>
        private float LinearToDb(float linear)
        {
            if (linear <= 0f)
                return -80f; // Silence
            
            return Mathf.LinearToDb(linear);
        }

        #endregion
    }
}
