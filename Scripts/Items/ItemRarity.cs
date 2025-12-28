using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Items
{
    /// <summary>
    /// Item rarity levels with associated drop rates and visual colors
    /// </summary>
    public enum ItemRarity
    {
        Common = 0,      // Grey - 60% drop
        Uncommon = 1,    // Green - 25% drop
        Rare = 2,        // Blue - 10% drop
        Epic = 3,        // Purple - 4% drop
        Legendary = 4,   // Orange - 0.9% drop
        Exotic = 5,      // Rainbow - 0.1% (quest only)
        Mythic = 6       // Prismatic - 0.01% (world boss only)
    }

    /// <summary>
    /// Configuration for item rarity system
    /// </summary>
    public static class RarityConfig
    {
        #region Color Definitions

        private static readonly Dictionary<ItemRarity, Color> _rarityColors = new()
        {
            { ItemRarity.Common, new Color(0.6f, 0.6f, 0.6f) },      // Grey
            { ItemRarity.Uncommon, new Color(0.2f, 0.8f, 0.2f) },    // Green
            { ItemRarity.Rare, new Color(0.2f, 0.4f, 1.0f) },        // Blue
            { ItemRarity.Epic, new Color(0.6f, 0.2f, 0.8f) },        // Purple
            { ItemRarity.Legendary, new Color(1.0f, 0.6f, 0.0f) },   // Orange
            { ItemRarity.Exotic, new Color(1.0f, 0.8f, 0.2f) },      // Golden/Rainbow
            { ItemRarity.Mythic, new Color(1.0f, 0.2f, 1.0f) }       // Prismatic/Magenta
        };

        #endregion

        #region Drop Rate Definitions

        private static readonly Dictionary<ItemRarity, float> _dropRates = new()
        {
            { ItemRarity.Common, 0.60f },      // 60%
            { ItemRarity.Uncommon, 0.25f },    // 25%
            { ItemRarity.Rare, 0.10f },        // 10%
            { ItemRarity.Epic, 0.04f },        // 4%
            { ItemRarity.Legendary, 0.009f },  // 0.9%
            { ItemRarity.Exotic, 0.001f },     // 0.1%
            { ItemRarity.Mythic, 0.0001f }     // 0.01%
        };

        #endregion

        #region Display Names

        private static readonly Dictionary<ItemRarity, string> _displayNames = new()
        {
            { ItemRarity.Common, "Common" },
            { ItemRarity.Uncommon, "Uncommon" },
            { ItemRarity.Rare, "Rare" },
            { ItemRarity.Epic, "Epic" },
            { ItemRarity.Legendary, "Legendary" },
            { ItemRarity.Exotic, "Exotic" },
            { ItemRarity.Mythic, "Mythic" }
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the display color for a given rarity
        /// </summary>
        /// <param name="rarity">The rarity level</param>
        /// <returns>Color associated with the rarity</returns>
        public static Color GetColor(ItemRarity rarity)
        {
            return _rarityColors.TryGetValue(rarity, out var color) 
                ? color 
                : _rarityColors[ItemRarity.Common];
        }

        /// <summary>
        /// Get the drop rate for a given rarity
        /// </summary>
        /// <param name="rarity">The rarity level</param>
        /// <returns>Drop rate as a decimal (0.0 to 1.0)</returns>
        public static float GetDropRate(ItemRarity rarity)
        {
            return _dropRates.TryGetValue(rarity, out var rate) 
                ? rate 
                : _dropRates[ItemRarity.Common];
        }

        /// <summary>
        /// Get the display name for a given rarity
        /// </summary>
        /// <param name="rarity">The rarity level</param>
        /// <returns>Human-readable name of the rarity</returns>
        public static string GetDisplayName(ItemRarity rarity)
        {
            return _displayNames.TryGetValue(rarity, out var name) 
                ? name 
                : _displayNames[ItemRarity.Common];
        }

        /// <summary>
        /// Roll a random rarity based on drop rates
        /// </summary>
        /// <param name="luckModifier">Multiplier for rare drop chances (1.0 = normal)</param>
        /// <returns>Randomly selected rarity</returns>
        public static ItemRarity RollRarity(float luckModifier = 1.0f)
        {
            float roll = GD.Randf();
            float cumulative = 0f;

            // Start from highest rarity and work down
            var rarities = new[] 
            { 
                ItemRarity.Mythic, 
                ItemRarity.Exotic, 
                ItemRarity.Legendary, 
                ItemRarity.Epic, 
                ItemRarity.Rare, 
                ItemRarity.Uncommon, 
                ItemRarity.Common 
            };

            foreach (var rarity in rarities)
            {
                float rate = GetDropRate(rarity);
                
                // Apply luck modifier to rare drops (Rare and above)
                if ((int)rarity >= (int)ItemRarity.Rare)
                {
                    rate *= luckModifier;
                }

                cumulative += rate;

                if (roll <= cumulative)
                {
                    return rarity;
                }
            }

            return ItemRarity.Common; // Fallback
        }

        #endregion
    }
}
