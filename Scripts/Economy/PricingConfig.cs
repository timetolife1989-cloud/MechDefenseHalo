using Godot;
using System.Collections.Generic;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Economy
{
    /// <summary>
    /// Configuration for pricing items and services
    /// </summary>
    public static class PricingConfig
    {
        #region Base Value Definitions

        private static readonly Dictionary<ItemRarity, int> BaseValues = new()
        {
            { ItemRarity.Common, 10 },
            { ItemRarity.Uncommon, 50 },
            { ItemRarity.Rare, 200 },
            { ItemRarity.Epic, 1000 },
            { ItemRarity.Legendary, 5000 },
            { ItemRarity.Exotic, 25000 },
            { ItemRarity.Mythic, 100000 }
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the sell value for an item
        /// </summary>
        /// <param name="item">Item to value</param>
        /// <returns>Sell value in credits</returns>
        public static int GetSellValue(ItemBase item)
        {
            if (item == null)
                return 0;

            // Use item's sell value if set, otherwise calculate from rarity
            if (item.SellValue > 0)
                return item.SellValue;

            int baseValue = GetBaseValue(item.Rarity);
            
            // Apply multipliers based on item type
            float multiplier = item switch
            {
                MechPartItem => 1.5f,      // Equipment is worth more
                WeaponModItem => 1.3f,
                DroneChipItem => 1.4f,
                ConsumableItem => 0.5f,    // Consumables are cheaper
                CraftingMaterialItem => 0.3f, // Materials are cheaper
                CosmeticItem => 2.0f,      // Cosmetics are premium
                _ => 1.0f
            };

            return Mathf.RoundToInt(baseValue * multiplier);
        }

        /// <summary>
        /// Get the crafting cost for a blueprint
        /// </summary>
        /// <param name="resultRarity">Rarity of the crafted item</param>
        /// <returns>Crafting cost in credits</returns>
        public static int GetCraftingCost(ItemRarity resultRarity)
        {
            int baseValue = GetBaseValue(resultRarity);
            return Mathf.RoundToInt(baseValue * 2.5f); // Crafting costs more than selling
        }

        /// <summary>
        /// Get the upgrade cost for improving item rarity
        /// </summary>
        /// <param name="fromRarity">Current rarity</param>
        /// <param name="toRarity">Target rarity</param>
        /// <returns>Upgrade cost in credits</returns>
        public static int GetUpgradeCost(ItemRarity fromRarity, ItemRarity toRarity)
        {
            if (toRarity <= fromRarity)
                return 0;

            int fromValue = GetBaseValue(fromRarity);
            int toValue = GetBaseValue(toRarity);
            
            return Mathf.RoundToInt((toValue - fromValue) * 3.0f);
        }

        /// <summary>
        /// Get the salvage yield for an item
        /// </summary>
        /// <param name="item">Item to salvage</param>
        /// <returns>Amount of materials yielded</returns>
        public static int GetSalvageYield(ItemBase item)
        {
            if (item == null)
                return 0;

            return item.Rarity switch
            {
                ItemRarity.Common => GD.RandRange(2, 5),
                ItemRarity.Uncommon => GD.RandRange(5, 10),
                ItemRarity.Rare => GD.RandRange(10, 15),
                ItemRarity.Epic => GD.RandRange(15, 20),
                ItemRarity.Legendary => GD.RandRange(20, 30),
                ItemRarity.Exotic => GD.RandRange(30, 50),
                ItemRarity.Mythic => GD.RandRange(50, 100),
                _ => 1
            };
        }

        /// <summary>
        /// Get inventory expansion cost
        /// </summary>
        /// <param name="currentSlots">Current number of slots</param>
        /// <param name="slotsToAdd">Number of slots to add</param>
        /// <returns>Cost in cores (premium currency)</returns>
        public static int GetInventoryExpansionCost(int currentSlots, int slotsToAdd)
        {
            // Base cost: 10 cores per slot, increases with current capacity
            float multiplier = 1.0f + (currentSlots / 500.0f);
            return Mathf.RoundToInt(slotsToAdd * 10 * multiplier);
        }

        /// <summary>
        /// Get instant craft cost
        /// </summary>
        /// <param name="remainingSeconds">Remaining craft time in seconds</param>
        /// <returns>Cost in cores to instant complete</returns>
        public static int GetInstantCraftCost(int remainingSeconds)
        {
            // 1 core per minute, minimum 10 cores
            int cores = Mathf.CeilToInt(remainingSeconds / 60.0f);
            return Mathf.Max(10, cores);
        }

        #endregion

        #region Private Methods

        private static int GetBaseValue(ItemRarity rarity)
        {
            return BaseValues.TryGetValue(rarity, out int value) ? value : 10;
        }

        #endregion
    }
}
