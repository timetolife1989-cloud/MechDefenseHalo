using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Items
{
    /// <summary>
    /// Types of stats that can be applied to items
    /// </summary>
    public enum StatType
    {
        // Primary Stats
        HP,
        Shield,
        Speed,
        Energy,
        
        // Secondary Stats
        CritChance,
        CritDamage,
        Dodge,
        Regeneration,
        
        // Resistances
        PhysicalResist,
        FireResist,
        IceResist,
        ElectricResist,
        ToxicResist,
        
        // Weapon Stats
        Damage,
        FireRate,
        Accuracy,
        Range,
        AmmoCapacity,
        ReloadSpeed,
        
        // Drone Stats
        DroneSpeed,
        DroneDamage,
        DroneHealth,
        EnergyEfficiency
    }

    /// <summary>
    /// Handles random stat rolling for items based on rarity
    /// </summary>
    public static class ItemStatRoll
    {
        #region Stat Range Definitions

        /// <summary>
        /// Defines min/max stat ranges for each rarity level
        /// </summary>
        private static readonly Dictionary<ItemRarity, Dictionary<StatType, (float min, float max)>> _statRanges = new()
        {
            {
                ItemRarity.Common, new Dictionary<StatType, (float, float)>
                {
                    { StatType.HP, (50f, 100f) },
                    { StatType.Shield, (25f, 50f) },
                    { StatType.Speed, (0.5f, 1.0f) },
                    { StatType.Energy, (10f, 20f) },
                    { StatType.CritChance, (0.01f, 0.03f) },
                    { StatType.CritDamage, (1.2f, 1.3f) },
                    { StatType.Dodge, (0.01f, 0.05f) },
                    { StatType.Regeneration, (1f, 2f) },
                    { StatType.PhysicalResist, (0.01f, 0.05f) },
                    { StatType.FireResist, (0.01f, 0.05f) },
                    { StatType.IceResist, (0.01f, 0.05f) },
                    { StatType.ElectricResist, (0.01f, 0.05f) },
                    { StatType.ToxicResist, (0.01f, 0.05f) },
                    { StatType.Damage, (10f, 20f) },
                    { StatType.FireRate, (0.8f, 1.0f) },
                    { StatType.Accuracy, (0.7f, 0.8f) },
                    { StatType.Range, (10f, 15f) },
                    { StatType.AmmoCapacity, (10f, 20f) },
                    { StatType.ReloadSpeed, (0.8f, 1.0f) },
                    { StatType.DroneSpeed, (3f, 5f) },
                    { StatType.DroneDamage, (5f, 10f) },
                    { StatType.DroneHealth, (30f, 50f) },
                    { StatType.EnergyEfficiency, (0.8f, 0.9f) }
                }
            },
            {
                ItemRarity.Uncommon, new Dictionary<StatType, (float, float)>
                {
                    { StatType.HP, (100f, 200f) },
                    { StatType.Shield, (50f, 100f) },
                    { StatType.Speed, (1.0f, 2.0f) },
                    { StatType.Energy, (20f, 40f) },
                    { StatType.CritChance, (0.03f, 0.06f) },
                    { StatType.CritDamage, (1.3f, 1.5f) },
                    { StatType.Dodge, (0.05f, 0.10f) },
                    { StatType.Regeneration, (2f, 4f) },
                    { StatType.PhysicalResist, (0.05f, 0.10f) },
                    { StatType.FireResist, (0.05f, 0.10f) },
                    { StatType.IceResist, (0.05f, 0.10f) },
                    { StatType.ElectricResist, (0.05f, 0.10f) },
                    { StatType.ToxicResist, (0.05f, 0.10f) },
                    { StatType.Damage, (20f, 40f) },
                    { StatType.FireRate, (1.0f, 1.2f) },
                    { StatType.Accuracy, (0.8f, 0.9f) },
                    { StatType.Range, (15f, 25f) },
                    { StatType.AmmoCapacity, (20f, 40f) },
                    { StatType.ReloadSpeed, (1.0f, 1.2f) },
                    { StatType.DroneSpeed, (5f, 8f) },
                    { StatType.DroneDamage, (10f, 20f) },
                    { StatType.DroneHealth, (50f, 80f) },
                    { StatType.EnergyEfficiency, (0.9f, 1.0f) }
                }
            },
            {
                ItemRarity.Rare, new Dictionary<StatType, (float, float)>
                {
                    { StatType.HP, (200f, 400f) },
                    { StatType.Shield, (100f, 200f) },
                    { StatType.Speed, (2.0f, 3.0f) },
                    { StatType.Energy, (40f, 80f) },
                    { StatType.CritChance, (0.06f, 0.10f) },
                    { StatType.CritDamage, (1.5f, 1.8f) },
                    { StatType.Dodge, (0.10f, 0.15f) },
                    { StatType.Regeneration, (4f, 8f) },
                    { StatType.PhysicalResist, (0.10f, 0.15f) },
                    { StatType.FireResist, (0.10f, 0.15f) },
                    { StatType.IceResist, (0.10f, 0.15f) },
                    { StatType.ElectricResist, (0.10f, 0.15f) },
                    { StatType.ToxicResist, (0.10f, 0.15f) },
                    { StatType.Damage, (40f, 80f) },
                    { StatType.FireRate, (1.2f, 1.4f) },
                    { StatType.Accuracy, (0.9f, 0.95f) },
                    { StatType.Range, (25f, 35f) },
                    { StatType.AmmoCapacity, (40f, 80f) },
                    { StatType.ReloadSpeed, (1.2f, 1.4f) },
                    { StatType.DroneSpeed, (8f, 12f) },
                    { StatType.DroneDamage, (20f, 40f) },
                    { StatType.DroneHealth, (80f, 120f) },
                    { StatType.EnergyEfficiency, (1.0f, 1.1f) }
                }
            },
            {
                ItemRarity.Epic, new Dictionary<StatType, (float, float)>
                {
                    { StatType.HP, (400f, 800f) },
                    { StatType.Shield, (200f, 400f) },
                    { StatType.Speed, (3.0f, 4.5f) },
                    { StatType.Energy, (80f, 150f) },
                    { StatType.CritChance, (0.10f, 0.15f) },
                    { StatType.CritDamage, (1.8f, 2.2f) },
                    { StatType.Dodge, (0.15f, 0.20f) },
                    { StatType.Regeneration, (8f, 15f) },
                    { StatType.PhysicalResist, (0.15f, 0.25f) },
                    { StatType.FireResist, (0.15f, 0.25f) },
                    { StatType.IceResist, (0.15f, 0.25f) },
                    { StatType.ElectricResist, (0.15f, 0.25f) },
                    { StatType.ToxicResist, (0.15f, 0.25f) },
                    { StatType.Damage, (80f, 150f) },
                    { StatType.FireRate, (1.4f, 1.6f) },
                    { StatType.Accuracy, (0.95f, 0.98f) },
                    { StatType.Range, (35f, 50f) },
                    { StatType.AmmoCapacity, (80f, 150f) },
                    { StatType.ReloadSpeed, (1.4f, 1.6f) },
                    { StatType.DroneSpeed, (12f, 18f) },
                    { StatType.DroneDamage, (40f, 80f) },
                    { StatType.DroneHealth, (120f, 180f) },
                    { StatType.EnergyEfficiency, (1.1f, 1.3f) }
                }
            },
            {
                ItemRarity.Legendary, new Dictionary<StatType, (float, float)>
                {
                    { StatType.HP, (800f, 1500f) },
                    { StatType.Shield, (400f, 750f) },
                    { StatType.Speed, (4.5f, 6.0f) },
                    { StatType.Energy, (150f, 250f) },
                    { StatType.CritChance, (0.15f, 0.25f) },
                    { StatType.CritDamage, (2.2f, 3.0f) },
                    { StatType.Dodge, (0.20f, 0.30f) },
                    { StatType.Regeneration, (15f, 25f) },
                    { StatType.PhysicalResist, (0.25f, 0.35f) },
                    { StatType.FireResist, (0.25f, 0.35f) },
                    { StatType.IceResist, (0.25f, 0.35f) },
                    { StatType.ElectricResist, (0.25f, 0.35f) },
                    { StatType.ToxicResist, (0.25f, 0.35f) },
                    { StatType.Damage, (150f, 300f) },
                    { StatType.FireRate, (1.6f, 2.0f) },
                    { StatType.Accuracy, (0.98f, 1.0f) },
                    { StatType.Range, (50f, 75f) },
                    { StatType.AmmoCapacity, (150f, 250f) },
                    { StatType.ReloadSpeed, (1.6f, 2.0f) },
                    { StatType.DroneSpeed, (18f, 25f) },
                    { StatType.DroneDamage, (80f, 150f) },
                    { StatType.DroneHealth, (180f, 300f) },
                    { StatType.EnergyEfficiency, (1.3f, 1.5f) }
                }
            },
            {
                ItemRarity.Exotic, new Dictionary<StatType, (float, float)>
                {
                    { StatType.HP, (1500f, 2500f) },
                    { StatType.Shield, (750f, 1250f) },
                    { StatType.Speed, (6.0f, 8.0f) },
                    { StatType.Energy, (250f, 400f) },
                    { StatType.CritChance, (0.25f, 0.35f) },
                    { StatType.CritDamage, (3.0f, 4.0f) },
                    { StatType.Dodge, (0.30f, 0.40f) },
                    { StatType.Regeneration, (25f, 40f) },
                    { StatType.PhysicalResist, (0.35f, 0.50f) },
                    { StatType.FireResist, (0.35f, 0.50f) },
                    { StatType.IceResist, (0.35f, 0.50f) },
                    { StatType.ElectricResist, (0.35f, 0.50f) },
                    { StatType.ToxicResist, (0.35f, 0.50f) },
                    { StatType.Damage, (300f, 500f) },
                    { StatType.FireRate, (2.0f, 2.5f) },
                    { StatType.Accuracy, (1.0f, 1.0f) },
                    { StatType.Range, (75f, 100f) },
                    { StatType.AmmoCapacity, (250f, 400f) },
                    { StatType.ReloadSpeed, (2.0f, 2.5f) },
                    { StatType.DroneSpeed, (25f, 35f) },
                    { StatType.DroneDamage, (150f, 250f) },
                    { StatType.DroneHealth, (300f, 500f) },
                    { StatType.EnergyEfficiency, (1.5f, 2.0f) }
                }
            },
            {
                ItemRarity.Mythic, new Dictionary<StatType, (float, float)>
                {
                    { StatType.HP, (2500f, 5000f) },
                    { StatType.Shield, (1250f, 2500f) },
                    { StatType.Speed, (8.0f, 12.0f) },
                    { StatType.Energy, (400f, 750f) },
                    { StatType.CritChance, (0.35f, 0.50f) },
                    { StatType.CritDamage, (4.0f, 6.0f) },
                    { StatType.Dodge, (0.40f, 0.60f) },
                    { StatType.Regeneration, (40f, 75f) },
                    { StatType.PhysicalResist, (0.50f, 0.75f) },
                    { StatType.FireResist, (0.50f, 0.75f) },
                    { StatType.IceResist, (0.50f, 0.75f) },
                    { StatType.ElectricResist, (0.50f, 0.75f) },
                    { StatType.ToxicResist, (0.50f, 0.75f) },
                    { StatType.Damage, (500f, 1000f) },
                    { StatType.FireRate, (2.5f, 3.5f) },
                    { StatType.Accuracy, (1.0f, 1.0f) },
                    { StatType.Range, (100f, 150f) },
                    { StatType.AmmoCapacity, (400f, 750f) },
                    { StatType.ReloadSpeed, (2.5f, 3.5f) },
                    { StatType.DroneSpeed, (35f, 50f) },
                    { StatType.DroneDamage, (250f, 500f) },
                    { StatType.DroneHealth, (500f, 1000f) },
                    { StatType.EnergyEfficiency, (2.0f, 3.0f) }
                }
            }
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Roll a random stat value based on rarity
        /// </summary>
        /// <param name="stat">The stat type to roll</param>
        /// <param name="rarity">The rarity level determining the range</param>
        /// <returns>Random value within the rarity's range for this stat</returns>
        public static float RollStat(StatType stat, ItemRarity rarity)
        {
            if (_statRanges.TryGetValue(rarity, out var statDict))
            {
                if (statDict.TryGetValue(stat, out var range))
                {
                    return GD.Randf() * (range.max - range.min) + range.min;
                }
            }

            GD.PrintErr($"No stat range defined for {stat} at rarity {rarity}");
            return 0f;
        }

        /// <summary>
        /// Get the stat ranges for a specific rarity
        /// </summary>
        /// <param name="rarity">The rarity level</param>
        /// <returns>Dictionary of stat ranges for this rarity</returns>
        public static Dictionary<StatType, (float min, float max)> GetStatRanges(ItemRarity rarity)
        {
            return _statRanges.TryGetValue(rarity, out var ranges) 
                ? new Dictionary<StatType, (float min, float max)>(ranges) 
                : new Dictionary<StatType, (float min, float max)>();
        }

        /// <summary>
        /// Roll multiple random stats for an item
        /// </summary>
        /// <param name="stats">List of stats to roll</param>
        /// <param name="rarity">The rarity level</param>
        /// <returns>Dictionary of rolled stat values</returns>
        public static Dictionary<StatType, float> RollStats(List<StatType> stats, ItemRarity rarity)
        {
            var result = new Dictionary<StatType, float>();
            
            foreach (var stat in stats)
            {
                result[stat] = RollStat(stat, rarity);
            }

            return result;
        }

        #endregion
    }
}
