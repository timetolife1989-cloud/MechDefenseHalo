using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Player;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Notifications
{
    /// <summary>
    /// Manages daily missions lifecycle and progress tracking
    /// </summary>
    public partial class DailyMissionManager : Node
    {
        #region Singleton

        private static DailyMissionManager _instance;

        public static DailyMissionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("DailyMissionManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Constants

        private const int DAILY_MISSION_COUNT = 3;

        #endregion

        #region Private Fields

        private List<Mission> activeMissions = new List<Mission>();
        private MissionGenerator generator;
        private NotificationQueue notificationQueue;

        #endregion

        #region Public Properties

        public List<Mission> ActiveMissions => new List<Mission>(activeMissions);

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple DailyMissionManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;

            // Setup child nodes
            generator = GetNodeOrNull<MissionGenerator>("MissionGenerator");
            notificationQueue = GetNodeOrNull<NotificationQueue>("NotificationQueue");

            if (generator == null)
            {
                generator = new MissionGenerator();
                AddChild(generator);
            }

            if (notificationQueue == null)
            {
                notificationQueue = new NotificationQueue();
                AddChild(notificationQueue);
            }

            LoadDailyMissions();
            ConnectEventHandlers();

            GD.Print("DailyMissionManager initialized");
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
        /// Claim reward for a completed mission
        /// </summary>
        public void ClaimMissionReward(string missionID)
        {
            var mission = activeMissions.FirstOrDefault(m => m.ID == missionID);

            if (mission == null || !mission.IsCompleted || mission.IsRewardClaimed)
                return;

            // Grant rewards
            foreach (var reward in mission.Rewards)
            {
                switch (reward.Key)
                {
                    case "Credits":
                        CurrencyManager.AddCredits(reward.Value, "daily_mission");
                        break;
                    case "Cores":
                        CurrencyManager.AddCores(reward.Value, "daily_mission");
                        break;
                    case "XP":
                        PlayerLevel.AddXP(reward.Value, "daily_mission");
                        break;
                }
            }

            mission.IsRewardClaimed = true;
            SaveManager.SetDailyMissions(activeMissions);

            string rewardText = "";
            if (mission.Rewards.ContainsKey("Credits"))
                rewardText += $"{mission.Rewards["Credits"]} Credits ";
            if (mission.Rewards.ContainsKey("Cores"))
                rewardText += $"{mission.Rewards["Cores"]} Cores ";
            if (mission.Rewards.ContainsKey("XP"))
                rewardText += $"{mission.Rewards["XP"]} XP";

            notificationQueue?.ShowNotification(
                $"Reward claimed: {rewardText.Trim()}!",
                NotificationType.Reward
            );

            EventBus.Emit("mission_reward_claimed", missionID);
        }

        /// <summary>
        /// Manually trigger daily mission refresh (for testing)
        /// </summary>
        public void ForceRefresh()
        {
            GenerateNewDailyMissions();
        }

        #endregion

        #region Private Methods

        private void LoadDailyMissions()
        {
            var savedMissions = SaveManager.GetDailyMissions();

            if (savedMissions == null || savedMissions.Count == 0 || IsNewDay())
            {
                GenerateNewDailyMissions();
            }
            else
            {
                activeMissions = savedMissions;
                CheckForExpiredMissions();
                GD.Print($"Loaded {activeMissions.Count} daily missions from save");
            }
        }

        private bool IsNewDay()
        {
            var lastResetDate = SaveManager.GetDateTime("last_daily_reset");
            var now = DateTime.Now;

            // Reset at 00:00 UTC
            return now.Date > lastResetDate.Date;
        }

        private void GenerateNewDailyMissions()
        {
            activeMissions.Clear();

            // Generate 3 random missions of varying difficulty
            activeMissions.Add(generator.GenerateMission(Difficulty.Easy));
            activeMissions.Add(generator.GenerateMission(Difficulty.Medium));
            activeMissions.Add(generator.GenerateMission(Difficulty.Hard));

            foreach (var mission in activeMissions)
            {
                mission.AssignedDate = DateTime.Now;
                mission.ExpirationDate = DateTime.Now.AddHours(24);
            }

            SaveManager.SetDailyMissions(activeMissions);
            SaveManager.SetDateTime("last_daily_reset", DateTime.Now);

            // Show notification
            notificationQueue?.ShowNotification("New daily missions available!", NotificationType.Info);

            EventBus.Emit("daily_missions_refreshed", activeMissions.Count);
            GD.Print($"Generated {activeMissions.Count} new daily missions");
        }

        private void CheckForExpiredMissions()
        {
            bool hasExpired = false;
            foreach (var mission in activeMissions)
            {
                if (mission.IsExpired())
                {
                    hasExpired = true;
                    break;
                }
            }

            if (hasExpired)
            {
                GenerateNewDailyMissions();
            }
        }

        private void ConnectEventHandlers()
        {
            EventBus.On("enemy_killed", OnEnemyKilled);
            EventBus.On("wave_completed", OnWaveCompleted);
            EventBus.On("boss_defeated", OnBossDefeated);
            EventBus.On("craft_completed", OnItemCrafted);
            EventBus.On("drone_deployed", OnDroneDeployed);
            EventBus.On("loot_picked_up", OnItemLooted);
            EventBus.On("damage_dealt", OnDamageDealt);
            EventBus.On("currency_changed", OnCurrencyChanged);
        }

        private void OnEnemyKilled(object data)
        {
            UpdateMissionProgress(MissionType.KillEnemies, 1);
        }

        private void OnWaveCompleted(object data)
        {
            UpdateMissionProgress(MissionType.CompleteWaves, 1);
        }

        private void OnBossDefeated(object data)
        {
            UpdateMissionProgress(MissionType.DefeatBosses, 1);
        }

        private void OnItemCrafted(object data)
        {
            UpdateMissionProgress(MissionType.CraftItems, 1);
        }

        private void OnDroneDeployed(object data)
        {
            UpdateMissionProgress(MissionType.DeployDrones, 1);
        }

        private void OnItemLooted(object data)
        {
            UpdateMissionProgress(MissionType.CollectLoot, 1);
        }

        private void OnDamageDealt(object data)
        {
            if (data is int damage)
            {
                UpdateMissionProgress(MissionType.DealDamage, damage);
            }
            else if (data is float damageFloat)
            {
                UpdateMissionProgress(MissionType.DealDamage, (int)damageFloat);
            }
        }

        private void OnCurrencyChanged(object data)
        {
            // Track currency spending for SpendCurrency missions
            if (data is CurrencyChangedData currencyData && currencyData.Change < 0)
            {
                UpdateMissionProgress(MissionType.SpendCurrency, Math.Abs(currencyData.Change));
            }
        }

        private void UpdateMissionProgress(MissionType type, int amount)
        {
            bool anyUpdated = false;

            foreach (var mission in activeMissions)
            {
                if (mission.Type == type && !mission.IsCompleted)
                {
                    mission.CurrentProgress += amount;

                    if (mission.CurrentProgress >= mission.RequiredProgress)
                    {
                        CompleteMission(mission);
                    }
                    else
                    {
                        EventBus.Emit("mission_progress_updated", new { MissionID = mission.ID, Progress = mission.CurrentProgress });
                    }

                    anyUpdated = true;
                }
            }

            if (anyUpdated)
            {
                SaveManager.SetDailyMissions(activeMissions);
            }
        }

        private void CompleteMission(Mission mission)
        {
            mission.IsCompleted = true;
            mission.CurrentProgress = mission.RequiredProgress; // Cap at required

            notificationQueue?.ShowNotification(
                $"Mission Complete: {mission.Title}",
                NotificationType.Success
            );

            EventBus.Emit("mission_completed", mission.ID);
            GD.Print($"Mission completed: {mission.Title}");
        }

        #endregion
    }
}
