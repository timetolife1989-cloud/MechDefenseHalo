using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Weapons.Ranged
{
    /// <summary>
    /// Chain lightning weapon with electric damage.
    /// Hits multiple targets in succession.
    /// </summary>
    public partial class TeslaCoil : WeaponBase
    {
        #region Exported Properties

        [Export] public int MaxChainTargets { get; set; } = 5;
        [Export] public float ChainRange { get; set; } = 15f;
        [Export] public float DamageReduction { get; set; } = 0.7f; // Each chain does 70% of previous

        #endregion

        #region Constructor

        public TeslaCoil()
        {
            WeaponName = "Tesla Coil";
            BaseDamage = 25f;
            FireRate = 1.2f; // ~0.8 shots per second
            Range = 50f;
            MaxAmmo = 15;
            ReloadTime = 2.5f;
            ElementType = ElementalType.Electric;
            IsAutomatic = false;
        }

        #endregion

        #region Protected Methods

        protected override void OnFire()
        {
            // Initial raycast to find first target
            var hit = PerformRaycast();

            if (hit.Hit)
            {
                // Start chain lightning
                ChainLightning(hit.Collider, GetMuzzlePosition(), BaseDamage);
                GD.Print("Tesla Coil fired - chain lightning!");
            }

            SpawnMuzzleFlash();
        }

        #endregion

        #region Private Methods

        private void ChainLightning(Node initialTarget, Vector3 sourcePosition, float initialDamage)
        {
            var hitTargets = new HashSet<Node>();
            Node currentTarget = initialTarget;
            Vector3 currentPosition = sourcePosition;
            float currentDamage = initialDamage;

            for (int i = 0; i < MaxChainTargets; i++)
            {
                if (currentTarget == null || hitTargets.Contains(currentTarget))
                    break;

                // Deal damage to current target
                var healthComp = currentTarget.GetNodeOrNull<HealthComponent>("HealthComponent");
                if (healthComp != null)
                {
                    healthComp.TakeDamage(currentDamage, this);
                    
                    // Apply shocked status
                    var statusEffect = currentTarget.GetNodeOrNull<StatusEffectComponent>("StatusEffectComponent");
                    if (statusEffect != null)
                    {
                        statusEffect.ApplyShocked(2f);
                    }

                    GD.Print($"Chain {i + 1}: Hit {currentTarget.Name} for {currentDamage} damage");
                }

                hitTargets.Add(currentTarget);

                // TODO: Draw lightning arc visual effect
                DrawLightningArc(currentPosition, GetTargetPosition(currentTarget));

                // Find next target
                Node3D nextTarget = FindNearestUnhitTarget(GetTargetPosition(currentTarget), hitTargets);
                if (nextTarget == null)
                    break;

                currentPosition = GetTargetPosition(currentTarget);
                currentTarget = nextTarget;
                currentDamage *= DamageReduction;
            }
        }

        private Node3D FindNearestUnhitTarget(Vector3 position, HashSet<Node> excludedTargets)
        {
            var enemies = GetTree().GetNodesInGroup("enemies");
            Node3D nearest = null;
            float nearestDistance = ChainRange;

            foreach (var enemy in enemies)
            {
                if (enemy is Node3D enemy3D && !excludedTargets.Contains(enemy))
                {
                    float distance = position.DistanceTo(enemy3D.GlobalPosition);
                    if (distance < nearestDistance)
                    {
                        nearest = enemy3D;
                        nearestDistance = distance;
                    }
                }
            }

            return nearest;
        }

        private Vector3 GetTargetPosition(Node target)
        {
            if (target is Node3D node3D)
                return node3D.GlobalPosition;
            return Vector3.Zero;
        }

        private void DrawLightningArc(Vector3 from, Vector3 to)
        {
            // TODO: Implement lightning visual effect
            // Could use ImmediateMesh or particles
        }

        private void SpawnMuzzleFlash()
        {
            // TODO: Implement tesla muzzle flash
        }

        #endregion
    }
}
