using Godot;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.AI
{
    /// <summary>
    /// Tracks player performance metrics for adaptive AI director.
    /// Monitors kills, deaths, accuracy, and engagement patterns.
    /// </summary>
    public partial class PlayerPerformanceTracker : Node
    {
        public struct PerformanceData
        {
            public float KillRate; // kills per minute
            public float HealthPercent;
            public int DeathsRecent; // last 5 minutes
            public float TimeSinceLastKill;
            public bool IsPlayerAggressive;
            public float AccuracyPercent;
        }
        
        private List<float> killTimestamps = new();
        private List<float> deathTimestamps = new();
        private float lastKillTime = 0f;
        
        public override void _Ready()
        {
            EventBus.On(EventBus.EnemyKilled, OnEnemyKilled);
            EventBus.On(EventBus.PlayerDied, OnPlayerDied);
            GD.Print("PlayerPerformanceTracker initialized");
        }
        
        public override void _ExitTree()
        {
            EventBus.Off(EventBus.EnemyKilled, OnEnemyKilled);
            EventBus.Off(EventBus.PlayerDied, OnPlayerDied);
        }
        
        private void OnEnemyKilled(object data)
        {
            lastKillTime = Time.GetTicksMsec() / 1000f;
            killTimestamps.Add(lastKillTime);
            
            // Keep only last 5 minutes of data
            killTimestamps.RemoveAll(t => lastKillTime - t > 300f);
        }
        
        private void OnPlayerDied(object data)
        {
            float now = Time.GetTicksMsec() / 1000f;
            deathTimestamps.Add(now);
            deathTimestamps.RemoveAll(t => now - t > 300f);
        }
        
        /// <summary>
        /// Get current player performance metrics
        /// </summary>
        public PerformanceData GetCurrentPerformance()
        {
            float now = Time.GetTicksMsec() / 1000f;
            
            // Try to get player health from player node
            float healthPercent = GetPlayerHealthPercent();
            
            return new PerformanceData
            {
                KillRate = killTimestamps.Count / 5f, // kills per minute (last 5 min)
                HealthPercent = healthPercent,
                DeathsRecent = deathTimestamps.Count,
                TimeSinceLastKill = now - lastKillTime,
                IsPlayerAggressive = killTimestamps.Count > 10,
                AccuracyPercent = CalculateAccuracy()
            };
        }
        
        private float GetPlayerHealthPercent()
        {
            // Try to find player health
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0 && players[0] is Node player)
            {
                var healthComponent = player.GetNodeOrNull<HealthComponent>("HealthComponent");
                if (healthComponent != null)
                {
                    return healthComponent.HealthPercent;
                }
            }
            
            // Default to 100% if can't find player
            return 1.0f;
        }
        
        private float CalculateAccuracy()
        {
            // Placeholder implementation
            // TODO: Track shots fired vs hits when weapon system provides this data
            return 0.65f;
        }
    }
}
