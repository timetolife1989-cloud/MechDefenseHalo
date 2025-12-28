using Godot;
using System;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Drones
{
    /// <summary>
    /// Repair drone that heals the player and allies over time.
    /// </summary>
    public partial class RepairDrone : DroneBase
    {
        #region Exported Properties

        [Export] public float HealRate { get; set; } = 10f; // HP per second
        [Export] public float HealRange { get; set; } = 10f;

        #endregion

        #region Constructor

        public RepairDrone()
        {
            DroneName = "Repair Drone";
            EnergyCost = 20f;
            Lifetime = 25f;
        }

        #endregion

        #region Protected Methods

        protected override Color GetDroneColor()
        {
            return new Color(0.3f, 1f, 0.3f); // Green for healing
        }

        protected override void OnUpdate(float delta)
        {
            // Heal orbit target (player)
            if (OrbitTarget != null && IsInstanceValid(OrbitTarget))
            {
                float distanceToTarget = GlobalPosition.DistanceTo(OrbitTarget.GlobalPosition);
                
                if (distanceToTarget <= HealRange)
                {
                    HealTarget(OrbitTarget, delta);
                }
            }

            // TODO: Also heal nearby allies
        }

        #endregion

        #region Private Methods

        private void HealTarget(Node3D target, float delta)
        {
            var healthComp = target.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (healthComp != null && !healthComp.IsDead)
            {
                float healAmount = HealRate * delta;
                healthComp.Heal(healAmount);
                
                // TODO: Spawn heal particle effect
                SpawnHealEffect(target);
            }
        }

        private void SpawnHealEffect(Node3D target)
        {
            // TODO: Create healing beam or particles
        }

        #endregion
    }
}
