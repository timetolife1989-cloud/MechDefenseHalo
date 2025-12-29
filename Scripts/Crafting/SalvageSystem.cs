using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Items;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Crafting
{
    /// <summary>
    /// Handles salvaging (dismantling) items for crafting materials
    /// </summary>
    public static class SalvageSystem
    {
        #region Public Methods

        /// <summary>
        /// Salvage an item for materials
        /// </summary>
        /// <param name="item">Item to salvage</param>
        /// <returns>Dictionary of material IDs and quantities</returns>
        public static Dictionary<string, int> SalvageItem(ItemBase item)
        {
            if (item == null)
            {
                GD.PrintErr("Cannot salvage null item");
                return new Dictionary<string, int>();
            }

            var materials = new Dictionary<string, int>();

            // Determine material type and quantity based on rarity
            string materialType = GetMaterialTypeForRarity(item.Rarity);
            int quantity = PricingConfig.GetSalvageYield(item);

            materials[materialType] = quantity;

            // Higher rarity items also yield common materials
            if (item.Rarity >= ItemRarity.Rare)
            {
                materials["scrap_metal"] = GD.RandRange(5, 10);
            }

            if (item.Rarity >= ItemRarity.Epic)
            {
                materials["circuits"] = GD.RandRange(10, 20);
                materials["alloy_plates"] = GD.RandRange(5, 10);
            }

            GD.Print($"Salvaged {item.DisplayName} for:");
            foreach (var mat in materials)
            {
                GD.Print($"  {mat.Value}x {mat.Key}");
            }

            // Emit event
            EventBus.Emit("item_salvaged", new ItemSalvagedData
            {
                Item = item,
                Materials = materials
            });

            return materials;
        }

        /// <summary>
        /// Salvage multiple items at once (bulk salvage)
        /// </summary>
        /// <param name="items">List of items to salvage</param>
        /// <returns>Combined materials from all items</returns>
        public static Dictionary<string, int> SalvageItems(List<ItemBase> items)
        {
            var totalMaterials = new Dictionary<string, int>();

            foreach (var item in items)
            {
                var materials = SalvageItem(item);
                
                foreach (var mat in materials)
                {
                    if (!totalMaterials.ContainsKey(mat.Key))
                    {
                        totalMaterials[mat.Key] = 0;
                    }
                    totalMaterials[mat.Key] += mat.Value;
                }
            }

            return totalMaterials;
        }

        /// <summary>
        /// Get the primary material type for a given rarity
        /// </summary>
        /// <param name="rarity">Item rarity</param>
        /// <returns>Material item ID</returns>
        public static string GetMaterialTypeForRarity(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => "scrap_metal",
                ItemRarity.Uncommon => "alloy_plates",
                ItemRarity.Rare => "plasma_core",
                ItemRarity.Epic => "quantum_chips",
                ItemRarity.Legendary => "void_crystal",
                ItemRarity.Exotic => "aether_fragment",
                ItemRarity.Mythic => "aether_fragment", // Mythic also gives aether
                _ => "scrap_metal"
            };
        }

        /// <summary>
        /// Check if an item can be salvaged
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>True if item can be salvaged</returns>
        public static bool CanSalvage(ItemBase item)
        {
            if (item == null) return false;

            // Cannot salvage materials themselves
            if (item is CraftingMaterialItem) return false;

            // Cannot salvage consumables
            if (item is ConsumableItem) return false;

            return true;
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Data for item salvaged event
    /// </summary>
    public class ItemSalvagedData
    {
        public ItemBase Item { get; set; }
        public Dictionary<string, int> Materials { get; set; }
    }

    #endregion
}
