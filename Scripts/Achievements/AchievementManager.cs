using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MechDefenseHalo.Core;
using MechDefenseHalo.Economy;

namespace MechDefenseHalo.Achievements
{
    /// <summary>
    /// Manages all achievements in the game.
    /// Handles loading, unlocking, and saving achievement progress.
    /// </summary>
    public partial class AchievementManager : Node
    {
        #region Singleton

        private static AchievementManager _instance;

        public static AchievementManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("AchievementManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Private Fields

        private Dictionary<string, Achievement> _achievements = new Dictionary<string, Achievement>();
        private const string AchievementDataPath = "res://Data/Achievements/";

        #endregion

        #region Public Properties

        public int TotalAchievements => _achievements.Count;
        public int CompletedAchievements => _achievements.Values.Count(a => a.IsCompleted);
        public float CompletionPercentage => TotalAchievements > 0 ? 
            (float)CompletedAchievements / TotalAchievements * 100f : 0f;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple AchievementManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            
            LoadAllAchievements();
            LoadProgress();

            GD.Print($"AchievementManager initialized - {TotalAchievements} achievements loaded ({CompletedAchievements} completed)");
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods - Achievement Access

        /// <summary>
        /// Get achievement by ID
        /// </summary>
        public Achievement GetAchievement(string achievementID)
        {
            return _achievements.TryGetValue(achievementID, out var achievement) ? achievement : null;
        }

        /// <summary>
        /// Get all achievements
        /// </summary>
        public List<Achievement> GetAllAchievements()
        {
            return new List<Achievement>(_achievements.Values);
        }

        /// <summary>
        /// Get achievements by category
        /// </summary>
        public List<Achievement> GetAchievementsByCategory(string category)
        {
            return _achievements.Values.Where(a => a.Category == category).ToList();
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        public List<string> GetCategories()
        {
            return _achievements.Values.Select(a => a.Category).Distinct().ToList();
        }

        #endregion

        #region Public Methods - Progress Tracking

        /// <summary>
        /// Track an event and update relevant achievements
        /// </summary>
        /// <param name="eventType">Type of event (e.g., "enemy_kill")</param>
        /// <param name="value">Value to add to progress</param>
        public void TrackEvent(string eventType, int value = 1)
        {
            // Get achievements that track this event type
            var relevantAchievements = GetAchievementsByEventType(eventType);

            foreach (var achievement in relevantAchievements)
            {
                if (!achievement.IsCompleted)
                {
                    bool canUnlock = achievement.AddProgress(value);
                    
                    if (canUnlock)
                    {
                        UnlockAchievement(achievement.ID);
                    }
                }
            }
        }

        /// <summary>
        /// Manually unlock an achievement (for special conditions)
        /// </summary>
        public bool UnlockAchievement(string achievementID)
        {
            var achievement = GetAchievement(achievementID);
            
            if (achievement == null)
            {
                GD.PrintErr($"Achievement not found: {achievementID}");
                return false;
            }

            if (achievement.IsCompleted)
            {
                return false; // Already unlocked
            }

            achievement.Complete();
            
            // Grant rewards
            GrantRewards(achievement);

            // Save progress
            SaveProgress();

            // Emit event
            EventBus.Emit(EventBus.AchievementUnlocked, achievement);

            GD.Print($"Achievement Unlocked: {achievement.Name}");

            return true;
        }

        #endregion

        #region Private Methods - Data Loading

        private void LoadAllAchievements()
        {
            _achievements.Clear();

            // Load each category
            LoadAchievementFile("combat_achievements.json", "Combat");
            LoadAchievementFile("collection_achievements.json", "Collection");
            LoadAchievementFile("progression_achievements.json", "Progression");
            LoadAchievementFile("boss_achievements.json", "Boss");
            LoadAchievementFile("secret_achievements.json", "Secret");
        }

        private void LoadAchievementFile(string fileName, string category)
        {
            string filePath = AchievementDataPath + fileName;
            
            if (!FileAccess.FileExists(filePath))
            {
                GD.PrintErr($"Achievement file not found: {filePath}");
                return;
            }

            try
            {
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"Failed to open achievement file: {filePath}");
                    return;
                }

                string json = file.GetAsText();
                var achievements = JsonSerializer.Deserialize<List<Achievement>>(json);

                if (achievements != null)
                {
                    foreach (var achievement in achievements)
                    {
                        achievement.Category = category;
                        if (category == "Secret")
                        {
                            achievement.IsSecret = true;
                        }
                        _achievements[achievement.ID] = achievement;
                    }

                    GD.Print($"Loaded {achievements.Count} achievements from {fileName}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to load achievements from {filePath}: {e.Message}");
            }
        }

        #endregion

        #region Private Methods - Save/Load Progress

        private void SaveProgress()
        {
            var saveManager = SaveManager.Instance;
            if (saveManager == null) return;

            // Update achievement data in save file
            var achievementData = new Dictionary<string, AchievementProgress>();
            
            foreach (var kvp in _achievements)
            {
                if (kvp.Value.IsCompleted || kvp.Value.Progress > 0)
                {
                    achievementData[kvp.Key] = new AchievementProgress
                    {
                        Progress = kvp.Value.Progress,
                        IsCompleted = kvp.Value.IsCompleted,
                        UnlockDate = kvp.Value.UnlockDate
                    };
                }
            }

            // Store in player data
            if (saveManager.CurrentPlayerData != null)
            {
                saveManager.CurrentPlayerData.AchievementProgress = achievementData;
                saveManager.SaveGame();
            }
        }

        private void LoadProgress()
        {
            var saveManager = SaveManager.Instance;
            if (saveManager?.CurrentPlayerData?.AchievementProgress == null) return;

            foreach (var kvp in saveManager.CurrentPlayerData.AchievementProgress)
            {
                if (_achievements.TryGetValue(kvp.Key, out var achievement))
                {
                    achievement.Progress = kvp.Value.Progress;
                    achievement.IsCompleted = kvp.Value.IsCompleted;
                    achievement.UnlockDate = kvp.Value.UnlockDate;
                }
            }

            GD.Print($"Loaded progress for {saveManager.CurrentPlayerData.AchievementProgress.Count} achievements");
        }

        #endregion

        #region Private Methods - Rewards

        private void GrantRewards(Achievement achievement)
        {
            if (achievement.Rewards == null || achievement.Rewards.Count == 0)
                return;

            foreach (var reward in achievement.Rewards)
            {
                switch (reward.Key.ToLower())
                {
                    case "credits":
                    case "reward_credits":
                        CurrencyManager.AddCredits(reward.Value, $"Achievement: {achievement.Name}");
                        break;
                    case "cores":
                    case "reward_cores":
                        CurrencyManager.AddCores(reward.Value, $"Achievement: {achievement.Name}");
                        break;
                    case "xp":
                    case "reward_xp":
                        // XP system integration would go here
                        GD.Print($"Granted {reward.Value} XP from achievement");
                        break;
                    case "legendary":
                    case "reward_legendary":
                        // Legendary item grant would go here
                        GD.Print($"Granted {reward.Value} legendary item(s) from achievement");
                        break;
                    default:
                        GD.PrintErr($"Unknown reward type: {reward.Key}");
                        break;
                }
            }
        }

        #endregion

        #region Private Methods - Event Mapping

        private List<Achievement> GetAchievementsByEventType(string eventType)
        {
            // Map event types to achievement IDs
            var results = new List<Achievement>();

            switch (eventType)
            {
                case "enemy_kill":
                    AddIfExists(results, "first_blood", "soldier", "warrior", "genocide");
                    break;
                case "weak_point_hit":
                    AddIfExists(results, "headhunter");
                    break;
                case "wave_no_damage":
                    AddIfExists(results, "no_damage");
                    break;
                case "wave_10_no_deaths":
                    AddIfExists(results, "perfect_wave");
                    break;
                case "inventory_count":
                    AddIfExists(results, "hoarder");
                    break;
                case "legendary_obtained":
                    AddIfExists(results, "legendary_hunter");
                    break;
                case "set_completed":
                    AddIfExists(results, "full_set");
                    break;
                case "weapon_count":
                    AddIfExists(results, "arsenal");
                    break;
                case "drone_unlocked":
                    AddIfExists(results, "drone_commander");
                    break;
                case "level_reached":
                    AddIfExists(results, "novice", "expert", "master");
                    break;
                case "wave_completed":
                    AddIfExists(results, "wave_breaker", "endgame");
                    break;
                case "boss_defeated":
                    AddIfExists(results, "titan_slayer", "flawless_victory", "speed_run", "all_weak_points", "boss_rush");
                    break;
                case "secret_area":
                    AddIfExists(results, "hidden");
                    break;
                case "easter_egg":
                    AddIfExists(results, "easter_egg");
                    break;
                case "triple_legendary":
                    AddIfExists(results, "lucky");
                    break;
                case "pacifist_wave":
                    AddIfExists(results, "pacifist");
                    break;
                case "common_gear_wave":
                    AddIfExists(results, "iron_man");
                    break;
            }

            return results;
        }

        private void AddIfExists(List<Achievement> results, params string[] achievementIDs)
        {
            foreach (var id in achievementIDs)
            {
                if (_achievements.TryGetValue(id, out var achievement))
                {
                    results.Add(achievement);
                }
            }
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Achievement progress data for saving
    /// </summary>
    public class AchievementProgress
    {
        public int Progress { get; set; }
        public bool IsCompleted { get; set; }
        public string UnlockDate { get; set; }
    }

    #endregion
}
