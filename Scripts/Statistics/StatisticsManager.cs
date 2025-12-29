using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Items;

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
            EventBus.On(EventBus.DamageDealt, OnDamageDealt);
            EventBus.On(EventBus.WeaponFired, OnWeaponFired);
            
            // Boss events
            EventBus.On(EventBus.BossSpawned, OnBossSpawned);
            EventBus.On(EventBus.BossDefeated, OnBossDefeated);
            
            // Wave events
            EventBus.On(EventBus.WaveCompleted, OnWaveCompleted);
            
            // Drone events
            EventBus.On(EventBus.DroneDeployed, OnDroneDeployed);
            
            // Economy events
            EventBus.On(EventBus.CurrencyChanged, OnCurrencyChanged);
            EventBus.On(EventBus.ItemPurchased, OnItemPurchased);
            
            // Loot events
            EventBus.On(EventBus.LootPickedUp, OnLootPickedUp);
            
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
            EventBus.Off(EventBus.DamageDealt, OnDamageDealt);
            EventBus.Off(EventBus.WeaponFired, OnWeaponFired);
            EventBus.Off(EventBus.BossSpawned, OnBossSpawned);
            EventBus.Off(EventBus.BossDefeated, OnBossDefeated);
            EventBus.Off(EventBus.WaveCompleted, OnWaveCompleted);
            EventBus.Off(EventBus.DroneDeployed, OnDroneDeployed);
            EventBus.Off(EventBus.CurrencyChanged, OnCurrencyChanged);
            EventBus.Off(EventBus.ItemPurchased, OnItemPurchased);
            EventBus.Off(EventBus.LootPickedUp, OnLootPickedUp);
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

                    // Try to determine weapon type from killer
                    if (diedData.Killer != null)
                    {
                        if (diedData.Killer.IsInGroup("drones"))
                        {
                            isDroneKill = true;
                        }
                        weaponType = diedData.Killer.Name;
                    }

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
            if (data is DamageData damageData)
            {
                // Track damage dealt by player
                if (damageData.Attacker != null && damageData.Attacker.IsInGroup("player"))
                {
                    Combat.TotalDamageDealt += (long)damageData.Amount;
                }
                // Track damage taken by player
                else if (damageData.Victim != null && damageData.Victim.IsInGroup("player"))
                {
                    Combat.TotalDamageTaken += (long)damageData.Amount;
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
            
            if (data is ItemPurchaseData purchaseData)
            {
                Economy.TotalSpentInShop += purchaseData.Price;
            }
        }

        #endregion

        #region Event Handlers - Loot

        private void OnLootPickedUp(object data)
        {
            Economy.ItemsLooted++;
            
            if (data is LootPickedUpData lootData && lootData.Item != null)
            {
                Economy.RecordItemObtained(lootData.Item.Rarity);
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
            
            if (data is CraftCompletedData craftData && craftData.Item != null)
            {
                Economy.RecordItemObtained(craftData.Item.Rarity);
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

    #region Data Structures

    /// <summary>
    /// Data for entity died event
    /// </summary>
    public class EntityDiedData
    {
        public Node3D Entity { get; set; }
        public Node3D Killer { get; set; }
    }

    /// <summary>
    /// Data for damage event
    /// </summary>
    public class DamageData
    {
        public Node Attacker { get; set; }
        public Node Victim { get; set; }
        public float Amount { get; set; }
    }

    /// <summary>
    /// Data for wave completed event
    /// </summary>
    public class WaveCompletedData
    {
        public int WaveNumber { get; set; }
    }

    /// <summary>
    /// Data for item purchase event
    /// </summary>
    public class ItemPurchaseData
    {
        public ItemBase Item { get; set; }
        public int Price { get; set; }
    }

    /// <summary>
    /// Data for loot picked up event
    /// </summary>
    public class LootPickedUpData
    {
        public ItemBase Item { get; set; }
    }

    /// <summary>
    /// Data for craft completed event
    /// </summary>
    public class CraftCompletedData
    {
        public ItemBase Item { get; set; }
    }

    #endregion
}
