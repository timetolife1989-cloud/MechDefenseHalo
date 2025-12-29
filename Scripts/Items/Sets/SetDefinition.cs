using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MechDefenseHalo.Items.Sets
{
    /// <summary>
    /// Defines a set of items with bonuses for wearing multiple pieces
    /// </summary>
    public partial class SetDefinition : Resource
    {
        #region Exported Properties

        [Export]
        [JsonPropertyName("set_id")]
        public string SetID { get; set; } = "";
        
        [Export]
        [JsonPropertyName("set_name")]
        public string SetName { get; set; } = "";
        
        [Export(PropertyHint.MultilineText)]
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
        
        [Export]
        [JsonPropertyName("minimum_rarity")]
        public ItemRarity MinimumRarity { get; set; } = ItemRarity.Common;

        #endregion

        #region Public Properties

        /// <summary>
        /// Item IDs that are part of this set
        /// </summary>
        [JsonPropertyName("required_item_ids")]
        public List<string> RequiredItemIDs { get; set; } = new();

        /// <summary>
        /// Bonus for wearing 2 pieces
        /// </summary>
        [JsonPropertyName("two_piece_bonus")]
        public SetBonus TwoPieceBonus { get; set; }

        /// <summary>
        /// Bonus for wearing 4 pieces
        /// </summary>
        [JsonPropertyName("four_piece_bonus")]
        public SetBonus FourPieceBonus { get; set; }

        /// <summary>
        /// Bonus for wearing 6 pieces (optional)
        /// </summary>
        [JsonPropertyName("six_piece_bonus")]
        public SetBonus SixPieceBonus { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the bonus for a specific number of equipped pieces
        /// </summary>
        /// <param name="pieceCount">Number of set pieces equipped</param>
        /// <returns>Active bonus or null</returns>
        public SetBonus GetBonusForPieceCount(int pieceCount)
        {
            if (pieceCount >= 6 && SixPieceBonus != null)
                return SixPieceBonus;
            
            if (pieceCount >= 4 && FourPieceBonus != null)
                return FourPieceBonus;
            
            if (pieceCount >= 2 && TwoPieceBonus != null)
                return TwoPieceBonus;

            return null;
        }

        /// <summary>
        /// Check if an item belongs to this set
        /// </summary>
        /// <param name="itemID">Item identifier</param>
        /// <returns>True if item is part of this set</returns>
        public bool ContainsItem(string itemID)
        {
            return RequiredItemIDs.Contains(itemID);
        }

        #endregion
    }

    /// <summary>
    /// Represents a bonus granted by wearing multiple set pieces
    /// </summary>
    public partial class SetBonus : Resource
    {
        #region Exported Properties

        [Export]
        [JsonPropertyName("bonus_name")]
        public string BonusName { get; set; } = "";
        
        [Export(PropertyHint.MultilineText)]
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
        
        [Export]
        [JsonPropertyName("special_ability_id")]
        public string SpecialAbilityID { get; set; } = ""; // e.g., "immovable", "vanish"

        #endregion

        #region Public Properties

        /// <summary>
        /// Stat bonuses provided by this set bonus
        /// </summary>
        [JsonPropertyName("stat_bonuses")]
        public Dictionary<StatType, float> StatBonuses { get; set; } = new();

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if this bonus has a special ability
        /// </summary>
        /// <returns>True if special ability exists</returns>
        public bool HasSpecialAbility()
        {
            return !string.IsNullOrEmpty(SpecialAbilityID);
        }

        /// <summary>
        /// Get formatted description of all bonuses
        /// </summary>
        /// <returns>Human-readable bonus description</returns>
        public string GetFormattedDescription()
        {
            var desc = Description;

            if (StatBonuses.Count > 0)
            {
                desc += "\n\nStat Bonuses:";
                foreach (var stat in StatBonuses)
                {
                    desc += $"\n  +{stat.Value:F1} {stat.Key}";
                }
            }

            if (HasSpecialAbility())
            {
                desc += $"\n\nSpecial: {SpecialAbilityID}";
            }

            return desc;
        }

        #endregion
    }
}
