using Godot;
using System;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Generates diverse enemy stats with trade-offs based on rarity
    /// </summary>
    public partial class EnemyStatMixer : Node
    {
        // Stat generation constants
        private const float MIN_ATTACK_RANGE = 3f;
        private const float MAX_ATTACK_RANGE = 20f;
        private const float MIN_SIZE = 0.8f;
        private const float MAX_SIZE = 1.5f;
        
        public EnemyStats GenerateStats(EnemyRarity rarity)
        {
            // Base stats
            float baseHP = 100f;
            float baseDamage = 10f;
            float baseSpeed = 5f;
            
            // Rarity multiplier
            float rarityMult = rarity switch
            {
                EnemyRarity.Common => 1.0f,
                EnemyRarity.Uncommon => 1.5f,
                EnemyRarity.Rare => 2.0f,
                EnemyRarity.Elite => 3.0f,
                EnemyRarity.Legendary => 5.0f,
                _ => 1.0f
            };
            
            // Generate random archetype
            float archetype = GD.Randf(); // 0.0-1.0 spectrum
            
            // Trade-offs based on archetype:
            // 0.0 = Glass Cannon (high damage, low HP, fast)
            // 0.5 = Balanced
            // 1.0 = Tank (high HP, low damage, slow)
            
            float hpMultiplier = Mathf.Lerp(0.5f, 3.0f, archetype);
            float damageMultiplier = Mathf.Lerp(2.0f, 0.5f, archetype);
            float speedMultiplier = Mathf.Lerp(1.5f, 0.5f, archetype);
            
            // Random variance (±20%)
            float variance = GD.RandfRange(0.8f, 1.2f);
            
            return new EnemyStats
            {
                HP = Mathf.RoundToInt(baseHP * hpMultiplier * rarityMult * variance),
                Damage = Mathf.RoundToInt(baseDamage * damageMultiplier * rarityMult * variance),
                Speed = baseSpeed * speedMultiplier * variance,
                Range = GD.RandfRange(MIN_ATTACK_RANGE, MAX_ATTACK_RANGE),
                Size = Mathf.Lerp(MIN_SIZE, MAX_SIZE, archetype),
                Archetype = archetype
            };
        }
    }
}
