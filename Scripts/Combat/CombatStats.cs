using Godot;
using System;

namespace MechDefenseHalo.Combat
{
    [GlobalClass]
    public partial class CombatStats : Resource
    {
        [Export] public float CritChance { get; set; } = 0.05f; // 5%
        [Export] public float CritMultiplier { get; set; } = 1.5f; // 150%
        [Export] public float ArmorPenetration { get; set; } = 0f;
        [Export] public float LifeSteal { get; set; } = 0f;
        [Export] public float DamageBonus { get; set; } = 0f;
    }
}
