using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Audio
{
    /// <summary>
    /// Controls background music with dynamic transitions based on game state.
    /// </summary>
    public partial class MusicController : Node
    {
        private AudioStreamPlayer currentMusicPlayer;
        private AudioStreamPlayer nextMusicPlayer;
        
        private Dictionary<string, AudioStream> musicTracks;
        private string currentTrack;
        
        [Export] private float crossfadeDuration = 2.0f;
        
        public override void _Ready()
        {
            currentMusicPlayer = new AudioStreamPlayer();
            nextMusicPlayer = new AudioStreamPlayer();
            
            currentMusicPlayer.Bus = "Music";
            nextMusicPlayer.Bus = "Music";
            
            AddChild(currentMusicPlayer);
            AddChild(nextMusicPlayer);
            
            LoadMusicTracks();
            
            // Connect to game events
            EventBus.On("GameStateChanged", Callable.From<object>(OnGameStateChanged));
            EventBus.On("BossWaveStarted", Callable.From<object>(OnBossWaveStarted));
            EventBus.On(EventBus.WaveCompleted, Callable.From<object>(OnWaveCompleted));
            
            GD.Print("MusicController initialized successfully");
        }
        
        private void LoadMusicTracks()
        {
            musicTracks = new Dictionary<string, AudioStream>
            {
                ["menu"] = LoadMusic("res://Assets/Audio/Music/menu_theme.ogg"),
                ["combat"] = LoadMusic("res://Assets/Audio/Music/combat_theme.ogg"),
                ["boss_fight"] = LoadMusic("res://Assets/Audio/Music/boss_theme.ogg"),
                ["victory"] = LoadMusic("res://Assets/Audio/Music/victory_theme.ogg"),
                ["game_over"] = LoadMusic("res://Assets/Audio/Music/game_over.ogg")
            };
        }
        
        private AudioStream LoadMusic(string path)
        {
            if (ResourceLoader.Exists(path))
            {
                return ResourceLoader.Load<AudioStream>(path);
            }
            return null;
        }
        
        public void TransitionTo(string trackName, bool loop = true)
        {
            if (currentTrack == trackName)
                return;
            
            if (!musicTracks.ContainsKey(trackName) || musicTracks[trackName] == null)
            {
                GD.PrintErr($"Music track not found: {trackName}");
                return;
            }
            
            nextMusicPlayer.Stream = musicTracks[trackName];
            nextMusicPlayer.VolumeDb = -80f; // Start silent
            nextMusicPlayer.Play();
            
            // Crossfade
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(currentMusicPlayer, "volume_db", -80f, crossfadeDuration);
            tween.TweenProperty(nextMusicPlayer, "volume_db", 0f, crossfadeDuration);
            
            tween.Chain().TweenCallback(Callable.From(() =>
            {
                currentMusicPlayer.Stop();
                
                // Swap players
                var temp = currentMusicPlayer;
                currentMusicPlayer = nextMusicPlayer;
                nextMusicPlayer = temp;
                
                currentTrack = trackName;
            }));
        }
        
        private void OnGameStateChanged(object data)
        {
            if (data is string newState)
            {
                switch (newState)
                {
                    case "MainMenu":
                        TransitionTo("menu");
                        break;
                    case "InGame":
                        TransitionTo("combat");
                        break;
                    case "Victory":
                        TransitionTo("victory", false);
                        break;
                    case "GameOver":
                        TransitionTo("game_over", false);
                        break;
                }
            }
        }
        
        private void OnBossWaveStarted(object data)
        {
            TransitionTo("boss_fight");
        }
        
        private void OnWaveCompleted(object data)
        {
            // Extract wave number from data
            int wave = 0;
            if (data != null)
            {
                var waveData = data.GetType().GetProperty("Wave");
                if (waveData != null)
                {
                    var waveValue = waveData.GetValue(data);
                    if (waveValue != null && waveValue is int)
                    {
                        wave = (int)waveValue;
                    }
                }
            }
            
            if (wave % 10 == 0) // Boss defeated
            {
                TransitionTo("combat");
            }
        }
    }
}
