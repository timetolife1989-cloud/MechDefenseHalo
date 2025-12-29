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
    /// Shop UI with 3 categories, featured items, and purchase confirmation.
    /// 
    /// REQUIRED SCENE STRUCTURE (create manually in Godot):
    /// 
    /// Control (ShopUI) - Script: ShopUI.cs
    /// ├─ Panel (Background)
    /// │  ├─ Label (Title) - text: "SHOP"
    /// │  ├─ HBoxContainer (CurrencyBar)
    /// │  │  ├─ Label (CreditsLabel) - text: "💰 Credits: 0"
    /// │  │  └─ Label (CoresLabel) - text: "💎 Cores: 0"
    /// │  ├─ Panel (FeaturedPanel) - Daily Rotation
    /// │  │  ├─ Label - text: "Featured Today"
    /// │  │  └─ HBoxContainer (FeaturedContainer)
    /// │  ├─ TabContainer (CategoryTabs)
    /// │  │  ├─ ScrollContainer (CosmeticsTab) - name: "Cosmetics"
    /// │  │  │  └─ GridContainer (CosmeticsGrid) - columns: 4
    /// │  │  ├─ ScrollContainer (ConvenienceTab) - name: "Convenience"
    /// │  │  │  └─ GridContainer (ConvenienceGrid) - columns: 4
    /// │  │  └─ ScrollContainer (MaterialsTab) - name: "Materials"
    /// │  │     └─ GridContainer (MaterialsGrid) - columns: 4
    /// │  ├─ Button (CloseButton) - text: "Close"
    /// │  └─ ConfirmationDialog (PurchaseConfirm) - title: "Confirm Purchase"
    /// </summary>
    public partial class ShopUI : Control
    {
        #region Export Variables (Wire these in Godot Editor)

        [Export] public TabContainer CategoryTabs { get; set; }
        [Export] public GridContainer CosmeticsGrid { get; set; }
        [Export] public GridContainer ConvenienceGrid { get; set; }
        [Export] public GridContainer MaterialsGrid { get; set; }
        [Export] public HBoxContainer FeaturedContainer { get; set; }
        [Export] public Label CreditsLabel { get; set; }
        [Export] public Label CoresLabel { get; set; }
        [Export] public Button CloseButton { get; set; }
        [Export] public ConfirmationDialog PurchaseConfirm { get; set; }
        [Export] public PackedScene ShopItemCardPrefab { get; set; } // Reference to ShopItemCard.tscn

        #endregion

        #region Private Fields

        private ShopManager _shopManager;
        private InventoryManager _inventoryManager;
        private ShopItem _pendingPurchase;
        private List<ShopItemCardUI> _itemCards = new();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get manager references
            _shopManager = GetNode<ShopManager>("/root/ShopManager");
            _inventoryManager = GetNode<InventoryManager>("/root/InventoryManager");

            // Connect signals
            if (CloseButton != null)
                CloseButton.Pressed += OnClosePressed;

            if (PurchaseConfirm != null)
            {
                PurchaseConfirm.Confirmed += OnPurchaseConfirmed;
                PurchaseConfirm.Canceled += OnPurchaseCanceled;
            }

            // Listen for currency and purchase events
            EventBus.On(EventBus.CurrencyChanged, OnCurrencyChangedEvent);
            EventBus.On(EventBus.ItemPurchased, OnItemPurchasedEvent);

            // Initial display
            RefreshDisplay();

            // Hide by default
            Hide();

            GD.Print("ShopUI initialized");
        }

        public override void _Input(InputEvent @event)
        {
            // Toggle with 'S' key
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.S)
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
                RefreshDisplay();
            }
        }

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
                CreditsLabel.Text = $"💰 Credits: {CurrencyManager.CurrentCredits:N0}";
            }

            if (CoresLabel != null)
            {
                CoresLabel.Text = $"💎 Cores: {CurrencyManager.CurrentCores:N0}";
            }
        }

        private void UpdateFeaturedItems()
        {
            if (FeaturedContainer == null || _shopManager == null) return;

            // Clear existing cards
            foreach (var child in FeaturedContainer.GetChildren())
            {
                child.QueueFree();
            }

            var featuredIDs = _shopManager.GetFeaturedItems();

            if (ShopItemCardPrefab != null)
            {
                foreach (var itemID in featuredIDs)
                {
                    var shopItem = _shopManager.GetShopItem(itemID);
                    if (shopItem != null)
                    {
                        var card = CreateShopItemCard(shopItem);
                        FeaturedContainer.AddChild(card);
                    }
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

            // Clear existing cards
            foreach (var child in grid.GetChildren())
            {
                child.QueueFree();
            }

            var items = _shopManager.GetItemsByCategory(category);

            if (ShopItemCardPrefab != null)
            {
                foreach (var shopItem in items)
                {
                    var card = CreateShopItemCard(shopItem);
                    grid.AddChild(card);
                }
            }
        }

        private ShopItemCardUI CreateShopItemCard(ShopItem shopItem)
        {
            var card = ShopItemCardPrefab.Instantiate<ShopItemCardUI>();
            card.SetShopItem(shopItem);
            card.UpdateAffordability(CurrencyManager.CurrentCredits, CurrencyManager.CurrentCores);

            // Connect purchase event
            card.PurchaseRequested += OnPurchaseItemPressed;

            _itemCards.Add(card);
            return card;
        }

        #endregion

        #region Event Handlers

        private void OnPurchaseItemPressed(string shopItemID)
        {
            if (_shopManager == null) return;

            var shopItem = _shopManager.GetShopItem(shopItemID);
            if (shopItem == null) return;

            _pendingPurchase = shopItem;

            if (PurchaseConfirm != null)
            {
                string price = shopItem.PriceCredits > 0 ?
                    $"{shopItem.PriceCredits:N0} Credits" :
                    $"{shopItem.PriceCores:N0} Cores";

                PurchaseConfirm.DialogText = $"Purchase {shopItem.DisplayName} for {price}?";
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

        private void OnCurrencyChangedEvent(object data)
        {
            UpdateCurrencyDisplay();

            // Update affordability of all cards
            foreach (var card in _itemCards)
            {
                card.UpdateAffordability(CurrencyManager.CurrentCredits, CurrencyManager.CurrentCores);
            }
        }

        private void OnItemPurchasedEvent(object data)
        {
            RefreshDisplay();
        }

        #endregion
    }
}
