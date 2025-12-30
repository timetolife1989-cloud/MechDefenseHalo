using Godot;
using System;

namespace MechDefenseHalo.Components
{
    /// <summary>
    /// Reusable HealthSystem component that handles health, damage, healing, death, and shield mechanics.
    /// Can be attached to any entity (player, enemies, structures).
    /// </summary>
    public partial class HealthSystem : Node
    {
        // Signals
        [Signal] public delegate void HealthChangedEventHandler(float current, float max);
        [Signal] public delegate void ShieldChangedEventHandler(float current, float max);
        [Signal] public delegate void DamageTakenEventHandler(float amount, string damageType);
        [Signal] public delegate void HealedEventHandler(float amount);
        [Signal] public delegate void DiedEventHandler();
        [Signal] public delegate void RevivedEventHandler();
        [Signal] public delegate void ShieldBrokenEventHandler();
        [Signal] public delegate void InvincibilityStartedEventHandler();
        [Signal] public delegate void InvincibilityEndedEventHandler();

        // Health Properties
        [ExportGroup("Health Settings")]
        [Export] public float MaxHealth { get; set; } = 100f;
        [Export] public float CurrentHealth { get; private set; } = 100f;
        [Export] public bool IsInvincible { get; set; } = false;
        [Export] public float InvincibilityDuration { get; set; } = 1.0f;

        private bool isDead = false;
        private bool isInvincibilityActive = false;

        // Shield Properties
        [ExportGroup("Shield Settings")]
        [Export] public bool HasShield { get; set; } = false;
        [Export] public float MaxShield { get; set; } = 50f;
        [Export] public float CurrentShield { get; private set; } = 50f;
        [Export] public float ShieldRegenRate { get; set; } = 5f; // HP per second
        [Export] public float ShieldRegenDelay { get; set; } = 3f; // Seconds after damage

        private float timeSinceLastDamage = 0f;

        /// <summary>
        /// Initialize health and shield values
        /// </summary>
        public override void _Ready()
        {
            CurrentHealth = MaxHealth;
            if (HasShield)
            {
                CurrentShield = MaxShield;
            }
            
            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
            if (HasShield)
            {
                EmitSignal(SignalName.ShieldChanged, CurrentShield, MaxShield);
            }
        }

        /// <summary>
        /// Apply damage to the entity. Damage is applied to shield first, then to health.
        /// </summary>
        /// <param name="amount">Amount of damage to apply</param>
        /// <param name="damageType">Type of damage (for future expansion)</param>
        public void TakeDamage(float amount, string damageType = "normal")
        {
            if (isDead || IsInvincible || isInvincibilityActive)
            {
                GD.Print($"Damage blocked: invincible or dead");
                return;
            }
            
            if (amount <= 0) return;
            
            timeSinceLastDamage = 0f;
            
            // Apply damage to shield first
            if (HasShield && CurrentShield > 0)
            {
                float shieldDamage = Mathf.Min(amount, CurrentShield);
                CurrentShield -= shieldDamage;
                amount -= shieldDamage;
                
                EmitSignal(SignalName.ShieldChanged, CurrentShield, MaxShield);
                EmitSignal(SignalName.DamageTaken, shieldDamage, damageType);
                
                if (CurrentShield <= 0)
                {
                    EmitSignal(SignalName.ShieldBroken);
                    GD.Print("Shield broken!");
                }
            }
            
            // Apply remaining damage to health
            if (amount > 0)
            {
                CurrentHealth -= amount;
                CurrentHealth = Mathf.Max(CurrentHealth, 0);
                
                EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
                EmitSignal(SignalName.DamageTaken, amount, damageType);
                
                GD.Print($"Took {amount} damage. Health: {CurrentHealth}/{MaxHealth}");
            }
            
            // Check death
            if (CurrentHealth <= 0 && !isDead)
            {
                Die();
            }
            else if (InvincibilityDuration > 0)
            {
                StartInvincibility();
            }
        }

        /// <summary>
        /// Heal the entity by the specified amount (cannot exceed MaxHealth)
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        public void Heal(float amount)
        {
            if (isDead || amount <= 0) return;
            
            CurrentHealth += amount;
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
            
            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
            EmitSignal(SignalName.Healed, amount);
            
            GD.Print($"Healed {amount}. Health: {CurrentHealth}/{MaxHealth}");
        }

        /// <summary>
        /// Restore shield by the specified amount (cannot exceed MaxShield)
        /// </summary>
        /// <param name="amount">Amount to restore</param>
        public void RestoreShield(float amount)
        {
            if (!HasShield || isDead || amount <= 0) return;
            
            CurrentShield += amount;
            CurrentShield = Mathf.Min(CurrentShield, MaxShield);
            
            EmitSignal(SignalName.ShieldChanged, CurrentShield, MaxShield);
            GD.Print($"Shield restored {amount}. Shield: {CurrentShield}/{MaxShield}");
        }

        /// <summary>
        /// Start temporary invincibility
        /// </summary>
        private async void StartInvincibility()
        {
            if (isInvincibilityActive) return;
            
            isInvincibilityActive = true;
            EmitSignal(SignalName.InvincibilityStarted);
            
            await ToSignal(GetTree().CreateTimer(InvincibilityDuration), "timeout");
            
            isInvincibilityActive = false;
            EmitSignal(SignalName.InvincibilityEnded);
        }

        /// <summary>
        /// Handle shield regeneration over time
        /// </summary>
        /// <param name="delta">Time elapsed since last frame</param>
        public override void _Process(double delta)
        {
            if (!HasShield || isDead || CurrentShield >= MaxShield) return;
            
            timeSinceLastDamage += (float)delta;
            
            // Start regenerating after delay
            if (timeSinceLastDamage >= ShieldRegenDelay)
            {
                CurrentShield += ShieldRegenRate * (float)delta;
                CurrentShield = Mathf.Min(CurrentShield, MaxShield);
                
                EmitSignal(SignalName.ShieldChanged, CurrentShield, MaxShield);
            }
        }

        /// <summary>
        /// Handle entity death
        /// </summary>
        private void Die()
        {
            if (isDead) return;
            
            isDead = true;
            CurrentHealth = 0;
            
            EmitSignal(SignalName.Died);
            GD.Print($"{GetParent().Name} died!");
            
            // Note: Death animation and cleanup should be handled by the parent entity
            // Connect to the Died signal to trigger animations, effects, and eventual cleanup
        }

        /// <summary>
        /// Revive the entity with specified health amount
        /// </summary>
        /// <param name="healthAmount">Amount of health to revive with. If negative, revive with MaxHealth</param>
        public void Revive(float healthAmount = -1)
        {
            if (!isDead) return;
            
            isDead = false;
            
            if (healthAmount < 0)
            {
                CurrentHealth = MaxHealth;
            }
            else
            {
                CurrentHealth = Mathf.Min(healthAmount, MaxHealth);
            }
            
            if (HasShield)
            {
                CurrentShield = MaxShield;
            }
            
            EmitSignal(SignalName.Revived);
            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
            
            GD.Print($"{GetParent().Name} revived!");
        }

        /// <summary>
        /// Check if entity is dead
        /// </summary>
        /// <returns>True if dead, false otherwise</returns>
        public bool IsDead() => isDead;

        /// <summary>
        /// Check if entity is alive
        /// </summary>
        /// <returns>True if alive, false otherwise</returns>
        public bool IsAlive() => !isDead;

        /// <summary>
        /// Get current health as a percentage of max health
        /// </summary>
        /// <returns>Health percentage (0.0 to 1.0)</returns>
        public float GetHealthPercent() => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;

        /// <summary>
        /// Get current shield as a percentage of max shield
        /// </summary>
        /// <returns>Shield percentage (0.0 to 1.0)</returns>
        public float GetShieldPercent() => MaxShield > 0 ? CurrentShield / MaxShield : 0;

        /// <summary>
        /// Set maximum health and adjust current health proportionally
        /// </summary>
        /// <param name="newMax">New maximum health value</param>
        public void SetMaxHealth(float newMax)
        {
            float ratio = GetHealthPercent();
            MaxHealth = newMax;
            CurrentHealth = MaxHealth * ratio;
            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
        }
    }
}
