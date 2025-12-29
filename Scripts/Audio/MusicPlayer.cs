using Godot;
using System;

namespace MechDefenseHalo.Audio
{
    /// <summary>
    /// Background music player with crossfade support.
    /// 
    /// USAGE:
    /// MusicPlayer.Instance.PlayMusic(SoundID.MusicCombat, crossfadeDuration: 2.0f);
    /// MusicPlayer.Instance.StopMusic();
    /// 
    /// SETUP:
    /// Add 2 AudioStreamPlayer nodes to AudioManager:
    /// - MusicPlayer1
    /// - MusicPlayer2
    /// </summary>
    public partial class MusicPlayer : Node
    {
        private const float SILENCE_VOLUME_DB = -80f;
        private const float DEFAULT_VOLUME_DB = 0f;
        
        public static MusicPlayer Instance { get; private set; }

        [Export] public AudioStreamPlayer Player1 { get; set; }
        [Export] public AudioStreamPlayer Player2 { get; set; }

        private AudioStreamPlayer _currentPlayer;
        private AudioStreamPlayer _otherPlayer;
        private SoundID _currentMusicID;
        private Tween _crossfadeTween;

        public override void _Ready()
        {
            Instance = this;
            _currentPlayer = Player1;
            _otherPlayer = Player2;
        }

        /// <summary>
        /// Play music with optional crossfade
        /// </summary>
        public void PlayMusic(SoundID musicID, float crossfadeDuration = 1.0f)
        {
            if (musicID == _currentMusicID && _currentPlayer.Playing)
                return;

            string musicPath = SoundLibrary.GetSound(musicID);
            var stream = GD.Load<AudioStream>(musicPath);

            if (stream == null)
            {
                GD.PrintErr($"Failed to load music: {musicPath}");
                return;
            }

            if (crossfadeDuration > 0 && _currentPlayer.Playing)
            {
                CrossfadeTo(stream, crossfadeDuration);
            }
            else
            {
                _currentPlayer.Stream = stream;
                _currentPlayer.Play();
            }

            _currentMusicID = musicID;
        }

        public void StopMusic(float fadeOutDuration = 1.0f)
        {
            if (fadeOutDuration > 0)
            {
                FadeOut(_currentPlayer, fadeOutDuration);
            }
            else
            {
                _currentPlayer.Stop();
            }
        }

        public void SetVolume(float volumeDb)
        {
            if (Player1 != null) Player1.VolumeDb = volumeDb;
            if (Player2 != null) Player2.VolumeDb = volumeDb;
        }

        private void CrossfadeTo(AudioStream newStream, float duration)
        {
            _crossfadeTween?.Kill();
            _crossfadeTween = CreateTween();
            _crossfadeTween.SetParallel(true);

            // Fade out current
            _crossfadeTween.TweenProperty(_currentPlayer, "volume_db", SILENCE_VOLUME_DB, duration);

            // Fade in new
            _otherPlayer.Stream = newStream;
            _otherPlayer.VolumeDb = SILENCE_VOLUME_DB;
            _otherPlayer.Play();
            _crossfadeTween.TweenProperty(_otherPlayer, "volume_db", DEFAULT_VOLUME_DB, duration);

            _crossfadeTween.Chain().TweenCallback(Callable.From(() =>
            {
                _currentPlayer.Stop();
                SwapPlayers();
            }));
        }

        private void FadeOut(AudioStreamPlayer player, float duration)
        {
            var tween = CreateTween();
            tween.TweenProperty(player, "volume_db", SILENCE_VOLUME_DB, duration);
            tween.TweenCallback(Callable.From(() => player.Stop()));
        }

        private void SwapPlayers()
        {
            (_currentPlayer, _otherPlayer) = (_otherPlayer, _currentPlayer);
        }
    }
}
