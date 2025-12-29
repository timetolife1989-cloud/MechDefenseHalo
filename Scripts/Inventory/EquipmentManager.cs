using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Inventory
{
    /// <summary>
    /// Manages equipped items and loadouts
    /// </summary>
    public partial class EquipmentManager : Node
    {
        #region Constants

        private const int MAX_LOADOUTS = 5;

        #endregion

        #region Private Fields

        private Dictionary<EquipmentSlot, ItemBase> _equippedItems = new();
        private Dictionary<int, Dictionary<EquipmentSlot, string>> _loadouts = new(); // loadoutID -> slot -> itemID
        private int _currentLoadoutID = 0;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            InitializeSlots();
            GD.Print("EquipmentManager initialized");
        }

        #endregion

        #region Public Methods - Equipment

        /// <summary>
        /// Equip an item to a specific slot
        /// </summary>
        /// <param name="slot">Equipment slot</param>
        /// <param name="item">Item to equip</param>
        /// <returns>Previously equipped item or null</returns>
        public ItemBase EquipItem(EquipmentSlot slot, ItemBase item)
        {
            if (item == null)
            {
                GD.PrintErr("Cannot equip null item");
                return null;
            }

            // Validate item type matches slot
            if (!IsValidItemForSlot(item, slot))
            {
                GD.PrintErr($"Item {item.DisplayName} cannot be equipped to slot {slot}");
                return null;
            }

            // Unequip current item
            ItemBase previousItem = null;
            if (_equippedItems.TryGetValue(slot, out var current))
            {
                previousItem = current;
            }

            // Equip new item
            _equippedItems[slot] = item.Clone();
            
            GD.Print($"Equipped {item.DisplayName} to {slot}");

            // Emit event
            EventBus.Emit("item_equipped", new ItemEquippedData
            {
                Slot = slot,
                Item = item,
                PreviousItem = previousItem
            });

            return previousItem;
        }

        /// <summary>
        /// Unequip an item from a specific slot
        /// </summary>
        /// <param name="slot">Equipment slot</param>
        /// <returns>Unequipped item or null</returns>
        public ItemBase UnequipItem(EquipmentSlot slot)
        {
            if (!_equippedItems.TryGetValue(slot, out var item))
            {
                GD.Print($"No item equipped in slot {slot}");
                return null;
            }

            _equippedItems.Remove(slot);
            
            GD.Print($"Unequipped {item.DisplayName} from {slot}");

            // Emit event
            EventBus.Emit("item_unequipped", new ItemUnequippedData
            {
                Slot = slot,
                Item = item
            });

            return item;
        }

        /// <summary>
        /// Get the item equipped in a specific slot
        /// </summary>
        /// <param name="slot">Equipment slot</param>
        /// <returns>Equipped item or null</returns>
        public ItemBase GetEquippedItem(EquipmentSlot slot)
        {
            return _equippedItems.TryGetValue(slot, out var item) ? item : null;
        }

        /// <summary>
        /// Get all equipped items
        /// </summary>
        /// <returns>Dictionary of equipped items by slot</returns>
        public Dictionary<EquipmentSlot, ItemBase> GetAllEquippedItems()
        {
            return new Dictionary<EquipmentSlot, ItemBase>(_equippedItems);
        }

        /// <summary>
        /// Unequip all items
        /// </summary>
        /// <returns>List of unequipped items</returns>
        public List<ItemBase> UnequipAll()
        {
            var items = new List<ItemBase>(_equippedItems.Values);
            _equippedItems.Clear();
            
            GD.Print("Unequipped all items");
            return items;
        }

        #endregion

        #region Public Methods - Loadouts

        /// <summary>
        /// Save current equipment as a loadout
        /// </summary>
        /// <param name="loadoutID">Loadout slot (0-4)</param>
        /// <returns>True if saved successfully</returns>
        public bool SaveLoadout(int loadoutID)
        {
            if (loadoutID < 0 || loadoutID >= MAX_LOADOUTS)
            {
                GD.PrintErr($"Invalid loadout ID: {loadoutID}");
                return false;
            }

            var loadout = new Dictionary<EquipmentSlot, string>();
            foreach (var kvp in _equippedItems)
            {
                loadout[kvp.Key] = kvp.Value.ItemID;
            }

            _loadouts[loadoutID] = loadout;
            GD.Print($"Saved loadout {loadoutID}");
            return true;
        }

        /// <summary>
        /// Load a saved loadout
        /// </summary>
        /// <param name="loadoutID">Loadout slot (0-4)</param>
        /// <param name="inventory">Inventory to load items from</param>
        /// <returns>True if loaded successfully</returns>
        public bool LoadLoadout(int loadoutID, InventoryManager inventory)
        {
            if (loadoutID < 0 || loadoutID >= MAX_LOADOUTS)
            {
                GD.PrintErr($"Invalid loadout ID: {loadoutID}");
                return false;
            }

            if (!_loadouts.TryGetValue(loadoutID, out var loadout))
            {
                GD.PrintErr($"Loadout {loadoutID} not found");
                return false;
            }

            // Unequip current items
            UnequipAll();

            // Equip items from loadout
            foreach (var kvp in loadout)
            {
                var item = inventory.GetItem(kvp.Value);
                if (item != null)
                {
                    EquipItem(kvp.Key, item);
                }
                else
                {
                    GD.PrintErr($"Item {kvp.Value} not found in inventory for loadout {loadoutID}");
                }
            }

            _currentLoadoutID = loadoutID;
            GD.Print($"Loaded loadout {loadoutID}");
            return true;
        }

        /// <summary>
        /// Delete a loadout
        /// </summary>
        /// <param name="loadoutID">Loadout slot to delete</param>
        public void DeleteLoadout(int loadoutID)
        {
            if (_loadouts.Remove(loadoutID))
            {
                GD.Print($"Deleted loadout {loadoutID}");
            }
        }

        /// <summary>
        /// Get current loadout ID
        /// </summary>
        public int GetCurrentLoadoutID()
        {
            return _currentLoadoutID;
        }

        #endregion

        #region Private Methods

        private void InitializeSlots()
        {
            // Initialize all slots as empty
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                _equippedItems[slot] = null;
            }
        }

        private bool IsValidItemForSlot(ItemBase item, EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Head or EquipmentSlot.Torso or EquipmentSlot.Arms or EquipmentSlot.Legs
                    => item is MechPartItem,
                EquipmentSlot.Weapon1 or EquipmentSlot.Weapon2 or EquipmentSlot.Weapon3 or EquipmentSlot.Weapon4
                    => item is WeaponModItem, // TODO: Add WeaponItem class when weapons are implemented
                EquipmentSlot.Drone1 or EquipmentSlot.Drone2 or EquipmentSlot.Drone3 or EquipmentSlot.Drone4 or EquipmentSlot.Drone5
                    => item is DroneChipItem,
                EquipmentSlot.Accessory1 or EquipmentSlot.Accessory2
                    => true, // Any item can go in accessory slots
                _ => false
            };
        }

        #endregion
    }

    #region Enums and Data Structures

    /// <summary>
    /// Equipment slots on the player
    /// </summary>
    public enum EquipmentSlot
    {
        Head,
        Torso,
        Arms,
        Legs,
        Weapon1,
        Weapon2,
        Weapon3,
        Weapon4,
        Drone1,
        Drone2,
        Drone3,
        Drone4,
        Drone5,
        Accessory1,
        Accessory2
    }

    /// <summary>
    /// Data for item equipped event
    /// </summary>
    public class ItemEquippedData
    {
        public EquipmentSlot Slot { get; set; }
        public ItemBase Item { get; set; }
        public ItemBase PreviousItem { get; set; }
    }

    /// <summary>
    /// Data for item unequipped event
    /// </summary>
    public class ItemUnequippedData
    {
        public EquipmentSlot Slot { get; set; }
        public ItemBase Item { get; set; }
    }

    #endregion
}
