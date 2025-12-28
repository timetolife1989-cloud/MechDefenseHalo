using Godot;
using System;

namespace MechDefenseHalo.Items
{
    /// <summary>
    /// Weapon modification items (Barrel, Magazine, Optic)
    /// </summary>
    public partial class WeaponModItem : ItemBase
    {
        #region Exported Properties

        [Export] public WeaponModType ModType { get; set; } = WeaponModType.Barrel;
        [Export] public string CompatibleWeaponType { get; set; } = ""; // Empty = all weapons

        #endregion

        #region Constructor

        public WeaponModItem()
        {
            MaxStackSize = 1; // Equipment items don't stack
        }

        #endregion

        #region Public Methods

        public override ItemBase Clone()
        {
            var clone = new WeaponModItem
            {
                ItemID = ItemID,
                DisplayName = DisplayName,
                Description = Description,
                Rarity = Rarity,
                Icon = Icon,
                SellValue = SellValue,
                MaxStackSize = MaxStackSize,
                ItemLevel = ItemLevel,
                ModType = ModType,
                CompatibleWeaponType = CompatibleWeaponType,
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
    /// Types of weapon modifications
    /// </summary>
    public enum WeaponModType
    {
        Barrel,
        Magazine,
        Optic,
        Stock,
        Grip
    }
}
