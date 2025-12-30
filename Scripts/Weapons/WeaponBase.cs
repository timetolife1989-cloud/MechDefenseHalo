using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Weapons
{
    public abstract partial class WeaponBase : Node3D
    {
        #region Exported Properties
        
        [Export] public string WeaponName { get; set; } = "Weapon";
        [Export] public float Damage { get; set; } = 10f;
        [Export] public float FireRate { get; set; } = 0.1f; // Time between shots
        [Export] public int MagazineSize { get; set; } = 30;
        [Export] public float ReloadTime { get; set; } = 2f;
        [Export] public float Range { get; set; } = 100f;
        [Export] public float Accuracy { get; set; } = 0.95f; // 1.0 = perfect
        [Export] public bool IsAutomatic { get; set; } = true;
        
        [Export] public PackedScene MuzzleFlashEffect { get; set; }
        [Export] public PackedScene ImpactEffect { get; set; }
        [Export] public AudioStream FireSound { get; set; }
        [Export] public AudioStream ReloadSound { get; set; }
        
        #endregion
        
        #region Public Properties
        
        public int CurrentAmmo { get; protected set; }
        public bool IsReloading { get; protected set; }
        public bool CanFire => !IsReloading && CurrentAmmo > 0 && _fireTimer <= 0;
        
        #endregion
        
        #region Protected Fields
        
        protected float _fireTimer;
        protected float _reloadTimer;
        protected Node3D _muzzlePoint;
        protected Camera3D _camera;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            CurrentAmmo = MagazineSize;
            _muzzlePoint = GetNodeOrNull<Node3D>("MuzzlePoint");
            _camera = GetViewport().GetCamera3D();
            
            GD.Print($"{WeaponName} initialized - Ammo: {CurrentAmmo}/{MagazineSize}");
        }
        
        public override void _Process(double delta)
        {
            float deltaF = (float)delta;
            
            if (_fireTimer > 0)
                _fireTimer -= deltaF;
            
            if (IsReloading)
            {
                _reloadTimer -= deltaF;
                if (_reloadTimer <= 0)
                {
                    FinishReload();
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        public void Fire()
        {
            if (!CanFire)
                return;
            
            CurrentAmmo--;
            _fireTimer = FireRate;
            
            // Visual/audio feedback
            PlayFireSound();
            SpawnMuzzleFlash();
            
            // Apply recoil
            ApplyRecoil();
            
            // Actual weapon-specific firing logic
            OnFire();
            
            // Emit event
            EventBus.Emit(EventBus.WeaponFired, WeaponName);
            
            // Auto-reload if empty
            if (CurrentAmmo == 0)
            {
                StartReload();
            }
        }
        
        public void StartReload()
        {
            if (IsReloading || CurrentAmmo == MagazineSize)
                return;
            
            IsReloading = true;
            _reloadTimer = ReloadTime;
            
            PlayReloadSound();
            EventBus.Emit(EventBus.WeaponReloading, WeaponName);
            
            GD.Print($"{WeaponName} reloading...");
        }
        
        #endregion
        
        #region Protected Abstract Methods
        
        protected abstract void OnFire();
        
        #endregion
        
        #region Protected Methods
        
        protected Vector3 GetAimDirection()
        {
            if (_camera == null)
                return -GlobalTransform.Basis.Z;
            
            var screenCenter = GetViewport().GetVisibleRect().Size / 2;
            var from = _camera.ProjectRayOrigin(screenCenter);
            var to = from + _camera.ProjectRayNormal(screenCenter) * Range;
            
            Vector3 direction = (to - from).Normalized();
            
            // Apply accuracy spread
            if (Accuracy < 1.0f)
            {
                float spread = (1.0f - Accuracy) * 0.1f;
                direction.X += GD.Randf() * spread - spread / 2;
                direction.Y += GD.Randf() * spread - spread / 2;
                direction = direction.Normalized();
            }
            
            return direction;
        }
        
        protected void ApplyRecoil()
        {
            // TODO: Camera shake/recoil
            EventBus.Emit(EventBus.WeaponRecoil, Damage * 0.01f);
        }
        
        protected void SpawnMuzzleFlash()
        {
            if (MuzzleFlashEffect != null && _muzzlePoint != null)
            {
                var flash = MuzzleFlashEffect.Instantiate<Node3D>();
                _muzzlePoint.AddChild(flash);
            }
        }
        
        protected void PlayFireSound()
        {
            if (FireSound != null)
            {
                // TODO: Use audio manager
                var player = new AudioStreamPlayer3D();
                player.Stream = FireSound;
                AddChild(player);
                player.Play();
                player.Finished += () => player.QueueFree();
            }
        }
        
        protected void PlayReloadSound()
        {
            if (ReloadSound != null)
            {
                var player = new AudioStreamPlayer3D();
                player.Stream = ReloadSound;
                AddChild(player);
                player.Play();
                player.Finished += () => player.QueueFree();
            }
        }
        
        private void FinishReload()
        {
            IsReloading = false;
            CurrentAmmo = MagazineSize;
            EventBus.Emit(EventBus.WeaponReloaded, WeaponName);
            GD.Print($"{WeaponName} reloaded - {CurrentAmmo}/{MagazineSize}");
        }
        
        #endregion
    }
}
