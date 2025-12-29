using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.WaveSystem
{
    /// <summary>
    /// Data structure defining a single wave's composition
    /// </summary>
    public class WaveDefinition
    {
        public int WaveNumber { get; set; }
        public bool IsBossWave { get; set; }
        public string BossType { get; set; }
        public List<SpawnGroup> SpawnGroups { get; set; } = new List<SpawnGroup>();
        public List<SupportEnemy> SupportEnemies { get; set; } = new List<SupportEnemy>();
    }

    /// <summary>
    /// Defines a group of enemies to spawn together
    /// </summary>
    public class SpawnGroup
    {
        public string EnemyType { get; set; }
        public int Count { get; set; }
        public float Delay { get; set; } = 1.0f;
        public string SpawnPattern { get; set; } = "Random";
    }

    /// <summary>
    /// Support enemies for boss waves
    /// </summary>
    public class SupportEnemy
    {
        public string EnemyType { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Wave event data
    /// </summary>
    public class WaveStartedEventData
    {
        public int WaveNumber { get; set; }
        public int TotalEnemies { get; set; }
        public bool IsBossWave { get; set; }
    }

    /// <summary>
    /// Wave completion event data
    /// </summary>
    public class WaveCompletedEventData
    {
        public int WaveNumber { get; set; }
        public int CreditsReward { get; set; }
        public int XPReward { get; set; }
    }
}
