using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.VFX
{
    /// <summary>
    /// Manages visual effects for status effects (burn, freeze, shock, poison).
    /// Provides attachment and detachment of looping particle effects to entities.
    /// 
    /// INTEGRATION:
    /// Status effects can use this manager to show visual feedback:
    /// - Burning entities show flame particles
    /// - Frozen entities show ice/frost particles
    /// - Shocked entities show electricity arcs
    /// - Poisoned entities show toxic smoke/drips
    /// 
    /// USAGE:
    /// // Apply status effect visuals when status is applied
    /// StatusEffectVFX.ApplyEffect(enemyNode, ElementalType.Fire, duration: 5f);
    /// 
    /// // Manually remove effect visuals early
    /// StatusEffectVFX.RemoveEffect(enemyNode, ElementalType.Fire);
    /// 
    /// // Remove all effects from entity
    /// StatusEffectVFX.RemoveAllEffects(enemyNode);
    /// 
    /// AUTOMATIC CLEANUP:
    /// - Effects are automatically removed after duration expires
    /// - Effects are automatically removed when entity is destroyed
    /// - Tracks active effects per entity to prevent duplicates
    /// 
    /// SCENE REQUIREMENTS:
    /// - VFXManager must be in AutoLoad as singleton
    /// - Effects attach to entity center, adjust with localOffset parameter
    /// - Looping effects use long duration (999s) and are manually controlled
    /// 
    /// PERFORMANCE:
    /// - Reuses particle instances from pool
    /// - Tracks active effects to prevent duplicate spawns
    /// - Automatically cleans up on entity destruction
    /// </summary>
    public static class StatusEffectVFX
    {
        #region Private Fields

        // Track active effects per entity: Entity -> (ElementType -> Effect Node)
        private static Dictionary<Node, Dictionary<ElementalType, Node3D>> _activeEffects = new();

        #endregion

        #region Public Methods

        /// <summary>
        /// Apply a status effect visual to an entity.
        /// Automatically removes after duration expires.
        /// </summary>
        /// <param name="entity">Entity node to attach effect to</param>
        /// <param name="elementType">Type of elemental status effect</param>
        /// <param name="duration">Duration in seconds (default 5.0)</param>
        /// <param name="localOffset">Local position offset from entity center (default Vector3.Zero)</param>
        public static void ApplyEffect(Node entity, ElementalType elementType, float duration = 5f, Vector3? localOffset = null)
        {
            if (VFXManager.Instance == null)
            {
                GD.PrintErr("StatusEffectVFX: VFXManager not initialized");
                return;
            }

            if (entity == null || !Node.IsInstanceValid(entity))
            {
                GD.PrintErr("StatusEffectVFX: Invalid entity node");
                return;
            }

            // Don't apply if entity already has this effect active
            if (HasEffect(entity, elementType))
            {
                // Refresh duration by removing and re-adding
                RemoveEffect(entity, elementType);
            }

            // Get entity as Node3D for positioning
            Node3D entityNode = entity as Node3D;
            if (entityNode == null)
            {
                // Try to find a Node3D parent
                entityNode = entity.GetParent() as Node3D;
                if (entityNode == null)
                {
                    GD.PrintErr("StatusEffectVFX: Entity is not Node3D and has no Node3D parent");
                    return;
                }
            }

            // Select effect based on element type
            string effectName = GetEffectNameForElement(elementType);
            if (string.IsNullOrEmpty(effectName))
            {
                return;
            }

            // Get effect from pool
            if (!VFXManager.Instance.HasEffect(effectName))
            {
                GD.PrintErr($"StatusEffectVFX: Effect not found: {effectName}");
                return;
            }

            var effectData = VFXManager.Instance.GetEffectData(effectName);
            
            // Spawn effect attached to entity
            VFXManager.Instance.PlayEffectAttached(
                effectName,
                entityNode,
                localOffset ?? Vector3.Zero
            );

            // Get the spawned effect node (most recent child)
            Node3D effectNode = null;
            var children = entityNode.GetChildren();
            if (children.Count > 0)
            {
                effectNode = children[children.Count - 1] as Node3D;
            }

            if (effectNode != null)
            {
                // Track active effect
                TrackEffect(entity, elementType, effectNode);

                // Schedule removal after duration
                var timer = entityNode.GetTree().CreateTimer(duration);
                timer.Timeout += () =>
                {
                    RemoveEffect(entity, elementType);
                };
            }
        }

        /// <summary>
        /// Remove a specific status effect visual from an entity.
        /// </summary>
        /// <param name="entity">Entity node to remove effect from</param>
        /// <param name="elementType">Type of elemental status effect to remove</param>
        public static void RemoveEffect(Node entity, ElementalType elementType)
        {
            if (entity == null || !Node.IsInstanceValid(entity))
            {
                return;
            }

            if (!_activeEffects.ContainsKey(entity))
            {
                return;
            }

            var entityEffects = _activeEffects[entity];
            if (!entityEffects.ContainsKey(elementType))
            {
                return;
            }

            // Get and remove effect node
            var effectNode = entityEffects[elementType];
            if (effectNode != null && Node.IsInstanceValid(effectNode))
            {
                // Stop emission
                if (effectNode is GpuParticles3D gpuParticles)
                {
                    gpuParticles.Emitting = false;
                }
                else if (effectNode is CpuParticles3D cpuParticles)
                {
                    cpuParticles.Emitting = false;
                }

                // Queue for deletion
                effectNode.QueueFree();
            }

            // Remove from tracking
            entityEffects.Remove(elementType);
            if (entityEffects.Count == 0)
            {
                _activeEffects.Remove(entity);
            }
        }

        /// <summary>
        /// Remove all status effect visuals from an entity.
        /// Useful when entity dies or is cleansed.
        /// </summary>
        /// <param name="entity">Entity node to remove all effects from</param>
        public static void RemoveAllEffects(Node entity)
        {
            if (entity == null || !Node.IsInstanceValid(entity))
            {
                return;
            }

            if (!_activeEffects.ContainsKey(entity))
            {
                return;
            }

            // Copy keys to avoid modification during iteration
            var elementTypes = new List<ElementalType>(_activeEffects[entity].Keys);
            
            foreach (var elementType in elementTypes)
            {
                RemoveEffect(entity, elementType);
            }
        }

        /// <summary>
        /// Check if entity has a specific status effect visual active.
        /// </summary>
        /// <param name="entity">Entity node to check</param>
        /// <param name="elementType">Type of elemental status effect</param>
        /// <returns>True if effect is currently active</returns>
        public static bool HasEffect(Node entity, ElementalType elementType)
        {
            if (entity == null || !Node.IsInstanceValid(entity))
            {
                return false;
            }

            return _activeEffects.ContainsKey(entity) && 
                   _activeEffects[entity].ContainsKey(elementType);
        }

        /// <summary>
        /// Get count of active status effects on an entity.
        /// </summary>
        /// <param name="entity">Entity node to check</param>
        /// <returns>Number of active status effect visuals</returns>
        public static int GetActiveEffectCount(Node entity)
        {
            if (entity == null || !Node.IsInstanceValid(entity))
            {
                return 0;
            }

            return _activeEffects.ContainsKey(entity) ? _activeEffects[entity].Count : 0;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the VFX effect name for a given elemental type.
        /// </summary>
        /// <param name="elementType">Elemental type</param>
        /// <returns>Effect name string, or null if no effect for this type</returns>
        private static string GetEffectNameForElement(ElementalType elementType)
        {
            return elementType switch
            {
                ElementalType.Fire => "burn_loop",
                ElementalType.Ice => "freeze_loop",
                ElementalType.Electric => "shock_loop",
                ElementalType.Toxic => "poison_loop",
                _ => null // Physical damage has no persistent status effect visual
            };
        }

        /// <summary>
        /// Track an active effect on an entity.
        /// </summary>
        /// <param name="entity">Entity the effect is attached to</param>
        /// <param name="elementType">Type of effect</param>
        /// <param name="effectNode">The effect node instance</param>
        private static void TrackEffect(Node entity, ElementalType elementType, Node3D effectNode)
        {
            if (!_activeEffects.ContainsKey(entity))
            {
                _activeEffects[entity] = new Dictionary<ElementalType, Node3D>();
            }

            _activeEffects[entity][elementType] = effectNode;
        }

        #endregion
    }
}
