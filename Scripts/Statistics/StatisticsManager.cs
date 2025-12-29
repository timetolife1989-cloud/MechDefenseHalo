using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Items;
using MechDefenseHalo.Components;
using MechDefenseHalo.Enemies.Bosses;
using MechDefenseHalo.GamePlay;
using MechDefenseHalo.Shop;
using MechDefenseHalo.Crafting;
using MechDefenseHalo.Loot;

namespace MechDefenseHalo.Statistics
{
    /// <summary>
    /// Central manager for all game statistics tracking
    /// </summary>
    public partial class StatisticsManager : Node
    {
        #region Singleton

        private static StatisticsManager _instance;

        public static StatisticsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("StatisticsManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Public Properties

        public CombatStats Combat { get; private set; }
        public EconomyStats Economy { get; private set; }
        public SessionStats Session { get; private set; }

        #endregion

        #region Private Fields

        private float _autoSaveTimer = 0f;
        private const float AutoSaveInterval = 60f; // Save every 60 seconds
        private int _currentKillStreak = 0;
        private float _bossKillTimer = 0f;
        private bool _trackingBossKill = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple StatisticsManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;

            LoadStatistics();
            ConnectEventHandlers();
            StartSessionTracking();

            GD.Print("StatisticsManager initialized successfully");
        }

        public override void _ExitTree()
        {
            DisconnectEventHandlers();
            SaveStatistics();

            if (_instance == this)
            {
                _instance = null;
            }
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // Update session time
            Session.UpdateSession(dt);

            // Auto-save timer
            _autoSaveTimer += dt;
            if (_autoSaveTimer >= AutoSaveInterval)
            {
                SaveStatistics();
                _autoSaveTimer = 0f;
            }

            // Boss kill timer
            if (_trackingBossKill)
            {
                _bossKillTimer += dt;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a summary of key statistics
        /// </summary>
        public Dictionary<string, object> GetStatSummary()
        {
            return new Dictionary<string, object>
            {
                {"Total Kills", Combat.TotalKills},
                {"Highest Wave", Combat.HighestWaveReached},
                {"Playtime", FormatTime(Session.TotalPlaytimeSeconds)},
                {"Legendaries Found", Economy.LegendariesObtained},
                {"Total Sessions", Session.TotalSessions},
                {"Login Streak", Session.DailyLoginStreak}
            };
        }

        /// <summary>
        /// Manually save statistics
        /// </summary>
        public void SaveStatistics()
        {
            StatisticsSaveHandler.SaveStatistics(Combat, Economy, Session);
        }

        /// <summary>
        /// Export statistics for analysis
        /// </summary>
        public bool ExportStatistics(string format = "json")
        {
            return StatisticsSaveHandler.ExportStatistics(Combat, Economy, Session, format);
        }

        #endregion

        #region Private Methods - Initialization

        private void LoadStatistics()
        {
            if (!StatisticsSaveHandler.LoadStatistics(out var combat, out var economy, out var session))
            {
                GD.Print("Failed to load statistics, using defaults");
            }

            Combat = combat ?? new CombatStats();
            Economy = economy ?? new EconomyStats();
            Session = session ?? new SessionStats();
        }

        private void StartSessionTracking()
        {
            Session.StartNewSession();
            SaveStatistics(); // Save the session start
        }

        private void ConnectEventHandlers()
        {
            // Combat events
            EventBus.On(EventBus.EntityDied, OnEntityDied);
            EventBus.On(EventBus.PlayerDied, OnPlayerDied);
            EventBus.On(EventBus.HealthChanged, OnDamageDealt);
            EventBus.On(EventBus.WeaponFired, OnWeaponFired);
            
            // Boss events
            EventBus.On(EventBus.BossSpawned, OnBossSpawned);
            EventBus.On(EventBus.BossDefeated, OnBossDefeated);
            
            // Wave events
            EventBus.On(EventBus.WaveCompleted, OnWaveCompleted);
            
            // Drone events
            EventBus.On(EventBus.DroneDeployed, OnDroneDeployed);
            
            // Economy events
            // Note: Using string literals where EventBus constants don't exist yet
            EventBus.On("currency_changed", OnCurrencyChanged); // CurrencyManager emits string
            EventBus.On(EventBus.ItemPurchased, OnItemPurchased);
            
            // Loot events
            EventBus.On("loot_dropped", OnLootDropped); // LootDropComponent emits string
            
            // Crafting events
            EventBus.On(EventBus.CraftCompleted, OnCraftCompleted);
            
            // Inventory events
            EventBus.On("item_salvaged", OnItemSalvaged);
            EventBus.On("chest_opened", OnChestOpened);
        }

        private void DisconnectEventHandlers()
        {
            EventBus.Off(EventBus.EntityDied, OnEntityDied);
            EventBus.Off(EventBus.PlayerDied, OnPlayerDied);
            EventBus.Off(EventBus.HealthChanged, OnDamageDealt);
            EventBus.Off(EventBus.WeaponFired, OnWeaponFired);
            EventBus.Off(EventBus.BossSpawned, OnBossSpawned);
            EventBus.Off(EventBus.BossDefeated, OnBossDefeated);
            EventBus.Off(EventBus.WaveCompleted, OnWaveCompleted);
            EventBus.Off(EventBus.DroneDeployed, OnDroneDeployed);
            EventBus.Off("currency_changed", OnCurrencyChanged);
            EventBus.Off(EventBus.ItemPurchased, OnItemPurchased);
            EventBus.Off("loot_dropped", OnLootDropped);
            EventBus.Off(EventBus.CraftCompleted, OnCraftCompleted);
            EventBus.Off("item_salvaged", OnItemSalvaged);
            EventBus.Off("chest_opened", OnChestOpened);
        }

        #endregion

        #region Event Handlers - Combat

        private void OnEntityDied(object data)
        {
            if (data is EntityDiedData diedData)
            {
                // Check if enemy died
                if (diedData.Entity != null && diedData.Entity.IsInGroup("enemies"))
                {
                    string enemyType = diedData.Entity.Name;
                    string weaponType = null;
                    bool isDroneKill = false;

                    // Note: Weapon type tracking would require additional data in EntityDiedData
                    // For now, we track kills by enemy type only

                    Combat.RecordKill(enemyType, weaponType, isDroneKill);
                    Session.CurrentSessionKills++;
                    
                    // Track kill streak
                    _currentKillStreak++;
                    if (_currentKillStreak > Combat.LongestKillStreak)
                    {
                        Combat.LongestKillStreak = _currentKillStreak;
                    }
                }
            }
        }

        private void OnPlayerDied(object data)
        {
            Combat.DeathCount++;
            _currentKillStreak = 0; // Reset streak on death
        }

        private void OnDamageDealt(object data)
        {
            if (data is HealthChangedData healthData)
            {
                // Track damage dealt TO enemies (player dealt damage)
                if (healthData.Entity != null && healthData.Entity.IsInGroup("enemies") && healthData.DamageAmount > 0)
                {
                    Combat.TotalDamageDealt += (long)healthData.DamageAmount;
                }
                // Track damage taken by player
                else if (healthData.Entity != null && healthData.Entity.IsInGroup("player") && healthData.DamageAmount > 0)
                {
                    Combat.TotalDamageTaken += (long)healthData.DamageAmount;
                }
            }
        }

        private void OnWeaponFired(object data)
        {
            Combat.ShotsFired++;
            Combat.UpdateAccuracy();
            
            // We'd need additional data to determine if shot hit
            // For now, assume a hit if damage is dealt shortly after
        }

        #endregion

        #region Event Handlers - Boss

        private void OnBossSpawned(object data)
        {
            _trackingBossKill = true;
            _bossKillTimer = 0f;
        }

        private void OnBossDefeated(object data)
        {
            Combat.BossesDefeated++;
            
            if (_trackingBossKill)
            {
                if (_bossKillTimer < Combat.FastestBossKill)
                {
                    Combat.FastestBossKill = _bossKillTimer;
                }
                _trackingBossKill = false;
            }

            // Significant event - save immediately
            SaveStatistics();
        }

        #endregion

        #region Event Handlers - Waves

        private void OnWaveCompleted(object data)
        {
            Session.CurrentSessionWaves++;
            
            if (data is WaveCompletedData waveData)
            {
                if (waveData.WaveNumber > Combat.HighestWaveReached)
                {
                    Combat.HighestWaveReached = waveData.WaveNumber;
                }
            }
        }

        #endregion

        #region Event Handlers - Drones

        private void OnDroneDeployed(object data)
        {
            Combat.DronesDeployed++;
        }

        #endregion

        #region Event Handlers - Economy

        private void OnCurrencyChanged(object data)
        {
            if (data is CurrencyChangedData currencyData)
            {
                if (currencyData.CurrencyType == "credits")
                {
                    if (currencyData.Change > 0)
                        Economy.TotalCreditsEarned += currencyData.Change;
                    else if (currencyData.Change < 0)
                        Economy.TotalCreditsSpent += Math.Abs(currencyData.Change);
                }
                else if (currencyData.CurrencyType == "cores")
                {
                    if (currencyData.Change > 0)
                        Economy.TotalCoresEarned += currencyData.Change;
                    else if (currencyData.Change < 0)
                        Economy.TotalCoresSpent += Math.Abs(currencyData.Change);
                }
            }
        }

        private void OnItemPurchased(object data)
        {
            Economy.ShopPurchases++;
            
            if (data is ItemPurchasedData purchaseData)
            {
                Economy.TotalSpentInShop += purchaseData.PriceCredits + purchaseData.PriceCores;
            }
        }

        #endregion

        #region Event Handlers - Loot

        private void OnLootDropped(object data)
        {
            // Note: This tracks loot DROP events when items are generated
            // Counts items when dropped by enemies. If tracking actual player collection
            // is desired, consider adding a separate "loot_picked_up" event
            if (data is LootDroppedData lootData)
            {
                Economy.ItemsLooted += lootData.ItemIDs?.Count ?? 0;
            }
        }

        private void OnChestOpened(object data)
        {
            Economy.ChestsOpened++;
        }

        #endregion

        #region Event Handlers - Crafting

        private void OnCraftCompleted(object data)
        {
            Economy.ItemsCrafted++;
            
            if (data is CraftEventData craftData)
            {
                // Note: May need ItemDatabase lookup to get item rarity from ResultItemID
                // For now, just increment crafted count
            }
        }

        private void OnItemSalvaged(object data)
        {
            Economy.ItemsSalvaged++;
        }

        #endregion

        #region Utility Methods

        private string FormatTime(float seconds)
        {
            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);
            
            if (hours > 0)
                return $"{hours}h {minutes}m";
            else
                return $"{minutes}m";
        }

        #endregion
    }

    // Note: Event data structures are defined in their respective component files:
    // - EntityDiedData: Components/HealthComponent.cs
    // - HealthChangedData: Components/HealthComponent.cs
    // - WaveCompletedData: Scripts/GamePlay/WaveSpawner.cs
    // - BossDefeatedData: Scripts/Enemies/Bosses/BossBase.cs
    // - ItemPurchasedData: Scripts/Shop/ShopManager.cs
    // - CraftEventData: Scripts/Crafting/CraftingManager.cs
    // - CurrencyChangedData: Scripts/Economy/CurrencyManager.cs
}
