using Godot;
using System;

namespace MechDefenseHalo.Combat
{
    [GlobalClass]
    public partial class CombatStats : Resource
    {
        [Export] public float CritChance { get; set; } = 0.05f; // 5%
        [Export] public float CritMultiplier { get; set; } = 1.5f; // 1.5x damage (50% increase)
        [Export] public float ArmorPenetration { get; set; } = 0f;
        [Export] public float LifeSteal { get; set; } = 0f;
        [Export] public float DamageBonus { get; set; } = 0f;
    }
}
