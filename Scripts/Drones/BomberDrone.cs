using Godot;
using System;
using MechDefenseHalo.Components;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Drones
{
    /// <summary>
    /// Bomber drone that performs AOE explosion attacks.
    /// </summary>
    public partial class BomberDrone : DroneBase
    {
        #region Exported Properties

        [Export] public float ExplosionDamage { get; set; } = 75f;
        [Export] public float ExplosionRadius { get; set; } = 8f;
        [Export] public float BombInterval { get; set; } = 3f; // Seconds between bombs

        #endregion

        #region Private Fields

        private float _bombTimer = 0f;
        private Node3D _currentTarget;

        #endregion

        #region Constructor

        public BomberDrone()
        {
            DroneName = "Bomber Drone";
            EnergyCost = 35f;
            Lifetime = 15f;
        }

        #endregion

        #region Protected Methods

        protected override Color GetDroneColor()
        {
            return new Color(1f, 0.5f, 0f); // Orange for bomber
        }

        protected override void OnUpdate(float delta)
        {
            _bombTimer += delta;

            // Get target from controller
            if (_controller != null)
            {
                _currentTarget = _controller.CurrentTarget;
            }

            // Drop bomb when ready and target available
            if (_bombTimer >= BombInterval && _currentTarget != null && IsInstanceValid(_currentTarget))
            {
                _bombTimer = 0f;
                DropBomb();
            }
        }

        #endregion

        #region Private Methods

        private void DropBomb()
        {
            if (_currentTarget == null)
                return;

            Vector3 bombPosition = _currentTarget.GlobalPosition;
            
            GD.Print($"Bomber Drone dropped bomb at {bombPosition}");

            // Deal AOE damage
            var enemies = GetTree().GetNodesInGroup("enemies");

            int hitCount = 0;

            foreach (var enemy in enemies)
            {
                if (enemy is Node3D enemy3D)
                {
                    float distance = bombPosition.DistanceTo(enemy3D.GlobalPosition);
                    
                    if (distance <= ExplosionRadius)
                    {
                        // Calculate damage falloff
                        float falloff = 1f - (distance / ExplosionRadius);
                        float finalDamage = ExplosionDamage * falloff;

                        var healthComp = enemy3D.GetNodeOrNull<HealthComponent>("HealthComponent");
                        if (healthComp != null)
                        {
                            healthComp.TakeDamage(finalDamage, this);
                            hitCount++;
                        }
                    }
                }
            }

            GD.Print($"Bomb hit {hitCount} enemies");

            // Spawn explosion effect
            SpawnExplosionEffect(bombPosition);

            // Emit damage dealt event
            EventBus.Emit(EventBus.DamageDealt, new DamageDealtData
            {
                Source = this,
                Target = null,
                Damage = ExplosionDamage,
                ElementType = ElementalType.Fire,
                IsCritical = false
            });
        }

        private void SpawnExplosionEffect(Vector3 position)
        {
            // TODO: Create explosion particle effect and shockwave
        }

        #endregion
    }
}
