using Godot;
using System.Collections.Generic;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Loot
{
    /// <summary>
    /// Main manager for loot system - spawning, rolling, and managing loot drops
    /// Singleton pattern for global access
    /// </summary>
    public partial class LootManager : Node
    {
        #region Singleton

        private static LootManager _instance;
        public static LootManager Instance => _instance;

        #endregion

        #region Exported Properties

        [Export] public PackedScene LootDropPrefab { get; set; }

        #endregion

        #region Private Fields

        private List<LootDrop> _activeLootDrops = new();
        private const int MAX_ACTIVE_DROPS = 100; // Performance limit

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple LootManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            
            // Try to load default loot drop prefab if not set
            if (LootDropPrefab == null)
            {
                // Prefab loading can be set in Godot editor or loaded at runtime
                // For now, LootDrop nodes will be created programmatically
                GD.Print("LootManager: No LootDropPrefab assigned. Loot will spawn as basic LootDrop nodes.");
            }

            GD.Print("LootManager initialized");
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods - Loot Spawning

        /// <summary>
        /// Spawn a loot drop at the specified position
        /// </summary>
        /// <param name="position">World position to spawn loot</param>
        /// <param name="itemId">Item ID to drop</param>
        /// <param name="rarity">Rarity of the item</param>
        public void SpawnLoot(Vector3 position, string itemId, ItemRarity rarity)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                GD.PrintErr("Cannot spawn loot with null or empty itemId");
                return;
            }

            // Check performance limit
            if (_activeLootDrops.Count >= MAX_ACTIVE_DROPS)
            {
                GD.Print($"Max loot drops ({MAX_ACTIVE_DROPS}) reached. Removing oldest drop.");
                RemoveOldestDrop();
            }

            LootDrop lootDrop;

            // Instantiate from prefab if available
            if (LootDropPrefab != null)
            {
                lootDrop = LootDropPrefab.Instantiate<LootDrop>();
                GetTree().Root.AddChild(lootDrop);
            }
            else
            {
                // Create basic node if prefab not available
                lootDrop = new LootDrop();
                GetTree().Root.AddChild(lootDrop);
            }

            lootDrop.GlobalPosition = position;
            lootDrop.Initialize(itemId, rarity);
            
            _activeLootDrops.Add(lootDrop);

            GD.Print($"Spawned {rarity} loot: {itemId} at {position}");
        }

        /// <summary>
        /// Spawn multiple loot items at once
        /// </summary>
        /// <param name="position">Base position for loot drops</param>
        /// <param name="lootEntries">List of items to drop with their rarities</param>
        /// <param name="scatterRadius">Radius to scatter loot within</param>
        public void SpawnMultipleLoot(Vector3 position, List<LootTableEntry> lootEntries, float scatterRadius = 2.0f)
        {
            if (lootEntries == null || lootEntries.Count == 0)
            {
                return;
            }

            for (int i = 0; i < lootEntries.Count; i++)
            {
                Vector3 spawnPos = position + GetRandomOffset(scatterRadius);
                SpawnLoot(spawnPos, lootEntries[i].ItemId, lootEntries[i].Rarity);
            }
        }

        #endregion

        #region Public Methods - Loot Rolling

        /// <summary>
        /// Roll loot from a loot table
        /// </summary>
        /// <param name="lootTable">List of possible loot entries</param>
        /// <returns>Selected item ID or null if nothing dropped</returns>
        public string RollLoot(List<LootTableEntry> lootTable)
        {
            if (lootTable == null || lootTable.Count == 0)
            {
                return null;
            }

            float roll = GD.Randf();
            float cumulative = 0;

            foreach (var entry in lootTable)
            {
                cumulative += entry.DropChance;
                if (roll <= cumulative)
                {
                    // Notify LootModifiers about the drop for bad luck protection tracking
                    LootModifiers.NotifyDrop(entry.Rarity);
                    return entry.ItemId;
                }
            }

            return null;
        }

        /// <summary>
        /// Roll multiple loot items from a table
        /// </summary>
        /// <param name="lootTable">List of possible loot entries</param>
        /// <param name="rollCount">Number of items to roll</param>
        /// <returns>List of dropped item entries</returns>
        public List<LootTableEntry> RollMultipleLoot(List<LootTableEntry> lootTable, int rollCount)
        {
            var results = new List<LootTableEntry>();

            for (int i = 0; i < rollCount; i++)
            {
                string itemId = RollLoot(lootTable);
                if (!string.IsNullOrEmpty(itemId))
                {
                    // Find the entry to get rarity
                    var entry = lootTable.Find(e => e.ItemId == itemId);
                    if (entry != null)
                    {
                        results.Add(entry);
                    }
                }
            }

            return results;
        }

        #endregion

        #region Public Methods - Loot Management

        /// <summary>
        /// Remove a loot drop from tracking
        /// </summary>
        /// <param name="lootDrop">The loot drop to remove</param>
        public void UnregisterLootDrop(LootDrop lootDrop)
        {
            _activeLootDrops.Remove(lootDrop);
        }

        /// <summary>
        /// Get all active loot drops in the world
        /// </summary>
        /// <returns>List of active loot drops</returns>
        public List<LootDrop> GetActiveLootDrops()
        {
            return new List<LootDrop>(_activeLootDrops);
        }

        /// <summary>
        /// Clear all active loot drops
        /// </summary>
        public void ClearAllLoot()
        {
            foreach (var drop in _activeLootDrops.ToArray())
            {
                if (IsInstanceValid(drop))
                {
                    drop.QueueFree();
                }
            }
            _activeLootDrops.Clear();
            GD.Print("All loot drops cleared");
        }

        #endregion

        #region Private Methods

        private Vector3 GetRandomOffset(float radius)
        {
            float angle = GD.Randf() * Mathf.Tau;
            float distance = GD.Randf() * radius;

            return new Vector3(
                Mathf.Cos(angle) * distance,
                0.5f, // Slight height offset
                Mathf.Sin(angle) * distance
            );
        }

        private void RemoveOldestDrop()
        {
            if (_activeLootDrops.Count > 0)
            {
                var oldest = _activeLootDrops[0];
                _activeLootDrops.RemoveAt(0);
                
                if (IsInstanceValid(oldest))
                {
                    oldest.QueueFree();
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Entry in a loot table with drop chance and rarity
    /// </summary>
    public class LootTableEntry
    {
        public string ItemId { get; set; }
        public float DropChance { get; set; }
        public ItemRarity Rarity { get; set; }
    }
}
