using Godot;
using System;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Reusable item slot component for inventory/equipment/shop.
    /// 
    /// REQUIRED SCENE STRUCTURE (create manually in Godot):
    /// 
    /// Panel (ItemSlotUI) - Script: ItemSlotUI.cs, custom_minimum_size: (70, 70)
    /// ├─ TextureRect (ItemIcon) - expand_mode: FIT_TO_RECT, anchors: full rect with 4px margins
    /// ├─ Label (QuantityLabel) - position: bottom-right (anchor_right: 1, anchor_bottom: 1)
    /// │                           horizontal_alignment: RIGHT, vertical_alignment: BOTTOM
    /// ├─ Panel (RarityBorder) - anchors: full rect, self_modulate: grey, mouse_filter: IGNORE
    /// │                          stylebox: StyleBoxFlat with 2px border, transparent background
    /// └─ Panel (SelectedOverlay) - visible: false, anchors: full rect, mouse_filter: IGNORE
    ///                               self_modulate: Color(1, 1, 1, 0.3)
    /// </summary>
    public partial class ItemSlotUI : Panel
    {
        #region Export Variables

        /// <summary>Icon texture display for the item</summary>
        [Export] public TextureRect ItemIcon { get; set; }
        
        /// <summary>Label showing item quantity (for stackable items)</summary>
        [Export] public Label QuantityLabel { get; set; }
        
        /// <summary>Border panel colored by item rarity</summary>
        [Export] public Panel RarityBorder { get; set; }
        
        /// <summary>Overlay shown when slot is selected</summary>
        [Export] public Panel SelectedOverlay { get; set; }

        #endregion

        #region Properties

        /// <summary>Currently displayed item (null if empty)</summary>
        public ItemBase CurrentItem { get; private set; }
        
        /// <summary>Quantity of items in this slot</summary>
        public int Quantity { get; private set; }
        
        /// <summary>Unique identifier for this slot (e.g., slot index or equipment slot type)</summary>
        public string SlotID { get; set; } = "";

        #endregion

        #region Events

        /// <summary>Fired when mouse enters the slot</summary>
        public event Action<ItemSlotUI> SlotHovered;
        
        /// <summary>Fired when mouse exits the slot</summary>
        public event Action SlotUnhovered;
        
        /// <summary>Fired when slot is clicked (passes slot reference and mouse button)</summary>
        public event Action<ItemSlotUI, MouseButton> SlotClicked;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Connect mouse signals
            MouseEntered += () => SlotHovered?.Invoke(this);
            MouseExited += () => SlotUnhovered?.Invoke();
            
            // Set default empty state
            ClearSlot();
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                SlotClicked?.Invoke(this, mouseEvent.ButtonIndex);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the item displayed in this slot
        /// </summary>
        /// <param name="item">Item to display (or null to clear)</param>
        /// <param name="quantity">Number of items (default: 1)</param>
        public void SetItem(ItemBase item, int quantity = 1)
        {
            CurrentItem = item;
            Quantity = quantity;

            if (item == null)
            {
                ClearSlot();
                return;
            }

            // Set icon
            if (ItemIcon != null && item.Icon != null)
            {
                ItemIcon.Texture = item.Icon;
                ItemIcon.Show();
            }
            else if (ItemIcon != null)
            {
                ItemIcon.Hide();
            }

            // Set quantity label (only show if quantity > 1 or if stackable)
            if (QuantityLabel != null)
            {
                if (quantity > 1)
                {
                    QuantityLabel.Text = quantity.ToString();
                    QuantityLabel.Show();
                }
                else
                {
                    QuantityLabel.Hide();
                }
            }

            // Set rarity border color
            if (RarityBorder != null)
            {
                RarityBorder.Modulate = RarityConfig.GetColor(item.Rarity);
            }
        }

        /// <summary>
        /// Clear the slot to empty state
        /// </summary>
        public void ClearSlot()
        {
            CurrentItem = null;
            Quantity = 0;

            if (ItemIcon != null)
                ItemIcon.Hide();
            
            if (QuantityLabel != null)
                QuantityLabel.Hide();
            
            if (RarityBorder != null)
                RarityBorder.Modulate = new Color(0.3f, 0.3f, 0.3f);
        }

        /// <summary>
        /// Set the selected state of this slot
        /// </summary>
        /// <param name="selected">True to show selection overlay</param>
        public void SetSelected(bool selected)
        {
            if (SelectedOverlay != null)
                SelectedOverlay.Visible = selected;
        }

        /// <summary>
        /// Check if this slot is empty
        /// </summary>
        /// <returns>True if no item is present</returns>
        public bool IsEmpty()
        {
            return CurrentItem == null;
        }

        /// <summary>
        /// Update the quantity without changing the item
        /// </summary>
        /// <param name="newQuantity">New quantity value</param>
        public void UpdateQuantity(int newQuantity)
        {
            if (CurrentItem == null) return;
            
            Quantity = newQuantity;
            
            if (QuantityLabel != null)
            {
                if (newQuantity > 1)
                {
                    QuantityLabel.Text = newQuantity.ToString();
                    QuantityLabel.Show();
                }
                else
                {
                    QuantityLabel.Hide();
                }
            }
        }

        #endregion
    }
}
