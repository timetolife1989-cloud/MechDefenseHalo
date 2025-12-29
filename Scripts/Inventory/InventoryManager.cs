using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Inventory
{
    /// <summary>
    /// Manages player inventory storage, sorting, and filtering
    /// </summary>
    public partial class InventoryManager : Node
    {
        #region Singleton

        private static InventoryManager _instance;

        public static InventoryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("InventoryManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Exported Properties

        [Export] public int MaxSlots { get; set; } = 500;

        #endregion

        #region Public Properties

        public int UsedSlots => _items.Values.Sum(stack => stack.IsStackable ? 1 : stack.Quantity);
        public int AvailableSlots => MaxSlots - UsedSlots;

        #endregion

        #region Private Fields

        private Dictionary<string, ItemStack> _items = new();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple InventoryManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            GD.Print($"InventoryManager initialized with {MaxSlots} slots");
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods - Add/Remove

        /// <summary>
        /// Add an item to the inventory
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="quantity">Amount to add</param>
        /// <returns>True if item was added successfully</returns>
        public bool AddItem(ItemBase item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
            {
                GD.PrintErr("Cannot add null item or invalid quantity");
                return false;
            }

            // Check if item already exists (for stacking)
            if (_items.TryGetValue(item.ItemID, out var existingStack))
            {
                if (existingStack.Item.MaxStackSize > 1)
                {
                    // Stack the item
                    int spaceLeft = existingStack.Item.MaxStackSize - existingStack.Quantity;
                    int amountToAdd = Mathf.Min(quantity, spaceLeft);
                    
                    existingStack.Quantity += amountToAdd;
                    
                    GD.Print($"Added {amountToAdd}x {item.DisplayName} (now {existingStack.Quantity})");
                    
                    EmitInventoryChanged();
                    
                    // If we couldn't add all, try to create new stack
                    if (amountToAdd < quantity)
                    {
                        return AddItem(item, quantity - amountToAdd);
                    }
                    
                    return true;
                }
            }

            // Check if we have space
            if (UsedSlots >= MaxSlots)
            {
                GD.PrintErr("Inventory is full!");
                return false;
            }

            // Add new item stack
            var newStack = new ItemStack
            {
                Item = item.Clone(),
                Quantity = quantity,
                IsEquipped = false,
                IsStackable = item.MaxStackSize > 1
            };

            _items[item.ItemID] = newStack;
            
            GD.Print($"Added {quantity}x {item.DisplayName} to inventory");
            
            EmitInventoryChanged();
            return true;
        }

        /// <summary>
        /// Remove an item from the inventory
        /// </summary>
        /// <param name="itemID">ID of item to remove</param>
        /// <param name="quantity">Amount to remove</param>
        /// <returns>True if item was removed successfully</returns>
        public bool RemoveItem(string itemID, int quantity = 1)
        {
            if (!_items.TryGetValue(itemID, out var stack))
            {
                GD.PrintErr($"Item not found in inventory: {itemID}");
                return false;
            }

            if (stack.Quantity < quantity)
            {
                GD.PrintErr($"Not enough items to remove: {itemID} (have {stack.Quantity}, need {quantity})");
                return false;
            }

            stack.Quantity -= quantity;

            if (stack.Quantity <= 0)
            {
                _items.Remove(itemID);
                GD.Print($"Removed all {stack.Item.DisplayName} from inventory");
            }
            else
            {
                GD.Print($"Removed {quantity}x {stack.Item.DisplayName} (now {stack.Quantity})");
            }

            EmitInventoryChanged();
            return true;
        }

        /// <summary>
        /// Check if inventory contains an item
        /// </summary>
        /// <param name="itemID">Item ID to check</param>
        /// <param name="quantity">Minimum quantity required</param>
        /// <returns>True if item exists with at least the specified quantity</returns>
        public bool HasItem(string itemID, int quantity = 1)
        {
            return _items.TryGetValue(itemID, out var stack) && stack.Quantity >= quantity;
        }

        /// <summary>
        /// Get the quantity of an item in inventory
        /// </summary>
        /// <param name="itemID">Item ID</param>
        /// <returns>Quantity or 0 if not found</returns>
        public int GetItemQuantity(string itemID)
        {
            return _items.TryGetValue(itemID, out var stack) ? stack.Quantity : 0;
        }

        #endregion

        #region Public Methods - Retrieval

        /// <summary>
        /// Get an item by ID
        /// </summary>
        /// <param name="itemID">Item ID</param>
        /// <returns>Item or null if not found</returns>
        public ItemBase GetItem(string itemID)
        {
            return _items.TryGetValue(itemID, out var stack) ? stack.Item : null;
        }

        /// <summary>
        /// Get items by type
        /// </summary>
        /// <typeparam name="T">Item type to filter by</typeparam>
        /// <returns>List of items matching the type</returns>
        public List<ItemBase> GetItemsByType<T>() where T : ItemBase
        {
            return _items.Values
                .Where(stack => stack.Item is T)
                .Select(stack => stack.Item)
                .ToList();
        }

        /// <summary>
        /// Get items by rarity
        /// </summary>
        /// <param name="rarity">Rarity to filter by</param>
        /// <returns>List of items matching the rarity</returns>
        public List<ItemBase> GetItemsByRarity(ItemRarity rarity)
        {
            return _items.Values
                .Where(stack => stack.Item.Rarity == rarity)
                .Select(stack => stack.Item)
                .ToList();
        }

        /// <summary>
        /// Get all items in inventory
        /// </summary>
        /// <returns>List of all items</returns>
        public List<ItemStack> GetAllItems()
        {
            return new List<ItemStack>(_items.Values);
        }

        #endregion

        #region Public Methods - Sorting

        /// <summary>
        /// Sort inventory by specified criteria
        /// </summary>
        /// <param name="sortType">How to sort</param>
        public void SortInventory(SortType sortType)
        {
            var sortedItems = sortType switch
            {
                SortType.Rarity => _items.Values.OrderByDescending(s => s.Item.Rarity),
                SortType.Name => _items.Values.OrderBy(s => s.Item.DisplayName),
                SortType.Type => _items.Values.OrderBy(s => s.Item.GetType().Name),
                SortType.Level => _items.Values.OrderByDescending(s => s.Item.ItemLevel),
                SortType.Value => _items.Values.OrderByDescending(s => s.Item.SellValue),
                _ => _items.Values.OrderBy(s => s.Item.DisplayName)
            };

            // Rebuild dictionary with sorted order
            _items = sortedItems.ToDictionary(s => s.Item.ItemID, s => s);
            
            GD.Print($"Inventory sorted by {sortType}");
            EmitInventoryChanged();
        }

        #endregion

        #region Public Methods - Utility

        /// <summary>
        /// Clear entire inventory
        /// </summary>
        public void ClearInventory()
        {
            _items.Clear();
            GD.Print("Inventory cleared");
            EmitInventoryChanged();
        }

        /// <summary>
        /// Expand inventory capacity
        /// </summary>
        /// <param name="additionalSlots">Number of slots to add</param>
        public void ExpandInventory(int additionalSlots)
        {
            MaxSlots += additionalSlots;
            GD.Print($"Inventory expanded by {additionalSlots} slots (now {MaxSlots})");
            EmitInventoryChanged();
        }

        #endregion

        #region Private Methods

        private void EmitInventoryChanged()
        {
            EventBus.Emit("inventory_changed", new InventoryChangedData
            {
                UsedSlots = UsedSlots,
                MaxSlots = MaxSlots,
                ItemCount = _items.Count
            });
        }

        #endregion

        #region Public Methods - Save/Load

        /// <summary>
        /// Get inventory data for saving
        /// </summary>
        /// <returns>Inventory save data</returns>
        public SaveSystem.InventorySaveData GetSaveData()
        {
            var items = new List<SaveSystem.ItemSaveData>();
            int slotIndex = 0;

            foreach (var kvp in _items)
            {
                var stack = kvp.Value;
                var itemSaveData = new SaveSystem.ItemSaveData
                {
                    ItemID = stack.Item.ItemID,
                    Quantity = stack.Quantity,
                    SlotIndex = slotIndex++,
                    Stats = new Dictionary<string, float>()
                };

                // Save item stats if available
                if (stack.Item is Items.EquipmentItem equipItem)
                {
                    itemSaveData.Stats["Damage"] = equipItem.Damage;
                    itemSaveData.Stats["Defense"] = equipItem.Defense;
                    itemSaveData.Stats["CritChance"] = equipItem.CritChance;
                }

                items.Add(itemSaveData);
            }

            return new SaveSystem.InventorySaveData
            {
                MaxSlots = MaxSlots,
                Items = items
            };
        }

        /// <summary>
        /// Load inventory data from save
        /// </summary>
        /// <param name="saveData">Inventory save data</param>
        public void LoadFromSave(SaveSystem.InventorySaveData saveData)
        {
            if (saveData == null)
            {
                GD.PrintErr("Cannot load from null save data");
                return;
            }

            // Clear existing inventory
            _items.Clear();

            // Update max slots
            MaxSlots = saveData.MaxSlots;

            // Load items - Note: This is a simplified version
            // In a real implementation, you would need to look up actual item objects
            // from an item database/registry based on ItemID
            foreach (var itemData in saveData.Items)
            {
                // TODO: Look up item from item database/registry using itemData.ItemID
                // For now, we'll just log that we should restore this item
                GD.Print($"Should restore item: {itemData.ItemID} x{itemData.Quantity}");
            }

            EmitInventoryChanged();
            GD.Print($"Inventory loaded from save: {saveData.Items.Count} items");
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Represents a stack of items in inventory
    /// </summary>
    public class ItemStack
    {
        public ItemBase Item { get; set; }
        public int Quantity { get; set; }
        public bool IsEquipped { get; set; }
        public bool IsStackable { get; set; }
    }

    /// <summary>
    /// Sorting options for inventory
    /// </summary>
    public enum SortType
    {
        Rarity,
        Name,
        Type,
        Level,
        Value
    }

    /// <summary>
    /// Data for inventory changed event
    /// </summary>
    public class InventoryChangedData
    {
        public int UsedSlots { get; set; }
        public int MaxSlots { get; set; }
        public int ItemCount { get; set; }
    }

    #endregion
}
