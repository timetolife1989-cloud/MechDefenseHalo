using Godot;
using System;

namespace MechDefenseHalo.Items
{
    /// <summary>
    /// Crafting materials used for blueprints
    /// </summary>
    public partial class CraftingMaterialItem : ItemBase
    {
        #region Exported Properties

        [Export] public MaterialTier Tier { get; set; } = MaterialTier.Common;

        #endregion

        #region Constructor

        public CraftingMaterialItem()
        {
            MaxStackSize = 999; // Materials stack heavily
        }

        #endregion

        #region Public Methods

        public override ItemBase Clone()
        {
            var clone = new CraftingMaterialItem
            {
                ItemID = ItemID,
                DisplayName = DisplayName,
                Description = Description,
                Rarity = Rarity,
                Icon = Icon,
                SellValue = SellValue,
                MaxStackSize = MaxStackSize,
                ItemLevel = ItemLevel,
                Tier = Tier
            };

            // Deep copy dictionaries
            clone.PrimaryStats = new(PrimaryStats);
            clone.SecondaryStats = new(SecondaryStats);
            clone.Resistances = new(Resistances);
            clone.Tags = new(Tags);

            return clone;
        }

        #endregion
    }

    /// <summary>
    /// Material rarity tiers
    /// </summary>
    public enum MaterialTier
    {
        Common,     // Scrap Metal, Circuits
        Uncommon,   // Alloy Plates, Power Cells
        Rare,       // Plasma Cores, Nanofibers
        Epic,       // Quantum Chips, Dark Matter
        Legendary,  // Void Crystals, Aether Fragments
        Exotic      // Phoenix Feather, Titan Heart (boss drops)
    }
}
