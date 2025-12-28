using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Items
{
    /// <summary>
    /// Central database for all items in the game
    /// Loads item definitions from JSON files on startup
    /// </summary>
    public partial class ItemDatabase : Node
    {
        #region Singleton

        private static ItemDatabase _instance;

        public static ItemDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("ItemDatabase accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Private Fields

        private Dictionary<string, ItemBase> _items = new();
        private const string ITEMS_PATH = "res://Data/Items/";

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple ItemDatabase instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            LoadAllItems();
            GD.Print("ItemDatabase initialized");
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get an item by ID
        /// </summary>
        /// <param name="itemID">Item identifier</param>
        /// <returns>Item or null if not found</returns>
        public static ItemBase GetItem(string itemID)
        {
            if (Instance == null) return null;

            if (Instance._items.TryGetValue(itemID, out var item))
            {
                return item.Clone(); // Return a clone to prevent modification of the template
            }

            GD.PrintErr($"Item not found in database: {itemID}");
            return null;
        }

        /// <summary>
        /// Get items by category
        /// </summary>
        /// <param name="category">Category name</param>
        /// <returns>List of items in the category</returns>
        public static List<ItemBase> GetItemsByCategory(string category)
        {
            if (Instance == null) return new List<ItemBase>();

            return Instance._items.Values
                .Where(item => item.Tags.Contains(category))
                .Select(item => item.Clone())
                .ToList();
        }

        /// <summary>
        /// Get items by type
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <returns>List of items of the specified type</returns>
        public static List<T> GetItemsByType<T>() where T : ItemBase
        {
            if (Instance == null) return new List<T>();

            return Instance._items.Values
                .OfType<T>()
                .Select(item => (T)item.Clone())
                .ToList();
        }

        /// <summary>
        /// Get items by rarity
        /// </summary>
        /// <param name="rarity">Rarity level</param>
        /// <returns>List of items with the specified rarity</returns>
        public static List<ItemBase> GetItemsByRarity(ItemRarity rarity)
        {
            if (Instance == null) return new List<ItemBase>();

            return Instance._items.Values
                .Where(item => item.Rarity == rarity)
                .Select(item => item.Clone())
                .ToList();
        }

        /// <summary>
        /// Check if an item exists in the database
        /// </summary>
        /// <param name="itemID">Item identifier</param>
        /// <returns>True if item exists</returns>
        public static bool HasItem(string itemID)
        {
            return Instance?._items.ContainsKey(itemID) ?? false;
        }

        /// <summary>
        /// Get total number of items in database
        /// </summary>
        /// <returns>Item count</returns>
        public static int GetItemCount()
        {
            return Instance?._items.Count ?? 0;
        }

        #endregion

        #region Private Methods

        private void LoadAllItems()
        {
            _items.Clear();

            // For now, create some sample items programmatically
            // In a full implementation, these would be loaded from JSON files
            CreateSampleItems();

            // Load from JSON when files exist
            // LoadItemsFromDirectory($"{ITEMS_PATH}MechParts/");
            // LoadItemsFromDirectory($"{ITEMS_PATH}WeaponMods/");
            // LoadItemsFromDirectory($"{ITEMS_PATH}DroneChips/");
            // LoadItemsFromDirectory($"{ITEMS_PATH}Consumables/");
            // LoadItemsFromDirectory($"{ITEMS_PATH}Materials/");

            GD.Print($"Loaded {_items.Count} items into database");
        }

        private void CreateSampleItems()
        {
            // Create sample materials
            CreateSampleMaterial("scrap_metal", "Scrap Metal", "Basic crafting material", ItemRarity.Common);
            CreateSampleMaterial("circuits", "Circuits", "Electronic components", ItemRarity.Common);
            CreateSampleMaterial("alloy_plates", "Alloy Plates", "Reinforced metal plates", ItemRarity.Uncommon);
            CreateSampleMaterial("power_cells", "Power Cells", "Energy storage units", ItemRarity.Uncommon);
            CreateSampleMaterial("plasma_core", "Plasma Core", "Advanced energy core", ItemRarity.Rare);
            CreateSampleMaterial("nanofibers", "Nanofibers", "High-tech synthetic fibers", ItemRarity.Rare);
            CreateSampleMaterial("quantum_chips", "Quantum Chips", "Quantum computing processors", ItemRarity.Epic);
            CreateSampleMaterial("dark_matter", "Dark Matter", "Exotic dark matter", ItemRarity.Epic);
            CreateSampleMaterial("void_crystal", "Void Crystal", "Crystallized void energy", ItemRarity.Legendary);
            CreateSampleMaterial("aether_fragment", "Aether Fragment", "Fragments of pure aether", ItemRarity.Legendary);

            // Create sample consumables
            CreateSampleConsumable("credits_small", "Small Credit Chip", "Worth 50 credits", ItemRarity.Common, 50);
            CreateSampleConsumable("credits_large", "Large Credit Chip", "Worth 500 credits", ItemRarity.Rare, 500);

            // Create sample mech parts
            CreateSampleMechPart("common_armor_part", "Standard Armor Plating", "Basic armor protection", ItemRarity.Common, MechPartType.Torso);
            CreateSampleMechPart("rare_mech_part", "Advanced Servo System", "Enhanced movement system", ItemRarity.Rare, MechPartType.Legs);
            
            // Create sample weapon mods
            CreateSampleWeaponMod("uncommon_weapon_mod", "Extended Barrel", "Increases range", ItemRarity.Uncommon, WeaponModType.Barrel);

            // Boss drops
            CreateSampleMaterial("frost_titan_core", "Frost Titan Core", "Core of the Frost Titan", ItemRarity.Legendary);
            CreateSampleMaterial("frost_titan_trophy", "Frost Titan Trophy", "Proof of victory", ItemRarity.Exotic);
        }

        private void CreateSampleMaterial(string id, string name, string desc, ItemRarity rarity)
        {
            var material = new CraftingMaterialItem
            {
                ItemID = id,
                DisplayName = name,
                Description = desc,
                Rarity = rarity,
                SellValue = GetBaseSellValue(rarity),
                MaxStackSize = 999,
                ItemLevel = 1,
                Tier = rarity switch
                {
                    ItemRarity.Common => MaterialTier.Common,
                    ItemRarity.Uncommon => MaterialTier.Uncommon,
                    ItemRarity.Rare => MaterialTier.Rare,
                    ItemRarity.Epic => MaterialTier.Epic,
                    ItemRarity.Legendary => MaterialTier.Legendary,
                    ItemRarity.Exotic => MaterialTier.Exotic,
                    _ => MaterialTier.Common
                }
            };

            material.Tags.Add("material");
            _items[id] = material;
        }

        private void CreateSampleConsumable(string id, string name, string desc, ItemRarity rarity, float value)
        {
            var consumable = new ConsumableItem
            {
                ItemID = id,
                DisplayName = name,
                Description = desc,
                Rarity = rarity,
                SellValue = (int)value,
                MaxStackSize = 99,
                ItemLevel = 1,
                ConsumableType = ConsumableType.Experience,
                EffectValue = value
            };

            consumable.Tags.Add("consumable");
            _items[id] = consumable;
        }

        private void CreateSampleMechPart(string id, string name, string desc, ItemRarity rarity, MechPartType partType)
        {
            var part = new MechPartItem
            {
                ItemID = id,
                DisplayName = name,
                Description = desc,
                Rarity = rarity,
                SellValue = GetBaseSellValue(rarity),
                MaxStackSize = 1,
                ItemLevel = 1,
                PartType = partType
            };

            // Add some random stats
            part.PrimaryStats[StatType.HP] = ItemStatRoll.RollStat(StatType.HP, rarity);
            part.SecondaryStats[StatType.PhysicalResist] = ItemStatRoll.RollStat(StatType.PhysicalResist, rarity);

            part.Tags.Add("equipment");
            part.Tags.Add("mech_part");
            _items[id] = part;
        }

        private void CreateSampleWeaponMod(string id, string name, string desc, ItemRarity rarity, WeaponModType modType)
        {
            var mod = new WeaponModItem
            {
                ItemID = id,
                DisplayName = name,
                Description = desc,
                Rarity = rarity,
                SellValue = GetBaseSellValue(rarity),
                MaxStackSize = 1,
                ItemLevel = 1,
                ModType = modType
            };

            // Add some random stats
            mod.SecondaryStats[StatType.Range] = ItemStatRoll.RollStat(StatType.Range, rarity);

            mod.Tags.Add("equipment");
            mod.Tags.Add("weapon_mod");
            _items[id] = mod;
        }

        private int GetBaseSellValue(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => 10,
                ItemRarity.Uncommon => 50,
                ItemRarity.Rare => 200,
                ItemRarity.Epic => 1000,
                ItemRarity.Legendary => 5000,
                ItemRarity.Exotic => 25000,
                ItemRarity.Mythic => 100000,
                _ => 10
            };
        }

        #endregion
    }
}
