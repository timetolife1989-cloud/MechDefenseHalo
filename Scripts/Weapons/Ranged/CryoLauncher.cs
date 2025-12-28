using Godot;
using System;
using MechDefenseHalo.Components;
using MechDefenseHalo.Weapons.Projectiles;

namespace MechDefenseHalo.Weapons.Ranged
{
    /// <summary>
    /// Projectile-based cryo launcher with ice damage and slow effect.
    /// Medium fire rate, applies frozen status.
    /// </summary>
    public partial class CryoLauncher : WeaponBase
    {
        #region Exported Properties

        [Export] public PackedScene ProjectileScene { get; set; }
        [Export] public float SlowDuration { get; set; } = 3f;

        #endregion

        #region Constructor

        public CryoLauncher()
        {
            WeaponName = "Cryo Launcher";
            BaseDamage = 30f;
            FireRate = 0.5f; // 2 rounds per second
            Range = 60f;
            MaxAmmo = 24;
            ReloadTime = 2.5f;
            ElementType = ElementalType.Ice;
            IsAutomatic = false;
        }

        #endregion

        #region Protected Methods

        protected override void OnFire()
        {
            var projectile = CreateProjectile();
            if (projectile != null)
            {
                GetTree().Root.AddChild(projectile);
                projectile.GlobalPosition = GetMuzzlePosition();
                
                projectile.Direction = GetFiringDirection();
                projectile.Damage = BaseDamage;
                projectile.ElementType = ElementType;
                projectile.SourceWeapon = this;

                // Apply slow effect on hit
                projectile.OnHitCallback = (target) => {
                    var statusEffect = target.GetNodeOrNull<StatusEffectComponent>("StatusEffectComponent");
                    if (statusEffect != null)
                    {
                        statusEffect.ApplyFrozen(SlowDuration);
                    }
                };

                GD.Print("Cryo Launcher fired!");
            }

            SpawnMuzzleFlash();
        }

        #endregion

        #region Private Methods

        private Projectile CreateProjectile()
        {
            if (ProjectileScene != null)
            {
                return ProjectileScene.Instantiate<Projectile>();
            }

            var projectile = new Projectile
            {
                Speed = 35f,
                Lifetime = 2.5f,
                ElementType = ElementalType.Ice
            };

            // Add visual (ice sphere)
            var mesh = new MeshInstance3D();
            var sphere = new SphereMesh();
            sphere.Radius = 0.25f;
            mesh.Mesh = sphere;
            
            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.3f, 0.7f, 1f); // Cyan for ice
            material.EmissionEnabled = true;
            material.Emission = new Color(0.3f, 0.7f, 1f);
            material.EmissionEnergy = 1.5f;
            mesh.MaterialOverride = material;
            
            projectile.AddChild(mesh);

            return projectile;
        }

        private void SpawnMuzzleFlash()
        {
            // TODO: Implement cryo muzzle flash
        }

        #endregion
    }
}
