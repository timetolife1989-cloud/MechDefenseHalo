using Godot;
using System;

namespace MechDefenseHalo.Audio
{
    /// <summary>
    /// 3D positional sound effect player.
    /// 
    /// USAGE:
    /// SFXPlayer.Instance.Play3D(SoundID.WeaponAssaultRifleFire, position);
    /// SFXPlayer.Instance.Play2D(SoundID.UIButtonClick);
    /// 
    /// FEATURES:
    /// - 3D positional audio with distance attenuation
    /// - 2D UI sounds
    /// - Pitch variation
    /// - Volume randomization
    /// </summary>
    public partial class SFXPlayer : Node
    {
        public static SFXPlayer Instance { get; private set; }

        [Export] public int MaxSimultaneousSounds { get; set; } = 32;
        [Export] public float DefaultMaxDistance { get; set; } = 50f;
        [Export] public float DefaultAttenuationFactor { get; set; } = 1.5f;

        private Node _audioRoot;

        public override void _Ready()
        {
            Instance = this;
            
            // Create root node for audio instances
            _audioRoot = new Node();
            _audioRoot.Name = "SFXInstances";
            AddChild(_audioRoot);
            
            GD.Print($"SFXPlayer initialized with max {MaxSimultaneousSounds} simultaneous sounds");
        }

        /// <summary>
        /// Play a 3D positional sound effect
        /// </summary>
        public void Play3D(SoundID soundID, Vector3 position, float pitch = 1.0f, float volumeDb = 0f)
        {
            string soundPath = SoundLibrary.GetSound(soundID);
            if (string.IsNullOrEmpty(soundPath))
                return;

            var stream = GD.Load<AudioStream>(soundPath);
            if (stream == null)
            {
                GD.PrintErr($"Failed to load sound: {soundPath}");
                return;
            }

            var player = new AudioStreamPlayer3D();
            player.Stream = stream;
            player.PitchScale = pitch;
            player.VolumeDb = volumeDb;
            player.GlobalPosition = position;
            player.MaxDistance = DefaultMaxDistance;
            player.AttenuationModel = AudioStreamPlayer3D.AttenuationModelEnum.InverseDistance;
            player.AttenuationFilterDb = DefaultAttenuationFactor;
            player.Bus = "SFX";
            
            _audioRoot.AddChild(player);
            player.Play();
            
            // Auto-cleanup when finished
            player.Finished += () => 
            {
                player.QueueFree();
            };
            
            // Enforce max simultaneous sounds
            EnforceMaxSounds();
        }

        /// <summary>
        /// Play a 3D positional sound with custom distance settings
        /// </summary>
        public void Play3DCustom(SoundID soundID, Vector3 position, float maxDistance, float pitch = 1.0f, float volumeDb = 0f)
        {
            string soundPath = SoundLibrary.GetSound(soundID);
            if (string.IsNullOrEmpty(soundPath))
                return;

            var stream = GD.Load<AudioStream>(soundPath);
            if (stream == null)
            {
                GD.PrintErr($"Failed to load sound: {soundPath}");
                return;
            }

            var player = new AudioStreamPlayer3D();
            player.Stream = stream;
            player.PitchScale = pitch;
            player.VolumeDb = volumeDb;
            player.GlobalPosition = position;
            player.MaxDistance = maxDistance;
            player.AttenuationModel = AudioStreamPlayer3D.AttenuationModelEnum.InverseDistance;
            player.Bus = "SFX";
            
            _audioRoot.AddChild(player);
            player.Play();
            
            // Auto-cleanup when finished
            player.Finished += () => 
            {
                player.QueueFree();
            };
            
            EnforceMaxSounds();
        }

        /// <summary>
        /// Play a 2D sound effect (no position)
        /// </summary>
        public void Play2D(SoundID soundID, float pitch = 1.0f, float volumeDb = 0f)
        {
            string soundPath = SoundLibrary.GetSound(soundID);
            if (string.IsNullOrEmpty(soundPath))
                return;

            var stream = GD.Load<AudioStream>(soundPath);
            if (stream == null)
            {
                GD.PrintErr($"Failed to load sound: {soundPath}");
                return;
            }

            var player = new AudioStreamPlayer();
            player.Stream = stream;
            player.PitchScale = pitch;
            player.VolumeDb = volumeDb;
            player.Bus = "SFX";
            
            _audioRoot.AddChild(player);
            player.Play();
            
            // Auto-cleanup when finished
            player.Finished += () => 
            {
                player.QueueFree();
            };
            
            EnforceMaxSounds();
        }

        /// <summary>
        /// Play sound with random pitch variation
        /// </summary>
        public void Play3DVaried(SoundID soundID, Vector3 position, float pitchMin = 0.9f, float pitchMax = 1.1f, float volumeDb = 0f)
        {
            float pitch = (float)GD.RandRange(pitchMin, pitchMax);
            Play3D(soundID, position, pitch, volumeDb);
        }

        /// <summary>
        /// Play 2D sound with random pitch variation
        /// </summary>
        public void Play2DVaried(SoundID soundID, float pitchMin = 0.9f, float pitchMax = 1.1f, float volumeDb = 0f)
        {
            float pitch = (float)GD.RandRange(pitchMin, pitchMax);
            Play2D(soundID, pitch, volumeDb);
        }

        /// <summary>
        /// Stop all currently playing sounds
        /// </summary>
        public void StopAll()
        {
            foreach (Node child in _audioRoot.GetChildren())
            {
                if (child is AudioStreamPlayer player)
                {
                    player.Stop();
                    player.QueueFree();
                }
                else if (child is AudioStreamPlayer3D player3D)
                {
                    player3D.Stop();
                    player3D.QueueFree();
                }
            }
        }

        private void EnforceMaxSounds()
        {
            int childCount = _audioRoot.GetChildCount();
            if (childCount > MaxSimultaneousSounds)
            {
                // Remove oldest (first) sound
                var oldest = _audioRoot.GetChild(0);
                if (oldest is AudioStreamPlayer player)
                {
                    player.Stop();
                    player.QueueFree();
                }
                else if (oldest is AudioStreamPlayer3D player3D)
                {
                    player3D.Stop();
                    player3D.QueueFree();
                }
            }
        }
    }
}
