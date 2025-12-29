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
    /// Inventory UI system with grid display, search, sort, and filtering
    /// </summary>
    public partial class InventoryUI : Control
    {
        #region Exported Nodes
        
        [Export] public GridContainer ItemGrid { get; set; }
        [Export] public LineEdit SearchBar { get; set; }
        [Export] public OptionButton SortDropdown { get; set; }
        [Export] public Panel ItemTooltip { get; set; }
        [Export] public Label SlotCountLabel { get; set; }
        [Export] public Button SalvageAllButton { get; set; }
        [Export] public Button CloseButton { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private InventoryManager _inventoryManager;
        private List<Panel> _itemSlots = new();
        private string _searchFilter = "";
        private SortType _currentSort = SortType.Rarity;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Initially hidden
            Visible = false;
            
            // Connect signals
            if (SearchBar != null)
            {
                SearchBar.TextChanged += OnSearchTextChanged;
            }
            
            if (SortDropdown != null)
            {
                SortDropdown.ItemSelected += OnSortSelected;
                PopulateSortDropdown();
            }
            
            if (SalvageAllButton != null)
            {
                SalvageAllButton.Pressed += OnSalvageAllPressed;
            }
            
            if (CloseButton != null)
            {
                CloseButton.Pressed += OnClosePressed;
            }
            
            // Listen for inventory changes
            EventBus.On("inventory_changed", OnInventoryChanged);
            
            GD.Print("InventoryUI initialized");
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.I)
                {
                    ToggleVisibility();
                }
                else if (keyEvent.Keycode == Key.Escape && Visible)
                {
                    Hide();
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize with inventory manager reference
        /// </summary>
        public void Initialize(InventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;
            GenerateItemSlots();
            RefreshDisplay();
        }
        
        /// <summary>
        /// Toggle inventory visibility
        /// </summary>
        public void ToggleVisibility()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshDisplay();
            }
        }
        
        /// <summary>
        /// Refresh the entire inventory display
        /// </summary>
        public void RefreshDisplay()
        {
            if (_inventoryManager == null) return;
            
            UpdateSlotCountLabel();
            UpdateItemGrid();
        }
        
        #endregion
        
        #region Private Methods - UI Setup
        
        private void PopulateSortDropdown()
        {
            SortDropdown.Clear();
            SortDropdown.AddItem("Rarity");
            SortDropdown.AddItem("Name");
            SortDropdown.AddItem("Type");
            SortDropdown.AddItem("Value");
            SortDropdown.Selected = 0;
        }
        
        private void GenerateItemSlots()
        {
            if (ItemGrid == null) return;
            
            // Clear existing slots
            foreach (var child in ItemGrid.GetChildren())
            {
                child.QueueFree();
            }
            _itemSlots.Clear();
            
            // Create 500 slots (10x50 grid)
            for (int i = 0; i < 500; i++)
            {
                var slot = CreateItemSlot(i);
                ItemGrid.AddChild(slot);
                _itemSlots.Add(slot);
            }
        }
        
        private Panel CreateItemSlot(int index)
        {
            var slot = new Panel();
            slot.CustomMinimumSize = new Vector2(64, 64);
            slot.Name = $"ItemSlot_{index}";
            
            // Add tooltip on hover
            slot.MouseEntered += () => OnSlotHoverEnter(index);
            slot.MouseExited += OnSlotHoverExit;
            
            // Add click handler
            slot.GuiInput += (inputEvent) => OnSlotClicked(inputEvent, index);
            
            return slot;
        }
        
        #endregion
        
        #region Private Methods - Display Update
        
        private void UpdateSlotCountLabel()
        {
            if (SlotCountLabel == null || _inventoryManager == null) return;
            
            SlotCountLabel.Text = $"{_inventoryManager.UsedSlots}/{_inventoryManager.MaxSlots} Slots";
        }
        
        private void UpdateItemGrid()
        {
            if (_inventoryManager == null) return;
            
            var items = _inventoryManager.GetAllItems();
            
            // Apply search filter
            if (!string.IsNullOrEmpty(_searchFilter))
            {
                items = items.Where(stack => 
                    stack.Item.DisplayName.ToLower().Contains(_searchFilter.ToLower())
                ).ToList();
            }
            
            // Sort items
            items = SortItems(items);
            
            // Update slots
            for (int i = 0; i < _itemSlots.Count; i++)
            {
                if (i < items.Count)
                {
                    UpdateSlotWithItem(_itemSlots[i], items[i]);
                }
                else
                {
                    ClearSlot(_itemSlots[i]);
                }
            }
        }
        
        private List<ItemStack> SortItems(List<ItemStack> items)
        {
            return _currentSort switch
            {
                SortType.Rarity => items.OrderByDescending(s => s.Item.Rarity).ToList(),
                SortType.Name => items.OrderBy(s => s.Item.DisplayName).ToList(),
                SortType.Type => items.OrderBy(s => s.Item.GetType().Name).ToList(),
                SortType.Value => items.OrderByDescending(s => s.Item.SellValue).ToList(),
                _ => items
            };
        }
        
        private void UpdateSlotWithItem(Panel slot, ItemStack stack)
        {
            // Set background color based on rarity
            slot.Modulate = GetRarityColor(stack.Item.Rarity);
            
            // TODO: Add TextureRect and Label child nodes in .tscn file for:
            // - Item icon (TextureRect with stack.Item.Icon)
            // - Quantity label (Label showing stack.Quantity)
            // This will be implemented when creating the .tscn scene file
        }
        
        private void ClearSlot(Panel slot)
        {
            slot.Modulate = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        }
        
        private Color GetRarityColor(ItemRarity rarity)
        {
            return RarityConfig.GetColor(rarity);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnSearchTextChanged(string newText)
        {
            _searchFilter = newText;
            RefreshDisplay();
        }
        
        private void OnSortSelected(long index)
        {
            _currentSort = (SortType)index;
            
            if (_inventoryManager != null)
            {
                _inventoryManager.SortInventory(_currentSort);
            }
            
            RefreshDisplay();
        }
        
        private void OnSalvageAllPressed()
        {
            // TODO: Implement salvage all common items
            GD.Print("Salvage all common items");
        }
        
        private void OnClosePressed()
        {
            Hide();
        }
        
        private void OnSlotHoverEnter(int slotIndex)
        {
            // TODO: Show tooltip with item info
            if (ItemTooltip != null)
            {
                ItemTooltip.Visible = true;
            }
        }
        
        private void OnSlotHoverExit()
        {
            if (ItemTooltip != null)
            {
                ItemTooltip.Visible = false;
            }
        }
        
        private void OnSlotClicked(InputEvent inputEvent, int slotIndex)
        {
            if (inputEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Left)
                {
                    // Left click - select/use item
                    GD.Print($"Clicked slot {slotIndex}");
                }
                else if (mouseEvent.ButtonIndex == MouseButton.Right)
                {
                    // Right click - show context menu
                    ShowContextMenu(slotIndex);
                }
            }
        }
        
        private void ShowContextMenu(int slotIndex)
        {
            // TODO: Show context menu (Equip, Salvage, Sell, Favorite)
            GD.Print($"Show context menu for slot {slotIndex}");
        }
        
        private void OnInventoryChanged(object data)
        {
            RefreshDisplay();
        }
        
        #endregion
    }
}
