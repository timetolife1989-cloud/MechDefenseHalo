using Godot;
using System;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Ranged enemy - keeps distance and shoots at player.
    /// </summary>
    public partial class Shooter : EnemyBase
    {
        #region Exported Properties

        [Export] public float PreferredDistance { get; set; } = 15f;
        [Export] public float ProjectileSpeed { get; set; } = 30f;

        #endregion

        #region Constructor

        public Shooter()
        {
            EnemyName = "Shooter";
            MaxHealth = 40f;
            MoveSpeed = 3f;
            AttackDamage = 12f;
            AttackRange = 25f; // Shoot from range
            AttackCooldown = 2f;
            DetectionRange = 40f;
        }

        #endregion

        #region Protected Methods

        protected override void UpdateBehavior(float delta)
        {
            if (Target == null || !IsInstanceValid(Target))
                return;

            float distanceToTarget = GlobalPosition.DistanceTo(Target.GlobalPosition);

            // Maintain preferred distance
            if (distanceToTarget < PreferredDistance * 0.8f)
            {
                // Too close, back away
                Vector3 awayDirection = (GlobalPosition - Target.GlobalPosition).Normalized();
                awayDirection.Y = 0;
                
                if (_movement != null)
                {
                    _movement.SetDirection(awayDirection);
                }
            }
            else if (distanceToTarget > AttackRange)
            {
                // Too far, move closer
                MoveTowardsTarget(delta);
            }
            else
            {
                // In good position, stop and shoot
                if (_movement != null)
                {
                    _movement.Stop();
                }
            }

            // Always look at target
            if (_movement != null && Target != null)
            {
                _movement.LookAtTarget(Target.GlobalPosition, delta);
            }

            // Try to shoot
            if (distanceToTarget <= AttackRange)
            {
                TryAttack();
            }
        }

        protected override void PerformAttack()
        {
            if (Target == null)
                return;

            // Shoot projectile at target
            ShootProjectile();
            OnAttackPerformed();
        }

        protected override void OnAttackPerformed()
        {
            // TODO: Play shoot animation
        }

        protected override void OnDeath()
        {
            GD.Print($"{EnemyName} defeated!");
        }

        #endregion

        #region Private Methods

        private void ShootProjectile()
        {
            // Simple projectile creation
            var projectile = new Area3D();
            projectile.Name = "EnemyProjectile";
            
            // Add collision shape
            var shape = new CollisionShape3D();
            var sphereShape = new SphereShape3D();
            sphereShape.Radius = 0.2f;
            shape.Shape = sphereShape;
            projectile.AddChild(shape);

            // Add visual
            var mesh = new MeshInstance3D();
            var sphereMesh = new SphereMesh();
            sphereMesh.Radius = 0.2f;
            mesh.Mesh = sphereMesh;
            
            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(1f, 0.3f, 0.3f);
            material.EmissionEnabled = true;
            material.Emission = new Color(1f, 0.3f, 0.3f);
            material.EmissionEnergy = 1.5f;
            mesh.MaterialOverride = material;
            projectile.AddChild(mesh);

            // Add to scene
            GetTree().Root.AddChild(projectile);
            projectile.GlobalPosition = GlobalPosition + new Vector3(0, 1, 0);

            // Calculate direction to target
            Vector3 direction = (Target.GlobalPosition - projectile.GlobalPosition).Normalized();

            // Store velocity and target for movement
            projectile.SetMeta("velocity", direction * ProjectileSpeed);
            projectile.SetMeta("lifetime", 5f);
            projectile.SetMeta("damage", AttackDamage);
            projectile.SetMeta("source", this);

            // Connect collision
            projectile.BodyEntered += (body) => {
                if (body != this)
                {
                    var health = body.GetNodeOrNull<HealthComponent>("HealthComponent");
                    if (health != null)
                    {
                        health.TakeDamage((float)projectile.GetMeta("damage"), projectile);
                    }
                    projectile.QueueFree();
                }
            };

            // Move projectile in a process callback
            projectile.SetProcess(true);
            projectile.ProcessMode = ProcessModeEnum.Inherit;

            GD.Print($"{EnemyName} fired projectile!");
        }

        #endregion
    }
}
