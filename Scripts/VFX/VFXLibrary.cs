using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.VFX
{
    /// <summary>
    /// Library of all VFX effects with metadata.
    /// 
    /// REGISTRATION SYSTEM:
    /// Each effect is registered with:
    /// - Unique name identifier (used for PlayEffect calls)
    /// - Prefab path (relative to project root, e.g., "res://Assets/VFX/MuzzleFlash.tscn")
    /// - Duration in seconds (controls auto-pooling return time)
    /// - Category for organization and filtering
    /// 
    /// EFFECT CATEGORIES:
    /// - Weapon: Muzzle flashes, projectile trails, laser beams
    /// - Impact: Hit sparks, metal impacts, energy impacts
    /// - Explosion: Small/medium/large explosions
    /// - StatusEffect: Looping effects for burn/freeze/shock/poison (long duration)
    /// - UI: Item pickups, level up effects, notifications
    /// - Environment: Ambient effects, weather, terrain interactions
    /// 
    /// ADDING NEW EFFECTS:
    /// 1. Create particle effect scene (.tscn) using GpuParticles3D
    /// 2. Save to appropriate Assets/VFX/ subdirectory
    /// 3. Register in RegisterAllEffects() with appropriate duration and category
    /// 4. Effect is immediately available via VFXManager.Instance.PlayEffect("effect_name", ...)
    /// 
    /// DURATION GUIDELINES:
    /// - Muzzle flashes: 0.1-0.3s (quick flash)
    /// - Hit impacts: 0.3-0.5s (brief spark)
    /// - Explosions: 1.0-2.0s (depending on size)
    /// - Status effects: 999s (effectively infinite, manually controlled)
    /// - UI effects: 1.0-2.0s (visible but not intrusive)
    /// </summary>
    public class VFXLibrary
    {
        #region Private Fields

        private Dictionary<string, VFXEffectData> _effects = new();

        #endregion

        #region Constructor

        public VFXLibrary()
        {
            RegisterAllEffects();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if an effect is registered in the library.
        /// </summary>
        /// <param name="name">Effect name identifier</param>
        /// <returns>True if effect exists</returns>
        public bool HasEffect(string name)
        {
            return _effects.ContainsKey(name);
        }

        /// <summary>
        /// Get effect data by name.
        /// </summary>
        /// <param name="name">Effect name identifier</param>
        /// <returns>Effect data structure</returns>
        public VFXEffectData GetEffect(string name)
        {
            if (!_effects.ContainsKey(name))
            {
                GD.PrintErr($"VFX effect not found in library: {name}");
                return default;
            }
            return _effects[name];
        }

        /// <summary>
        /// Get all effects in a specific category.
        /// </summary>
        /// <param name="category">Category to filter by</param>
        /// <returns>List of effect names in the category</returns>
        public List<string> GetEffectsByCategory(VFXCategory category)
        {
            var result = new List<string>();
            foreach (var kvp in _effects)
            {
                if (kvp.Value.Category == category)
                {
                    result.Add(kvp.Key);
                }
            }
            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Register all VFX effects in the library.
        /// Add new effects here following the pattern:
        /// Register("effect_name", "res://path/to/effect.tscn", durationSeconds, VFXCategory)
        /// </summary>
        private void RegisterAllEffects()
        {
            // ===== WEAPON EFFECTS =====
            // Muzzle flashes for different weapon types
            Register("muzzle_flash", "res://Assets/VFX/MuzzleFlash.tscn", 0.2f, VFXCategory.Weapon);
            Register("muzzle_flash_plasma", "res://Assets/VFX/MuzzleFlashPlasma.tscn", 0.3f, VFXCategory.Weapon);
            Register("muzzle_flash_energy", "res://Assets/VFX/MuzzleFlashEnergy.tscn", 0.25f, VFXCategory.Weapon);
            
            // Projectile and beam effects
            Register("projectile_trail", "res://Assets/VFX/ProjectileTrail.tscn", 1.0f, VFXCategory.Weapon);
            Register("laser_beam", "res://Assets/VFX/LaserBeam.tscn", 0.5f, VFXCategory.Weapon);
            Register("rocket_trail", "res://Assets/VFX/RocketTrail.tscn", 2.0f, VFXCategory.Weapon);

            // ===== IMPACT EFFECTS =====
            // Generic and material-specific impacts
            Register("hit_spark", "res://Assets/VFX/HitSpark.tscn", 0.3f, VFXCategory.Impact);
            Register("hit_metal", "res://Assets/VFX/HitMetal.tscn", 0.4f, VFXCategory.Impact);
            Register("hit_energy", "res://Assets/VFX/HitEnergy.tscn", 0.5f, VFXCategory.Impact);
            Register("hit_flesh", "res://Assets/VFX/HitFlesh.tscn", 0.35f, VFXCategory.Impact);

            // ===== EXPLOSIONS =====
            // Scaled explosion effects
            Register("explosion_small", "res://Assets/VFX/ExplosionSmall.tscn", 1.0f, VFXCategory.Explosion);
            Register("explosion_medium", "res://Assets/VFX/ExplosionMedium.tscn", 1.5f, VFXCategory.Explosion);
            Register("explosion_large", "res://Assets/VFX/ExplosionLarge.tscn", 2.0f, VFXCategory.Explosion);
            Register("explosion_energy", "res://Assets/VFX/ExplosionEnergy.tscn", 1.2f, VFXCategory.Explosion);

            // ===== STATUS EFFECTS =====
            // Looping effects for elemental damage (long duration for manual control)
            Register("burn_loop", "res://Assets/VFX/BurnEffect.tscn", 999f, VFXCategory.StatusEffect);
            Register("freeze_loop", "res://Assets/VFX/FreezeEffect.tscn", 999f, VFXCategory.StatusEffect);
            Register("shock_loop", "res://Assets/VFX/ShockEffect.tscn", 999f, VFXCategory.StatusEffect);
            Register("poison_loop", "res://Assets/VFX/PoisonEffect.tscn", 999f, VFXCategory.StatusEffect);
            
            // Status effect triggers (one-shot effects)
            Register("ignite", "res://Assets/VFX/IgniteEffect.tscn", 0.5f, VFXCategory.StatusEffect);
            Register("shatter", "res://Assets/VFX/ShatterEffect.tscn", 0.8f, VFXCategory.StatusEffect);

            // ===== UI EFFECTS =====
            // Player feedback and notifications
            Register("item_pickup", "res://Assets/VFX/ItemPickup.tscn", 1.0f, VFXCategory.UI);
            Register("levelup", "res://Assets/VFX/LevelUp.tscn", 2.0f, VFXCategory.UI);
            Register("achievement", "res://Assets/VFX/Achievement.tscn", 1.5f, VFXCategory.UI);
            Register("heal", "res://Assets/VFX/HealEffect.tscn", 1.0f, VFXCategory.UI);

            // ===== ENVIRONMENT EFFECTS =====
            // World interaction effects
            Register("dust_impact", "res://Assets/VFX/DustImpact.tscn", 0.8f, VFXCategory.Environment);
            Register("water_splash", "res://Assets/VFX/WaterSplash.tscn", 1.0f, VFXCategory.Environment);
            Register("smoke_puff", "res://Assets/VFX/SmokePuff.tscn", 2.0f, VFXCategory.Environment);

            GD.Print($"VFXLibrary registered {_effects.Count} effects");
        }

        /// <summary>
        /// Register a single effect in the library.
        /// </summary>
        /// <param name="name">Unique identifier for the effect</param>
        /// <param name="prefabPath">Path to the effect's scene file</param>
        /// <param name="duration">Duration in seconds before auto-return to pool</param>
        /// <param name="category">Category for organization</param>
        private void Register(string name, string prefabPath, float duration, VFXCategory category)
        {
            if (_effects.ContainsKey(name))
            {
                GD.PrintErr($"Duplicate VFX effect registration: {name}");
                return;
            }

            _effects[name] = new VFXEffectData
            {
                Name = name,
                PrefabPath = prefabPath,
                Duration = duration,
                Category = category
            };
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Data structure containing metadata for a VFX effect.
    /// </summary>
    public struct VFXEffectData
    {
        /// <summary>Unique identifier for the effect</summary>
        public string Name;
        
        /// <summary>Path to the effect's scene file (e.g., "res://Assets/VFX/MuzzleFlash.tscn")</summary>
        public string PrefabPath;
        
        /// <summary>Duration in seconds before auto-return to pool (use 999+ for manual control)</summary>
        public float Duration;
        
        /// <summary>Category for organization and filtering</summary>
        public VFXCategory Category;
    }

    /// <summary>
    /// Categories for organizing VFX effects.
    /// </summary>
    public enum VFXCategory
    {
        /// <summary>Weapon-related effects (muzzle flashes, projectile trails)</summary>
        Weapon,
        
        /// <summary>Impact and hit effects</summary>
        Impact,
        
        /// <summary>Explosion effects of various sizes</summary>
        Explosion,
        
        /// <summary>Status effect visuals (burn, freeze, shock, poison)</summary>
        StatusEffect,
        
        /// <summary>UI and player feedback effects</summary>
        UI,
        
        /// <summary>Environmental interaction effects</summary>
        Environment
    }

    #endregion
}
