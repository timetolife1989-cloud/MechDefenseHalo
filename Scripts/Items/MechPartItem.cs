using Godot;
using System;

namespace MechDefenseHalo.Items
{
    /// <summary>
    /// Mech part items (Head, Torso, Arms, Legs)
    /// </summary>
    public partial class MechPartItem : ItemBase
    {
        #region Exported Properties

        [Export] public MechPartType PartType { get; set; } = MechPartType.Torso;

        #endregion

        #region Constructor

        public MechPartItem()
        {
            MaxStackSize = 1; // Equipment items don't stack
        }

        #endregion

        #region Public Methods

        public override ItemBase Clone()
        {
            var clone = new MechPartItem
            {
                ItemID = ItemID,
                DisplayName = DisplayName,
                Description = Description,
                Rarity = Rarity,
                Icon = Icon,
                SellValue = SellValue,
                MaxStackSize = MaxStackSize,
                ItemLevel = ItemLevel,
                PartType = PartType,
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
    /// Types of mech parts
    /// </summary>
    public enum MechPartType
    {
        Head,
        Torso,
        Arms,
        Legs
    }
}
