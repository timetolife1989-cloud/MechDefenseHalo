using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Shop;
using MechDefenseHalo.Inventory;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Items;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Shop UI system with categories, featured items, and purchase confirmation
    /// </summary>
    public partial class ShopUI : Control
    {
        #region Nodes
        
        [Export] public TabContainer CategoryTabs { get; set; }
        [Export] public GridContainer CosmeticsGrid { get; set; }
        [Export] public GridContainer ConvenienceGrid { get; set; }
        [Export] public GridContainer MaterialsGrid { get; set; }
        [Export] public HBoxContainer FeaturedPanel { get; set; }
        [Export] public Label CreditsLabel { get; set; }
        [Export] public Label CoresLabel { get; set; }
        [Export] public Button CloseButton { get; set; }
        [Export] public ConfirmationDialog PurchaseConfirm { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private ShopManager _shopManager;
        private InventoryManager _inventoryManager;
        private ShopItem _pendingPurchase;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Initially hidden
            Visible = false;
            
            // Connect signals
            if (CloseButton != null)
            {
                CloseButton.Pressed += OnClosePressed;
            }
            
            if (PurchaseConfirm != null)
            {
                PurchaseConfirm.Confirmed += OnPurchaseConfirmed;
                PurchaseConfirm.Canceled += OnPurchaseCanceled;
            }
            
            // Listen for currency changes
            EventBus.On("credits_changed", OnCurrencyChanged);
            EventBus.On("cores_changed", OnCurrencyChanged);
            EventBus.On(EventBus.ItemPurchased, OnItemPurchased);
            
            GD.Print("ShopUI initialized");
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.S)
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
        /// Initialize with manager references
        /// </summary>
        public void Initialize(ShopManager shopManager, InventoryManager inventoryManager)
        {
            _shopManager = shopManager;
            _inventoryManager = inventoryManager;
            RefreshDisplay();
        }
        
        /// <summary>
        /// Toggle shop UI visibility
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
        /// Refresh the entire shop display
        /// </summary>
        public void RefreshDisplay()
        {
            if (_shopManager == null) return;
            
            UpdateCurrencyDisplay();
            UpdateFeaturedItems();
            UpdateShopGrids();
        }
        
        #endregion
        
        #region Private Methods - Display Update
        
        private void UpdateCurrencyDisplay()
        {
            if (CreditsLabel != null)
            {
                CreditsLabel.Text = $"💰 Credits: {CurrencyManager.Credits:N0}";
            }
            
            if (CoresLabel != null)
            {
                CoresLabel.Text = $"💎 Cores: {CurrencyManager.Cores:N0}";
            }
        }
        
        private void UpdateFeaturedItems()
        {
            if (FeaturedPanel == null || _shopManager == null) return;
            
            // Clear existing
            foreach (var child in FeaturedPanel.GetChildren())
            {
                child.QueueFree();
            }
            
            var featuredIDs = _shopManager.GetFeaturedItems();
            
            foreach (var itemID in featuredIDs)
            {
                var shopItem = _shopManager.GetShopItem(itemID);
                if (shopItem != null)
                {
                    var card = CreateShopItemCard(shopItem);
                    FeaturedPanel.AddChild(card);
                }
            }
        }
        
        private void UpdateShopGrids()
        {
            if (_shopManager == null) return;
            
            UpdateShopGrid(CosmeticsGrid, ShopCategory.Cosmetic);
            UpdateShopGrid(ConvenienceGrid, ShopCategory.Convenience);
            UpdateShopGrid(MaterialsGrid, ShopCategory.Material);
        }
        
        private void UpdateShopGrid(GridContainer grid, ShopCategory category)
        {
            if (grid == null) return;
            
            // Clear existing
            foreach (var child in grid.GetChildren())
            {
                child.QueueFree();
            }
            
            var items = _shopManager.GetItemsByCategory(category);
            
            foreach (var shopItem in items)
            {
                var card = CreateShopItemCard(shopItem);
                grid.AddChild(card);
            }
        }
        
        private Panel CreateShopItemCard(ShopItem shopItem)
        {
            var card = new Panel();
            card.CustomMinimumSize = new Vector2(150, 200);
            
            var vbox = new VBoxContainer();
            card.AddChild(vbox);
            
            // Item icon (placeholder)
            var icon = new ColorRect();
            icon.CustomMinimumSize = new Vector2(100, 100);
            icon.Color = GetRarityColor(shopItem.Rarity);
            vbox.AddChild(icon);
            
            // Item name
            var nameLabel = new Label();
            nameLabel.Text = shopItem.DisplayName;
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            vbox.AddChild(nameLabel);
            
            // Price
            var priceLabel = new Label();
            if (shopItem.PriceCredits > 0)
            {
                priceLabel.Text = $"💰 {shopItem.PriceCredits:N0}";
            }
            else if (shopItem.PriceCores > 0)
            {
                priceLabel.Text = $"💎 {shopItem.PriceCores:N0}";
            }
            priceLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(priceLabel);
            
            // Owned indicator (for cosmetics)
            if (shopItem.Category == ShopCategory.Cosmetic && _inventoryManager != null)
            {
                if (_inventoryManager.HasItem(shopItem.ItemID))
                {
                    var ownedLabel = new Label();
                    ownedLabel.Text = "✓ OWNED";
                    ownedLabel.Modulate = Colors.Green;
                    ownedLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    vbox.AddChild(ownedLabel);
                }
            }
            
            // Purchase button
            var buyButton = new Button();
            bool canPurchase = CanAffordItem(shopItem);
            bool alreadyOwned = shopItem.Category == ShopCategory.Cosmetic && 
                              _inventoryManager != null && 
                              _inventoryManager.HasItem(shopItem.ItemID);
            
            if (alreadyOwned)
            {
                buyButton.Text = "OWNED";
                buyButton.Disabled = true;
            }
            else
            {
                buyButton.Text = "BUY";
                buyButton.Disabled = !canPurchase;
                buyButton.Pressed += () => OnPurchaseItemPressed(shopItem);
            }
            
            vbox.AddChild(buyButton);
            
            return card;
        }
        
        private bool CanAffordItem(ShopItem item)
        {
            if (item.PriceCredits > 0 && item.PriceCores == 0)
            {
                return CurrencyManager.HasCredits(item.PriceCredits);
            }
            else if (item.PriceCores > 0 && item.PriceCredits == 0)
            {
                return CurrencyManager.HasCores(item.PriceCores);
            }
            else if (item.PriceCredits > 0 && item.PriceCores > 0)
            {
                return CurrencyManager.HasCredits(item.PriceCredits) && 
                       CurrencyManager.HasCores(item.PriceCores);
            }
            
            return false;
        }
        
        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => new Color(0.7f, 0.7f, 0.7f),
                ItemRarity.Uncommon => new Color(0.1f, 0.8f, 0.1f),
                ItemRarity.Rare => new Color(0.2f, 0.5f, 1.0f),
                ItemRarity.Epic => new Color(0.6f, 0.2f, 0.9f),
                ItemRarity.Legendary => new Color(1.0f, 0.5f, 0.0f),
                ItemRarity.Exotic => new Color(1.0f, 0.9f, 0.0f),
                ItemRarity.Mythic => new Color(1.0f, 0.2f, 0.2f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnPurchaseItemPressed(ShopItem item)
        {
            _pendingPurchase = item;
            
            if (PurchaseConfirm != null)
            {
                string price = item.PriceCredits > 0 ? 
                    $"{item.PriceCredits:N0} Credits" : 
                    $"{item.PriceCores:N0} Cores";
                
                PurchaseConfirm.DialogText = $"Purchase {item.DisplayName} for {price}?";
                PurchaseConfirm.PopupCentered();
            }
            else
            {
                // No confirmation dialog, purchase directly
                ProcessPurchase();
            }
        }
        
        private void OnPurchaseConfirmed()
        {
            ProcessPurchase();
        }
        
        private void OnPurchaseCanceled()
        {
            _pendingPurchase = null;
        }
        
        private void ProcessPurchase()
        {
            if (_pendingPurchase == null || _shopManager == null || _inventoryManager == null)
            {
                return;
            }
            
            bool success = _shopManager.PurchaseItem(_pendingPurchase.ShopItemID, _inventoryManager);
            
            if (success)
            {
                GD.Print($"Successfully purchased: {_pendingPurchase.DisplayName}");
                RefreshDisplay();
            }
            else
            {
                GD.PrintErr($"Failed to purchase: {_pendingPurchase.DisplayName}");
            }
            
            _pendingPurchase = null;
        }
        
        private void OnClosePressed()
        {
            Hide();
        }
        
        private void OnCurrencyChanged(object data)
        {
            UpdateCurrencyDisplay();
        }
        
        private void OnItemPurchased(object data)
        {
            RefreshDisplay();
        }
        
        #endregion
    }
}
