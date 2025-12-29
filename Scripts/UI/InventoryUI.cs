using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Inventory;
using MechDefenseHalo.Items;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Inventory UI with 500 slots, search, sort, and item management.
    /// 
    /// REQUIRED SCENE STRUCTURE (create manually in Godot):
    /// 
    /// Control (InventoryUI) - Script: InventoryUI.cs
    /// ├─ Panel (Background)
    /// │  ├─ Label (Title) - text: "INVENTORY"
    /// │  ├─ HBoxContainer (TopBar)
    /// │  │  ├─ LineEdit (SearchBar) - placeholder: "Search items..."
    /// │  │  ├─ OptionButton (SortDropdown)
    /// │  │  └─ Label (SlotCountLabel) - text: "0/500"
    /// │  ├─ ScrollContainer (ScrollContainer)
    /// │  │  └─ GridContainer (ItemGrid) - columns: 10
    /// │  ├─ HBoxContainer (BottomBar)
    /// │  │  ├─ Button (SalvageAllButton) - text: "Salvage All Common"
    /// │  │  └─ Button (CloseButton) - text: "Close"
    /// │  └─ Panel (ItemTooltip) - visible: false
    /// │     ├─ VBoxContainer
    /// │        ├─ Label (TooltipItemName)
    /// │        ├─ Label (TooltipItemStats)
    /// │        └─ Label (TooltipItemDescription)
    /// </summary>
    public partial class InventoryUI : Control
    {
        #region Export Variables (Wire these in Godot Editor)

        [Export] public GridContainer ItemGrid { get; set; }
        [Export] public LineEdit SearchBar { get; set; }
        [Export] public OptionButton SortDropdown { get; set; }
        [Export] public Label SlotCountLabel { get; set; }
        [Export] public Button SalvageAllButton { get; set; }
        [Export] public Button CloseButton { get; set; }
        [Export] public Panel ItemTooltip { get; set; }
        [Export] public Label TooltipItemName { get; set; }
        [Export] public Label TooltipItemStats { get; set; }
        [Export] public Label TooltipItemDescription { get; set; }
        [Export] public PackedScene ItemSlotPrefab { get; set; } // Reference to ItemSlot.tscn

        #endregion

        #region Private Fields

        private InventoryManager _inventory;
        private List<ItemSlotUI> _itemSlots = new();
        private string _currentSearchFilter = "";
        private SortType _currentSort = SortType.Rarity;

        private enum SortType
        {
            Rarity,
            Name,
            Type,
            Value,
            Level
        }

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get manager reference
            _inventory = GetNode<InventoryManager>("/root/InventoryManager");

            // Connect signals
            if (SearchBar != null)
                SearchBar.TextChanged += OnSearchTextChanged;
            if (SortDropdown != null)
                SortDropdown.ItemSelected += OnSortChanged;
            if (SalvageAllButton != null)
                SalvageAllButton.Pressed += OnSalvageAllPressed;
            if (CloseButton != null)
                CloseButton.Pressed += OnClosePressed;

            // Populate sort dropdown
            if (SortDropdown != null)
            {
                SortDropdown.AddItem("Rarity");
                SortDropdown.AddItem("Name");
                SortDropdown.AddItem("Type");
                SortDropdown.AddItem("Value");
                SortDropdown.AddItem("Level");
            }

            // Create item slots (500 slots)
            if (ItemGrid != null && ItemSlotPrefab != null)
            {
                for (int i = 0; i < 500; i++)
                {
                    var slot = ItemSlotPrefab.Instantiate<ItemSlotUI>();
                    slot.SlotID = i.ToString();
                    ItemGrid.AddChild(slot);
                    _itemSlots.Add(slot);

                    // Connect slot events
                    slot.SlotHovered += OnSlotHovered;
                    slot.SlotUnhovered += OnSlotUnhovered;
                    slot.SlotClicked += OnSlotClicked;
                }
            }

            // Listen for inventory changes
            EventBus.On(EventBus.InventoryChanged, OnInventoryChangedEvent);

            // Refresh display
            RefreshInventory();

            // Hide by default
            Hide();
            
            GD.Print("InventoryUI initialized");
        }

        public override void _Input(InputEvent @event)
        {
            // Toggle with 'I' key
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.I)
            {
                ToggleVisibility();
                GetViewport().SetInputAsHandled();
            }
        }

        #endregion

        #region Public Methods

        public void ToggleVisibility()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshInventory();
            }
        }

        public void RefreshInventory()
        {
            if (_inventory == null) return;

            // Update slot count
            if (SlotCountLabel != null)
            {
                SlotCountLabel.Text = $"{_inventory.UsedSlots}/{_inventory.MaxSlots} Slots";
            }

            // Get filtered and sorted items
            var items = GetFilteredAndSortedItems();

            // Clear all slots
            foreach (var slot in _itemSlots)
            {
                slot.ClearSlot();
            }

            // Fill slots with items
            int slotIndex = 0;
            foreach (var itemStack in items)
            {
                if (slotIndex >= _itemSlots.Count) break;

                _itemSlots[slotIndex].SetItem(itemStack.Item, itemStack.Quantity);
                slotIndex++;
            }
        }

        #endregion

        #region Private Methods

        private List<ItemStack> GetFilteredAndSortedItems()
        {
            var items = _inventory.GetAllItems();

            // Apply search filter
            if (!string.IsNullOrEmpty(_currentSearchFilter))
            {
                items = items.Where(stack => 
                    stack.Item.DisplayName.Contains(_currentSearchFilter, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Apply sorting
            items = _currentSort switch
            {
                SortType.Rarity => items.OrderByDescending(s => (int)s.Item.Rarity).ToList(),
                SortType.Name => items.OrderBy(s => s.Item.DisplayName).ToList(),
                SortType.Type => items.OrderBy(s => s.Item.GetType().Name).ToList(),
                SortType.Value => items.OrderByDescending(s => s.Item.SellValue).ToList(),
                SortType.Level => items.OrderByDescending(s => s.Item.ItemLevel).ToList(),
                _ => items
            };

            return items;
        }

        private void OnSearchTextChanged(string newText)
        {
            _currentSearchFilter = newText;
            RefreshInventory();
        }

        private void OnSortChanged(long index)
        {
            _currentSort = (SortType)index;
            RefreshInventory();
        }

        private void OnSalvageAllPressed()
        {
            // TODO: Implement bulk salvage
            GD.Print("Salvage all common items");
        }

        private void OnClosePressed()
        {
            Hide();
        }

        private void OnSlotHovered(ItemSlotUI slot)
        {
            if (slot.CurrentItem == null || ItemTooltip == null) return;

            // Update tooltip content
            if (TooltipItemName != null)
                TooltipItemName.Text = slot.CurrentItem.DisplayName;
            if (TooltipItemStats != null)
                TooltipItemStats.Text = slot.CurrentItem.GetStatsDescription();
            if (TooltipItemDescription != null)
                TooltipItemDescription.Text = slot.CurrentItem.Description;

            // Position tooltip near mouse
            ItemTooltip.GlobalPosition = GetGlobalMousePosition() + new Vector2(20, 20);
            ItemTooltip.Show();
        }

        private void OnSlotUnhovered()
        {
            if (ItemTooltip != null)
                ItemTooltip.Hide();
        }

        private void OnSlotClicked(ItemSlotUI slot, MouseButton button)
        {
            if (slot.CurrentItem == null) return;

            if (button == MouseButton.Right)
            {
                // Right-click context menu
                ShowContextMenu(slot);
            }
        }

        private void ShowContextMenu(ItemSlotUI slot)
        {
            // TODO: Implement popup menu
            GD.Print($"Context menu for: {slot.CurrentItem.DisplayName}");
        }

        private void OnInventoryChangedEvent(object data)
        {
            RefreshInventory();
        }

        #endregion
    }
}
