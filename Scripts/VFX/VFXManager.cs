using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.VFX
{
    /// <summary>
    /// Central VFX manager with object pooling and effect spawning.
    /// 
    /// USAGE:
    /// VFXManager.Instance.PlayEffect("muzzle_flash", weaponPosition, weaponRotation);
    /// VFXManager.Instance.PlayEffect("explosion_large", hitPosition);
    /// 
    /// SETUP (Godot):
    /// Add as AutoLoad singleton: Project Settings > AutoLoad > VFXManager.cs
    /// 
    /// SCENE STRUCTURE:
    /// VFXManager (Node)
    /// └── VFXContainer (Node3D) - Parent for all pooled effects
    ///     ├── MuzzleFlash_Pool_0 (GpuParticles3D)
    ///     ├── MuzzleFlash_Pool_1 (GpuParticles3D)
    ///     ├── Explosion_Pool_0 (GpuParticles3D)
    ///     └── ... (other pooled effects)
    /// 
    /// PERFORMANCE:
    /// - Object pooling prevents runtime instantiation lag
    /// - Pre-warmed pools for common effects (muzzle flash, hit spark, small explosion)
    /// - Effects automatically returned to pool after duration
    /// - Configurable initial pool size per effect type
    /// </summary>
    public partial class VFXManager : Node
    {
        #region Constants

        /// <summary>
        /// Name of the container node for all VFX effects.
        /// </summary>
        public const string VFXContainerNodeName = "VFXContainer";

        #endregion

        #region Singleton

        public static VFXManager Instance { get; private set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the VFX container node that holds all pooled effects.
        /// </summary>
        public Node3D VFXContainer => _effectsContainer;

        #endregion

        #region Private Fields

        private VFXLibrary _library;
        private Dictionary<string, ParticlePool> _pools = new();
        private Node3D _effectsContainer;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Setup singleton
            if (Instance != null)
            {
                GD.PrintErr("Multiple VFXManager instances detected! Using first instance.");
                QueueFree();
                return;
            }

            Instance = this;
            
            // Create container for all effects
            _effectsContainer = new Node3D { Name = VFXContainerNodeName };
            AddChild(_effectsContainer);

            // Load VFX library
            _library = new VFXLibrary();
            
            // Pre-warm pools for common effects
            PreWarmPools();

            GD.Print("VFXManager initialized with object pooling");
        }

        public override void _ExitTree()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Play a VFX effect at position with optional rotation and scale.
        /// </summary>
        /// <param name="effectName">Name of the effect from VFXLibrary</param>
        /// <param name="position">World position to spawn effect</param>
        /// <param name="rotation">Optional world rotation (defaults to Vector3.Zero)</param>
        /// <param name="scale">Optional scale multiplier (defaults to 1.0)</param>
        public void PlayEffect(string effectName, Vector3 position, Vector3? rotation = null, float scale = 1.0f)
        {
            if (!_library.HasEffect(effectName))
            {
                GD.PrintErr($"VFX effect not found: {effectName}");
                return;
            }

            var effectData = _library.GetEffect(effectName);
            var pool = GetOrCreatePool(effectName, effectData.PrefabPath);
            
            if (pool == null)
            {
                GD.PrintErr($"Failed to create pool for effect: {effectName}");
                return;
            }
            
            var effect = pool.Get();
            if (effect == null)
            {
                GD.PrintErr($"Failed to get effect from pool: {effectName}");
                return;
            }

            // Set transform
            effect.GlobalPosition = position;
            effect.GlobalRotation = rotation ?? Vector3.Zero;
            effect.Scale = Vector3.One * scale;

            // Restart particle emission if it's a GpuParticles3D
            if (effect is GpuParticles3D particles)
            {
                particles.Restart();
                particles.Emitting = true;
            }
            else if (effect is CpuParticles3D cpuParticles)
            {
                cpuParticles.Restart();
                cpuParticles.Emitting = true;
            }

            // Auto-return to pool after duration
            var timer = GetTree().CreateTimer(effectData.Duration);
            timer.Timeout += () => 
            {
                if (IsInstanceValid(effect))
                {
                    pool.Return(effect);
                }
            };
        }

        /// <summary>
        /// Play effect attached to a node (follows parent).
        /// </summary>
        /// <param name="effectName">Name of the effect from VFXLibrary</param>
        /// <param name="parent">Parent node to attach effect to</param>
        /// <param name="localOffset">Local position offset from parent (defaults to Vector3.Zero)</param>
        /// <returns>The spawned effect node, or null if failed</returns>
        public Node3D PlayEffectAttached(string effectName, Node3D parent, Vector3? localOffset = null)
        {
            if (!_library.HasEffect(effectName))
            {
                GD.PrintErr($"VFX effect not found: {effectName}");
                return null;
            }

            if (!IsInstanceValid(parent))
            {
                GD.PrintErr($"Invalid parent node for attached effect: {effectName}");
                return null;
            }

            var effectData = _library.GetEffect(effectName);
            var pool = GetOrCreatePool(effectName, effectData.PrefabPath);
            
            if (pool == null)
            {
                GD.PrintErr($"Failed to create pool for attached effect: {effectName}");
                return null;
            }
            
            var effect = pool.Get();
            if (effect == null)
            {
                GD.PrintErr($"Failed to get effect from pool: {effectName}");
                return null;
            }

            // Attach to parent
            parent.AddChild(effect);
            effect.Position = localOffset ?? Vector3.Zero;

            // Restart particle emission
            if (effect is GpuParticles3D particles)
            {
                particles.Restart();
                particles.Emitting = true;
            }
            else if (effect is CpuParticles3D cpuParticles)
            {
                cpuParticles.Restart();
                cpuParticles.Emitting = true;
            }

            // Auto-return to pool after duration
            var timer = GetTree().CreateTimer(effectData.Duration);
            timer.Timeout += () => 
            {
                if (IsInstanceValid(effect) && IsInstanceValid(parent))
                {
                    effect.Reparent(_effectsContainer);
                    pool.Return(effect);
                }
            };

            return effect;
        }

        /// <summary>
        /// Check if an effect exists in the library.
        /// </summary>
        /// <param name="effectName">Name of the effect to check</param>
        /// <returns>True if effect exists</returns>
        public bool HasEffect(string effectName)
        {
            return _library.HasEffect(effectName);
        }

        /// <summary>
        /// Get effect metadata from library.
        /// </summary>
        /// <param name="effectName">Name of the effect</param>
        /// <returns>Effect data structure</returns>
        public VFXEffectData GetEffectData(string effectName)
        {
            return _library.GetEffect(effectName);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get or create a particle pool for the specified effect.
        /// </summary>
        private ParticlePool GetOrCreatePool(string effectName, string prefabPath)
        {
            if (!_pools.ContainsKey(effectName))
            {
                // Try to load the prefab
                var prefab = GD.Load<PackedScene>(prefabPath);
                
                if (prefab == null)
                {
                    GD.PrintErr($"Failed to load VFX prefab for effect '{effectName}': {prefabPath}");
                    // Don't create a pool for missing prefabs - return null to fail fast
                    return null;
                }
                
                _pools[effectName] = new ParticlePool(prefab, _effectsContainer, initialSize: 10);
            }
            return _pools[effectName];
        }

        /// <summary>
        /// Pre-create pools for commonly used effects to avoid runtime lag.
        /// </summary>
        private void PreWarmPools()
        {
            // Pre-create common effects
            var commonEffects = new[] 
            { 
                "muzzle_flash", 
                "hit_spark", 
                "explosion_small" 
            };

            foreach (var effectName in commonEffects)
            {
                if (_library.HasEffect(effectName))
                {
                    var effectData = _library.GetEffect(effectName);
                    GetOrCreatePool(effectName, effectData.PrefabPath);
                }
            }

            GD.Print($"Pre-warmed {commonEffects.Length} common VFX pools");
        }

        #endregion
    }
}
