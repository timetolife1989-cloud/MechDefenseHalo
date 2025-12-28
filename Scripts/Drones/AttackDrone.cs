using Godot;
using System;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Drones
{
    /// <summary>
    /// Attack drone that automatically shoots enemies.
    /// </summary>
    public partial class AttackDrone : DroneBase
    {
        #region Exported Properties

        [Export] public float Damage { get; set; } = 15f;
        [Export] public float FireRate { get; set; } = 0.5f;
        [Export] public float AttackRange { get; set; } = 25f;

        #endregion

        #region Private Fields

        private float _fireCooldown = 0f;
        private Node3D _currentTarget;

        #endregion

        #region Constructor

        public AttackDrone()
        {
            DroneName = "Attack Drone";
            EnergyCost = 25f;
            Lifetime = 30f;
        }

        #endregion

        #region Protected Methods

        protected override Color GetDroneColor()
        {
            return new Color(1f, 0.3f, 0.3f); // Red for attack
        }

        protected override void OnUpdate(float delta)
        {
            // Update fire cooldown
            if (_fireCooldown > 0)
            {
                _fireCooldown -= delta;
            }

            // Get target from controller
            if (_controller != null)
            {
                _currentTarget = _controller.CurrentTarget;
            }

            // Try to fire at target
            if (_currentTarget != null && IsInstanceValid(_currentTarget) && _fireCooldown <= 0)
            {
                float distanceToTarget = GlobalPosition.DistanceTo(_currentTarget.GlobalPosition);
                
                if (distanceToTarget <= AttackRange)
                {
                    FireAtTarget();
                }
            }
        }

        #endregion

        #region Private Methods

        private void FireAtTarget()
        {
            if (_currentTarget == null)
                return;

            // Deal damage to target
            var healthComp = _currentTarget.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (healthComp != null)
            {
                healthComp.TakeDamage(Damage, this);
                GD.Print($"Attack Drone hit {_currentTarget.Name} for {Damage} damage");
            }

            _fireCooldown = FireRate;

            // TODO: Spawn projectile visual or beam effect
            SpawnAttackEffect();
        }

        private void SpawnAttackEffect()
        {
            // TODO: Create laser beam or projectile visual
        }

        #endregion
    }
}
