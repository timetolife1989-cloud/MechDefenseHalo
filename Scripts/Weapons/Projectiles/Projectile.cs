using Godot;
using System;
using MechDefenseHalo.Components;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Weapons.Projectiles
{
    /// <summary>
    /// Base projectile class for all projectile-based weapons.
    /// Handles movement, collision, and damage.
    /// </summary>
    public partial class Projectile : Area3D
    {
        #region Exported Properties

        [Export] public float Speed { get; set; } = 50f;
        [Export] public float Damage { get; set; } = 10f;
        [Export] public float Lifetime { get; set; } = 5f;
        [Export] public ElementalType ElementType { get; set; } = ElementalType.Physical;

        #endregion

        #region Public Properties

        public Vector3 Direction { get; set; } = Vector3.Forward;
        public Node SourceWeapon { get; set; }
        public Action<Node> OnHitCallback { get; set; }

        #endregion

        #region Private Fields

        private float _lifeTimer = 0f;
        private bool _hasHit = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Setup collision
            BodyEntered += OnBodyEntered;
            AreaEntered += OnAreaEntered;

            // Ensure we have a collision shape
            if (GetChildCount() == 0 || GetNode<CollisionShape3D>(0) == null)
            {
                var shape = new CollisionShape3D();
                var sphereShape = new SphereShape3D();
                sphereShape.Radius = 0.2f;
                shape.Shape = sphereShape;
                AddChild(shape);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;

            // Move projectile
            GlobalPosition += Direction * Speed * dt;

            // Update lifetime
            _lifeTimer += dt;
            if (_lifeTimer >= Lifetime)
            {
                Destroy();
            }
        }

        #endregion

        #region Signal Handlers

        private void OnBodyEntered(Node3D body)
        {
            if (_hasHit)
                return;

            HandleHit(body);
        }

        private void OnAreaEntered(Area3D area)
        {
            if (_hasHit)
                return;

            HandleHit(area);
        }

        #endregion

        #region Private Methods

        private void HandleHit(Node target)
        {
            // Don't hit the source weapon's owner
            if (target == SourceWeapon || target.GetParent() == SourceWeapon)
                return;

            _hasHit = true;

            // Try to deal damage
            var healthComp = target.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (healthComp == null)
            {
                // Try parent
                healthComp = target.GetParent()?.GetNodeOrNull<HealthComponent>("HealthComponent");
            }

            if (healthComp != null)
            {
                healthComp.TakeDamage(Damage, SourceWeapon);
                GD.Print($"Projectile hit {target.Name} for {Damage} damage");

                // Apply elemental status effect
                ElementalSystem.ApplyStatusEffect(ElementType, target.GetParent() ?? target, 3f);
            }

            // Call custom callback if set
            OnHitCallback?.Invoke(target);

            // Spawn impact effect
            SpawnImpactEffect();

            // Destroy projectile
            Destroy();
        }

        private void SpawnImpactEffect()
        {
            // TODO: Spawn particle effect based on ElementType
            // For now, just a simple flash
        }

        private void Destroy()
        {
            QueueFree();
        }

        #endregion
    }
}
