using Godot;
using System;
using MechDefenseHalo.Components;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Weapons.Melee
{
    /// <summary>
    /// Slow melee weapon with massive AOE damage.
    /// War hammer with ground pound attack.
    /// </summary>
    public partial class WarHammer : WeaponBase
    {
        #region Exported Properties

        [Export] public float AOERadius { get; set; } = 5f;
        [Export] public float KnockbackForce { get; set; } = 10f;

        #endregion

        #region Constructor

        public WarHammer()
        {
            WeaponName = "War Hammer";
            BaseDamage = 100f;
            FireRate = 1.5f; // 0.66 swings per second (slow)
            Range = 5f; // AOE range
            MaxAmmo = 1; // Single charge
            ReloadTime = 3f; // Recharge time
            ElementType = ElementalType.Physical;
            IsAutomatic = false;
        }

        #endregion

        #region Protected Methods

        protected override void OnFire()
        {
            // Ground pound attack - hits all enemies in AOE
            Vector3 impactPosition = GlobalPosition + (-GlobalTransform.Basis.Z * 2f);

            var enemies = GetEnemiesInAOE(impactPosition);

            foreach (var enemy in enemies)
            {
                var healthComp = enemy.GetNodeOrNull<HealthComponent>("HealthComponent");
                if (healthComp != null)
                {
                    // Calculate damage falloff based on distance
                    float distance = impactPosition.DistanceTo(enemy.GlobalPosition);
                    float falloff = 1f - (distance / AOERadius);
                    float finalDamage = BaseDamage * falloff;

                    healthComp.TakeDamage(finalDamage, this);
                    GD.Print($"War Hammer hit {enemy.Name} for {finalDamage} damage");

                    // Apply knockback
                    ApplyKnockback(enemy, impactPosition);
                }
            }

            // Play slam animation
            PlaySlamAnimation();

            // Spawn ground impact effect
            SpawnImpactEffect(impactPosition);

            // Emit event for camera shake
            EventBus.Emit("war_hammer_impact", impactPosition);
        }

        #endregion

        #region Private Methods

        private Godot.Collections.Array<Node3D> GetEnemiesInAOE(Vector3 center)
        {
            var result = new Godot.Collections.Array<Node3D>();
            var enemies = GetTree().GetNodesInGroup("enemies");

            foreach (var enemy in enemies)
            {
                if (enemy is Node3D enemy3D)
                {
                    float distance = center.DistanceTo(enemy3D.GlobalPosition);
                    if (distance <= AOERadius)
                    {
                        result.Add(enemy3D);
                    }
                }
            }

            return result;
        }

        private void ApplyKnockback(Node3D enemy, Vector3 impactPosition)
        {
            // Try to find movement component
            var movement = enemy.GetNodeOrNull<MovementComponent>("MovementComponent");
            if (movement != null)
            {
                Vector3 direction = (enemy.GlobalPosition - impactPosition).Normalized();
                direction.Y = 0; // Keep on ground
                
                // Apply knockback velocity
                movement.Velocity += direction * KnockbackForce;
            }
        }

        private void PlaySlamAnimation()
        {
            // TODO: Implement hammer slam animation
        }

        private void SpawnImpactEffect(Vector3 position)
        {
            // TODO: Spawn ground impact particle effect and shockwave
        }

        #endregion
    }
}
