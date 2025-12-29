using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Components;
using MechDefenseHalo.Progression;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Abstract base class for all enemies.
    /// Provides common functionality for health, movement, and attacking.
    /// </summary>
    public abstract partial class EnemyBase : CharacterBody3D
    {
        #region Exported Properties

        [Export] public string EnemyName { get; set; } = "Enemy";
        [Export] public int Level { get; set; } = 1;
        [Export] public float MaxHealth { get; set; } = 100f;
        [Export] public float MoveSpeed { get; set; } = 3f;
        [Export] public float AttackDamage { get; set; } = 10f;
        [Export] public float AttackRange { get; set; } = 2f;
        [Export] public float AttackCooldown { get; set; } = 1f;
        [Export] public float DetectionRange { get; set; } = 30f;

        #endregion

        #region Public Properties

        public Node3D Target { get; protected set; }
        public bool IsAlive => _health != null && !_health.IsDead;

        #endregion

        #region Protected Fields

        protected HealthComponent _health;
        protected MovementComponent _movement;
        protected ElementalResistanceComponent _resistance;
        protected StatusEffectComponent _statusEffect;
        protected float _attackTimer = 0f;
        protected float _gravity;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Add to enemies group
            AddToGroup("enemies");

            // Get or create HealthComponent
            _health = GetNodeOrNull<HealthComponent>("HealthComponent");
            if (_health == null)
            {
                _health = new HealthComponent();
                _health.Name = "HealthComponent";
                _health.MaxHealth = MaxHealth;
                AddChild(_health);
            }

            // Get or create MovementComponent
            _movement = GetNodeOrNull<MovementComponent>("MovementComponent");
            if (_movement == null)
            {
                _movement = new MovementComponent();
                _movement.Name = "MovementComponent";
                _movement.MaxSpeed = MoveSpeed;
                AddChild(_movement);
            }

            // Get optional components
            _resistance = GetNodeOrNull<ElementalResistanceComponent>("ElementalResistanceComponent");
            _statusEffect = GetNodeOrNull<StatusEffectComponent>("StatusEffectComponent");

            // Get gravity
            _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

            // Listen for death event
            EventBus.On(EventBus.EntityDied, OnEntityDied);

            OnReady();
        }

        public override void _ExitTree()
        {
            EventBus.Off(EventBus.EntityDied, OnEntityDied);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!IsAlive)
                return;

            float dt = (float)delta;

            // Update attack timer
            if (_attackTimer > 0)
            {
                _attackTimer -= dt;
            }

            // Find target if we don't have one
            if (Target == null || !IsInstanceValid(Target))
            {
                FindTarget();
            }

            // Update AI behavior
            UpdateBehavior(dt);

            // Update movement
            if (_movement != null)
            {
                _movement.UpdateMovement(dt);
            }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Called once during _Ready - override for custom initialization
        /// </summary>
        protected virtual void OnReady() { }

        /// <summary>
        /// Called each physics frame - implement enemy-specific behavior
        /// </summary>
        protected abstract void UpdateBehavior(float delta);

        /// <summary>
        /// Called when enemy dies - override for custom death behavior
        /// </summary>
        protected virtual void OnDeath() { }

        #endregion

        #region Protected Methods

        protected virtual void FindTarget()
        {
            // Find player by group
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0 && players[0] is Node3D player)
            {
                float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
                if (distance <= DetectionRange)
                {
                    Target = player;
                }
            }
        }

        protected void MoveTowardsTarget(float delta)
        {
            if (Target == null || !IsInstanceValid(Target))
                return;

            Vector3 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
            direction.Y = 0; // Keep on ground

            if (_movement != null)
            {
                _movement.SetDirection(direction);
                _movement.LookAtTarget(Target.GlobalPosition, delta);
            }
        }

        protected bool IsInAttackRange()
        {
            if (Target == null || !IsInstanceValid(Target))
                return false;

            float distance = GlobalPosition.DistanceTo(Target.GlobalPosition);
            return distance <= AttackRange;
        }

        protected void TryAttack()
        {
            if (_attackTimer > 0 || !IsInAttackRange())
                return;

            PerformAttack();
            _attackTimer = AttackCooldown;
        }

        protected virtual void PerformAttack()
        {
            if (Target == null)
                return;

            // Try to damage target
            var targetHealth = Target.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(AttackDamage, this);
                GD.Print($"{EnemyName} attacked {Target.Name} for {AttackDamage} damage");
            }

            OnAttackPerformed();
        }

        protected virtual void OnAttackPerformed() { }

        #endregion

        #region Private Methods

        private void OnEntityDied(object data)
        {
            if (data is EntityDiedData diedData && diedData.Entity == this)
            {
                OnDeath();
                
                // Grant XP to player (base 10 XP * enemy level)
                int xpAmount = 10 * Level;
                PlayerLevel.AddXP(xpAmount, $"{EnemyName} kill");
                
                // Remove from group
                RemoveFromGroup("enemies");

                // Despawn after a delay
                var timer = GetTree().CreateTimer(2f);
                timer.Timeout += () => QueueFree();
            }
        }

        #endregion
    }
}
