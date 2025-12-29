using System;
using System.Collections.Generic;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Statistics
{
    /// <summary>
    /// Tracks all economy-related statistics
    /// </summary>
    public class EconomyStats
    {
        // Currency
        public int TotalCreditsEarned { get; set; } = 0;
        public int TotalCreditsSpent { get; set; } = 0;
        public int TotalCoresEarned { get; set; } = 0;
        public int TotalCoresSpent { get; set; } = 0;
        
        // Items
        public int ItemsLooted { get; set; } = 0;
        public int ItemsCrafted { get; set; } = 0;
        public int ItemsSalvaged { get; set; } = 0;
        public Dictionary<ItemRarity, int> ItemsByRarity { get; set; } = new Dictionary<ItemRarity, int>();
        
        // Shop
        public int ShopPurchases { get; set; } = 0;
        public int TotalSpentInShop { get; set; } = 0;
        
        // Loot
        public int ChestsOpened { get; set; } = 0;
        public int LegendariesObtained { get; set; } = 0;

        // Helper methods
        public void RecordItemObtained(ItemRarity rarity)
        {
            if (!ItemsByRarity.ContainsKey(rarity))
                ItemsByRarity[rarity] = 0;
            ItemsByRarity[rarity]++;

            if (rarity >= ItemRarity.Legendary)
                LegendariesObtained++;
        }
    }
}
