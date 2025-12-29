using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Crafting
{
    /// <summary>
    /// Blueprint definition for crafting items
    /// </summary>
    public partial class Blueprint : Resource
    {
        #region Exported Properties

        [Export] public string BlueprintID { get; set; } = "";
        [Export] public string DisplayName { get; set; } = "";
        [Export] public string ResultItemID { get; set; } = "";
        [Export] public ItemRarity ResultRarity { get; set; } = ItemRarity.Common;
        [Export] public int CreditCost { get; set; } = 0;
        [Export] public int CraftingTimeSeconds { get; set; } = 60;
        [Export] public int RequiredPlayerLevel { get; set; } = 1;
        [Export] public bool IsClanResearch { get; set; } = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Materials required to craft (itemID -> quantity)
        /// </summary>
        public Dictionary<string, int> RequiredMaterials { get; set; } = new();

        /// <summary>
        /// Quests that must be completed to unlock this blueprint
        /// </summary>
        public List<string> RequiredQuestsCompleted { get; set; } = new();

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if blueprint can be crafted
        /// </summary>
        /// <param name="inventory">Player inventory to check materials</param>
        /// <param name="playerLevel">Player's current level</param>
        /// <param name="credits">Player's current credits</param>
        /// <returns>True if can craft</returns>
        public bool CanCraft(Inventory.InventoryManager inventory, int playerLevel, int credits)
        {
            // Check level requirement
            if (playerLevel < RequiredPlayerLevel)
                return false;

            // Check credit cost
            if (credits < CreditCost)
                return false;

            // Check materials
            foreach (var material in RequiredMaterials)
            {
                if (!inventory.HasItem(material.Key, material.Value))
                    return false;
            }

            return true;
        }

        #endregion
    }
}
