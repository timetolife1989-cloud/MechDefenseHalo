using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Notifications
{
    /// <summary>
    /// Represents a daily mission
    /// </summary>
    public class Mission
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public MissionType Type { get; set; }
        public int CurrentProgress { get; set; }
        public int RequiredProgress { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsRewardClaimed { get; set; }
        
        public Dictionary<string, int> Rewards { get; set; } = new Dictionary<string, int>();
        
        public DateTime AssignedDate { get; set; }
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Get progress as a percentage (0-100)
        /// </summary>
        public float GetProgressPercentage()
        {
            if (RequiredProgress <= 0) return 0f;
            return Mathf.Min(100f, (float)CurrentProgress / RequiredProgress * 100f);
        }

        /// <summary>
        /// Check if the mission is expired
        /// </summary>
        public bool IsExpired()
        {
            return DateTime.Now > ExpirationDate;
        }
    }

    /// <summary>
    /// Types of missions available
    /// </summary>
    public enum MissionType
    {
        KillEnemies,
        CompleteWaves,
        DefeatBosses,
        CraftItems,
        DeployDrones,
        CollectLoot,
        SpendCurrency,
        DealDamage,
        SurviveTime
    }

    /// <summary>
    /// Mission difficulty levels
    /// </summary>
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    /// <summary>
    /// Template for generating missions
    /// </summary>
    public class MissionTemplate
    {
        public MissionType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Vector2I ProgressRange { get; set; }
        public Difficulty Difficulty { get; set; }
        public Dictionary<string, int> Rewards { get; set; } = new Dictionary<string, int>();
    }
}
