using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Progression
{
    /// <summary>
    /// Manages prestige system that allows players to reset level at 100 for permanent bonuses.
    /// Max 10 prestiges, each granting +5% to all stats.
    /// </summary>
    public partial class PrestigeSystem : Node
    {
        #region Singleton

        private static PrestigeSystem _instance;

        public static PrestigeSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("PrestigeSystem accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Constants

        private const int MAX_PRESTIGE = 10;
        private const float BONUS_PER_PRESTIGE = 0.05f; // 5% per prestige

        #endregion

        #region Public Properties

        public int PrestigeLevel { get; private set; } = 0;
        public float TotalStatBonus => PrestigeLevel * BONUS_PER_PRESTIGE;
        public bool CanPrestige => PlayerLevel.Instance != null && PlayerLevel.Instance.CurrentLevel >= 100 && PrestigeLevel < MAX_PRESTIGE;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple PrestigeSystem instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            GD.Print($"PrestigeSystem initialized at prestige level {PrestigeLevel}");
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Perform prestige - reset level to 1 but keep items/currency and gain permanent stat bonus
        /// </summary>
        public static bool Prestige()
        {
            if (Instance == null) return false;

            if (!Instance.CanPrestige)
            {
                GD.PrintErr("Cannot prestige! Requirements: Level 100 and prestige level < 10");
                return false;
            }

            Instance.PrestigeLevel++;

            // Reset level but keep XP tracking
            PlayerLevel.SetLevel(1, 0);

            // Emit prestige event
            EventBus.Emit("player_prestiged", new PrestigeData
            {
                PrestigeLevel = Instance.PrestigeLevel,
                StatBonus = Instance.TotalStatBonus
            });

            GD.Print($"★★★ PRESTIGE {Instance.PrestigeLevel}! +{Instance.TotalStatBonus * 100}% All Stats ★★★");
            return true;
        }

        /// <summary>
        /// Set prestige level directly (for loading save data)
        /// </summary>
        public static void SetPrestigeLevel(int level)
        {
            if (Instance == null) return;

            Instance.PrestigeLevel = Mathf.Clamp(level, 0, MAX_PRESTIGE);
            GD.Print($"Prestige level set to {Instance.PrestigeLevel}");
        }

        /// <summary>
        /// Get stat multiplier to apply to player stats
        /// </summary>
        public static float GetStatMultiplier()
        {
            if (Instance == null) return 1.0f;
            return 1.0f + Instance.TotalStatBonus;
        }

        /// <summary>
        /// Get prestige bonuses as percentage
        /// </summary>
        public static int GetBonusPercentage()
        {
            if (Instance == null) return 0;
            return (int)(Instance.TotalStatBonus * 100);
        }

        /// <summary>
        /// Get remaining prestiges available
        /// </summary>
        public static int GetRemainingPrestiges()
        {
            if (Instance == null) return 0;
            return MAX_PRESTIGE - Instance.PrestigeLevel;
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Prestige event data
    /// </summary>
    public class PrestigeData
    {
        public int PrestigeLevel { get; set; }
        public float StatBonus { get; set; }
    }

    #endregion
}
