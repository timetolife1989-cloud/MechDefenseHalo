using Godot;
using GdUnit4;
using MechDefenseHalo.Loot;
using static GdUnit4.Assertions;
using System.Collections.Generic;

namespace MechDefenseHalo.Tests.Loot
{
    /// <summary>
    /// Unit tests for LootTableManager system
    /// Tests loot generation and loot table operations
    /// </summary>
    [TestSuite]
    public class LootTableManagerTests
    {
        private LootTableManager _lootManager;

        [Before]
        public void Setup()
        {
            _lootManager = new LootTableManager();
        }

        [After]
        public void Teardown()
        {
            _lootManager = null;
        }

        [TestCase]
        public void GetLootTable_NonExistent_ShouldReturnNull()
        {
            // Act
            var table = LootTableManager.GetLootTable("nonexistent_enemy");

            // Assert
            AssertObject(table).IsNull();
        }

        [TestCase]
        public void RollLoot_NonExistentTable_ShouldReturnEmptyList()
        {
            // Act
            var loot = LootTableManager.RollLoot("nonexistent_enemy");

            // Assert
            AssertObject(loot).IsNotNull();
            AssertInt(loot.Count).IsEqual(0);
        }

        [TestCase]
        public void RollLoot_WithNullInstance_ShouldReturnEmptyList()
        {
            // Note: This tests static method behavior when instance is null
            // In real scenarios, instance should be initialized
            var loot = LootTableManager.RollLoot("any_enemy");

            // Assert
            AssertObject(loot).IsNotNull();
        }

        [TestCase]
        public void LootTable_Construction_ShouldHaveDefaults()
        {
            // Arrange & Act
            var table = new LootTable();

            // Assert
            AssertString(table.EnemyType).IsEqual("");
            AssertObject(table.LootPools).IsNotNull();
            AssertObject(table.GuaranteedDrops).IsNotNull();
            AssertObject(table.DropCountRange).IsNotNull();
        }

        [TestCase]
        public void LootTable_WithGuaranteedDrops_ShouldHaveItems()
        {
            // Arrange
            var table = new LootTable
            {
                EnemyType = "test_enemy",
                GuaranteedDrops = new string[] { "item1", "item2" }
            };

            // Assert
            AssertInt(table.GuaranteedDrops.Length).IsEqual(2);
        }

        [TestCase]
        public void LootPool_Construction_ShouldHaveDefaults()
        {
            // Arrange & Act
            var pool = new LootPool();

            // Assert
            AssertString(pool.PoolName).IsEqual("");
            AssertFloat(pool.Weight).IsEqual(1.0f);
            AssertObject(pool.Items).IsNotNull();
        }

        [TestCase]
        public void LootPool_WithItems_ShouldStoreItems()
        {
            // Arrange
            var pool = new LootPool
            {
                PoolName = "common_pool",
                Weight = 0.5f,
                Items = new string[] { "item1", "item2", "item3" }
            };

            // Assert
            AssertString(pool.PoolName).IsEqual("common_pool");
            AssertFloat(pool.Weight).IsEqual(0.5f);
            AssertInt(pool.Items.Length).IsEqual(3);
        }

        [TestCase]
        public void LootTable_DropCountRange_DefaultValues()
        {
            // Arrange & Act
            var table = new LootTable();

            // Assert
            AssertInt(table.DropCountRange[0]).IsEqual(1);
            AssertInt(table.DropCountRange[1]).IsEqual(3);
        }

        [TestCase]
        public void LootTable_WithCustomDropRange_ShouldStore()
        {
            // Arrange
            var table = new LootTable
            {
                DropCountRange = new int[] { 2, 5 }
            };

            // Assert
            AssertInt(table.DropCountRange[0]).IsEqual(2);
            AssertInt(table.DropCountRange[1]).IsEqual(5);
        }

        [TestCase]
        public void LootTable_CompleteConfiguration_ShouldStoreAllFields()
        {
            // Arrange
            var table = new LootTable
            {
                EnemyType = "grunt",
                LootPools = new LootPool[]
                {
                    new LootPool
                    {
                        PoolName = "common",
                        Weight = 0.7f,
                        Items = new string[] { "credits_10", "credits_20" }
                    },
                    new LootPool
                    {
                        PoolName = "rare",
                        Weight = 0.3f,
                        Items = new string[] { "rare_material" }
                    }
                },
                GuaranteedDrops = new string[] { "xp_orb" },
                DropCountRange = new int[] { 1, 2 }
            };

            // Assert
            AssertString(table.EnemyType).IsEqual("grunt");
            AssertInt(table.LootPools.Length).IsEqual(2);
            AssertInt(table.GuaranteedDrops.Length).IsEqual(1);
            AssertInt(table.DropCountRange[0]).IsEqual(1);
            AssertInt(table.DropCountRange[1]).IsEqual(2);
        }

        [TestCase]
        public void LootPool_MultipleItems_ShouldAccessByIndex()
        {
            // Arrange
            var pool = new LootPool
            {
                Items = new string[] { "item1", "item2", "item3", "item4" }
            };

            // Assert
            AssertString(pool.Items[0]).IsEqual("item1");
            AssertString(pool.Items[1]).IsEqual("item2");
            AssertString(pool.Items[2]).IsEqual("item3");
            AssertString(pool.Items[3]).IsEqual("item4");
        }

        [TestCase]
        public void LootTable_EmptyPools_ShouldBeValid()
        {
            // Arrange
            var table = new LootTable
            {
                EnemyType = "empty_enemy",
                LootPools = new LootPool[] { },
                GuaranteedDrops = new string[] { },
                DropCountRange = new int[] { 0, 0 }
            };

            // Assert
            AssertString(table.EnemyType).IsEqual("empty_enemy");
            AssertInt(table.LootPools.Length).IsEqual(0);
            AssertInt(table.GuaranteedDrops.Length).IsEqual(0);
        }

        [TestCase]
        public void LootPool_ZeroWeight_ShouldBeValid()
        {
            // Arrange
            var pool = new LootPool
            {
                PoolName = "zero_weight",
                Weight = 0.0f,
                Items = new string[] { "item1" }
            };

            // Assert
            AssertFloat(pool.Weight).IsEqual(0.0f);
        }

        [TestCase]
        public void LootPool_HighWeight_ShouldBeValid()
        {
            // Arrange
            var pool = new LootPool
            {
                PoolName = "high_weight",
                Weight = 100.0f,
                Items = new string[] { "common_item" }
            };

            // Assert
            AssertFloat(pool.Weight).IsEqual(100.0f);
        }

        [TestCase]
        public void LootTable_MultiplePools_DifferentWeights()
        {
            // Arrange
            var table = new LootTable
            {
                LootPools = new LootPool[]
                {
                    new LootPool { PoolName = "common", Weight = 0.6f },
                    new LootPool { PoolName = "uncommon", Weight = 0.3f },
                    new LootPool { PoolName = "rare", Weight = 0.1f }
                }
            };

            // Assert
            AssertInt(table.LootPools.Length).IsEqual(3);
            AssertFloat(table.LootPools[0].Weight).IsEqual(0.6f);
            AssertFloat(table.LootPools[1].Weight).IsEqual(0.3f);
            AssertFloat(table.LootPools[2].Weight).IsEqual(0.1f);
        }

        [TestCase]
        public void RollLoot_WithLuckModifier_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => LootTableManager.RollLoot("test_enemy", 1.5f))
                .Not().ThrowsException();
        }
    }
}
