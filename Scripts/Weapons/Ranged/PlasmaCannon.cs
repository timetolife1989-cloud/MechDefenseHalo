using Godot;
using System;
using MechDefenseHalo.Components;
using MechDefenseHalo.Weapons.Projectiles;

namespace MechDefenseHalo.Weapons.Ranged
{
    /// <summary>
    /// Projectile-based plasma cannon with fire damage.
    /// Slow fire rate, high damage.
    /// </summary>
    public partial class PlasmaCannon : WeaponBase
    {
        #region Exported Properties

        [Export] public PackedScene ProjectileScene { get; set; }

        #endregion

        #region Constructor

        public PlasmaCannon()
        {
            WeaponName = "Plasma Cannon";
            BaseDamage = 45f;
            FireRate = 0.8f; // ~1.25 rounds per second
            Range = 80f;
            MaxAmmo = 20;
            ReloadTime = 3.0f;
            ElementType = ElementalType.Fire;
            IsAutomatic = false;
        }

        #endregion

        #region Protected Methods

        protected override void OnFire()
        {
            // Spawn projectile
            var projectile = CreateProjectile();
            if (projectile != null)
            {
                GetTree().Root.AddChild(projectile);
                projectile.GlobalPosition = GetMuzzlePosition();
                
                // Set projectile properties
                projectile.Direction = GetFiringDirection();
                projectile.Damage = BaseDamage;
                projectile.ElementType = ElementType;
                projectile.SourceWeapon = this;

                GD.Print("Plasma Cannon fired!");
            }

            SpawnMuzzleFlash();
        }

        #endregion

        #region Private Methods

        private Projectile CreateProjectile()
        {
            // If we have a scene, instantiate it
            if (ProjectileScene != null)
            {
                return ProjectileScene.Instantiate<Projectile>();
            }

            // Otherwise create a basic projectile
            var projectile = new Projectile
            {
                Speed = 40f,
                Lifetime = 3f,
                ElementType = ElementType.Fire
            };

            // Add visual (sphere)
            var mesh = new MeshInstance3D();
            var sphere = new SphereMesh();
            sphere.Radius = 0.2f;
            mesh.Mesh = sphere;
            
            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(1f, 0.3f, 0f); // Orange for fire
            material.EmissionEnabled = true;
            material.Emission = new Color(1f, 0.3f, 0f);
            material.EmissionEnergy = 2f;
            mesh.MaterialOverride = material;
            
            projectile.AddChild(mesh);

            return projectile;
        }

        private void SpawnMuzzleFlash()
        {
            // TODO: Implement plasma muzzle flash
        }

        #endregion
    }
}
