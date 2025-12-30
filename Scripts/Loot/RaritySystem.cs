using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.Items
{
    /// <summary>
    /// Centralized rarity system configuration
    /// Provides utilities for working with item rarities, colors, and drop rates
    /// 
    /// This class complements the existing RarityConfig in ItemRarity.cs by providing:
    /// - Additional utility methods for rarity checks (IsRareOrAbove, IsEpicOrAbove, etc.)
    /// - Visual feedback configuration (glow intensity, particle multipliers, scale)
    /// - Extended display utilities (colored names, symbols, descriptions)
    /// - Stat and sell value multipliers
    /// 
    /// Use RarityConfig for: Basic color/drop rate lookups and random rarity rolls
    /// Use RaritySystem for: Extended utilities, visual effects config, and advanced operations
    /// </summary>
    public static class RaritySystem
    {
        #region Rarity Tier Configuration

        /// <summary>
        /// Get the tier level for a rarity (0-6)
        /// </summary>
        public static int GetTier(ItemRarity rarity)
        {
            return (int)rarity;
        }

        /// <summary>
        /// Get rarity from tier level
        /// </summary>
        public static ItemRarity GetRarityFromTier(int tier)
        {
            tier = Mathf.Clamp(tier, 0, 6);
            return (ItemRarity)tier;
        }

        /// <summary>
        /// Check if a rarity is considered "rare" (Rare or above)
        /// </summary>
        public static bool IsRareOrAbove(ItemRarity rarity)
        {
            return (int)rarity >= (int)ItemRarity.Rare;
        }

        /// <summary>
        /// Check if a rarity is considered "epic" (Epic or above)
        /// </summary>
        public static bool IsEpicOrAbove(ItemRarity rarity)
        {
            return (int)rarity >= (int)ItemRarity.Epic;
        }

        /// <summary>
        /// Check if a rarity is legendary tier
        /// </summary>
        public static bool IsLegendaryTier(ItemRarity rarity)
        {
            return (int)rarity >= (int)ItemRarity.Legendary;
        }

        #endregion

        #region Color Utilities

        /// <summary>
        /// Get a color for UI display based on rarity
        /// </summary>
        public static Color GetUIColor(ItemRarity rarity)
        {
            return RarityConfig.GetColor(rarity);
        }

        /// <summary>
        /// Get an emission color for 3D objects (slightly brighter than base color)
        /// </summary>
        public static Color GetEmissionColor(ItemRarity rarity)
        {
            Color baseColor = RarityConfig.GetColor(rarity);
            return baseColor * 1.5f; // Brighter for emission
        }

        /// <summary>
        /// Get a particle color for effects
        /// </summary>
        public static Color GetParticleColor(ItemRarity rarity)
        {
            return RarityConfig.GetColor(rarity);
        }

        /// <summary>
        /// Get color with custom alpha
        /// </summary>
        public static Color GetColorWithAlpha(ItemRarity rarity, float alpha)
        {
            Color color = RarityConfig.GetColor(rarity);
            color.A = alpha;
            return color;
        }

        #endregion

        #region Visual Feedback Configuration

        /// <summary>
        /// Get glow intensity based on rarity
        /// </summary>
        public static float GetGlowIntensity(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => 0.3f,
                ItemRarity.Uncommon => 0.5f,
                ItemRarity.Rare => 0.8f,
                ItemRarity.Epic => 1.2f,
                ItemRarity.Legendary => 1.5f,
                ItemRarity.Exotic => 2.0f,
                ItemRarity.Mythic => 2.5f,
                _ => 0.5f
            };
        }

        /// <summary>
        /// Get particle count multiplier based on rarity
        /// </summary>
        public static float GetParticleMultiplier(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => 0.5f,
                ItemRarity.Uncommon => 0.8f,
                ItemRarity.Rare => 1.0f,
                ItemRarity.Epic => 1.5f,
                ItemRarity.Legendary => 2.0f,
                ItemRarity.Exotic => 2.5f,
                ItemRarity.Mythic => 3.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Get scale multiplier for visual emphasis
        /// </summary>
        public static float GetScaleMultiplier(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => 1.0f,
                ItemRarity.Uncommon => 1.1f,
                ItemRarity.Rare => 1.15f,
                ItemRarity.Epic => 1.2f,
                ItemRarity.Legendary => 1.3f,
                ItemRarity.Exotic => 1.4f,
                ItemRarity.Mythic => 1.5f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Should this rarity show a beam effect?
        /// </summary>
        public static bool ShowBeamEffect(ItemRarity rarity)
        {
            return (int)rarity >= (int)ItemRarity.Epic;
        }

        /// <summary>
        /// Should this rarity play a special sound?
        /// </summary>
        public static bool PlaySpecialSound(ItemRarity rarity)
        {
            return (int)rarity >= (int)ItemRarity.Rare;
        }

        #endregion

        #region Drop Rate Utilities

        /// <summary>
        /// Get the base drop rate for a rarity
        /// </summary>
        public static float GetDropRate(ItemRarity rarity)
        {
            return RarityConfig.GetDropRate(rarity);
        }

        /// <summary>
        /// Calculate modified drop rate with luck
        /// </summary>
        public static float GetModifiedDropRate(ItemRarity rarity, float luckModifier)
        {
            float baseRate = GetDropRate(rarity);
            
            // Luck only affects rare+ items
            if (IsRareOrAbove(rarity))
            {
                return baseRate * luckModifier;
            }
            
            return baseRate;
        }

        /// <summary>
        /// Get all rarities sorted by tier (common to mythic)
        /// </summary>
        public static ItemRarity[] GetAllRaritiesSorted()
        {
            return new[]
            {
                ItemRarity.Common,
                ItemRarity.Uncommon,
                ItemRarity.Rare,
                ItemRarity.Epic,
                ItemRarity.Legendary,
                ItemRarity.Exotic,
                ItemRarity.Mythic
            };
        }

        /// <summary>
        /// Get all rarities in reverse order (mythic to common)
        /// </summary>
        public static ItemRarity[] GetAllRaritiesReverse()
        {
            return new[]
            {
                ItemRarity.Mythic,
                ItemRarity.Exotic,
                ItemRarity.Legendary,
                ItemRarity.Epic,
                ItemRarity.Rare,
                ItemRarity.Uncommon,
                ItemRarity.Common
            };
        }

        #endregion

        #region Display Utilities

        /// <summary>
        /// Get display name for rarity
        /// </summary>
        public static string GetDisplayName(ItemRarity rarity)
        {
            return RarityConfig.GetDisplayName(rarity);
        }

        /// <summary>
        /// Get a formatted string with color tags for rich text
        /// </summary>
        public static string GetColoredDisplayName(ItemRarity rarity)
        {
            Color color = GetUIColor(rarity);
            string hexColor = color.ToHtml(false);
            return $"[color=#{hexColor}]{GetDisplayName(rarity)}[/color]";
        }

        /// <summary>
        /// Get rarity description
        /// </summary>
        public static string GetDescription(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => "Common items with standard properties",
                ItemRarity.Uncommon => "Uncommon items with slightly enhanced properties",
                ItemRarity.Rare => "Rare items with notably improved stats",
                ItemRarity.Epic => "Epic items with exceptional attributes",
                ItemRarity.Legendary => "Legendary items with unique powers",
                ItemRarity.Exotic => "Exotic items with game-changing abilities",
                ItemRarity.Mythic => "Mythic items of unparalleled power",
                _ => "Unknown rarity"
            };
        }

        /// <summary>
        /// Get a symbol/icon character for the rarity
        /// </summary>
        public static string GetSymbol(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => "◆",
                ItemRarity.Uncommon => "◇",
                ItemRarity.Rare => "★",
                ItemRarity.Epic => "◈",
                ItemRarity.Legendary => "✦",
                ItemRarity.Exotic => "❖",
                ItemRarity.Mythic => "✨",
                _ => "?"
            };
        }

        #endregion

        #region Stat Scaling

        /// <summary>
        /// Get stat multiplier based on rarity
        /// </summary>
        public static float GetStatMultiplier(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => 1.0f,
                ItemRarity.Uncommon => 1.2f,
                ItemRarity.Rare => 1.5f,
                ItemRarity.Epic => 2.0f,
                ItemRarity.Legendary => 2.5f,
                ItemRarity.Exotic => 3.0f,
                ItemRarity.Mythic => 4.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Get sell value multiplier based on rarity
        /// </summary>
        public static float GetSellValueMultiplier(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => 1.0f,
                ItemRarity.Uncommon => 2.0f,
                ItemRarity.Rare => 5.0f,
                ItemRarity.Epic => 10.0f,
                ItemRarity.Legendary => 25.0f,
                ItemRarity.Exotic => 50.0f,
                ItemRarity.Mythic => 100.0f,
                _ => 1.0f
            };
        }

        #endregion

        #region Random Selection

        /// <summary>
        /// Roll a random rarity using the standard drop rate table
        /// </summary>
        public static ItemRarity RollRandom(float luckModifier = 1.0f)
        {
            return RarityConfig.RollRarity(luckModifier);
        }

        /// <summary>
        /// Roll a random rarity within a tier range
        /// </summary>
        public static ItemRarity RollInRange(ItemRarity minRarity, ItemRarity maxRarity)
        {
            int minTier = (int)minRarity;
            int maxTier = (int)maxRarity;
            int rolledTier = GD.RandRange(minTier, maxTier);
            return (ItemRarity)rolledTier;
        }

        #endregion
    }
}
