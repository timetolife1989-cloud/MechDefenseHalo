using Godot;
using System;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.VFX
{
    /// <summary>
    /// Helper class for weapon-specific VFX effects.
    /// Provides convenient methods for common weapon visual feedback.
    /// 
    /// INTEGRATION:
    /// Weapons can use this helper to easily spawn appropriate effects:
    /// - Muzzle flashes at weapon fire point
    /// - Impact effects at hit locations
    /// - Projectile trails for bullets/missiles
    /// - Elemental weapon effects based on damage type
    /// 
    /// USAGE IN WEAPON SCRIPTS:
    /// public partial class MyWeapon : WeaponBase
    /// {
    ///     private Node3D _muzzlePoint;
    ///     
    ///     protected override void OnFire()
    ///     {
    ///         WeaponVFX.SpawnMuzzleFlash(_muzzlePoint, ElementType);
    ///         
    ///         var hit = PerformRaycast();
    ///         if (hit.Hit)
    ///         {
    ///             WeaponVFX.SpawnImpactEffect(hit.Position, hit.Normal, ElementType);
    ///         }
    ///     }
    /// }
    /// 
    /// EFFECT SELECTION:
    /// Automatically selects appropriate effect based on:
    /// - Elemental damage type (Physical, Fire, Ice, Electric, Toxic)
    /// - Weapon category (Ballistic, Energy, Explosive)
    /// - Impact surface type (future enhancement)
    /// 
    /// SCENE REQUIREMENTS:
    /// - VFXManager must be in AutoLoad as singleton
    /// - Weapon nodes should have a "MuzzlePoint" child node for accurate positioning
    /// - Effects spawn at world positions, not parented to weapons
    /// </summary>
    public static class WeaponVFX
    {
        #region Constants

        /// <summary>
        /// Default length of laser beam prefab in world units.
        /// Used for calculating scale when spawning laser beams.
        /// </summary>
        private const float LaserBeamPrefabLength = 10f;

        #endregion

        #region Public Methods

        /// <summary>
        /// Spawn a muzzle flash effect at weapon fire point.
        /// Effect type varies based on elemental damage type.
        /// </summary>
        /// <param name="muzzlePoint">Node3D representing weapon muzzle position</param>
        /// <param name="elementType">Type of elemental damage for effect selection</param>
        /// <param name="scale">Scale multiplier for effect size (default 1.0)</param>
        public static void SpawnMuzzleFlash(Node3D muzzlePoint, ElementalType elementType = ElementalType.Physical, float scale = 1.0f)
        {
            if (VFXManager.Instance == null)
            {
                GD.PrintErr("WeaponVFX: VFXManager not initialized");
                return;
            }

            if (muzzlePoint == null || !Node.IsInstanceValid(muzzlePoint))
            {
                GD.PrintErr("WeaponVFX: Invalid muzzle point");
                return;
            }

            // Select muzzle flash based on element type
            string effectName = elementType switch
            {
                ElementalType.Fire => "muzzle_flash_plasma",
                ElementalType.Electric => "muzzle_flash_energy",
                ElementalType.Ice => "muzzle_flash_energy",
                ElementalType.Toxic => "muzzle_flash_energy",
                _ => "muzzle_flash" // Physical and default
            };

            VFXManager.Instance.PlayEffect(
                effectName,
                muzzlePoint.GlobalPosition,
                muzzlePoint.GlobalRotation,
                scale
            );
        }

        /// <summary>
        /// Spawn an impact effect at hit location.
        /// Effect type varies based on elemental damage type.
        /// </summary>
        /// <param name="hitPosition">World position of impact</param>
        /// <param name="hitNormal">Surface normal at impact point</param>
        /// <param name="elementType">Type of elemental damage for effect selection</param>
        /// <param name="scale">Scale multiplier for effect size (default 1.0)</param>
        public static void SpawnImpactEffect(Vector3 hitPosition, Vector3 hitNormal, ElementalType elementType = ElementalType.Physical, float scale = 1.0f)
        {
            if (VFXManager.Instance == null)
            {
                GD.PrintErr("WeaponVFX: VFXManager not initialized");
                return;
            }

            // Select impact effect based on element type
            string effectName = elementType switch
            {
                ElementalType.Fire => "hit_energy",
                ElementalType.Electric => "hit_energy",
                ElementalType.Ice => "hit_energy",
                ElementalType.Toxic => "hit_energy",
                _ => "hit_spark" // Physical and default
            };

            // Calculate rotation from normal (align effect with surface)
            Vector3 rotation = Vector3.Zero;
            if (hitNormal != Vector3.Zero)
            {
                // Create a basis looking along the normal
                var basis = Basis.LookingAt(hitNormal, Vector3.Up);
                rotation = basis.GetEuler();
            }

            VFXManager.Instance.PlayEffect(
                effectName,
                hitPosition,
                rotation,
                scale
            );
        }

        /// <summary>
        /// Spawn a projectile trail effect that follows a moving projectile.
        /// </summary>
        /// <param name="projectile">The projectile node to attach trail to</param>
        /// <param name="elementType">Type of elemental damage for effect selection</param>
        /// <param name="localOffset">Local offset from projectile center (default Vector3.Zero)</param>
        public static void SpawnProjectileTrail(Node3D projectile, ElementalType elementType = ElementalType.Physical, Vector3? localOffset = null)
        {
            if (VFXManager.Instance == null)
            {
                GD.PrintErr("WeaponVFX: VFXManager not initialized");
                return;
            }

            if (projectile == null || !Node.IsInstanceValid(projectile))
            {
                GD.PrintErr("WeaponVFX: Invalid projectile node");
                return;
            }

            // For projectile trails, use attached effects
            VFXManager.Instance.PlayEffectAttached(
                "projectile_trail",
                projectile,
                localOffset ?? Vector3.Zero
            );
        }

        /// <summary>
        /// Spawn an explosion effect at specified location.
        /// </summary>
        /// <param name="position">World position of explosion center</param>
        /// <param name="size">Size of explosion (Small, Medium, Large)</param>
        /// <param name="elementType">Type of elemental damage for effect selection</param>
        /// <param name="scale">Additional scale multiplier (default 1.0)</param>
        public static void SpawnExplosion(Vector3 position, ExplosionSize size = ExplosionSize.Medium, ElementalType elementType = ElementalType.Physical, float scale = 1.0f)
        {
            if (VFXManager.Instance == null)
            {
                GD.PrintErr("WeaponVFX: VFXManager not initialized");
                return;
            }

            // Select explosion based on size
            string effectName = size switch
            {
                ExplosionSize.Small => "explosion_small",
                ExplosionSize.Medium => "explosion_medium",
                ExplosionSize.Large => "explosion_large",
                _ => "explosion_medium"
            };

            // Use energy explosion for non-physical damage
            if (elementType != ElementalType.Physical)
            {
                effectName = "explosion_energy";
            }

            VFXManager.Instance.PlayEffect(
                effectName,
                position,
                Vector3.Zero,
                scale
            );
        }

        /// <summary>
        /// Spawn a laser beam effect between two points.
        /// </summary>
        /// <param name="startPosition">Start position of laser</param>
        /// <param name="endPosition">End position of laser</param>
        /// <param name="duration">Duration of beam in seconds (default 0.5)</param>
        public static void SpawnLaserBeam(Vector3 startPosition, Vector3 endPosition, float duration = 0.5f)
        {
            if (VFXManager.Instance == null)
            {
                GD.PrintErr("WeaponVFX: VFXManager not initialized");
                return;
            }

            // Calculate direction and rotation
            Vector3 direction = (endPosition - startPosition).Normalized();
            Vector3 rotation = Vector3.Zero;
            if (direction != Vector3.Zero)
            {
                var basis = Basis.LookingAt(direction, Vector3.Up);
                rotation = basis.GetEuler();
            }

            // Calculate scale based on distance
            float distance = startPosition.DistanceTo(endPosition);
            float scale = distance / LaserBeamPrefabLength;

            VFXManager.Instance.PlayEffect(
                "laser_beam",
                startPosition,
                rotation,
                scale
            );
        }

        #endregion
    }

    #region Enums

    /// <summary>
    /// Explosion size categories for effect selection.
    /// </summary>
    public enum ExplosionSize
    {
        /// <summary>Small explosion (grenade, small missile)</summary>
        Small,
        
        /// <summary>Medium explosion (rocket, barrel)</summary>
        Medium,
        
        /// <summary>Large explosion (heavy ordnance, vehicle destruction)</summary>
        Large
    }

    #endregion
}
