using Godot;
using GdUnit4;
using MechDefenseHalo.Inventory;
using MechDefenseHalo.Items;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Inventory
{
    /// <summary>
    /// Unit tests for InventoryManager system
    /// Tests inventory operations like add, remove, stack, and query
    /// </summary>
    [TestSuite]
    public class InventoryManagerTests
    {
        private InventoryManager _inventory;

        [Before]
        public void Setup()
        {
            _inventory = new InventoryManager();
        }

        [After]
        public void Teardown()
        {
            _inventory = null;
        }

        [TestCase]
        public void AddItem_WithValidItem_ShouldReturnTrue()
        {
            // Arrange
            var item = CreateTestItem("test_item");

            // Act
            var result = _inventory.AddItem(item, 1);

            // Assert
            AssertBool(result).IsTrue();
        }

        [TestCase]
        public void AddItem_WithNullItem_ShouldReturnFalse()
        {
            // Act
            var result = _inventory.AddItem(null, 1);

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void AddItem_WithZeroQuantity_ShouldReturnFalse()
        {
            // Arrange
            var item = CreateTestItem("test_item");

            // Act
            var result = _inventory.AddItem(item, 0);

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void AddItem_StackableItems_ShouldStack()
        {
            // Arrange
            var item = CreateTestItem("potion", maxStack: 10);

            // Act
            _inventory.AddItem(item, 5);
            _inventory.AddItem(item, 3);

            // Assert
            var quantity = _inventory.GetItemQuantity("potion");
            AssertInt(quantity).IsEqual(8);
        }

        [TestCase]
        public void AddItem_StackableItemsExceedMax_ShouldCreateNewStack()
        {
            // Arrange
            var item = CreateTestItem("potion", maxStack: 5);

            // Act
            _inventory.AddItem(item, 5);
            var result = _inventory.AddItem(item, 3);

            // Assert
            AssertBool(result).IsTrue();
            var quantity = _inventory.GetItemQuantity("potion");
            AssertInt(quantity).IsGreaterEqual(8);
        }

        [TestCase]
        public void RemoveItem_ExistingItem_ShouldReturnTrue()
        {
            // Arrange
            var item = CreateTestItem("test_item");
            _inventory.AddItem(item, 5);

            // Act
            var result = _inventory.RemoveItem("test_item", 3);

            // Assert
            AssertBool(result).IsTrue();
            var remaining = _inventory.GetItemQuantity("test_item");
            AssertInt(remaining).IsEqual(2);
        }

        [TestCase]
        public void RemoveItem_NonExistentItem_ShouldReturnFalse()
        {
            // Act
            var result = _inventory.RemoveItem("nonexistent", 1);

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void RemoveItem_InsufficientQuantity_ShouldReturnFalse()
        {
            // Arrange
            var item = CreateTestItem("test_item");
            _inventory.AddItem(item, 2);

            // Act
            var result = _inventory.RemoveItem("test_item", 5);

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void RemoveItem_AllQuantity_ShouldRemoveFromInventory()
        {
            // Arrange
            var item = CreateTestItem("test_item");
            _inventory.AddItem(item, 3);

            // Act
            _inventory.RemoveItem("test_item", 3);

            // Assert
            var hasItem = _inventory.HasItem("test_item");
            AssertBool(hasItem).IsFalse();
        }

        [TestCase]
        public void HasItem_ExistingItem_ShouldReturnTrue()
        {
            // Arrange
            var item = CreateTestItem("test_item");
            _inventory.AddItem(item, 1);

            // Act & Assert
            AssertBool(_inventory.HasItem("test_item")).IsTrue();
        }

        [TestCase]
        public void HasItem_NonExistentItem_ShouldReturnFalse()
        {
            // Act & Assert
            AssertBool(_inventory.HasItem("nonexistent")).IsFalse();
        }

        [TestCase]
        public void HasItem_WithQuantity_ShouldCheckAmount()
        {
            // Arrange
            var item = CreateTestItem("test_item");
            _inventory.AddItem(item, 5);

            // Act & Assert
            AssertBool(_inventory.HasItem("test_item", 5)).IsTrue();
            AssertBool(_inventory.HasItem("test_item", 10)).IsFalse();
        }

        [TestCase]
        public void GetItemQuantity_ExistingItem_ShouldReturnCorrectAmount()
        {
            // Arrange
            var item = CreateTestItem("test_item");
            _inventory.AddItem(item, 7);

            // Act
            var quantity = _inventory.GetItemQuantity("test_item");

            // Assert
            AssertInt(quantity).IsEqual(7);
        }

        [TestCase]
        public void GetItemQuantity_NonExistentItem_ShouldReturnZero()
        {
            // Act
            var quantity = _inventory.GetItemQuantity("nonexistent");

            // Assert
            AssertInt(quantity).IsEqual(0);
        }

        [TestCase]
        public void GetItem_ExistingItem_ShouldReturnItem()
        {
            // Arrange
            var item = CreateTestItem("test_item");
            _inventory.AddItem(item, 1);

            // Act
            var retrieved = _inventory.GetItem("test_item");

            // Assert
            AssertObject(retrieved).IsNotNull();
            AssertString(retrieved.ItemID).IsEqual("test_item");
        }

        [TestCase]
        public void GetItem_NonExistentItem_ShouldReturnNull()
        {
            // Act
            var retrieved = _inventory.GetItem("nonexistent");

            // Assert
            AssertObject(retrieved).IsNull();
        }

        [TestCase]
        public void GetAllItems_EmptyInventory_ShouldReturnEmptyList()
        {
            // Act
            var items = _inventory.GetAllItems();

            // Assert
            AssertInt(items.Count).IsEqual(0);
        }

        [TestCase]
        public void GetAllItems_WithItems_ShouldReturnAllStacks()
        {
            // Arrange
            _inventory.AddItem(CreateTestItem("item1"), 1);
            _inventory.AddItem(CreateTestItem("item2"), 2);
            _inventory.AddItem(CreateTestItem("item3"), 3);

            // Act
            var items = _inventory.GetAllItems();

            // Assert
            AssertInt(items.Count).IsEqual(3);
        }

        [TestCase]
        public void ClearInventory_ShouldRemoveAllItems()
        {
            // Arrange
            _inventory.AddItem(CreateTestItem("item1"), 1);
            _inventory.AddItem(CreateTestItem("item2"), 2);

            // Act
            _inventory.ClearInventory();

            // Assert
            var items = _inventory.GetAllItems();
            AssertInt(items.Count).IsEqual(0);
        }

        [TestCase]
        public void ExpandInventory_ShouldIncreaseMaxSlots()
        {
            // Arrange
            int initialMax = _inventory.MaxSlots;

            // Act
            _inventory.ExpandInventory(50);

            // Assert
            AssertInt(_inventory.MaxSlots).IsEqual(initialMax + 50);
        }

        // Helper methods
        private ItemBase CreateTestItem(string id, int maxStack = 1)
        {
            return new ConsumableItem
            {
                ItemID = id,
                DisplayName = id,
                MaxStackSize = maxStack,
                Rarity = ItemRarity.Common,
                SellValue = 10
            };
        }
    }
}
