using Godot;
using System.Collections.Generic;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Loot
{
    /// <summary>
    /// Defines a loot table structure for organizing item drops
    /// This file provides the data structures used by LootManager and LootTableManager
    /// </summary>
    
    /// <summary>
    /// Complete loot table definition for an enemy or loot source
    /// Note: This is different from the LootTable class in LootTableManager which is for JSON deserialization
    /// </summary>
    public class LootTableDefinition
    {
        /// <summary>
        /// Identifier for this loot table (e.g., "grunt", "elite", "boss_hunter")
        /// </summary>
        public string TableId { get; set; } = "";

        /// <summary>
        /// Display name for this loot table
        /// </summary>
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// List of possible loot entries with drop chances
        /// </summary>
        public List<LootTableEntry> Entries { get; set; } = new();

        /// <summary>
        /// Minimum number of items to drop
        /// </summary>
        public int MinDrops { get; set; } = 0;

        /// <summary>
        /// Maximum number of items to drop
        /// </summary>
        public int MaxDrops { get; set; } = 1;

        /// <summary>
        /// Guaranteed drops that always occur
        /// </summary>
        public List<string> GuaranteedDrops { get; set; } = new();

        /// <summary>
        /// Luck modifier for this loot table
        /// </summary>
        public float LuckModifier { get; set; } = 1.0f;

        /// <summary>
        /// Create a loot table for testing/debugging
        /// </summary>
        public static LootTableDefinition CreateTestTable(string tableId)
        {
            return new LootTableDefinition
            {
                TableId = tableId,
                DisplayName = $"Test Table: {tableId}",
                Entries = new List<LootTableEntry>
                {
                    new LootTableEntry
                    {
                        ItemId = "credits_small",
                        DropChance = 0.6f,
                        Rarity = ItemRarity.Common
                    },
                    new LootTableEntry
                    {
                        ItemId = "health_pickup",
                        DropChance = 0.25f,
                        Rarity = ItemRarity.Uncommon
                    },
                    new LootTableEntry
                    {
                        ItemId = "rare_material",
                        DropChance = 0.1f,
                        Rarity = ItemRarity.Rare
                    },
                    new LootTableEntry
                    {
                        ItemId = "epic_weapon_mod",
                        DropChance = 0.04f,
                        Rarity = ItemRarity.Epic
                    },
                    new LootTableEntry
                    {
                        ItemId = "legendary_item",
                        DropChance = 0.01f,
                        Rarity = ItemRarity.Legendary
                    }
                },
                MinDrops = 1,
                MaxDrops = 3
            };
        }

        /// <summary>
        /// Create a loot table from enemy type
        /// </summary>
        public static LootTableDefinition CreateForEnemy(string enemyType, int enemyLevel)
        {
            var table = new LootTableDefinition
            {
                TableId = enemyType,
                DisplayName = $"{enemyType} Loot"
            };

            // Scale drops based on enemy level
            table.MinDrops = Mathf.Max(1, enemyLevel / 5);
            table.MaxDrops = Mathf.Max(2, enemyLevel / 3);

            // Create scaled entries
            table.Entries = new List<LootTableEntry>
            {
                new LootTableEntry
                {
                    ItemId = $"credits_{enemyLevel}",
                    DropChance = 0.7f,
                    Rarity = ItemRarity.Common
                },
                new LootTableEntry
                {
                    ItemId = $"material_{enemyLevel}",
                    DropChance = 0.2f,
                    Rarity = ItemRarity.Uncommon
                },
                new LootTableEntry
                {
                    ItemId = $"upgrade_{enemyLevel}",
                    DropChance = 0.08f,
                    Rarity = ItemRarity.Rare
                },
                new LootTableEntry
                {
                    ItemId = $"weapon_{enemyLevel}",
                    DropChance = 0.02f,
                    Rarity = ItemRarity.Epic
                }
            };

            return table;
        }
    }

    /// <summary>
    /// Weighted group of items in a loot table
    /// </summary>
    public class LootGroup
    {
        /// <summary>
        /// Name of this loot group
        /// </summary>
        public string GroupName { get; set; } = "";

        /// <summary>
        /// Weight for selecting this group (higher = more likely)
        /// </summary>
        public float Weight { get; set; } = 1.0f;

        /// <summary>
        /// Items in this group
        /// </summary>
        public List<LootTableEntry> Items { get; set; } = new();

        /// <summary>
        /// Whether this group guarantees a drop
        /// </summary>
        public bool IsGuaranteed { get; set; } = false;
    }

    /// <summary>
    /// Builder for creating loot tables fluently
    /// </summary>
    public class LootTableBuilder
    {
        private LootTableDefinition _table;

        public LootTableBuilder(string tableId)
        {
            _table = new LootTableDefinition { TableId = tableId };
        }

        public LootTableBuilder WithDisplayName(string displayName)
        {
            _table.DisplayName = displayName;
            return this;
        }

        public LootTableBuilder WithDropCount(int min, int max)
        {
            _table.MinDrops = min;
            _table.MaxDrops = max;
            return this;
        }

        public LootTableBuilder AddEntry(string itemId, float dropChance, ItemRarity rarity)
        {
            _table.Entries.Add(new LootTableEntry
            {
                ItemId = itemId,
                DropChance = dropChance,
                Rarity = rarity
            });
            return this;
        }

        public LootTableBuilder AddGuaranteedDrop(string itemId)
        {
            _table.GuaranteedDrops.Add(itemId);
            return this;
        }

        public LootTableBuilder WithLuckModifier(float modifier)
        {
            _table.LuckModifier = modifier;
            return this;
        }

        public LootTableDefinition Build()
        {
            return _table;
        }
    }

    /// <summary>
    /// Extension methods for loot tables
    /// </summary>
    public static class LootTableExtensions
    {
        /// <summary>
        /// Get total drop chance across all entries
        /// </summary>
        public static float GetTotalDropChance(this LootTableDefinition table)
        {
            float total = 0f;
            foreach (var entry in table.Entries)
            {
                total += entry.DropChance;
            }
            return total;
        }

        /// <summary>
        /// Normalize drop chances to sum to 1.0
        /// </summary>
        public static void NormalizeDropChances(this LootTableDefinition table)
        {
            float total = table.GetTotalDropChance();
            if (total > 0)
            {
                foreach (var entry in table.Entries)
                {
                    entry.DropChance /= total;
                }
            }
        }

        /// <summary>
        /// Get entries of a specific rarity
        /// </summary>
        public static List<LootTableEntry> GetEntriesByRarity(this LootTableDefinition table, ItemRarity rarity)
        {
            var results = new List<LootTableEntry>();
            foreach (var entry in table.Entries)
            {
                if (entry.Rarity == rarity)
                {
                    results.Add(entry);
                }
            }
            return results;
        }

        /// <summary>
        /// Get the rarest item in the table
        /// </summary>
        public static LootTableEntry GetRarestEntry(this LootTableDefinition table)
        {
            LootTableEntry rarest = null;
            foreach (var entry in table.Entries)
            {
                if (rarest == null || entry.Rarity > rarest.Rarity)
                {
                    rarest = entry;
                }
            }
            return rarest;
        }
    }
}
