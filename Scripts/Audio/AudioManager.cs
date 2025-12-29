using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Audio
{
    /// <summary>
    /// Central audio management system with dynamic music, sound effect pooling, and volume control.
    /// </summary>
    public partial class AudioManager : Node
    {
        private static AudioManager instance;
        public static AudioManager Instance => instance;
        
        private MusicController musicController;
        private SoundEffectPool sfxPool;
        private Dictionary<string, AudioStream> soundLibrary;
        
        public override void _Ready()
        {
            if (instance != null)
            {
                QueueFree();
                return;
            }
            instance = this;
            
            musicController = GetNode<MusicController>("MusicController");
            sfxPool = GetNode<SoundEffectPool>("SFXPool");
            
            LoadSoundLibrary();
            SetupAudioBuses();
            
            EventBus.On("SettingsChanged", Callable.From<object>(ApplyAudioSettings));
            
            GD.Print("AudioManager initialized successfully");
        }
        
        private void SetupAudioBuses()
        {
            // Ensure buses exist: Master → Music, SFX, UI
            if (AudioServer.GetBusIndex("Music") == -1)
            {
                AudioServer.AddBus(1);
                AudioServer.SetBusName(1, "Music");
                AudioServer.SetBusSend(1, "Master");
            }
            
            if (AudioServer.GetBusIndex("SFX") == -1)
            {
                AudioServer.AddBus(2);
                AudioServer.SetBusName(2, "SFX");
                AudioServer.SetBusSend(2, "Master");
            }
            
            if (AudioServer.GetBusIndex("UI") == -1)
            {
                AudioServer.AddBus(3);
                AudioServer.SetBusName(3, "UI");
                AudioServer.SetBusSend(3, "Master");
            }
        }
        
        public void PlaySound(string soundName, Vector3 position = default, float pitch = 1.0f)
        {
            if (!soundLibrary.ContainsKey(soundName))
            {
                GD.PrintErr($"Sound not found: {soundName}");
                return;
            }
            
            var stream = soundLibrary[soundName];
            
            if (position == default)
            {
                sfxPool.Play2D(stream, pitch);
            }
            else
            {
                sfxPool.Play3D(stream, position, pitch);
            }
        }
        
        public void PlayUISound(string soundName)
        {
            if (!soundLibrary.ContainsKey(soundName))
                return;
            
            sfxPool.PlayOnBus(soundLibrary[soundName], "UI");
        }
        
        private void LoadSoundLibrary()
        {
            soundLibrary = new Dictionary<string, AudioStream>();
            
            // Load all sounds from Assets/Audio/SFX/
            // Placeholder references until audio files exist
            soundLibrary["weapon_fire"] = LoadSound("res://Assets/Audio/SFX/weapon_fire.ogg");
            soundLibrary["enemy_hit"] = LoadSound("res://Assets/Audio/SFX/enemy_hit.ogg");
            soundLibrary["enemy_death"] = LoadSound("res://Assets/Audio/SFX/enemy_death.ogg");
            soundLibrary["loot_pickup"] = LoadSound("res://Assets/Audio/SFX/loot_pickup.ogg");
            soundLibrary["ui_click"] = LoadSound("res://Assets/Audio/SFX/ui_click.ogg");
            soundLibrary["level_up"] = LoadSound("res://Assets/Audio/SFX/level_up.ogg");
            soundLibrary["boss_roar"] = LoadSound("res://Assets/Audio/SFX/boss_roar.ogg");
            soundLibrary["achievement"] = LoadSound("res://Assets/Audio/SFX/achievement.ogg");
        }
        
        private AudioStream LoadSound(string path)
        {
            if (ResourceLoader.Exists(path))
            {
                return ResourceLoader.Load<AudioStream>(path);
            }
            return null; // Return null if placeholder doesn't exist yet
        }
        
        private void ApplyAudioSettings(object data)
        {
            // Settings will be applied through AudioSettingsApplier
            GD.Print("Audio settings changed");
        }
    }
}
