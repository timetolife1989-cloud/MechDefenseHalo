using Godot;
using GdUnit4;
using MechDefenseHalo.Inventory;
using MechDefenseHalo.Items;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Inventory
{
    /// <summary>
    /// Unit tests for EquipmentManager system
    /// Tests equipment operations like equip, unequip, and loadout management
    /// </summary>
    [TestSuite]
    public class EquipmentManagerTests
    {
        private EquipmentManager _equipmentManager;
        private InventoryManager _inventory;

        [Before]
        public void Setup()
        {
            _equipmentManager = new EquipmentManager();
            _inventory = new InventoryManager();
        }

        [After]
        public void Teardown()
        {
            _equipmentManager = null;
            _inventory = null;
        }

        [TestCase]
        public void EquipItem_WithValidItem_ShouldReturnPreviousItem()
        {
            // Arrange
            var item = CreateTestMechPart("head_armor");

            // Act
            var previous = _equipmentManager.EquipItem(EquipmentSlot.Head, item);

            // Assert - first equip should return null
            AssertObject(previous).IsNull();
        }

        [TestCase]
        public void EquipItem_WithNullItem_ShouldReturnNull()
        {
            // Act
            var result = _equipmentManager.EquipItem(EquipmentSlot.Head, null);

            // Assert
            AssertObject(result).IsNull();
        }

        [TestCase]
        public void EquipItem_ReplaceExisting_ShouldReturnOldItem()
        {
            // Arrange
            var firstItem = CreateTestMechPart("head_armor_1");
            var secondItem = CreateTestMechPart("head_armor_2");
            _equipmentManager.EquipItem(EquipmentSlot.Head, firstItem);

            // Act
            var previous = _equipmentManager.EquipItem(EquipmentSlot.Head, secondItem);

            // Assert
            AssertObject(previous).IsNotNull();
            AssertString(previous.ItemID).IsEqual("head_armor_1");
        }

        [TestCase]
        public void UnequipItem_WithEquippedItem_ShouldReturnItem()
        {
            // Arrange
            var item = CreateTestMechPart("head_armor");
            _equipmentManager.EquipItem(EquipmentSlot.Head, item);

            // Act
            var unequipped = _equipmentManager.UnequipItem(EquipmentSlot.Head);

            // Assert
            AssertObject(unequipped).IsNotNull();
            AssertString(unequipped.ItemID).IsEqual("head_armor");
        }

        [TestCase]
        public void UnequipItem_WithEmptySlot_ShouldReturnNull()
        {
            // Act
            var unequipped = _equipmentManager.UnequipItem(EquipmentSlot.Head);

            // Assert
            AssertObject(unequipped).IsNull();
        }

        [TestCase]
        public void GetEquippedItem_WithEquippedItem_ShouldReturnItem()
        {
            // Arrange
            var item = CreateTestMechPart("head_armor");
            _equipmentManager.EquipItem(EquipmentSlot.Head, item);

            // Act
            var equipped = _equipmentManager.GetEquippedItem(EquipmentSlot.Head);

            // Assert
            AssertObject(equipped).IsNotNull();
            AssertString(equipped.ItemID).IsEqual("head_armor");
        }

        [TestCase]
        public void GetEquippedItem_WithEmptySlot_ShouldReturnNull()
        {
            // Act
            var equipped = _equipmentManager.GetEquippedItem(EquipmentSlot.Head);

            // Assert
            AssertObject(equipped).IsNull();
        }

        [TestCase]
        public void GetAllEquippedItems_WithMultipleItems_ShouldReturnAll()
        {
            // Arrange
            _equipmentManager.EquipItem(EquipmentSlot.Head, CreateTestMechPart("head_armor"));
            _equipmentManager.EquipItem(EquipmentSlot.Torso, CreateTestMechPart("torso_armor"));
            _equipmentManager.EquipItem(EquipmentSlot.Arms, CreateTestMechPart("arm_armor"));

            // Act
            var allItems = _equipmentManager.GetAllEquippedItems();

            // Assert
            AssertInt(allItems.Count).IsGreaterEqual(3);
        }

        [TestCase]
        public void UnequipAll_WithEquippedItems_ShouldReturnAllItems()
        {
            // Arrange
            _equipmentManager.EquipItem(EquipmentSlot.Head, CreateTestMechPart("head_armor"));
            _equipmentManager.EquipItem(EquipmentSlot.Torso, CreateTestMechPart("torso_armor"));

            // Act
            var unequipped = _equipmentManager.UnequipAll();

            // Assert
            AssertInt(unequipped.Count).IsGreaterEqual(2);
        }

        [TestCase]
        public void UnequipAll_ShouldClearAllSlots()
        {
            // Arrange
            _equipmentManager.EquipItem(EquipmentSlot.Head, CreateTestMechPart("head_armor"));
            _equipmentManager.EquipItem(EquipmentSlot.Torso, CreateTestMechPart("torso_armor"));

            // Act
            _equipmentManager.UnequipAll();

            // Assert
            var head = _equipmentManager.GetEquippedItem(EquipmentSlot.Head);
            var torso = _equipmentManager.GetEquippedItem(EquipmentSlot.Torso);
            AssertObject(head).IsNull();
            AssertObject(torso).IsNull();
        }

        [TestCase]
        public void SaveLoadout_WithValidID_ShouldReturnTrue()
        {
            // Arrange
            _equipmentManager.EquipItem(EquipmentSlot.Head, CreateTestMechPart("head_armor"));

            // Act
            var result = _equipmentManager.SaveLoadout(0);

            // Assert
            AssertBool(result).IsTrue();
        }

        [TestCase]
        public void SaveLoadout_WithInvalidID_ShouldReturnFalse()
        {
            // Act
            var result = _equipmentManager.SaveLoadout(-1);

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void SaveLoadout_WithTooHighID_ShouldReturnFalse()
        {
            // Act
            var result = _equipmentManager.SaveLoadout(10);

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void LoadLoadout_NonExistent_ShouldReturnFalse()
        {
            // Act
            var result = _equipmentManager.LoadLoadout(0, _inventory);

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void LoadLoadout_AfterSave_ShouldRestoreEquipment()
        {
            // Arrange
            var headItem = CreateTestMechPart("head_armor");
            _inventory.AddItem(headItem, 1);
            _equipmentManager.EquipItem(EquipmentSlot.Head, headItem);
            _equipmentManager.SaveLoadout(0);
            _equipmentManager.UnequipAll();

            // Act
            var result = _equipmentManager.LoadLoadout(0, _inventory);

            // Assert
            AssertBool(result).IsTrue();
        }

        [TestCase]
        public void DeleteLoadout_ExistingLoadout_ShouldRemove()
        {
            // Arrange
            _equipmentManager.EquipItem(EquipmentSlot.Head, CreateTestMechPart("head_armor"));
            _equipmentManager.SaveLoadout(0);

            // Act
            _equipmentManager.DeleteLoadout(0);

            // Assert - attempting to load should fail
            var result = _equipmentManager.LoadLoadout(0, _inventory);
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void GetCurrentLoadoutID_InitialValue_ShouldBeZero()
        {
            // Act
            var currentID = _equipmentManager.GetCurrentLoadoutID();

            // Assert
            AssertInt(currentID).IsEqual(0);
        }

        [TestCase]
        public void MultipleSlots_DifferentItems_ShouldNotInterfere()
        {
            // Arrange & Act
            _equipmentManager.EquipItem(EquipmentSlot.Head, CreateTestMechPart("head_armor"));
            _equipmentManager.EquipItem(EquipmentSlot.Torso, CreateTestMechPart("torso_armor"));
            _equipmentManager.EquipItem(EquipmentSlot.Arms, CreateTestMechPart("arm_armor"));
            _equipmentManager.EquipItem(EquipmentSlot.Legs, CreateTestMechPart("leg_armor"));

            // Assert
            AssertString(_equipmentManager.GetEquippedItem(EquipmentSlot.Head).ItemID).IsEqual("head_armor");
            AssertString(_equipmentManager.GetEquippedItem(EquipmentSlot.Torso).ItemID).IsEqual("torso_armor");
            AssertString(_equipmentManager.GetEquippedItem(EquipmentSlot.Arms).ItemID).IsEqual("arm_armor");
            AssertString(_equipmentManager.GetEquippedItem(EquipmentSlot.Legs).ItemID).IsEqual("leg_armor");
        }

        // Helper methods
        private MechPartItem CreateTestMechPart(string id)
        {
            return new MechPartItem
            {
                ItemID = id,
                DisplayName = id,
                Rarity = ItemRarity.Common,
                SellValue = 100,
                MaxStackSize = 1
            };
        }

        private DroneChipItem CreateTestDroneChip(string id)
        {
            return new DroneChipItem
            {
                ItemID = id,
                DisplayName = id,
                Rarity = ItemRarity.Common,
                SellValue = 50,
                MaxStackSize = 1
            };
        }
    }
}
