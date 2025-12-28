using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Components
{
    /// <summary>
    /// Reusable weapon component for firing, ammo management, and cooldowns.
    /// </summary>
    public partial class WeaponComponent : Node
    {
        #region Exported Properties

        [Export] public int MaxAmmo { get; set; } = 30;
        [Export] public int CurrentAmmo { get; set; } = 30;
        [Export] public float FireRate { get; set; } = 0.1f; // Seconds between shots
        [Export] public float ReloadTime { get; set; } = 2f;
        [Export] public float Damage { get; set; } = 10f;
        [Export] public float Range { get; set; } = 100f;
        [Export] public bool AutoReload { get; set; } = true;
        [Export] public ElementalType ElementType { get; set; } = ElementalType.Physical;

        #endregion

        #region Public Properties

        public bool IsReloading { get; private set; } = false;
        public bool CanFire => !IsReloading && CurrentAmmo > 0 && _fireCooldown <= 0;
        public float ReloadProgress => IsReloading ? (_reloadTimer / ReloadTime) : 0f;

        #endregion

        #region Private Fields

        private float _fireCooldown = 0f;
        private float _reloadTimer = 0f;

        #endregion

        #region Godot Lifecycle

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // Update fire cooldown
            if (_fireCooldown > 0)
            {
                _fireCooldown -= dt;
            }

            // Update reload timer
            if (IsReloading)
            {
                _reloadTimer += dt;
                if (_reloadTimer >= ReloadTime)
                {
                    FinishReload();
                }
            }

            // Auto reload when empty
            if (AutoReload && CurrentAmmo <= 0 && !IsReloading)
            {
                StartReload();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempt to fire the weapon
        /// </summary>
        /// <returns>True if weapon fired successfully</returns>
        public bool TryFire()
        {
            if (!CanFire)
                return false;

            CurrentAmmo--;
            _fireCooldown = FireRate;

            EventBus.Emit(EventBus.WeaponFired, new WeaponFiredData
            {
                Weapon = this,
                Damage = Damage,
                ElementType = ElementType,
                RemainingAmmo = CurrentAmmo
            });

            EventBus.Emit(EventBus.AmmoChanged, new AmmoChangedData
            {
                CurrentAmmo = CurrentAmmo,
                MaxAmmo = MaxAmmo
            });

            return true;
        }

        /// <summary>
        /// Start reloading the weapon
        /// </summary>
        public void StartReload()
        {
            if (IsReloading || CurrentAmmo >= MaxAmmo)
                return;

            IsReloading = true;
            _reloadTimer = 0f;
        }

        /// <summary>
        /// Cancel reload (if needed for weapon switching)
        /// </summary>
        public void CancelReload()
        {
            IsReloading = false;
            _reloadTimer = 0f;
        }

        /// <summary>
        /// Add ammo to the weapon (from pickups)
        /// </summary>
        public void AddAmmo(int amount)
        {
            CurrentAmmo = Mathf.Min(MaxAmmo, CurrentAmmo + amount);
            
            EventBus.Emit(EventBus.AmmoChanged, new AmmoChangedData
            {
                CurrentAmmo = CurrentAmmo,
                MaxAmmo = MaxAmmo
            });
        }

        /// <summary>
        /// Reset weapon to full ammo
        /// </summary>
        public void ResetAmmo()
        {
            CurrentAmmo = MaxAmmo;
            IsReloading = false;
            _reloadTimer = 0f;
            _fireCooldown = 0f;
        }

        #endregion

        #region Private Methods

        private void FinishReload()
        {
            IsReloading = false;
            _reloadTimer = 0f;
            CurrentAmmo = MaxAmmo;

            EventBus.Emit(EventBus.WeaponReloaded, this);
            EventBus.Emit(EventBus.AmmoChanged, new AmmoChangedData
            {
                CurrentAmmo = CurrentAmmo,
                MaxAmmo = MaxAmmo
            });
        }

        #endregion
    }

    #region Event Data Structures

    public class WeaponFiredData
    {
        public WeaponComponent Weapon { get; set; }
        public float Damage { get; set; }
        public ElementalType ElementType { get; set; }
        public int RemainingAmmo { get; set; }
    }

    public class AmmoChangedData
    {
        public int CurrentAmmo { get; set; }
        public int MaxAmmo { get; set; }
    }

    #endregion
}
