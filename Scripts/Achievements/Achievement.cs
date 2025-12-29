using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Achievements
{
    /// <summary>
    /// Represents a single achievement that can be unlocked
    /// </summary>
    public class Achievement
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // Combat, Collection, Progression, Boss, Secret
        public bool IsSecret { get; set; }
        public int Progress { get; set; }
        public int RequiredProgress { get; set; }
        public bool IsCompleted { get; set; }
        public Dictionary<string, int> Rewards { get; set; } = new Dictionary<string, int>();
        public string UnlockDate { get; set; } // ISO 8601 format

        public Achievement()
        {
        }

        /// <summary>
        /// Calculate completion percentage
        /// </summary>
        public float GetCompletionPercent()
        {
            if (RequiredProgress <= 0) return IsCompleted ? 100f : 0f;
            return Mathf.Min(100f, (float)Progress / RequiredProgress * 100f);
        }

        /// <summary>
        /// Check if achievement is ready to unlock
        /// </summary>
        public bool CanUnlock()
        {
            return !IsCompleted && Progress >= RequiredProgress;
        }

        /// <summary>
        /// Add progress to achievement
        /// </summary>
        public bool AddProgress(int amount)
        {
            if (IsCompleted) return false;
            
            Progress += amount;
            if (Progress < 0) Progress = 0;
            
            return CanUnlock();
        }

        /// <summary>
        /// Mark achievement as completed
        /// </summary>
        public void Complete()
        {
            IsCompleted = true;
            Progress = RequiredProgress;
            UnlockDate = DateTime.Now.ToString("o");
        }

        /// <summary>
        /// Get display name (hidden if secret and not completed)
        /// </summary>
        public string GetDisplayName()
        {
            if (IsSecret && !IsCompleted)
                return "???";
            return Name;
        }

        /// <summary>
        /// Get display description (hidden if secret and not completed)
        /// </summary>
        public string GetDisplayDescription()
        {
            if (IsSecret && !IsCompleted)
                return "Hidden Achievement";
            return Description;
        }
    }
}
