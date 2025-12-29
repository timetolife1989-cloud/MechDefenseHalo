using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Progression
{
    /// <summary>
    /// Manages player level, XP tracking, and level progression.
    /// Singleton pattern for global access.
    /// </summary>
    public partial class PlayerLevel : Node
    {
        #region Singleton

        private static PlayerLevel _instance;

        public static PlayerLevel Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("PlayerLevel accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Public Properties

        public int CurrentLevel { get; private set; } = 1;
        public int CurrentXP { get; private set; } = 0;
        public int XPToNextLevel => XPCurve.GetXPForNextLevel(CurrentLevel);
        public int TotalXP => XPCurve.GetXPRequired(CurrentLevel) + CurrentXP;

        #endregion

        #region Private Fields

        private PrestigeSystem _prestigeSystem;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple PlayerLevel instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            
            // Initialize prestige system
            _prestigeSystem = GetNodeOrNull<PrestigeSystem>("PrestigeSystem");
            if (_prestigeSystem == null)
            {
                _prestigeSystem = new PrestigeSystem();
                AddChild(_prestigeSystem);
            }

            GD.Print($"PlayerLevel initialized at level {CurrentLevel} with {CurrentXP} XP");
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
        /// Add XP to the player and handle level ups
        /// </summary>
        /// <param name="amount">Amount of XP to add</param>
        /// <param name="source">Source of XP (for logging and events)</param>
        public static void AddXP(int amount, string source)
        {
            if (Instance == null) return;

            if (amount <= 0)
            {
                GD.PrintErr("Cannot add negative or zero XP");
                return;
            }

            Instance.CurrentXP += amount;

            // Check for level ups
            while (Instance.CurrentXP >= Instance.XPToNextLevel && Instance.CurrentLevel < 100)
            {
                Instance.LevelUp();
            }

            // Emit XP gained event
            EventBus.Emit("xp_gained", new XPGainedData
            {
                Amount = amount,
                Source = source,
                CurrentXP = Instance.CurrentXP,
                CurrentLevel = Instance.CurrentLevel,
                XPToNextLevel = Instance.XPToNextLevel
            });

            GD.Print($"Gained {amount} XP from {source}. Current: {Instance.CurrentLevel} ({Instance.CurrentXP}/{Instance.XPToNextLevel})");
        }

        /// <summary>
        /// Set level directly (for loading save data)
        /// </summary>
        public static void SetLevel(int level, int xp = 0)
        {
            if (Instance == null) return;

            Instance.CurrentLevel = Mathf.Clamp(level, 1, 100);
            Instance.CurrentXP = Mathf.Max(0, xp);

            GD.Print($"Level set to {Instance.CurrentLevel} with {Instance.CurrentXP} XP");
        }

        /// <summary>
        /// Get current progress to next level (0.0 to 1.0)
        /// </summary>
        public static float GetProgressToNextLevel()
        {
            if (Instance == null) return 0f;
            if (Instance.CurrentLevel >= 100) return 1f;

            int xpNeeded = Instance.XPToNextLevel;
            if (xpNeeded <= 0) return 1f;

            return (float)Instance.CurrentXP / xpNeeded;
        }

        /// <summary>
        /// Check if prestige is available
        /// </summary>
        public static bool CanPrestige()
        {
            return Instance != null && Instance.CurrentLevel >= 100;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handle level up logic
        /// </summary>
        private void LevelUp()
        {
            CurrentLevel++;
            CurrentXP -= XPToNextLevel;

            // Ensure XP doesn't go negative
            if (CurrentXP < 0)
            {
                CurrentXP = 0;
            }

            // Grant rewards
            LevelRewards.GrantReward(CurrentLevel);

            // Emit level up event
            EventBus.Emit("player_leveled_up", new PlayerLevelUpData
            {
                NewLevel = CurrentLevel,
                IsMilestone = LevelRewards.IsMilestone(CurrentLevel)
            });

            GD.Print($"★ LEVEL UP! Now level {CurrentLevel} ★");
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// XP gained event data
    /// </summary>
    public class XPGainedData
    {
        public int Amount { get; set; }
        public string Source { get; set; }
        public int CurrentXP { get; set; }
        public int CurrentLevel { get; set; }
        public int XPToNextLevel { get; set; }
    }

    /// <summary>
    /// Player level up event data
    /// </summary>
    public class PlayerLevelUpData
    {
        public int NewLevel { get; set; }
        public bool IsMilestone { get; set; }
    }

    #endregion
}
