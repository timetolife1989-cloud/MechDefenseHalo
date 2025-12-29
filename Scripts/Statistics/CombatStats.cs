using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Statistics
{
    /// <summary>
    /// Tracks all combat-related statistics
    /// </summary>
    public class CombatStats
    {
        // Kills
        public int TotalKills { get; set; } = 0;
        public Dictionary<string, int> KillsByEnemyType { get; set; } = new Dictionary<string, int>();
        public int BossesDefeated { get; set; } = 0;
        public int DeathCount { get; set; } = 0;
        
        // Damage
        public long TotalDamageDealt { get; set; } = 0;
        public long TotalDamageTaken { get; set; } = 0;
        public int WeakPointHits { get; set; } = 0;
        public float AccuracyPercentage { get; set; } = 0f;
        
        // Weapons
        public Dictionary<string, int> KillsByWeapon { get; set; } = new Dictionary<string, int>();
        public int ShotsFired { get; set; } = 0;
        public int ShotsHit { get; set; } = 0;
        
        // Drones
        public int DronesDeployed { get; set; } = 0;
        public int DroneKills { get; set; } = 0;
        
        // Records
        public int HighestWaveReached { get; set; } = 0;
        public const float NO_BOSS_KILL_RECORDED = float.MaxValue;
        public float FastestBossKill { get; set; } = NO_BOSS_KILL_RECORDED;
        public int LongestKillStreak { get; set; } = 0;

        // Helper methods
        public void RecordKill(string enemyType, string weaponType = null, bool isDrone = false)
        {
            TotalKills++;
            
            if (!string.IsNullOrEmpty(enemyType))
            {
                if (!KillsByEnemyType.ContainsKey(enemyType))
                    KillsByEnemyType[enemyType] = 0;
                KillsByEnemyType[enemyType]++;
            }

            if (!string.IsNullOrEmpty(weaponType))
            {
                if (!KillsByWeapon.ContainsKey(weaponType))
                    KillsByWeapon[weaponType] = 0;
                KillsByWeapon[weaponType]++;
            }

            if (isDrone)
            {
                DroneKills++;
            }
        }

        public void UpdateAccuracy()
        {
            if (ShotsFired > 0)
            {
                AccuracyPercentage = (float)ShotsHit / ShotsFired * 100f;
            }
        }

        /// <summary>
        /// Check if fastest boss kill has been recorded
        /// </summary>
        public bool HasBossKillRecord()
        {
            return FastestBossKill != NO_BOSS_KILL_RECORDED;
        }

        /// <summary>
        /// Get fastest boss kill time in a formatted string
        /// </summary>
        public string GetFormattedBossKillTime()
        {
            if (!HasBossKillRecord())
                return "N/A";
            
            int minutes = (int)(FastestBossKill / 60);
            int seconds = (int)(FastestBossKill % 60);
            return $"{minutes}:{seconds:D2}";
        }
    }
}
