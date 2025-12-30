using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Manages elite enemy abilities
    /// </summary>
    public partial class EliteAbilitySystem : Node
    {
        private List<string> availableAbilities = new()
        {
            "Teleport",
            "Shield",
            "Regeneration",
            "Explosion",
            "Summon"
        };
        
        public void AddRandomAbility(Node3D enemy)
        {
            string ability = availableAbilities[GD.RandRange(0, availableAbilities.Count - 1)];
            
            var abilityComponent = ability switch
            {
                "Teleport" => new TeleportAbility(),
                "Shield" => new ShieldAbility(),
                "Regeneration" => new RegenerationAbility(),
                "Explosion" => new ExplosionAbility(),
                "Summon" => new SummonAbility(),
                _ => null
            };
            
            if (abilityComponent != null)
            {
                enemy.AddChild(abilityComponent);
            }
        }
    }

    // Placeholder ability components (to be implemented later)
    
    /// <summary>
    /// Allows enemy to teleport short distances
    /// </summary>
    public partial class TeleportAbility : Node
    {
        public override void _Ready()
        {
            GD.Print("TeleportAbility added - Placeholder");
        }
    }

    /// <summary>
    /// Provides temporary damage immunity
    /// </summary>
    public partial class ShieldAbility : Node
    {
        public override void _Ready()
        {
            GD.Print("ShieldAbility added - Placeholder");
        }
    }

    /// <summary>
    /// Regenerates health over time
    /// </summary>
    public partial class RegenerationAbility : Node
    {
        public override void _Ready()
        {
            GD.Print("RegenerationAbility added - Placeholder");
        }
    }

    /// <summary>
    /// Explodes on death dealing AoE damage
    /// </summary>
    public partial class ExplosionAbility : Node
    {
        public override void _Ready()
        {
            GD.Print("ExplosionAbility added - Placeholder");
        }
    }

    /// <summary>
    /// Summons additional enemies
    /// </summary>
    public partial class SummonAbility : Node
    {
        public override void _Ready()
        {
            GD.Print("SummonAbility added - Placeholder");
        }
    }
}
