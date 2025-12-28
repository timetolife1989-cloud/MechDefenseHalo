using Godot;
using System;

namespace MechDefenseHalo.Items
{
    /// <summary>
    /// Cosmetic items (Skins, emotes, effects)
    /// </summary>
    public partial class CosmeticItem : ItemBase
    {
        #region Exported Properties

        [Export] public CosmeticType CosmeticType { get; set; } = CosmeticType.MechSkin;
        [Export] public string AssetPath { get; set; } = ""; // Path to 3D model or texture

        #endregion

        #region Constructor

        public CosmeticItem()
        {
            MaxStackSize = 1; // Cosmetics don't stack (one-time unlocks)
        }

        #endregion

        #region Public Methods

        public override ItemBase Clone()
        {
            var clone = new CosmeticItem
            {
                ItemID = ItemID,
                DisplayName = DisplayName,
                Description = Description,
                Rarity = Rarity,
                Icon = Icon,
                SellValue = SellValue,
                MaxStackSize = MaxStackSize,
                ItemLevel = ItemLevel,
                CosmeticType = CosmeticType,
                AssetPath = AssetPath
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
    /// Types of cosmetic items
    /// </summary>
    public enum CosmeticType
    {
        MechSkin,
        WeaponSkin,
        DroneSkin,
        Emote,
        KillEffect,
        Banner,
        Title
    }
}
