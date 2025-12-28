using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Components
{
    /// <summary>
    /// Reusable health management component.
    /// Handles HP, damage, death, and regeneration.
    /// </summary>
    public partial class HealthComponent : Node
    {
        #region Exported Properties

        [Export] public float MaxHealth { get; set; } = 100f;
        [Export] public float RegenerationRate { get; set; } = 0f; // HP per second
        [Export] public float RegenerationDelay { get; set; } = 3f; // Seconds after damage before regen starts
        [Export] public bool IsInvulnerable { get; set; } = false;

        #endregion

        #region Public Properties

        public float CurrentHealth { get; private set; }
        public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
        public bool IsDead => CurrentHealth <= 0;

        #endregion

        #region Private Fields

        private float _timeSinceLastDamage = 0f;
        private bool _isDead = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            CurrentHealth = MaxHealth;
        }

        public override void _Process(double delta)
        {
            if (_isDead || RegenerationRate <= 0)
                return;

            _timeSinceLastDamage += (float)delta;

            // Start regeneration after delay
            if (_timeSinceLastDamage >= RegenerationDelay && CurrentHealth < MaxHealth)
            {
                Heal(RegenerationRate * (float)delta);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Apply damage to this entity
        /// </summary>
        /// <param name="amount">Amount of damage to apply</param>
        /// <param name="damageSource">Optional source of damage for tracking</param>
        public void TakeDamage(float amount, Node damageSource = null)
        {
            if (_isDead || IsInvulnerable || amount <= 0)
                return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            _timeSinceLastDamage = 0f;

            // Emit health changed event
            EventBus.Emit(EventBus.HealthChanged, new HealthChangedData
            {
                Entity = GetParent(),
                CurrentHealth = CurrentHealth,
                MaxHealth = MaxHealth,
                DamageAmount = amount,
                DamageSource = damageSource
            });

            // Check for death
            if (CurrentHealth <= 0 && !_isDead)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal this entity
        /// </summary>
        /// <param name="amount">Amount of health to restore</param>
        public void Heal(float amount)
        {
            if (_isDead || amount <= 0)
                return;

            float oldHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

            if (CurrentHealth != oldHealth)
            {
                EventBus.Emit(EventBus.HealthChanged, new HealthChangedData
                {
                    Entity = GetParent(),
                    CurrentHealth = CurrentHealth,
                    MaxHealth = MaxHealth,
                    DamageAmount = -(CurrentHealth - oldHealth), // Negative for healing
                    DamageSource = null
                });
            }
        }

        /// <summary>
        /// Instantly kill this entity
        /// </summary>
        public void Kill()
        {
            if (_isDead)
                return;

            CurrentHealth = 0;
            Die();
        }

        /// <summary>
        /// Reset health to maximum
        /// </summary>
        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            _isDead = false;
            _timeSinceLastDamage = 0f;
        }

        #endregion

        #region Private Methods

        private void Die()
        {
            _isDead = true;

            // Emit death event
            EventBus.Emit(EventBus.EntityDied, new EntityDiedData
            {
                Entity = GetParent(),
                Position = GetParent<Node3D>()?.GlobalPosition ?? Vector3.Zero
            });

            GD.Print($"{GetParent().Name} died");
        }

        #endregion
    }

    #region Event Data Structures

    /// <summary>
    /// Data structure for health changed events
    /// </summary>
    public class HealthChangedData
    {
        public Node Entity { get; set; }
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        public float DamageAmount { get; set; }
        public Node DamageSource { get; set; }
    }

    /// <summary>
    /// Data structure for entity died events
    /// </summary>
    public class EntityDiedData
    {
        public Node Entity { get; set; }
        public Vector3 Position { get; set; }
    }

    #endregion
}
