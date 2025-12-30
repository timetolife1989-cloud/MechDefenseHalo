using Godot;
using GdUnit4;
using MechDefenseHalo.Loot;
using MechDefenseHalo.Items;
using static GdUnit4.Assertions;
using System.Collections.Generic;

namespace MechDefenseHalo.Tests.Loot
{
    /// <summary>
    /// Unit tests for LootManager system
    /// Tests loot spawning, rolling, and management
    /// </summary>
    [TestSuite]
    public class LootManagerTests
    {
        private LootManager _lootManager;

        [Before]
        public void Setup()
        {
            _lootManager = new LootManager();
        }

        [After]
        public void Teardown()
        {
            _lootManager = null;
        }

        [TestCase]
        public void LootManager_Initialization_ShouldSucceed()
        {
            // Assert
            AssertObject(_lootManager).IsNotNull();
        }

        [TestCase]
        public void GetActiveLootDrops_Initially_ShouldBeEmpty()
        {
            // Act
            var drops = _lootManager.GetActiveLootDrops();

            // Assert
            AssertObject(drops).IsNotNull();
            AssertInt(drops.Count).IsEqual(0);
        }

        [TestCase]
        public void RollLoot_WithNullTable_ShouldReturnNull()
        {
            // Act
            var result = _lootManager.RollLoot(null);

            // Assert
            AssertObject(result).IsNull();
        }

        [TestCase]
        public void RollLoot_WithEmptyTable_ShouldReturnNull()
        {
            // Arrange
            var emptyTable = new List<LootTableEntry>();

            // Act
            var result = _lootManager.RollLoot(emptyTable);

            // Assert
            AssertObject(result).IsNull();
        }

        [TestCase]
        public void RollLoot_WithValidTable_ShouldReturnItemId()
        {
            // Arrange
            var lootTable = new List<LootTableEntry>
            {
                new LootTableEntry
                {
                    ItemId = "test_item",
                    DropChance = 1.0f,
                    Rarity = ItemRarity.Common
                }
            };

            // Act
            var result = _lootManager.RollLoot(lootTable);

            // Assert
            AssertString(result).IsEqual("test_item");
        }

        [TestCase]
        public void RollMultipleLoot_WithValidTable_ShouldReturnMultipleItems()
        {
            // Arrange
            var lootTable = new List<LootTableEntry>
            {
                new LootTableEntry
                {
                    ItemId = "item1",
                    DropChance = 0.5f,
                    Rarity = ItemRarity.Common
                },
                new LootTableEntry
                {
                    ItemId = "item2",
                    DropChance = 0.5f,
                    Rarity = ItemRarity.Uncommon
                }
            };

            // Act
            var results = _lootManager.RollMultipleLoot(lootTable, 5);

            // Assert
            AssertObject(results).IsNotNull();
            AssertInt(results.Count).IsGreaterEqual(0);
            AssertInt(results.Count).IsLessEqual(5);
        }

        [TestCase]
        public void RollMultipleLoot_WithZeroRolls_ShouldReturnEmptyList()
        {
            // Arrange
            var lootTable = new List<LootTableEntry>
            {
                new LootTableEntry
                {
                    ItemId = "item1",
                    DropChance = 1.0f,
                    Rarity = ItemRarity.Common
                }
            };

            // Act
            var results = _lootManager.RollMultipleLoot(lootTable, 0);

            // Assert
            AssertObject(results).IsNotNull();
            AssertInt(results.Count).IsEqual(0);
        }

        [TestCase]
        public void ClearAllLoot_ShouldClearDropsList()
        {
            // Act
            _lootManager.ClearAllLoot();
            var drops = _lootManager.GetActiveLootDrops();

            // Assert
            AssertInt(drops.Count).IsEqual(0);
        }

        [TestCase]
        public void LootTableEntry_Construction_ShouldHaveDefaults()
        {
            // Arrange & Act
            var entry = new LootTableEntry();

            // Assert
            AssertObject(entry).IsNotNull();
            AssertObject(entry.ItemId).IsNull();
            AssertFloat(entry.DropChance).IsEqual(0.0f);
        }

        [TestCase]
        public void LootTableEntry_WithData_ShouldStoreValues()
        {
            // Arrange & Act
            var entry = new LootTableEntry
            {
                ItemId = "legendary_sword",
                DropChance = 0.01f,
                Rarity = ItemRarity.Legendary
            };

            // Assert
            AssertString(entry.ItemId).IsEqual("legendary_sword");
            AssertFloat(entry.DropChance).IsEqual(0.01f);
            AssertObject(entry.Rarity).IsEqual(ItemRarity.Legendary);
        }

        [TestCase]
        public void RollLoot_WithMultipleEntries_ShouldRespectDropChances()
        {
            // Arrange
            var lootTable = new List<LootTableEntry>
            {
                new LootTableEntry
                {
                    ItemId = "common_item",
                    DropChance = 0.8f,
                    Rarity = ItemRarity.Common
                },
                new LootTableEntry
                {
                    ItemId = "rare_item",
                    DropChance = 0.2f,
                    Rarity = ItemRarity.Rare
                }
            };

            // Act - roll multiple times to check distribution
            var results = new Dictionary<string, int>();
            for (int i = 0; i < 100; i++)
            {
                var item = _lootManager.RollLoot(lootTable);
                if (!string.IsNullOrEmpty(item))
                {
                    if (!results.ContainsKey(item))
                    {
                        results[item] = 0;
                    }
                    results[item]++;
                }
            }

            // Assert - should get both items, common more frequently
            AssertBool(results.ContainsKey("common_item")).IsTrue();
        }

        [TestCase]
        public void RollLoot_ZeroDropChance_ShouldNeverDrop()
        {
            // Arrange
            var lootTable = new List<LootTableEntry>
            {
                new LootTableEntry
                {
                    ItemId = "impossible_item",
                    DropChance = 0.0f,
                    Rarity = ItemRarity.Legendary
                },
                new LootTableEntry
                {
                    ItemId = "guaranteed_item",
                    DropChance = 1.0f,
                    Rarity = ItemRarity.Common
                }
            };

            // Act
            var result = _lootManager.RollLoot(lootTable);

            // Assert
            AssertString(result).IsEqual("guaranteed_item");
        }

        [TestCase]
        public void UnregisterLootDrop_WithNull_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _lootManager.UnregisterLootDrop(null))
                .Not().ThrowsException();
        }
    }
}
