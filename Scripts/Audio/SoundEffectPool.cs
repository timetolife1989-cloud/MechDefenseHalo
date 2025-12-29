using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.Audio
{
    /// <summary>
    /// Object pooling system for sound effects to prevent performance issues.
    /// </summary>
    public partial class SoundEffectPool : Node
    {
        private List<AudioStreamPlayer> available2DPlayers = new();
        private List<AudioStreamPlayer3D> available3DPlayers = new();
        
        private const int INITIAL_POOL_SIZE_2D = 20;
        private const int INITIAL_POOL_SIZE_3D = 10;
        
        public override void _Ready()
        {
            // Pre-create player pool for performance
            for (int i = 0; i < INITIAL_POOL_SIZE_2D; i++)
            {
                CreateNew2DPlayer();
            }
            
            for (int i = 0; i < INITIAL_POOL_SIZE_3D; i++)
            {
                CreateNew3DPlayer();
            }
            
            GD.Print($"SoundEffectPool initialized with {INITIAL_POOL_SIZE_2D} 2D and {INITIAL_POOL_SIZE_3D} 3D players");
        }
        
        private void CreateNew2DPlayer()
        {
            var player = new AudioStreamPlayer();
            player.Bus = "SFX";
            player.Finished += () => Return2DPlayer(player);
            AddChild(player);
            available2DPlayers.Add(player);
        }
        
        private void CreateNew3DPlayer()
        {
            var player = new AudioStreamPlayer3D();
            player.Bus = "SFX";
            player.Finished += () => Return3DPlayer(player);
            AddChild(player);
            available3DPlayers.Add(player);
        }
        
        public void Play2D(AudioStream stream, float pitch = 1.0f)
        {
            if (stream == null)
                return;
                
            var player = GetOrCreate2DPlayer();
            player.Stream = stream;
            player.PitchScale = pitch;
            player.Play();
        }
        
        public void Play3D(AudioStream stream, Vector3 position, float pitch = 1.0f)
        {
            if (stream == null)
                return;
                
            var player = GetOrCreate3DPlayer();
            player.Stream = stream;
            player.GlobalPosition = position;
            player.PitchScale = pitch;
            player.Play();
        }
        
        public void PlayOnBus(AudioStream stream, string busName)
        {
            if (stream == null)
                return;
                
            var player = GetOrCreate2DPlayer();
            player.Bus = busName;
            player.Stream = stream;
            player.Play();
        }
        
        private AudioStreamPlayer GetOrCreate2DPlayer()
        {
            if (available2DPlayers.Count > 0)
            {
                var player = available2DPlayers[0];
                available2DPlayers.RemoveAt(0);
                return player;
            }
            
            // Pool exhausted, create new
            var newPlayer = new AudioStreamPlayer();
            newPlayer.Bus = "SFX";
            newPlayer.Finished += () => Return2DPlayer(newPlayer);
            AddChild(newPlayer);
            return newPlayer;
        }
        
        private AudioStreamPlayer3D GetOrCreate3DPlayer()
        {
            if (available3DPlayers.Count > 0)
            {
                var player = available3DPlayers[0];
                available3DPlayers.RemoveAt(0);
                return player;
            }
            
            var newPlayer = new AudioStreamPlayer3D();
            newPlayer.Bus = "SFX";
            newPlayer.Finished += () => Return3DPlayer(newPlayer);
            AddChild(newPlayer);
            return newPlayer;
        }
        
        private void Return2DPlayer(AudioStreamPlayer player)
        {
            available2DPlayers.Add(player);
        }
        
        private void Return3DPlayer(AudioStreamPlayer3D player)
        {
            available3DPlayers.Add(player);
        }
    }
}
