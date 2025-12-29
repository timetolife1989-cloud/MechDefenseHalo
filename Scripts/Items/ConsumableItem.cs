using Godot;
using System;

namespace MechDefenseHalo.Items
{
    /// <summary>
    /// Consumable items (Repair kits, boosters, buffs)
    /// </summary>
    public partial class ConsumableItem : ItemBase
    {
        #region Exported Properties

        [Export] public ConsumableType ConsumableType { get; set; } = ConsumableType.Heal;
        [Export] public float EffectValue { get; set; } = 0f;
        [Export] public float EffectDuration { get; set; } = 0f; // 0 = instant
        [Export] public int Cooldown { get; set; } = 0; // Seconds

        #endregion

        #region Constructor

        public ConsumableItem()
        {
            MaxStackSize = 99; // Consumables stack
        }

        #endregion

        #region Public Methods

        public override ItemBase Clone()
        {
            var clone = new ConsumableItem
            {
                ItemID = ItemID,
                DisplayName = DisplayName,
                Description = Description,
                Rarity = Rarity,
                Icon = Icon,
                SellValue = SellValue,
                MaxStackSize = MaxStackSize,
                ItemLevel = ItemLevel,
                ConsumableType = ConsumableType,
                EffectValue = EffectValue,
                EffectDuration = EffectDuration,
                Cooldown = Cooldown,
                SpecialAbilityID = SpecialAbilityID,
                SpecialDescription = SpecialDescription
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
    /// Types of consumable items
    /// </summary>
    public enum ConsumableType
    {
        Heal,           // Restore HP
        Shield,         // Restore Shield
        Energy,         // Restore Energy
        DamageBuff,     // Temporary damage increase
        DefenseBuff,    // Temporary defense increase
        SpeedBuff,      // Temporary speed increase
        Experience      // XP boost
    }
}
