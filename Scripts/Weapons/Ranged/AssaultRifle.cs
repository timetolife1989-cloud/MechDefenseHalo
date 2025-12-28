using Godot;
using System;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Weapons.Ranged
{
    /// <summary>
    /// Hitscan assault rifle with physical damage.
    /// Fast fire rate, medium damage.
    /// </summary>
    public partial class AssaultRifle : WeaponBase
    {
        #region Constructor

        public AssaultRifle()
        {
            WeaponName = "MA5B Assault Rifle";
            BaseDamage = 12f;
            FireRate = 0.08f; // ~12 rounds per second
            Range = 100f;
            MaxAmmo = 32;
            ReloadTime = 2.0f;
            ElementType = ElementalType.Physical;
            IsAutomatic = true;
        }

        #endregion

        #region Protected Methods

        protected override void OnFire()
        {
            // Perform hitscan raycast
            var hit = PerformRaycast();

            if (hit.Hit)
            {
                // Try to deal damage
                var healthComp = hit.Collider.GetNodeOrNull<HealthComponent>("HealthComponent");
                if (healthComp != null)
                {
                    healthComp.TakeDamage(BaseDamage, this);
                    GD.Print($"Assault Rifle hit {hit.Collider.Name} for {BaseDamage} damage");
                }
                
                // TODO: Spawn impact effect at hit.Position
                SpawnImpactEffect(hit.Position, hit.Normal);
            }

            // TODO: Spawn muzzle flash
            SpawnMuzzleFlash();
        }

        #endregion

        #region Private Methods

        private void SpawnMuzzleFlash()
        {
            // TODO: Implement muzzle flash particle effect
            // For now, just print debug message
        }

        private void SpawnImpactEffect(Vector3 position, Vector3 normal)
        {
            // TODO: Implement bullet impact particle effect
            // For now, just print debug message
        }

        #endregion
    }
}
