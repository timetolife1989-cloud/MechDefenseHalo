using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Audio
{
    /// <summary>
    /// Applies audio settings from the settings system to the audio buses.
    /// Listens for settings changes and updates volume accordingly.
    /// </summary>
    public partial class AudioSettingsApplier : Node
    {
        private const float SILENCE_DB = -80f;
        
        [Export] private float defaultMasterVolume = 1.0f;
        [Export] private float defaultMusicVolume = 0.7f;
        [Export] private float defaultSFXVolume = 1.0f;
        [Export] private float defaultUIVolume = 1.0f;
        
        public override void _Ready()
        {
            // Listen for settings changes
            EventBus.On("SettingsChanged", Callable.From<object>(OnSettingsChanged));
            EventBus.On("AudioSettingsChanged", Callable.From<object>(OnAudioSettingsChanged));
            
            // Apply default settings on startup
            ApplyDefaultSettings();
            
            GD.Print("AudioSettingsApplier initialized successfully");
        }
        
        private void ApplyDefaultSettings()
        {
            SetMasterVolume(defaultMasterVolume);
            SetMusicVolume(defaultMusicVolume);
            SetSFXVolume(defaultSFXVolume);
            SetUIVolume(defaultUIVolume);
        }
        
        private void OnSettingsChanged(object data)
        {
            // Generic settings changed event
            ApplyAudioSettings(data);
        }
        
        private void OnAudioSettingsChanged(object data)
        {
            // Specific audio settings changed event
            ApplyAudioSettings(data);
        }
        
        private void ApplyAudioSettings(object data)
        {
            if (data == null)
                return;
            
            // Try to extract volume settings from data
            var dataType = data.GetType();
            
            // Check for master volume
            var masterVolumeProp = dataType.GetProperty("MasterVolume");
            if (masterVolumeProp != null)
            {
                var volumeValue = masterVolumeProp.GetValue(data);
                if (volumeValue is float volume)
                {
                    SetMasterVolume(volume);
                }
            }
            
            // Check for music volume
            var musicVolumeProp = dataType.GetProperty("MusicVolume");
            if (musicVolumeProp != null)
            {
                var volumeValue = musicVolumeProp.GetValue(data);
                if (volumeValue is float volume)
                {
                    SetMusicVolume(volume);
                }
            }
            
            // Check for SFX volume
            var sfxVolumeProp = dataType.GetProperty("SFXVolume");
            if (sfxVolumeProp != null)
            {
                var volumeValue = sfxVolumeProp.GetValue(data);
                if (volumeValue is float volume)
                {
                    SetSFXVolume(volume);
                }
            }
            
            // Check for UI volume
            var uiVolumeProp = dataType.GetProperty("UIVolume");
            if (uiVolumeProp != null)
            {
                var volumeValue = uiVolumeProp.GetValue(data);
                if (volumeValue is float volume)
                {
                    SetUIVolume(volume);
                }
            }
        }
        
        /// <summary>
        /// Set master volume (0-1 range).
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            volume = Mathf.Clamp(volume, 0f, 1f);
            float volumeDb = LinearToDb(volume);
            
            int busIndex = AudioServer.GetBusIndex("Master");
            if (busIndex >= 0)
            {
                AudioServer.SetBusVolumeDb(busIndex, volumeDb);
            }
        }
        
        /// <summary>
        /// Set music volume (0-1 range).
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            volume = Mathf.Clamp(volume, 0f, 1f);
            float volumeDb = LinearToDb(volume);
            
            int busIndex = AudioServer.GetBusIndex("Music");
            if (busIndex >= 0)
            {
                AudioServer.SetBusVolumeDb(busIndex, volumeDb);
            }
        }
        
        /// <summary>
        /// Set SFX volume (0-1 range).
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            volume = Mathf.Clamp(volume, 0f, 1f);
            float volumeDb = LinearToDb(volume);
            
            int busIndex = AudioServer.GetBusIndex("SFX");
            if (busIndex >= 0)
            {
                AudioServer.SetBusVolumeDb(busIndex, volumeDb);
            }
        }
        
        /// <summary>
        /// Set UI volume (0-1 range).
        /// </summary>
        public void SetUIVolume(float volume)
        {
            volume = Mathf.Clamp(volume, 0f, 1f);
            float volumeDb = LinearToDb(volume);
            
            int busIndex = AudioServer.GetBusIndex("UI");
            if (busIndex >= 0)
            {
                AudioServer.SetBusVolumeDb(busIndex, volumeDb);
            }
        }
        
        /// <summary>
        /// Convert linear volume (0-1) to decibels.
        /// </summary>
        private float LinearToDb(float linear)
        {
            if (linear <= 0f)
                return SILENCE_DB; // Silence
            
            return Mathf.LinearToDb(linear);
        }
    }
}
