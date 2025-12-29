using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Items;
using MechDefenseHalo.Loot;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Inventory;
using MechDefenseHalo.Crafting;

namespace MechDefenseHalo.Debug
{
    /// <summary>
    /// Debug console for testing economy and loot systems
    /// </summary>
    public partial class EconomyDebugConsole : Node
    {
        #region Private Fields

        private InventoryManager _inventory;
        private CraftingManager _crafting;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            GD.Print("=== Economy Debug Console ===");
            GD.Print("Available commands:");
            GD.Print("  give_item <item_id> <quantity>");
            GD.Print("  give_credits <amount>");
            GD.Print("  give_cores <amount>");
            GD.Print("  unlock_blueprint <blueprint_id>");
            GD.Print("  set_rarity <item_id> <rarity>");
            GD.Print("  drop_chest <rarity>");
            GD.Print("  complete_set <set_id>");
            GD.Print("  test_loot_drop <enemy_type>");
            GD.Print("  list_items");
            GD.Print("  show_stats");
            GD.Print("============================");
        }

        #endregion

        #region Public Methods - Debug Commands

        /// <summary>
        /// Give an item to the player
        /// Usage: give_item scrap_metal 100
        /// </summary>
        public void GiveItem(string itemID, int quantity = 1)
        {
            var item = ItemDatabase.GetItem(itemID);
            
            if (item == null)
            {
                GD.PrintErr($"Item not found: {itemID}");
                return;
            }

            if (_inventory == null)
            {
                GD.PrintErr("Inventory not initialized!");
                return;
            }

            if (_inventory.AddItem(item, quantity))
            {
                GD.Print($"[DEBUG] Gave {quantity}x {item.DisplayName}");
            }
        }

        /// <summary>
        /// Give credits to the player
        /// Usage: give_credits 1000
        /// </summary>
        public void GiveCredits(int amount)
        {
            if (CurrencyManager.AddCredits(amount, "debug"))
            {
                GD.Print($"[DEBUG] Gave {amount} credits");
            }
        }

        /// <summary>
        /// Give cores to the player
        /// Usage: give_cores 500
        /// </summary>
        public void GiveCores(int amount)
        {
            if (CurrencyManager.AddCores(amount, "debug"))
            {
                GD.Print($"[DEBUG] Gave {amount} cores");
            }
        }

        /// <summary>
        /// Test loot drop from a specific enemy type
        /// Usage: test_loot_drop Grunt
        /// </summary>
        public void TestLootDrop(string enemyType)
        {
            var items = LootTableManager.RollLoot(enemyType, 1.0f);
            
            GD.Print($"[DEBUG] Loot drop test for {enemyType}:");
            foreach (var itemID in items)
            {
                GD.Print($"  - {itemID}");
            }
        }

        /// <summary>
        /// Simulate opening a chest of specific rarity
        /// Usage: drop_chest Legendary
        /// </summary>
        public void DropChest(string rarityStr)
        {
            if (Enum.TryParse<ItemRarity>(rarityStr, true, out var rarity))
            {
                GD.Print($"[DEBUG] Opening {rarity} chest...");
                
                var chest = new LootChest
                {
                    ChestRarity = rarity,
                    MinItems = 2,
                    MaxItems = 5
                };

                // Simulate opening
                // chest.Open would normally be called with player reference
                GD.Print($"[DEBUG] {rarity} chest opened (simulation)");
            }
            else
            {
                GD.PrintErr($"Invalid rarity: {rarityStr}");
            }
        }

        /// <summary>
        /// List all available items in the database
        /// </summary>
        public void ListItems()
        {
            int count = ItemDatabase.GetItemCount();
            GD.Print($"[DEBUG] Total items in database: {count}");

            // List by rarity
            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
            {
                var items = ItemDatabase.GetItemsByRarity(rarity);
                if (items.Count > 0)
                {
                    GD.Print($"\n{RarityConfig.GetDisplayName(rarity)} ({items.Count}):");
                    foreach (var item in items)
                    {
                        GD.Print($"  - {item.ItemID}: {item.DisplayName}");
                    }
                }
            }
        }

        /// <summary>
        /// Show current player stats
        /// </summary>
        public void ShowStats()
        {
            GD.Print("[DEBUG] Current Stats:");
            GD.Print($"  Credits: {CurrencyManager.Credits}");
            GD.Print($"  Cores: {CurrencyManager.Cores}");
            
            if (_inventory != null)
            {
                GD.Print($"  Inventory: {_inventory.UsedSlots}/{_inventory.MaxSlots} slots");
            }

            GD.Print($"\nLoot Modifiers:");
            GD.Print(LootModifiers.GetModifiersSummary());
        }

        /// <summary>
        /// Set loot difficulty multiplier
        /// </summary>
        public void SetDifficulty(string difficulty)
        {
            LootModifiers.SetDifficultyMultiplier(difficulty);
            GD.Print($"[DEBUG] Difficulty set to {difficulty}");
        }

        /// <summary>
        /// Test rarity rolling
        /// </summary>
        public void TestRarityRoll(int count = 100)
        {
            var results = new Dictionary<ItemRarity, int>();
            
            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
            {
                results[rarity] = 0;
            }

            for (int i = 0; i < count; i++)
            {
                var rolled = RarityConfig.RollRarity(1.0f);
                results[rolled]++;
            }

            GD.Print($"[DEBUG] Rarity roll test ({count} rolls):");
            foreach (var kvp in results)
            {
                float percentage = (kvp.Value / (float)count) * 100f;
                GD.Print($"  {RarityConfig.GetDisplayName(kvp.Key)}: {kvp.Value} ({percentage:F1}%)");
            }
        }

        /// <summary>
        /// Clear inventory
        /// </summary>
        public void ClearInventory()
        {
            if (_inventory != null)
            {
                _inventory.ClearInventory();
                GD.Print("[DEBUG] Inventory cleared");
            }
        }

        /// <summary>
        /// Reset all currencies
        /// </summary>
        public void ResetCurrency()
        {
            CurrencyManager.ResetCurrencies();
            GD.Print("[DEBUG] Currencies reset");
        }

        #endregion

        #region Private Methods

        public void SetInventoryManager(InventoryManager inventory)
        {
            _inventory = inventory;
        }

        public void SetCraftingManager(CraftingManager crafting)
        {
            _crafting = crafting;
        }

        #endregion
    }
}
