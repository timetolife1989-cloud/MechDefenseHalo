using Godot;
using GdUnit4;
using MechDefenseHalo.Loot;
using MechDefenseHalo.Items;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Loot
{
    /// <summary>
    /// Unit tests for LootTable data structures
    /// </summary>
    [TestSuite]
    public class LootTableTests
    {
        [TestCase]
        public void LootTable_Construction_ShouldHaveDefaults()
        {
            // Arrange & Act
            var table = new LootTableDefinition();

            // Assert
            AssertString(table.TableId).IsEqual("");
            AssertString(table.DisplayName).IsEqual("");
            AssertObject(table.Entries).IsNotNull();
            AssertObject(table.GuaranteedDrops).IsNotNull();
            AssertInt(table.MinDrops).IsEqual(0);
            AssertInt(table.MaxDrops).IsEqual(1);
            AssertFloat(table.LuckModifier).IsEqual(1.0f);
        }

        [TestCase]
        public void LootTable_WithData_ShouldStoreValues()
        {
            // Arrange & Act
            var table = new LootTableDefinition
            {
                TableId = "test_table",
                DisplayName = "Test Table",
                MinDrops = 2,
                MaxDrops = 5,
                LuckModifier = 1.5f
            };

            // Assert
            AssertString(table.TableId).IsEqual("test_table");
            AssertString(table.DisplayName).IsEqual("Test Table");
            AssertInt(table.MinDrops).IsEqual(2);
            AssertInt(table.MaxDrops).IsEqual(5);
            AssertFloat(table.LuckModifier).IsEqual(1.5f);
        }

        [TestCase]
        public void LootTable_CreateTestTable_ShouldReturnValidTable()
        {
            // Act
            var table = LootTableDefinition.CreateTestTable("test_enemy");

            // Assert
            AssertObject(table).IsNotNull();
            AssertString(table.TableId).IsEqual("test_enemy");
            AssertInt(table.Entries.Count).IsGreater(0);
        }

        [TestCase]
        public void LootTable_CreateForEnemy_ShouldScaleWithLevel()
        {
            // Act
            var table1 = LootTableDefinition.CreateForEnemy("grunt", 1);
            var table2 = LootTableDefinition.CreateForEnemy("grunt", 10);

            // Assert
            AssertObject(table1).IsNotNull();
            AssertObject(table2).IsNotNull();
            AssertInt(table2.MaxDrops).IsGreaterEqual(table1.MaxDrops);
        }

        [TestCase]
        public void LootTableBuilder_BasicBuild_ShouldCreateTable()
        {
            // Arrange & Act
            var table = new LootTableBuilder("test")
                .WithDisplayName("Test Table")
                .WithDropCount(1, 3)
                .Build();

            // Assert
            AssertObject(table).IsNotNull();
            AssertString(table.TableId).IsEqual("test");
            AssertString(table.DisplayName).IsEqual("Test Table");
            AssertInt(table.MinDrops).IsEqual(1);
            AssertInt(table.MaxDrops).IsEqual(3);
        }

        [TestCase]
        public void LootTableBuilder_AddEntry_ShouldAddToEntries()
        {
            // Arrange & Act
            var table = new LootTableBuilder("test")
                .AddEntry("item1", 0.5f, ItemRarity.Common)
                .AddEntry("item2", 0.3f, ItemRarity.Rare)
                .Build();

            // Assert
            AssertInt(table.Entries.Count).IsEqual(2);
            AssertString(table.Entries[0].ItemId).IsEqual("item1");
            AssertString(table.Entries[1].ItemId).IsEqual("item2");
        }

        [TestCase]
        public void LootTableBuilder_AddGuaranteedDrop_ShouldAddToList()
        {
            // Arrange & Act
            var table = new LootTableBuilder("test")
                .AddGuaranteedDrop("xp_orb")
                .AddGuaranteedDrop("credits")
                .Build();

            // Assert
            AssertInt(table.GuaranteedDrops.Count).IsEqual(2);
            AssertString(table.GuaranteedDrops[0]).IsEqual("xp_orb");
            AssertString(table.GuaranteedDrops[1]).IsEqual("credits");
        }

        [TestCase]
        public void LootTableBuilder_WithLuckModifier_ShouldSetModifier()
        {
            // Arrange & Act
            var table = new LootTableBuilder("test")
                .WithLuckModifier(2.0f)
                .Build();

            // Assert
            AssertFloat(table.LuckModifier).IsEqual(2.0f);
        }

        [TestCase]
        public void LootTableBuilder_ChainedCalls_ShouldWork()
        {
            // Arrange & Act
            var table = new LootTableBuilder("full_test")
                .WithDisplayName("Full Test")
                .WithDropCount(2, 4)
                .AddEntry("common", 0.6f, ItemRarity.Common)
                .AddEntry("rare", 0.3f, ItemRarity.Rare)
                .AddEntry("legendary", 0.1f, ItemRarity.Legendary)
                .AddGuaranteedDrop("xp")
                .WithLuckModifier(1.5f)
                .Build();

            // Assert
            AssertString(table.TableId).IsEqual("full_test");
            AssertString(table.DisplayName).IsEqual("Full Test");
            AssertInt(table.MinDrops).IsEqual(2);
            AssertInt(table.MaxDrops).IsEqual(4);
            AssertInt(table.Entries.Count).IsEqual(3);
            AssertInt(table.GuaranteedDrops.Count).IsEqual(1);
            AssertFloat(table.LuckModifier).IsEqual(1.5f);
        }

        [TestCase]
        public void LootTableExtensions_GetTotalDropChance_ShouldSumAllEntries()
        {
            // Arrange
            var table = new LootTableDefinition();
            table.Entries.Add(new LootTableEntry { DropChance = 0.3f });
            table.Entries.Add(new LootTableEntry { DropChance = 0.5f });
            table.Entries.Add(new LootTableEntry { DropChance = 0.2f });

            // Act
            float total = table.GetTotalDropChance();

            // Assert
            AssertFloat(total).IsEqual(1.0f);
        }

        [TestCase]
        public void LootTableExtensions_GetEntriesByRarity_ShouldFilterCorrectly()
        {
            // Arrange
            var table = new LootTableDefinition();
            table.Entries.Add(new LootTableEntry { ItemId = "common1", Rarity = ItemRarity.Common });
            table.Entries.Add(new LootTableEntry { ItemId = "rare1", Rarity = ItemRarity.Rare });
            table.Entries.Add(new LootTableEntry { ItemId = "common2", Rarity = ItemRarity.Common });

            // Act
            var commonItems = table.GetEntriesByRarity(ItemRarity.Common);
            var rareItems = table.GetEntriesByRarity(ItemRarity.Rare);

            // Assert
            AssertInt(commonItems.Count).IsEqual(2);
            AssertInt(rareItems.Count).IsEqual(1);
        }

        [TestCase]
        public void LootTableExtensions_GetRarestEntry_ShouldReturnHighestRarity()
        {
            // Arrange
            var table = new LootTableDefinition();
            table.Entries.Add(new LootTableEntry { ItemId = "common", Rarity = ItemRarity.Common });
            table.Entries.Add(new LootTableEntry { ItemId = "legendary", Rarity = ItemRarity.Legendary });
            table.Entries.Add(new LootTableEntry { ItemId = "rare", Rarity = ItemRarity.Rare });

            // Act
            var rarest = table.GetRarestEntry();

            // Assert
            AssertString(rarest.ItemId).IsEqual("legendary");
            AssertObject(rarest.Rarity).IsEqual(ItemRarity.Legendary);
        }

        [TestCase]
        public void LootTableExtensions_GetRarestEntry_EmptyTable_ShouldReturnNull()
        {
            // Arrange
            var table = new LootTableDefinition();

            // Act
            var rarest = table.GetRarestEntry();

            // Assert
            AssertObject(rarest).IsNull();
        }

        [TestCase]
        public void LootTableExtensions_NormalizeDropChances_ShouldSumToOne()
        {
            // Arrange
            var table = new LootTableDefinition();
            table.Entries.Add(new LootTableEntry { DropChance = 50f });
            table.Entries.Add(new LootTableEntry { DropChance = 30f });
            table.Entries.Add(new LootTableEntry { DropChance = 20f });

            // Act
            table.NormalizeDropChances();
            float total = table.GetTotalDropChance();

            // Assert
            AssertFloat(total).IsEqual(1.0f);
        }

        [TestCase]
        public void LootGroup_Construction_ShouldHaveDefaults()
        {
            // Arrange & Act
            var group = new LootGroup();

            // Assert
            AssertString(group.GroupName).IsEqual("");
            AssertFloat(group.Weight).IsEqual(1.0f);
            AssertObject(group.Items).IsNotNull();
            AssertBool(group.IsGuaranteed).IsFalse();
        }

        [TestCase]
        public void LootGroup_WithData_ShouldStoreValues()
        {
            // Arrange & Act
            var group = new LootGroup
            {
                GroupName = "rare_drops",
                Weight = 0.2f,
                IsGuaranteed = true
            };

            // Assert
            AssertString(group.GroupName).IsEqual("rare_drops");
            AssertFloat(group.Weight).IsEqual(0.2f);
            AssertBool(group.IsGuaranteed).IsTrue();
        }
    }
}
