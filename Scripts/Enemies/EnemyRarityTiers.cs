using Godot;
using System;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Defines enemy rarity levels for procedural generation
    /// </summary>
    public enum EnemyRarity
    {
        Common,
        Uncommon,
        Rare,
        Elite,
        Legendary
    }

    /// <summary>
    /// Enemy stat data structure for procedural generation
    /// </summary>
    public struct EnemyStats
    {
        public int HP;
        public int Damage;
        public float Speed;
        public float Range;
        public float Size;
        public float Archetype; // 0.0-1.0 for visual variation
    }
}
