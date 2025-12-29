using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Player
{
    /// <summary>
    /// Manages player level and experience
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

        public int CurrentLevel => SaveManager.Instance?.CurrentPlayerData?.Level ?? 1;
        public int CurrentXP => SaveManager.Instance?.CurrentPlayerData?.Experience ?? 0;

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
            GD.Print("PlayerLevel initialized");
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
        /// Add XP to the player
        /// </summary>
        public static void AddXP(int amount, string source = "")
        {
            if (Instance == null || SaveManager.Instance?.CurrentPlayerData == null) return;

            if (amount <= 0)
            {
                GD.PrintErr("Cannot add negative or zero XP");
                return;
            }

            var playerData = SaveManager.Instance.CurrentPlayerData;
            playerData.Experience += amount;

            GD.Print($"Added {amount} XP from {source}. Total: {playerData.Experience}");

            // Check for level up
            int requiredXP = GetXPForNextLevel(playerData.Level);
            while (playerData.Experience >= requiredXP && playerData.Level < 100)
            {
                playerData.Experience -= requiredXP;
                playerData.Level++;
                
                GD.Print($"Level Up! New level: {playerData.Level}");
                EventBus.Emit("player_level_up", playerData.Level);
                
                requiredXP = GetXPForNextLevel(playerData.Level);
            }

            SaveManager.Instance.SaveGame();
            EventBus.Emit("player_xp_changed", new { Level = playerData.Level, XP = playerData.Experience });
        }

        /// <summary>
        /// Get XP required for next level
        /// </summary>
        public static int GetXPForNextLevel(int currentLevel)
        {
            // Simple exponential formula: 100 * level^1.5
            return Mathf.RoundToInt(100 * Mathf.Pow(currentLevel, 1.5f));
        }

        #endregion
    }
}
