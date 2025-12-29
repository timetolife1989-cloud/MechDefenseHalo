using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Items;
using MechDefenseHalo.Items.Sets;
using MechDefenseHalo.Inventory;

namespace MechDefenseHalo.Player
{
    /// <summary>
    /// Manages player stats aggregation from base stats, equipment, and set bonuses
    /// </summary>
    public partial class PlayerStatsManager : Node
    {
        #region Exported Properties - Base Stats

        [Export] public float BaseHP { get; set; } = 1000f;
        [Export] public float BaseShield { get; set; } = 500f;
        [Export] public float BaseSpeed { get; set; } = 5.0f;
        [Export] public float BaseEnergy { get; set; } = 100f;

        #endregion

        #region Public Properties - Calculated Stats

        public float TotalHP { get; private set; }
        public float TotalShield { get; private set; }
        public float TotalSpeed { get; private set; }
        public float TotalEnergy { get; private set; }
        public float CritChance { get; private set; }
        public float CritDamage { get; private set; }
        public float Dodge { get; private set; }
        public float Regeneration { get; private set; }
        public Dictionary<string, float> Resistances { get; private set; } = new();

        #endregion

        #region Private Fields

        private EquipmentManager _equipmentManager;
        private SetManager _setManager;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get references to managers
            _equipmentManager = GetNodeOrNull<EquipmentManager>("../EquipmentManager");
            _setManager = GetNodeOrNull<SetManager>("../SetManager");

            if (_equipmentManager == null)
            {
                GD.PrintErr("PlayerStatsManager: EquipmentManager not found!");
            }

            if (_setManager == null)
            {
                GD.PrintErr("PlayerStatsManager: SetManager not found!");
            }

            RecalculateStats();
            GD.Print("PlayerStatsManager initialized");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Recalculate all stats from base + equipment + sets + buffs
        /// </summary>
        public void RecalculateStats()
        {
            // Start with base stats
            TotalHP = BaseHP;
            TotalShield = BaseShield;
            TotalSpeed = BaseSpeed;
            TotalEnergy = BaseEnergy;
            CritChance = 0.05f; // 5% base
            CritDamage = 1.5f;  // 150% base
            Dodge = 0f;
            Regeneration = 0f;
            Resistances.Clear();

            // Apply equipment stats
            ApplyEquipmentStats();

            // Apply set bonuses
            ApplySetBonuses();

            // Apply temporary buffs (TODO: when buff system exists)
            // ApplyBuffs();

            GD.Print($"Stats recalculated - HP: {TotalHP:F0}, Shield: {TotalShield:F0}, " +
                    $"Crit: {CritChance * 100:F1}%, Speed: {TotalSpeed:F1}");
        }

        /// <summary>
        /// Get stat value by type
        /// </summary>
        /// <param name="statType">Stat to query</param>
        /// <returns>Stat value</returns>
        public float GetStat(StatType statType)
        {
            return statType switch
            {
                StatType.HP => TotalHP,
                StatType.Shield => TotalShield,
                StatType.Speed => TotalSpeed,
                StatType.Energy => TotalEnergy,
                StatType.CritChance => CritChance,
                StatType.CritDamage => CritDamage,
                StatType.Dodge => Dodge,
                StatType.Regeneration => Regeneration,
                _ => 0f
            };
        }

        /// <summary>
        /// Get resistance value
        /// </summary>
        /// <param name="resistanceType">Type of resistance</param>
        /// <returns>Resistance value (0.0 to 1.0)</returns>
        public float GetResistance(string resistanceType)
        {
            return Resistances.TryGetValue(resistanceType, out float value) ? value : 0f;
        }

        #endregion

        #region Private Methods

        private void ApplyEquipmentStats()
        {
            if (_equipmentManager == null) return;

            var equippedItems = _equipmentManager.GetAllEquippedItems();

            foreach (var kvp in equippedItems)
            {
                var item = kvp.Value;
                if (item == null) continue;

                // Apply primary stats
                foreach (var stat in item.PrimaryStats)
                {
                    ApplyStat(stat.Key, stat.Value);
                }

                // Apply secondary stats
                foreach (var stat in item.SecondaryStats)
                {
                    ApplyStat(stat.Key, stat.Value);
                }

                // Apply resistances
                foreach (var resist in item.Resistances)
                {
                    ApplyResistance(resist.Key.ToString(), resist.Value);
                }
            }
        }

        private void ApplySetBonuses()
        {
            if (_setManager == null) return;

            var setBonuses = _setManager.GetTotalStatBonuses();

            foreach (var bonus in setBonuses)
            {
                ApplyStat(bonus.Key, bonus.Value);
            }
        }

        private void ApplyStat(StatType statType, float value)
        {
            switch (statType)
            {
                case StatType.HP:
                    TotalHP += value;
                    break;
                case StatType.Shield:
                    TotalShield += value;
                    break;
                case StatType.Speed:
                    TotalSpeed += value;
                    break;
                case StatType.Energy:
                    TotalEnergy += value;
                    break;
                case StatType.CritChance:
                    CritChance += value;
                    break;
                case StatType.CritDamage:
                    CritDamage += value;
                    break;
                case StatType.Dodge:
                    Dodge += value;
                    break;
                case StatType.Regeneration:
                    Regeneration += value;
                    break;
            }
        }

        private void ApplyResistance(string resistanceType, float value)
        {
            if (!Resistances.ContainsKey(resistanceType))
            {
                Resistances[resistanceType] = 0f;
            }
            Resistances[resistanceType] += value;
            
            // Cap resistances at 75%
            Resistances[resistanceType] = Mathf.Min(0.75f, Resistances[resistanceType]);
        }

        #endregion
    }
}
