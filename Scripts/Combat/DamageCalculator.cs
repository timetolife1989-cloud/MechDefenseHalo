using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Combat
{
    public partial class DamageCalculator : Node
    {
        private static DamageCalculator _instance;
        public static DamageCalculator Instance => _instance;
        
        // Damage type effectiveness vs armor types
        private Dictionary<DamageType, Dictionary<string, float>> _effectiveness = new()
        {
            [DamageType.Kinetic] = new() { ["Light"] = 1.2f, ["Heavy"] = 0.8f, ["Shield"] = 0.5f },
            [DamageType.Energy] = new() { ["Light"] = 0.9f, ["Heavy"] = 1.1f, ["Shield"] = 2.0f },
            [DamageType.Explosive] = new() { ["Light"] = 1.5f, ["Heavy"] = 1.0f, ["Shield"] = 0.3f },
            [DamageType.Cryo] = new() { ["Light"] = 1.0f, ["Heavy"] = 0.7f, ["Shield"] = 1.5f },
            [DamageType.Tesla] = new() { ["Light"] = 1.3f, ["Heavy"] = 0.9f, ["Shield"] = 2.5f },
            [DamageType.True] = new() { ["Light"] = 1.0f, ["Heavy"] = 1.0f, ["Shield"] = 1.0f }
        };
        
        public override void _Ready()
        {
            _instance = this;
        }
        
        public float CalculateDamage(
            float baseDamage,
            DamageType damageType,
            float armor,
            string armorType,
            bool isCritical,
            float critMultiplier)
        {
            float damage = baseDamage;
            
            // Apply armor reduction
            if (damageType != DamageType.True)
            {
                float armorReduction = armor / (armor + 100);
                damage *= (1 - armorReduction);
            }
            
            // Apply type effectiveness
            if (_effectiveness.ContainsKey(damageType) && 
                _effectiveness[damageType].ContainsKey(armorType))
            {
                damage *= _effectiveness[damageType][armorType];
            }
            
            // Apply critical hit
            if (isCritical)
            {
                damage *= critMultiplier;
            }
            
            return Mathf.Max(1, damage); // Minimum 1 damage
        }
        
        public bool RollCritical(float critChance)
        {
            return GD.Randf() < critChance;
        }
    }
}
