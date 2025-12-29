using Godot;
using System;

namespace MechDefenseHalo.Items
{
    /// <summary>
    /// Drone AI behavior modifier chips
    /// </summary>
    public partial class DroneChipItem : ItemBase
    {
        #region Exported Properties

        [Export] public DroneChipType ChipType { get; set; } = DroneChipType.Combat;
        [Export] public string DroneAIBehavior { get; set; } = ""; // AI behavior script ID

        #endregion

        #region Constructor

        public DroneChipItem()
        {
            MaxStackSize = 1; // Equipment items don't stack
        }

        #endregion

        #region Public Methods

        public override ItemBase Clone()
        {
            var clone = new DroneChipItem
            {
                ItemID = ItemID,
                DisplayName = DisplayName,
                Description = Description,
                Rarity = Rarity,
                Icon = Icon,
                SellValue = SellValue,
                MaxStackSize = MaxStackSize,
                ItemLevel = ItemLevel,
                ChipType = ChipType,
                DroneAIBehavior = DroneAIBehavior,
                SpecialAbilityID = SpecialAbilityID,
                SpecialDescription = SpecialDescription,
                SetID = SetID
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
    /// Types of drone chips
    /// </summary>
    public enum DroneChipType
    {
        Combat,     // Attack behavior
        Support,    // Shield/heal behavior
        Utility,    // Recon/scan behavior
        Swarm       // Multi-drone coordination
    }
}
