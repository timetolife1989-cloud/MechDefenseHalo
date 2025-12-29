using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;
using MechDefenseHalo.Items;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Inventory;

namespace MechDefenseHalo.Shop
{
    /// <summary>
    /// Manages the in-game shop with multiple categories
    /// </summary>
    public partial class ShopManager : Node
    {
        #region Private Fields

        private Dictionary<string, ShopItem> _shopItems = new();
        private List<string> _featuredItems = new();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            LoadShopItems();
            RefreshFeaturedItems();
            GD.Print($"ShopManager initialized with {_shopItems.Count} items");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Purchase an item from the shop
        /// </summary>
        /// <param name="shopItemID">Shop item ID</param>
        /// <param name="inventory">Player inventory to add item to</param>
        /// <returns>True if purchase successful</returns>
        public bool PurchaseItem(string shopItemID, InventoryManager inventory)
        {
            if (!_shopItems.TryGetValue(shopItemID, out var shopItem))
            {
                GD.PrintErr($"Shop item not found: {shopItemID}");
                return false;
            }

            // Check if already owned (for cosmetics)
            if (shopItem.Category == ShopCategory.Cosmetic && inventory.HasItem(shopItem.ItemID))
            {
                GD.PrintErr($"Already own {shopItem.DisplayName}");
                return false;
            }

            // Check currency requirements
            bool canAfford = false;

            if (shopItem.PriceCredits > 0 && shopItem.PriceCores == 0)
            {
                canAfford = CurrencyManager.HasCredits(shopItem.PriceCredits);
            }
            else if (shopItem.PriceCores > 0 && shopItem.PriceCredits == 0)
            {
                canAfford = CurrencyManager.HasCores(shopItem.PriceCores);
            }
            else if (shopItem.PriceCredits > 0 && shopItem.PriceCores > 0)
            {
                canAfford = CurrencyManager.HasCredits(shopItem.PriceCredits) && 
                           CurrencyManager.HasCores(shopItem.PriceCores);
            }

            if (!canAfford)
            {
                GD.PrintErr($"Cannot afford {shopItem.DisplayName}");
                return false;
            }

            // Spend currency
            if (shopItem.PriceCredits > 0)
            {
                CurrencyManager.SpendCredits(shopItem.PriceCredits, $"purchasing {shopItem.DisplayName}");
            }

            if (shopItem.PriceCores > 0)
            {
                CurrencyManager.SpendCores(shopItem.PriceCores, $"purchasing {shopItem.DisplayName}");
            }

            // Add item to inventory
            var item = ItemDatabase.GetItem(shopItem.ItemID);
            if (item != null)
            {
                inventory.AddItem(item, shopItem.Quantity);
            }

            GD.Print($"Purchased: {shopItem.DisplayName}");

            // Emit event
            EventBus.Emit(EventBus.ItemPurchased, new ItemPurchasedData
            {
                ShopItemID = shopItemID,
                ItemName = shopItem.DisplayName,
                PriceCredits = shopItem.PriceCredits,
                PriceCores = shopItem.PriceCores
            });

            return true;
        }

        /// <summary>
        /// Get shop items by category
        /// </summary>
        /// <param name="category">Shop category</param>
        /// <returns>List of shop items</returns>
        public List<ShopItem> GetItemsByCategory(ShopCategory category)
        {
            return _shopItems.Values
                .Where(item => item.Category == category)
                .ToList();
        }

        /// <summary>
        /// Get featured items (daily rotation)
        /// </summary>
        /// <returns>List of featured item IDs</returns>
        public List<string> GetFeaturedItems()
        {
            return new List<string>(_featuredItems);
        }

        /// <summary>
        /// Refresh featured items (call daily)
        /// </summary>
        public void RefreshFeaturedItems()
        {
            _featuredItems.Clear();

            // Select 3 random items to feature
            var allItems = _shopItems.Values.ToList();
            
            for (int i = 0; i < Mathf.Min(3, allItems.Count); i++)
            {
                var randomItem = allItems[GD.RandRange(0, allItems.Count - 1)];
                _featuredItems.Add(randomItem.ShopItemID);
                allItems.Remove(randomItem);
            }

            GD.Print($"Featured items refreshed: {_featuredItems.Count} items");
        }

        /// <summary>
        /// Get a shop item by ID
        /// </summary>
        /// <param name="shopItemID">Shop item ID</param>
        /// <returns>Shop item or null</returns>
        public ShopItem GetShopItem(string shopItemID)
        {
            return _shopItems.TryGetValue(shopItemID, out var item) ? item : null;
        }

        #endregion

        #region Private Methods

        private void LoadShopItems()
        {
            _shopItems.Clear();

            // Create sample shop items
            CreateSampleShopItems();

            GD.Print($"Loaded {_shopItems.Count} shop items");
        }

        private void CreateSampleShopItems()
        {
            // Cosmetics (Cores only)
            AddShopItem(new ShopItem
            {
                ShopItemID = "shop_skin_obsidian",
                ItemID = "skin_mech_obsidian",
                DisplayName = "Obsidian Warframe Skin",
                Description = "Sleek black and gold mech skin",
                Category = ShopCategory.Cosmetic,
                PriceCredits = 0,
                PriceCores = 2000,
                Rarity = ItemRarity.Legendary,
                RequiredLevel = 20
            });

            AddShopItem(new ShopItem
            {
                ShopItemID = "shop_emote_victory",
                ItemID = "emote_victory_pose",
                DisplayName = "Victory Pose Emote",
                Description = "Strike a heroic victory pose",
                Category = ShopCategory.Cosmetic,
                PriceCredits = 0,
                PriceCores = 500,
                Rarity = ItemRarity.Rare,
                RequiredLevel = 1
            });

            // Convenience (Cores only)
            AddShopItem(new ShopItem
            {
                ShopItemID = "shop_inventory_expansion",
                ItemID = "inventory_expansion_50",
                DisplayName = "Inventory Expansion +50",
                Description = "Permanently adds 50 inventory slots",
                Category = ShopCategory.Convenience,
                PriceCredits = 0,
                PriceCores = 500,
                Rarity = ItemRarity.Epic,
                RequiredLevel = 1
            });

            AddShopItem(new ShopItem
            {
                ShopItemID = "shop_instant_craft_token",
                ItemID = "instant_craft_token",
                DisplayName = "Instant Craft Token",
                Description = "Instantly complete any crafting job",
                Category = ShopCategory.Convenience,
                PriceCredits = 0,
                PriceCores = 50,
                Rarity = ItemRarity.Uncommon,
                RequiredLevel = 1,
                Quantity = 1
            });

            // Materials (Credits only - rotating stock)
            AddShopItem(new ShopItem
            {
                ShopItemID = "shop_scrap_metal_bundle",
                ItemID = "scrap_metal",
                DisplayName = "Scrap Metal Bundle",
                Description = "100x Scrap Metal",
                Category = ShopCategory.Material,
                PriceCredits = 500,
                PriceCores = 0,
                Rarity = ItemRarity.Common,
                RequiredLevel = 1,
                Quantity = 100
            });

            AddShopItem(new ShopItem
            {
                ShopItemID = "shop_plasma_core_pack",
                ItemID = "plasma_core",
                DisplayName = "Plasma Core Pack",
                Description = "10x Plasma Cores",
                Category = ShopCategory.Material,
                PriceCredits = 2000,
                PriceCores = 0,
                Rarity = ItemRarity.Rare,
                RequiredLevel = 10,
                Quantity = 10
            });
        }

        private void AddShopItem(ShopItem item)
        {
            _shopItems[item.ShopItemID] = item;
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Shop item definition
    /// </summary>
    public class ShopItem
    {
        public string ShopItemID { get; set; } = "";
        public string ItemID { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";
        public ShopCategory Category { get; set; } = ShopCategory.Cosmetic;
        public int PriceCredits { get; set; } = 0;
        public int PriceCores { get; set; } = 0;
        public ItemRarity Rarity { get; set; } = ItemRarity.Common;
        public int RequiredLevel { get; set; } = 1;
        public int Quantity { get; set; } = 1;
        public bool LimitedStock { get; set; } = false;
    }

    /// <summary>
    /// Shop categories
    /// </summary>
    public enum ShopCategory
    {
        Cosmetic,
        Convenience,
        Material
    }

    /// <summary>
    /// Data for item purchased event
    /// </summary>
    public class ItemPurchasedData
    {
        public string ShopItemID { get; set; }
        public string ItemName { get; set; }
        public int PriceCredits { get; set; }
        public int PriceCores { get; set; }
    }

    #endregion
}
