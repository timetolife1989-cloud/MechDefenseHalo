using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Weapons
{
    /// <summary>
    /// Abstract base class for all weapons in the game.
    /// Provides common functionality for firing, ammo, and damage.
    /// </summary>
    public abstract partial class WeaponBase : Node3D
    {
        #region Exported Properties

        [Export] public string WeaponName { get; set; } = "Weapon";
        [Export] public float BaseDamage { get; set; } = 10f;
        [Export] public float FireRate { get; set; } = 0.1f;
        [Export] public float Range { get; set; } = 100f;
        [Export] public int MaxAmmo { get; set; } = 30;
        [Export] public float ReloadTime { get; set; } = 2f;
        [Export] public ElementalType ElementType { get; set; } = ElementalType.Physical;
        [Export] public bool IsAutomatic { get; set; } = false;

        #endregion

        #region Public Properties

        public int CurrentAmmo { get; protected set; }
        public bool IsReloading { get; protected set; } = false;
        public bool CanFire => !IsReloading && CurrentAmmo > 0 && _fireCooldown <= 0;
        public float ReloadProgress => IsReloading ? (_reloadTimer / ReloadTime) : 0f;

        #endregion

        #region Protected Fields

        protected float _fireCooldown = 0f;
        protected float _reloadTimer = 0f;
        protected Camera3D _camera;
        protected Node3D _muzzle;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            CurrentAmmo = MaxAmmo;
            _muzzle = GetNodeOrNull<Node3D>("Muzzle");
            
            // Find camera in scene
            CallDeferred(nameof(FindCamera));
        }

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
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempt to fire the weapon
        /// </summary>
        public bool TryFire()
        {
            if (!CanFire)
                return false;

            CurrentAmmo--;
            _fireCooldown = FireRate;

            // Call abstract method for weapon-specific firing
            OnFire();

            // Emit events
            EventBus.Emit(EventBus.WeaponFired, new WeaponFiredData
            {
                Weapon = null, // WeaponComponent reference not needed here
                Damage = BaseDamage,
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
            
            OnReloadStart();
        }

        /// <summary>
        /// Cancel reload
        /// </summary>
        public void CancelReload()
        {
            IsReloading = false;
            _reloadTimer = 0f;
        }

        /// <summary>
        /// Add ammo to weapon
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

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Called when weapon fires - implement weapon-specific logic
        /// </summary>
        protected abstract void OnFire();

        /// <summary>
        /// Called when reload starts - override for custom effects
        /// </summary>
        protected virtual void OnReloadStart() { }

        /// <summary>
        /// Called when reload completes - override for custom effects
        /// </summary>
        protected virtual void OnReloadComplete() { }

        #endregion

        #region Protected Methods

        protected void FindCamera()
        {
            var viewport = GetViewport();
            _camera = viewport?.GetCamera3D();
        }

        protected void FinishReload()
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

            OnReloadComplete();
        }

        /// <summary>
        /// Perform a raycast from camera to detect hits
        /// </summary>
        protected RaycastResult PerformRaycast()
        {
            if (_camera == null)
                return new RaycastResult { Hit = false };

            var spaceState = GetWorld3D().DirectSpaceState;
            
            // Raycast from camera center
            Vector3 from = _camera.GlobalPosition;
            Vector3 to = from + (-_camera.GlobalTransform.Basis.Z * Range);

            var query = PhysicsRayQueryParameters3D.Create(from, to);
            query.CollideWithAreas = true;
            query.CollideWithBodies = true;

            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                return new RaycastResult
                {
                    Hit = true,
                    Position = (Vector3)result["position"],
                    Normal = (Vector3)result["normal"],
                    Collider = (Node)result["collider"]
                };
            }

            return new RaycastResult { Hit = false };
        }

        /// <summary>
        /// Get muzzle position for projectile spawning
        /// </summary>
        protected Vector3 GetMuzzlePosition()
        {
            return _muzzle != null ? _muzzle.GlobalPosition : GlobalPosition;
        }

        /// <summary>
        /// Get firing direction from camera
        /// </summary>
        protected Vector3 GetFiringDirection()
        {
            if (_camera == null)
                return -GlobalTransform.Basis.Z;

            return -_camera.GlobalTransform.Basis.Z;
        }

        #endregion
    }

    #region Helper Structures

    /// <summary>
    /// Result of a raycast operation
    /// </summary>
    public struct RaycastResult
    {
        public bool Hit;
        public Vector3 Position;
        public Vector3 Normal;
        public Node Collider;
    }

    #endregion
}
