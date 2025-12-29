using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Core
{
    /// <summary>
    /// Central event bus for decoupled communication between systems.
    /// Implements singleton pattern for global access.
    /// </summary>
    public partial class EventBus : Node
    {
        #region Singleton

        private static EventBus _instance;
        
        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("EventBus accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Private Fields

        private Dictionary<string, List<Action<object>>> _eventListeners = new Dictionary<string, List<Action<object>>>();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple EventBus instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            GD.Print("EventBus initialized successfully");
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
        /// Subscribe to an event
        /// </summary>
        /// <param name="eventName">Name of the event to listen for</param>
        /// <param name="callback">Callback function to execute when event fires</param>
        public static void On(string eventName, Action<object> callback)
        {
            if (Instance == null) return;

            if (!Instance._eventListeners.ContainsKey(eventName))
            {
                Instance._eventListeners[eventName] = new List<Action<object>>();
            }

            if (!Instance._eventListeners[eventName].Contains(callback))
            {
                Instance._eventListeners[eventName].Add(callback);
            }
        }

        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        /// <param name="eventName">Name of the event to stop listening to</param>
        /// <param name="callback">Callback function to remove</param>
        public static void Off(string eventName, Action<object> callback)
        {
            if (Instance == null) return;

            if (Instance._eventListeners.ContainsKey(eventName))
            {
                Instance._eventListeners[eventName].Remove(callback);
            }
        }

        /// <summary>
        /// Emit an event to all subscribers
        /// </summary>
        /// <param name="eventName">Name of the event to emit</param>
        /// <param name="data">Optional data to pass to listeners</param>
        public static void Emit(string eventName, object data = null)
        {
            if (Instance == null) return;

            if (Instance._eventListeners.ContainsKey(eventName))
            {
                // Create a copy to avoid modification during iteration
                var listeners = new List<Action<object>>(Instance._eventListeners[eventName]);
                
                foreach (var listener in listeners)
                {
                    try
                    {
                        listener?.Invoke(data);
                    }
                    catch (Exception e)
                    {
                        GD.PrintErr($"Error in event listener for '{eventName}': {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Remove all listeners for a specific event
        /// </summary>
        /// <param name="eventName">Name of the event to clear</param>
        public static void ClearEvent(string eventName)
        {
            if (Instance == null) return;

            if (Instance._eventListeners.ContainsKey(eventName))
            {
                Instance._eventListeners[eventName].Clear();
            }
        }

        /// <summary>
        /// Remove all event listeners
        /// </summary>
        public static void ClearAll()
        {
            if (Instance == null) return;

            Instance._eventListeners.Clear();
            GD.Print("All event listeners cleared");
        }

        #endregion

        #region Common Event Names

        // Health and Combat
        public const string HealthChanged = "health_changed";
        public const string EntityDied = "entity_died";
        public const string DamageDealt = "damage_dealt";
        
        // Weapons
        public const string WeaponFired = "weapon_fired";
        public const string WeaponReloaded = "weapon_reloaded";
        public const string WeaponSwitched = "weapon_switched";
        
        // Drones
        public const string DroneDeployed = "drone_deployed";
        public const string DroneRecalled = "drone_recalled";
        public const string DroneDestroyed = "drone_destroyed";
        
        // Wave System
        public const string WaveStarted = "wave_started";
        public const string WaveCompleted = "wave_completed";
        public const string AllWavesCompleted = "all_waves_completed";
        
        // Boss
        public const string BossSpawned = "boss_spawned";
        public const string BossPhaseChanged = "boss_phase_changed";
        public const string BossDefeated = "boss_defeated";
        
        // Game State
        public const string GameStarted = "game_started";
        public const string GamePaused = "game_paused";
        public const string GameOver = "game_over";
        public const string PlayerDied = "player_died";
        
        // Resources
        public const string EnergyChanged = "energy_changed";
        public const string AmmoChanged = "ammo_changed";
        
        // Loot Events
        public const string LootDropped = "loot_dropped";
        public const string LootPickedUp = "loot_picked_up";
        public const string RareItemDropped = "rare_item_dropped"; // Legendary+
        
        // Inventory Events
        public const string InventoryChanged = "inventory_changed";
        public const string ItemEquipped = "item_equipped";
        public const string ItemUnequipped = "item_unequipped";
        
        // Crafting Events
        public const string CraftStarted = "craft_started";
        public const string CraftCompleted = "craft_completed";
        public const string BlueprintUnlocked = "blueprint_unlocked";
        
        // Set Events
        public const string SetBonusActivated = "set_bonus_activated";
        public const string SetBonusDeactivated = "set_bonus_deactivated";
        
        // Economy Events
        public const string CurrencyChanged = "currency_changed";
        public const string ItemPurchased = "item_purchased";
        public const string ItemSold = "item_sold";
        
        // Monetization Events
        public const string AdOffered = "ad_offered";
        public const string AdWatched = "ad_watched";
        public const string AdSkipped = "ad_skipped";
        public const string ShowVictoryBonusOffer = "show_victory_bonus_offer";
        public const string ShowMilestoneRewardOffer = "show_milestone_reward_offer";
        public const string ShowDailyLoginOffer = "show_daily_login_offer";
        public const string ShowConsentDialog = "show_consent_dialog";
        public const string ConsentStatusChanged = "consent_status_changed";
        
        // Tutorial Events
        public const string TutorialStarted = "tutorial_started";
        public const string TutorialCompleted = "tutorial_completed";
        public const string TutorialStopped = "tutorial_stopped";
        public const string TutorialStepStarted = "tutorial_step_started";
        public const string TutorialObjectiveComplete = "tutorial_objective_complete";

        #endregion
    }
}
