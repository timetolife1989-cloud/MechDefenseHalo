using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;
using MechDefenseHalo.Items;
using MechDefenseHalo.Inventory;

namespace MechDefenseHalo.Items.Sets
{
    /// <summary>
    /// Manages set bonuses and tracks active sets
    /// </summary>
    public partial class SetManager : Node
    {
        #region Private Fields

        private Dictionary<string, int> _activeSetCounts = new(); // setID -> piece count
        private List<SetBonus> _activeBonuses = new();
        private Dictionary<string, SetDefinition> _setDefinitions = new();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            LoadSetDefinitions();
            GD.Print("SetManager initialized");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update equipment and recalculate active set bonuses
        /// </summary>
        /// <param name="equippedItems">Currently equipped items</param>
        public void UpdateEquippedItems(List<ItemBase> equippedItems)
        {
            if (equippedItems == null)
            {
                GD.PrintErr("Cannot update with null equipped items");
                return;
            }

            // Clear previous counts
            var previousBonuses = new List<SetBonus>(_activeBonuses);
            _activeSetCounts.Clear();
            _activeBonuses.Clear();

            // Count set pieces
            foreach (var item in equippedItems)
            {
                if (item != null && item.IsSetItem())
                {
                    if (!_activeSetCounts.ContainsKey(item.SetID))
                    {
                        _activeSetCounts[item.SetID] = 0;
                    }
                    _activeSetCounts[item.SetID]++;
                }
            }

            // Calculate active bonuses
            foreach (var kvp in _activeSetCounts)
            {
                string setID = kvp.Key;
                int pieceCount = kvp.Value;

                if (_setDefinitions.TryGetValue(setID, out var setDef))
                {
                    var bonus = setDef.GetBonusForPieceCount(pieceCount);
                    if (bonus != null)
                    {
                        _activeBonuses.Add(bonus);
                        GD.Print($"Activated {setDef.SetName} bonus: {bonus.BonusName} ({pieceCount} pieces)");

                        // Emit activation event
                        EventBus.Emit("set_bonus_activated", new SetBonusData
                        {
                            SetID = setID,
                            SetName = setDef.SetName,
                            Bonus = bonus,
                            PieceCount = pieceCount
                        });
                    }
                }
            }

            // Check for deactivated bonuses
            foreach (var oldBonus in previousBonuses)
            {
                if (!_activeBonuses.Contains(oldBonus))
                {
                    EventBus.Emit("set_bonus_deactivated", new SetBonusData
                    {
                        Bonus = oldBonus
                    });
                }
            }

            GD.Print($"Active set bonuses: {_activeBonuses.Count}");
        }

        /// <summary>
        /// Get all currently active bonuses
        /// </summary>
        /// <returns>List of active bonuses</returns>
        public List<SetBonus> GetActiveBonuses()
        {
            return new List<SetBonus>(_activeBonuses);
        }

        /// <summary>
        /// Get total stat bonuses from all active sets
        /// </summary>
        /// <returns>Dictionary of combined stat bonuses</returns>
        public Dictionary<StatType, float> GetTotalStatBonuses()
        {
            var totalBonuses = new Dictionary<StatType, float>();

            foreach (var bonus in _activeBonuses)
            {
                foreach (var stat in bonus.StatBonuses)
                {
                    if (!totalBonuses.ContainsKey(stat.Key))
                    {
                        totalBonuses[stat.Key] = 0f;
                    }
                    totalBonuses[stat.Key] += stat.Value;
                }
            }

            return totalBonuses;
        }

        /// <summary>
        /// Get active special abilities from sets
        /// </summary>
        /// <returns>List of special ability IDs</returns>
        public List<string> GetActiveSpecialAbilities()
        {
            return _activeBonuses
                .Where(b => b.HasSpecialAbility())
                .Select(b => b.SpecialAbilityID)
                .ToList();
        }

        /// <summary>
        /// Get set progress (how many pieces of each set are equipped)
        /// </summary>
        /// <returns>Dictionary of set progress</returns>
        public Dictionary<string, SetProgressInfo> GetSetProgress()
        {
            var progress = new Dictionary<string, SetProgressInfo>();

            foreach (var kvp in _activeSetCounts)
            {
                if (_setDefinitions.TryGetValue(kvp.Key, out var setDef))
                {
                    progress[kvp.Key] = new SetProgressInfo
                    {
                        SetID = kvp.Key,
                        SetName = setDef.SetName,
                        EquippedPieces = kvp.Value,
                        TotalPieces = setDef.RequiredItemIDs.Count,
                        ActiveBonus = setDef.GetBonusForPieceCount(kvp.Value)
                    };
                }
            }

            return progress;
        }

        /// <summary>
        /// Get a set definition by ID
        /// </summary>
        /// <param name="setID">Set identifier</param>
        /// <returns>Set definition or null</returns>
        public SetDefinition GetSetDefinition(string setID)
        {
            return _setDefinitions.TryGetValue(setID, out var def) ? def : null;
        }

        #endregion

        #region Private Methods

        private void LoadSetDefinitions()
        {
            _setDefinitions.Clear();

            // For now, create sample set definitions programmatically
            // In a full implementation, these would be loaded from JSON files
            CreateSampleSets();

            GD.Print($"Loaded {_setDefinitions.Count} set definitions");
        }

        private void CreateSampleSets()
        {
            // Juggernaut Bulwark Set (Tank)
            var juggernautSet = new SetDefinition
            {
                SetID = "juggernaut_bulwark",
                SetName = "Juggernaut Bulwark",
                Description = "Heavy armor set focused on maximum survivability",
                MinimumRarity = ItemRarity.Epic
            };
            juggernautSet.RequiredItemIDs.AddRange(new[] { "jugg_head", "jugg_torso", "jugg_arms", "jugg_legs" });

            juggernautSet.TwoPieceBonus = new SetBonus
            {
                BonusName = "Fortified",
                Description = "Increased health and resistance",
                StatBonuses = new Dictionary<StatType, float>
                {
                    { StatType.HP, 500f },
                    { StatType.PhysicalResist, 0.10f }
                }
            };

            juggernautSet.FourPieceBonus = new SetBonus
            {
                BonusName = "Immovable",
                Description = "Immune to crowd control effects",
                SpecialAbilityID = "immovable",
                StatBonuses = new Dictionary<StatType, float>
                {
                    { StatType.HP, 1000f },
                    { StatType.PhysicalResist, 0.20f }
                }
            };

            _setDefinitions[juggernautSet.SetID] = juggernautSet;

            // Phantom Striker Set (DPS)
            var phantomSet = new SetDefinition
            {
                SetID = "phantom_striker",
                SetName = "Phantom Striker",
                Description = "Agile damage-focused set",
                MinimumRarity = ItemRarity.Rare
            };
            phantomSet.RequiredItemIDs.AddRange(new[] { "phantom_head", "phantom_torso", "phantom_arms", "phantom_legs" });

            phantomSet.TwoPieceBonus = new SetBonus
            {
                BonusName = "Critical Precision",
                Description = "Increased critical chance and damage",
                StatBonuses = new Dictionary<StatType, float>
                {
                    { StatType.CritChance, 0.10f },
                    { StatType.CritDamage, 0.25f }
                }
            };

            phantomSet.FourPieceBonus = new SetBonus
            {
                BonusName = "Shadow Step",
                Description = "Become invisible for 2 seconds after dodging",
                SpecialAbilityID = "vanish_on_dodge",
                StatBonuses = new Dictionary<StatType, float>
                {
                    { StatType.CritChance, 0.20f },
                    { StatType.CritDamage, 0.50f },
                    { StatType.Dodge, 0.15f }
                }
            };

            _setDefinitions[phantomSet.SetID] = phantomSet;

            // Inferno Warlord Set (Fire Damage)
            var infernoSet = new SetDefinition
            {
                SetID = "inferno_warlord",
                SetName = "Inferno Warlord",
                Description = "Blazing fire damage specialist",
                MinimumRarity = ItemRarity.Epic
            };
            infernoSet.RequiredItemIDs.AddRange(new[] { "inferno_head", "inferno_torso", "inferno_arms", "inferno_legs" });

            infernoSet.TwoPieceBonus = new SetBonus
            {
                BonusName = "Burning Touch",
                Description = "Attacks apply burn damage over time",
                SpecialAbilityID = "burning_damage",
                StatBonuses = new Dictionary<StatType, float>
                {
                    { StatType.Damage, 100f },
                    { StatType.FireResist, 0.25f }
                }
            };

            infernoSet.FourPieceBonus = new SetBonus
            {
                BonusName = "Pyroclasm",
                Description = "Burn effects explode dealing AoE damage",
                SpecialAbilityID = "burn_explosion",
                StatBonuses = new Dictionary<StatType, float>
                {
                    { StatType.Damage, 250f },
                    { StatType.FireResist, 0.50f }
                }
            };

            _setDefinitions[infernoSet.SetID] = infernoSet;
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Information about set progress
    /// </summary>
    public class SetProgressInfo
    {
        public string SetID { get; set; }
        public string SetName { get; set; }
        public int EquippedPieces { get; set; }
        public int TotalPieces { get; set; }
        public SetBonus ActiveBonus { get; set; }
    }

    /// <summary>
    /// Data for set bonus events
    /// </summary>
    public class SetBonusData
    {
        public string SetID { get; set; }
        public string SetName { get; set; }
        public SetBonus Bonus { get; set; }
        public int PieceCount { get; set; }
    }

    #endregion
}
