using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Achievements
{
    /// <summary>
    /// Tracks game events and updates achievement progress automatically.
    /// Listens to EventBus and calls AchievementManager.TrackEvent().
    /// </summary>
    public partial class AchievementTracker : Node
    {
        #region Private Fields

        private AchievementManager _achievementManager;

        // Tracking state for special achievements
        private bool _tookDamageThisWave = false;
        private bool _playerDiedThisWave = false;
        private int _currentWave = 0;
        private DateTime _bossSpawnTime;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            _achievementManager = GetNode<AchievementManager>("/root/AchievementManager");
            
            if (_achievementManager == null)
            {
                GD.PrintErr("AchievementTracker: AchievementManager not found!");
                return;
            }

            SubscribeToEvents();
            GD.Print("AchievementTracker initialized");
        }

        public override void _ExitTree()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            // Combat Events
            EventBus.On(EventBus.EntityDied, OnEntityDied);
            EventBus.On(EventBus.DamageDealt, OnDamageDealt);
            EventBus.On(EventBus.PlayerDied, OnPlayerDied);

            // Wave Events
            EventBus.On(EventBus.WaveStarted, OnWaveStarted);
            EventBus.On(EventBus.WaveCompleted, OnWaveCompleted);

            // Boss Events
            EventBus.On(EventBus.BossSpawned, OnBossSpawned);
            EventBus.On(EventBus.BossDefeated, OnBossDefeated);

            // Loot Events
            EventBus.On(EventBus.RareItemDropped, OnRareItemDropped);
            EventBus.On(EventBus.LootPickedUp, OnLootPickedUp);

            // Inventory Events
            EventBus.On(EventBus.ItemEquipped, OnItemEquipped);
            EventBus.On(EventBus.InventoryChanged, OnInventoryChanged);

            // Set Events
            EventBus.On(EventBus.SetBonusActivated, OnSetBonusActivated);

            // Drone Events
            EventBus.On(EventBus.DroneDeployed, OnDroneDeployed);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Off(EventBus.EntityDied, OnEntityDied);
            EventBus.Off(EventBus.DamageDealt, OnDamageDealt);
            EventBus.Off(EventBus.PlayerDied, OnPlayerDied);
            EventBus.Off(EventBus.WaveStarted, OnWaveStarted);
            EventBus.Off(EventBus.WaveCompleted, OnWaveCompleted);
            EventBus.Off(EventBus.BossSpawned, OnBossSpawned);
            EventBus.Off(EventBus.BossDefeated, OnBossDefeated);
            EventBus.Off(EventBus.RareItemDropped, OnRareItemDropped);
            EventBus.Off(EventBus.LootPickedUp, OnLootPickedUp);
            EventBus.Off(EventBus.ItemEquipped, OnItemEquipped);
            EventBus.Off(EventBus.InventoryChanged, OnInventoryChanged);
            EventBus.Off(EventBus.SetBonusActivated, OnSetBonusActivated);
            EventBus.Off(EventBus.DroneDeployed, OnDroneDeployed);
        }

        #endregion

        #region Event Handlers - Combat

        private void OnEntityDied(object data)
        {
            // Track enemy kills (exclude player)
            if (data != null && !data.ToString().Contains("Player"))
            {
                _achievementManager.TrackEvent("enemy_kill", 1);
            }
        }

        private void OnDamageDealt(object data)
        {
            // Track weak point hits
            // Assuming data contains information about weak point hits
            if (data is Godot.Collections.Dictionary dict)
            {
                if (dict.ContainsKey("weak_point") && (bool)dict["weak_point"])
                {
                    _achievementManager.TrackEvent("weak_point_hit", 1);
                }
            }
        }

        private void OnPlayerDied(object data)
        {
            _playerDiedThisWave = true;
        }

        #endregion

        #region Event Handlers - Waves

        private void OnWaveStarted(object data)
        {
            if (data is int waveNumber)
            {
                _currentWave = waveNumber;
            }
            else if (data is Godot.Collections.Dictionary dict && dict.ContainsKey("wave"))
            {
                _currentWave = (int)dict["wave"];
            }

            // Reset wave tracking
            _tookDamageThisWave = false;
            _playerDiedThisWave = false;
        }

        private void OnWaveCompleted(object data)
        {
            // Track wave completion
            _achievementManager.TrackEvent("wave_completed", 1);

            // Check for no damage wave
            if (!_tookDamageThisWave)
            {
                _achievementManager.TrackEvent("wave_no_damage", 1);
            }

            // Check for perfect wave 10
            if (_currentWave == 10 && !_playerDiedThisWave)
            {
                _achievementManager.TrackEvent("wave_10_no_deaths", 1);
            }
        }

        #endregion

        #region Event Handlers - Boss

        private void OnBossSpawned(object data)
        {
            _bossSpawnTime = DateTime.Now;
        }

        private void OnBossDefeated(object data)
        {
            _achievementManager.TrackEvent("boss_defeated", 1);

            // Check for speed run (under 3 minutes)
            var fightDuration = (DateTime.Now - _bossSpawnTime).TotalSeconds;
            if (fightDuration < 180)
            {
                _achievementManager.TrackEvent("speed_run", 1);
            }

            // Check for flawless victory (no deaths)
            if (!_playerDiedThisWave)
            {
                _achievementManager.TrackEvent("flawless_victory", 1);
            }

            // Check for specific boss (Frost Titan)
            if (data is string bossName && bossName.Contains("Frost"))
            {
                _achievementManager.UnlockAchievement("titan_slayer");
            }
        }

        #endregion

        #region Event Handlers - Loot

        private void OnRareItemDropped(object data)
        {
            // Track legendary items
            _achievementManager.TrackEvent("legendary_obtained", 1);
        }

        private void OnLootPickedUp(object data)
        {
            // Could track specific loot patterns here
        }

        #endregion

        #region Event Handlers - Inventory

        private void OnInventoryChanged(object data)
        {
            // This would need to check inventory size
            // For now, placeholder - would integrate with InventoryManager
        }

        private void OnItemEquipped(object data)
        {
            // Track weapon count and other equipment
        }

        #endregion

        #region Event Handlers - Sets

        private void OnSetBonusActivated(object data)
        {
            if (data is Godot.Collections.Dictionary dict)
            {
                // Check if it's a 4-piece bonus (full set)
                if (dict.ContainsKey("pieces") && (int)dict["pieces"] >= 4)
                {
                    _achievementManager.TrackEvent("set_completed", 1);
                }
            }
        }

        #endregion

        #region Event Handlers - Drones

        private void OnDroneDeployed(object data)
        {
            // Track drone unlocks
            _achievementManager.TrackEvent("drone_unlocked", 1);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually notify that player took damage (to be called by health component)
        /// </summary>
        public void NotifyPlayerDamaged()
        {
            _tookDamageThisWave = true;
        }

        #endregion
    }
}
