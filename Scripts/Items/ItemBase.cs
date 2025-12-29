using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Items
{
    /// <summary>
    /// Abstract base class for all items in the game
    /// </summary>
    public abstract partial class ItemBase : Resource
    {
        #region Exported Properties

        [Export] public string ItemID { get; set; } = "";
        [Export] public string DisplayName { get; set; } = "";
        [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
        [Export] public ItemRarity Rarity { get; set; } = ItemRarity.Common;
        [Export] public Texture2D Icon { get; set; }
        [Export] public int SellValue { get; set; } = 0;
        [Export] public int MaxStackSize { get; set; } = 1;
        [Export] public int ItemLevel { get; set; } = 1;

        #endregion

        #region Public Properties

        /// <summary>
        /// Primary stats (HP, Shield, Speed, Energy)
        /// </summary>
        public Dictionary<StatType, float> PrimaryStats { get; set; } = new();

        /// <summary>
        /// Secondary stats (Crit, Dodge, Regeneration, etc.)
        /// </summary>
        public Dictionary<StatType, float> SecondaryStats { get; set; } = new();

        /// <summary>
        /// Elemental resistances
        /// </summary>
        public Dictionary<StatType, float> Resistances { get; set; } = new();

        /// <summary>
        /// Special ability ID for unique item effects
        /// </summary>
        public string SpecialAbilityID { get; set; } = "";

        /// <summary>
        /// Description of the special ability
        /// </summary>
        public string SpecialDescription { get; set; } = "";

        /// <summary>
        /// Set ID if this item belongs to a set
        /// </summary>
        public string SetID { get; set; } = "";

        /// <summary>
        /// Tags for categorization and filtering
        /// </summary>
        public List<string> Tags { get; set; } = new();

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a formatted string of all stats on this item
        /// </summary>
        /// <returns>Human-readable stat description</returns>
        public virtual string GetStatsDescription()
        {
            var description = "";

            if (PrimaryStats.Count > 0)
            {
                description += "Primary Stats:\n";
                foreach (var stat in PrimaryStats)
                {
                    description += $"  +{stat.Value:F1} {stat.Key}\n";
                }
            }

            if (SecondaryStats.Count > 0)
            {
                description += "\nSecondary Stats:\n";
                foreach (var stat in SecondaryStats)
                {
                    description += $"  +{stat.Value:F1} {stat.Key}\n";
                }
            }

            if (Resistances.Count > 0)
            {
                description += "\nResistances:\n";
                foreach (var resist in Resistances)
                {
                    description += $"  +{resist.Value * 100:F1}% {resist.Key}\n";
                }
            }

            if (!string.IsNullOrEmpty(SpecialDescription))
            {
                description += $"\n[Special] {SpecialDescription}";
            }

            return description;
        }

        /// <summary>
        /// Get the total stat value for a specific stat type
        /// </summary>
        /// <param name="statType">The stat to query</param>
        /// <returns>Stat value or 0 if not present</returns>
        public float GetStatValue(StatType statType)
        {
            if (PrimaryStats.TryGetValue(statType, out float primary))
                return primary;
            
            if (SecondaryStats.TryGetValue(statType, out float secondary))
                return secondary;
            
            if (Resistances.TryGetValue(statType, out float resistance))
                return resistance;

            return 0f;
        }

        /// <summary>
        /// Check if this item belongs to a set
        /// </summary>
        /// <returns>True if part of a set</returns>
        public bool IsSetItem()
        {
            return !string.IsNullOrEmpty(SetID);
        }

        /// <summary>
        /// Check if this item has a special ability
        /// </summary>
        /// <returns>True if has special ability</returns>
        public bool HasSpecialAbility()
        {
            return !string.IsNullOrEmpty(SpecialAbilityID);
        }

        /// <summary>
        /// Clone this item
        /// </summary>
        /// <returns>Deep copy of the item</returns>
        public abstract ItemBase Clone();

        #endregion
    }
}
