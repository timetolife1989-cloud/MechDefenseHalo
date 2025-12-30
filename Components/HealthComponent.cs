using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Combat;

namespace MechDefenseHalo.Components
{
    /// <summary>
    /// Reusable health management component.
    /// Handles HP, damage, death, regeneration, and shields.
    /// </summary>
    public partial class HealthComponent : Node
    {
        #region Signals
        
        [Signal] public delegate void HealthChangedEventHandler(float current, float max);
        [Signal] public delegate void DamageTakenEventHandler(float amount, int damageType, Vector3 hitPosition);
        [Signal] public delegate void HealedEventHandler(float amount);
        [Signal] public delegate void DiedEventHandler();
        [Signal] public delegate void RevivedEventHandler();
        
        #endregion
        
        #region Exported Properties

        [Export] public float MaxHealth { get; set; } = 100f;
        [Export] public float MaxShield { get; set; } = 0f;
        [Export] public float ShieldRegenRate { get; set; } = 10f; // HP per second
        [Export] public float ShieldRegenDelay { get; set; } = 3f; // Delay after damage
        [Export] public float RegenerationRate { get; set; } = 0f; // HP per second
        [Export] public float RegenerationDelay { get; set; } = 3f; // Seconds after damage before regen starts
        [Export] public bool IsInvulnerable { get; set; } = false;

        #endregion

        #region Public Properties

        public float CurrentHealth { get; private set; }
        public float CurrentShield { get; private set; }
        public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
        public float ShieldPercent => MaxShield > 0 ? CurrentShield / MaxShield : 0f;
        public bool IsDead => CurrentHealth <= 0;

        #endregion

        #region Private Fields

        private float _timeSinceLastDamage = 0f;
        private float _shieldRegenTimer;
        private bool _isDead = false;
        private ArmorComponent _armorComponent;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            CurrentHealth = MaxHealth;
            CurrentShield = MaxShield;
            _armorComponent = GetParent().GetNodeOrNull<ArmorComponent>("ArmorComponent");
        }

        public override void _Process(double delta)
        {
            if (_isDead)
                return;

            // Shield regeneration
            if (MaxShield > 0 && CurrentShield < MaxShield)
            {
                _shieldRegenTimer -= (float)delta;
                
                if (_shieldRegenTimer <= 0)
                {
                    float regenAmount = ShieldRegenRate * (float)delta;
                    CurrentShield = Mathf.Min(CurrentShield + regenAmount, MaxShield);
                    EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
                }
            }

            // Health regeneration
            if (RegenerationRate > 0)
            {
                _timeSinceLastDamage += (float)delta;

                // Start regeneration after delay
                if (_timeSinceLastDamage >= RegenerationDelay && CurrentHealth < MaxHealth)
                {
                    Heal(RegenerationRate * (float)delta);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Apply damage to this entity with damage type support
        /// </summary>
        /// <param name="rawDamage">Raw damage amount before calculations</param>
        /// <param name="hitPosition">Position where the damage occurred</param>
        /// <param name="damageType">Type of damage being dealt</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        public void TakeDamage(float rawDamage, Vector3 hitPosition, DamageType damageType = DamageType.Kinetic, bool isCritical = false)
        {
            if (_isDead || IsInvulnerable || rawDamage <= 0)
                return;

            // Calculate final damage
            float armor = _armorComponent?.GetArmor() ?? 0;
            string armorType = _armorComponent?.GetArmorType() ?? "Light";
            float critMultiplier = 1.5f; // TODO: Get from attacker stats
            
            float finalDamage = DamageCalculator.Instance?.CalculateDamage(
                rawDamage, damageType, armor, armorType, isCritical, critMultiplier) ?? rawDamage;
            
            // Apply to shield first
            if (CurrentShield > 0)
            {
                float shieldDamage = Mathf.Min(finalDamage, CurrentShield);
                CurrentShield -= shieldDamage;
                finalDamage -= shieldDamage;
                
                _shieldRegenTimer = ShieldRegenDelay; // Reset regen delay
            }
            
            // Apply remaining damage to health
            if (finalDamage > 0)
            {
                CurrentHealth -= finalDamage;
                CurrentHealth = Mathf.Max(0, CurrentHealth);
            }

            _timeSinceLastDamage = 0f;

            // Emit signals
            EmitSignal(SignalName.DamageTaken, finalDamage, (int)damageType, hitPosition);
            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
            
            // Emit event for combat feedback
            EventBus.Emit(EventBus.EntityDamaged, new Combat.EntityDamagedData
            {
                Target = GetParent(),
                Damage = finalDamage,
                Position = hitPosition,
                DamageType = damageType,
                IsCritical = isCritical
            });

            // Emit legacy health changed event for compatibility
            EventBus.Emit(EventBus.HealthChanged, new HealthChangedData
            {
                Entity = GetParent(),
                CurrentHealth = CurrentHealth,
                MaxHealth = MaxHealth,
                DamageAmount = finalDamage,
                DamageSource = null
            });

            // Check for death
            if (CurrentHealth <= 0 && !_isDead)
            {
                Die();
            }

            GD.Print($"Took {finalDamage:F1} {damageType} damage - HP: {CurrentHealth:F0}/{MaxHealth:F0}, Shield: {CurrentShield:F0}/{MaxShield:F0}");
        }

        /// <summary>
        /// Apply damage to this entity (legacy method for backward compatibility)
        /// </summary>
        /// <param name="amount">Amount of damage to apply</param>
        /// <param name="damageSource">Optional source of damage for tracking</param>
        public void TakeDamage(float amount, Node damageSource = null)
        {
            TakeDamage(amount, GetParent<Node3D>()?.GlobalPosition ?? Vector3.Zero, DamageType.Kinetic, false);
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
                EmitSignal(SignalName.Healed, amount);
                EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
                
                EventBus.Emit(EventBus.EntityHealed, new EntityHealedData
                {
                    Target = GetParent(),
                    Amount = CurrentHealth - oldHealth
                });
                
                // Legacy event
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
        /// Restore shield amount
        /// </summary>
        /// <param name="amount">Amount of shield to restore</param>
        public void RestoreShield(float amount)
        {
            if (_isDead || MaxShield == 0 || amount <= 0)
                return;
            
            CurrentShield = Mathf.Min(CurrentShield + amount, MaxShield);
            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
        }

        /// <summary>
        /// Revive this entity
        /// </summary>
        /// <param name="healthPercent">Health percentage to revive with (0.0 to 1.0)</param>
        public void Revive(float healthPercent = 0.5f)
        {
            if (!_isDead)
                return;
            
            CurrentHealth = MaxHealth * healthPercent;
            CurrentShield = MaxShield;
            _isDead = false;
            _timeSinceLastDamage = 0f;
            
            EmitSignal(SignalName.Revived);
            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
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
            CurrentShield = MaxShield;
            _isDead = false;
            _timeSinceLastDamage = 0f;
        }

        #endregion

        #region Private Methods

        private void Die()
        {
            _isDead = true;

            // Emit signals
            EmitSignal(SignalName.Died);

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

    /// <summary>
    /// Data structure for entity healed events
    /// </summary>
    public class EntityHealedData
    {
        public Node Target { get; set; }
        public float Amount { get; set; }
    }

    #endregion
}
