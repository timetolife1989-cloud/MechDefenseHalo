using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Core
{
    /// <summary>
    /// Central audio manager singleton.
    /// Manages music, sound effects, and volume controls.
    /// </summary>
    public partial class AudioManager : Node
    {
        #region Singleton

        private static AudioManager _instance;

        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("AudioManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Exported Properties

        [Export] public float MasterVolume { get; set; } = 1f;
        [Export] public float MusicVolume { get; set; } = 0.7f;
        [Export] public float SFXVolume { get; set; } = 1f;
        [Export] public int MaxSimultaneousSounds { get; set; } = 32;

        #endregion

        #region Private Fields

        private AudioStreamPlayer _musicPlayer;
        private List<AudioStreamPlayer> _sfxPlayers = new List<AudioStreamPlayer>();
        private Dictionary<string, AudioStream> _soundLibrary = new Dictionary<string, AudioStream>();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple AudioManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;

            // Create music player
            _musicPlayer = new AudioStreamPlayer();
            _musicPlayer.Name = "MusicPlayer";
            _musicPlayer.Bus = "Music";
            AddChild(_musicPlayer);

            // Create SFX player pool
            for (int i = 0; i < MaxSimultaneousSounds; i++)
            {
                var sfxPlayer = new AudioStreamPlayer();
                sfxPlayer.Name = $"SFXPlayer_{i}";
                sfxPlayer.Bus = "SFX";
                AddChild(sfxPlayer);
                _sfxPlayers.Add(sfxPlayer);
            }

            GD.Print($"AudioManager initialized with {_sfxPlayers.Count} SFX channels");
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods - Music

        /// <summary>
        /// Play music track
        /// </summary>
        public void PlayMusic(AudioStream music, bool loop = true)
        {
            if (_musicPlayer == null || music == null)
                return;

            _musicPlayer.Stream = music;
            _musicPlayer.VolumeDb = Mathf.LinearToDb(MusicVolume * MasterVolume);
            _musicPlayer.Play();

            // Set looping
            if (music is AudioStreamOggVorbis oggStream)
            {
                oggStream.Loop = loop;
            }
            else if (music is AudioStreamWav wavStream)
            {
                wavStream.LoopMode = loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled;
            }

            GD.Print($"Playing music: {music.ResourcePath}");
        }

        /// <summary>
        /// Stop music
        /// </summary>
        public void StopMusic()
        {
            _musicPlayer?.Stop();
        }

        /// <summary>
        /// Fade out music
        /// </summary>
        public void FadeOutMusic(float duration = 1f)
        {
            if (_musicPlayer == null || !_musicPlayer.Playing)
                return;

            var tween = CreateTween();
            tween.TweenProperty(_musicPlayer, "volume_db", -80f, duration);
            tween.TweenCallback(Callable.From(() => _musicPlayer.Stop()));
        }

        #endregion

        #region Public Methods - SFX

        /// <summary>
        /// Play a sound effect
        /// </summary>
        public void PlaySFX(AudioStream sound, float pitch = 1f, float volumeMultiplier = 1f)
        {
            if (sound == null)
                return;

            // Find available player
            AudioStreamPlayer availablePlayer = null;
            foreach (var player in _sfxPlayers)
            {
                if (!player.Playing)
                {
                    availablePlayer = player;
                    break;
                }
            }

            if (availablePlayer == null)
            {
                // All players busy, use first one (interrupt oldest sound)
                availablePlayer = _sfxPlayers[0];
            }

            availablePlayer.Stream = sound;
            availablePlayer.PitchScale = pitch;
            availablePlayer.VolumeDb = Mathf.LinearToDb(SFXVolume * MasterVolume * volumeMultiplier);
            availablePlayer.Play();
        }

        /// <summary>
        /// Play 3D positioned sound
        /// </summary>
        public void PlaySFX3D(AudioStream sound, Vector3 position, float pitch = 1f)
        {
            var player3D = new AudioStreamPlayer3D();
            player3D.Stream = sound;
            player3D.PitchScale = pitch;
            player3D.VolumeDb = Mathf.LinearToDb(SFXVolume * MasterVolume);
            player3D.GlobalPosition = position;
            player3D.Autoplay = true;
            
            GetTree().Root.AddChild(player3D);
            
            // Remove after playing
            player3D.Finished += () => player3D.QueueFree();
        }

        /// <summary>
        /// Stop all sound effects
        /// </summary>
        public void StopAllSFX()
        {
            foreach (var player in _sfxPlayers)
            {
                player.Stop();
            }
        }

        #endregion

        #region Public Methods - Volume Control

        /// <summary>
        /// Set master volume (0-1)
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp(volume, 0f, 1f);
            UpdateVolumes();
        }

        /// <summary>
        /// Set music volume (0-1)
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp(volume, 0f, 1f);
            if (_musicPlayer != null)
            {
                _musicPlayer.VolumeDb = Mathf.LinearToDb(MusicVolume * MasterVolume);
            }
        }

        /// <summary>
        /// Set SFX volume (0-1)
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp(volume, 0f, 1f);
        }

        #endregion

        #region Private Methods

        private void UpdateVolumes()
        {
            if (_musicPlayer != null)
            {
                _musicPlayer.VolumeDb = Mathf.LinearToDb(MusicVolume * MasterVolume);
            }
        }

        #endregion
    }
}
