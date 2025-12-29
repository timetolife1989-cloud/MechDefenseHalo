using Godot;
using System;
using MechDefenseHalo.Shop;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Displays a shop item with icon, name, price, and purchase button.
    /// 
    /// REQUIRED SCENE STRUCTURE (create manually in Godot):
    /// 
    /// Panel (ShopItemCardUI) - Script: ShopItemCardUI.cs, custom_minimum_size: (160, 220)
    /// ├─ MarginContainer (margins: 8px all sides)
    /// │  └─ VBoxContainer (separation: 6)
    /// │     ├─ Panel (IconPanel) - custom_minimum_size: (128, 128)
    /// │     │  └─ TextureRect (ItemIcon) - expand_mode: FIT_TO_RECT, stretch_mode: KEEP_ASPECT_CENTERED
    /// │     ├─ Label (ItemNameLabel) - horizontal_alignment: CENTER, autowrap_mode: WORD_SMART
    /// │     │                          theme_override_font_sizes/font_size: 13
    /// │     ├─ Label (ItemDescLabel) - horizontal_alignment: CENTER, autowrap_mode: WORD_SMART
    /// │     │                          theme_override_font_sizes/font_size: 10
    /// │     │                          modulate: Color(0.7, 0.7, 0.7)
    /// │     ├─ HSeparator
    /// │     ├─ HBoxContainer (PriceRow)
    /// │     │  ├─ Label (CreditsPriceLabel) - text: "0 ¢", size_flags_h: EXPAND_FILL
    /// │     │  └─ Label (CoresPriceLabel) - text: "0 ◆", size_flags_h: EXPAND_FILL
    /// │     │                                horizontal_alignment: RIGHT
    /// │     └─ Button (PurchaseButton) - text: "Purchase", size_flags_h: FILL
    /// </summary>
    public partial class ShopItemCardUI : Panel
    {
        #region Export Variables

        /// <summary>Icon showing the shop item</summary>
        [Export] public TextureRect ItemIcon { get; set; }
        
        /// <summary>Label showing item name</summary>
        [Export] public Label ItemNameLabel { get; set; }
        
        /// <summary>Label showing item description</summary>
        [Export] public Label ItemDescLabel { get; set; }
        
        /// <summary>Label showing credits price</summary>
        [Export] public Label CreditsPriceLabel { get; set; }
        
        /// <summary>Label showing cores price</summary>
        [Export] public Label CoresPriceLabel { get; set; }
        
        /// <summary>Purchase button</summary>
        [Export] public Button PurchaseButton { get; set; }
        
        /// <summary>Panel behind the icon</summary>
        [Export] public Panel IconPanel { get; set; }

        #endregion

        #region Properties

        /// <summary>The shop item this card represents</summary>
        public ShopItem ShopItem { get; private set; }
        
        /// <summary>Whether the player can afford this item</summary>
        public bool CanAfford { get; private set; }

        #endregion

        #region Events

        /// <summary>Fired when purchase button is clicked</summary>
        public event Action<string> PurchaseRequested;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Connect button signal
            if (PurchaseButton != null)
            {
                PurchaseButton.Pressed += OnPurchasePressed;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the card with a shop item
        /// </summary>
        /// <param name="shopItem">The shop item to display</param>
        public void SetShopItem(ShopItem shopItem)
        {
            ShopItem = shopItem;
            RefreshDisplay();
        }

        /// <summary>
        /// Update the affordability state of this card
        /// </summary>
        /// <param name="playerCredits">Player's current credits</param>
        /// <param name="playerCores">Player's current cores</param>
        public void UpdateAffordability(int playerCredits, int playerCores)
        {
            if (ShopItem == null)
            {
                CanAfford = false;
                UpdatePurchaseButton();
                return;
            }

            // Check if player can afford the item
            bool canAffordCredits = ShopItem.PriceCredits == 0 || playerCredits >= ShopItem.PriceCredits;
            bool canAffordCores = ShopItem.PriceCores == 0 || playerCores >= ShopItem.PriceCores;
            
            CanAfford = canAffordCredits && canAffordCores;
            UpdatePurchaseButton();
        }

        /// <summary>
        /// Refresh the entire display
        /// </summary>
        public void RefreshDisplay()
        {
            if (ShopItem == null) return;

            // Set item name with rarity color
            if (ItemNameLabel != null)
            {
                ItemNameLabel.Text = ShopItem.DisplayName;
                ItemNameLabel.Modulate = RarityConfig.GetColor(ShopItem.Rarity);
            }

            // Set description
            if (ItemDescLabel != null)
            {
                ItemDescLabel.Text = ShopItem.Description;
            }

            // Set icon (would need to load from ItemDatabase in real implementation)
            if (ItemIcon != null)
            {
                // TODO: Load icon from ItemDatabase using ItemID
                // For now, hide if no texture
                ItemIcon.Visible = false;
            }

            // Set icon panel background color by rarity (subtle)
            if (IconPanel != null)
            {
                var rarityColor = RarityConfig.GetColor(ShopItem.Rarity);
                rarityColor.A = 0.3f;
                IconPanel.Modulate = rarityColor;
            }

            // Set credits price
            if (CreditsPriceLabel != null)
            {
                if (ShopItem.PriceCredits > 0)
                {
                    CreditsPriceLabel.Text = $"{ShopItem.PriceCredits} ¢";
                    CreditsPriceLabel.Show();
                }
                else
                {
                    CreditsPriceLabel.Hide();
                }
            }

            // Set cores price
            if (CoresPriceLabel != null)
            {
                if (ShopItem.PriceCores > 0)
                {
                    CoresPriceLabel.Text = $"{ShopItem.PriceCores} ◆";
                    CoresPriceLabel.Modulate = new Color(1.0f, 0.8f, 0.2f); // Gold color for cores
                    CoresPriceLabel.Show();
                }
                else
                {
                    CoresPriceLabel.Hide();
                }
            }

            // Set card background based on rarity
            var bgColor = RarityConfig.GetColor(ShopItem.Rarity);
            bgColor.A = 0.15f;
            Modulate = new Color(1, 1, 1, 1); // Reset base modulate
            // Note: Actual background would be set via StyleBox in .tscn

            UpdatePurchaseButton();
        }

        #endregion

        #region Private Methods

        private void UpdatePurchaseButton()
        {
            if (PurchaseButton == null) return;

            if (CanAfford)
            {
                PurchaseButton.Disabled = false;
                PurchaseButton.Text = "Purchase";
                PurchaseButton.Modulate = new Color(1, 1, 1);
            }
            else
            {
                PurchaseButton.Disabled = true;
                PurchaseButton.Text = "Cannot Afford";
                PurchaseButton.Modulate = new Color(0.5f, 0.5f, 0.5f);
            }
        }

        private void OnPurchasePressed()
        {
            if (ShopItem != null && CanAfford)
            {
                PurchaseRequested?.Invoke(ShopItem.ShopItemID);
            }
        }

        #endregion
    }
}
