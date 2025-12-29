using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.VFX
{
    /// <summary>
    /// Object pool for particle effects to avoid runtime instantiation lag.
    /// 
    /// POOL ARCHITECTURE:
    /// - Pre-instantiates a set number of particle effect instances
    /// - Reuses inactive instances instead of creating new ones
    /// - Grows dynamically if all instances are in use
    /// - Maintains parent hierarchy for scene organization
    /// 
    /// PERFORMANCE BENEFITS:
    /// - Eliminates instantiation spikes during combat
    /// - Reduces garbage collection pressure
    /// - Maintains consistent frame times
    /// - Supports hundreds of simultaneous effects
    /// 
    /// USAGE:
    /// var pool = new ParticlePool(prefabScene, containerNode, initialSize: 20);
    /// var effect = pool.Get(); // Get effect from pool
    /// // ... use effect ...
    /// pool.Return(effect); // Return to pool when done
    /// 
    /// SCENE HIERARCHY:
    /// VFXContainer (Node3D)
    /// ├── EffectName_Pool_0 (GpuParticles3D) [Active/Inactive]
    /// ├── EffectName_Pool_1 (GpuParticles3D) [Active/Inactive]
    /// └── EffectName_Pool_N (GpuParticles3D) [Active/Inactive]
    /// 
    /// STATE MANAGEMENT:
    /// - Active: Effect is currently playing in the scene
    /// - Inactive: Effect is in pool, ready to be reused
    /// - Effects are marked inactive when returned to pool
    /// - Inactive effects have particles disabled and are hidden
    /// </summary>
    public class ParticlePool
    {
        #region Private Fields

        private PackedScene _prefab;
        private Node3D _container;
        private Queue<Node3D> _available = new();
        private HashSet<Node3D> _inUse = new();
        private int _poolIndex = 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Create a new particle pool.
        /// </summary>
        /// <param name="prefab">Packed scene containing the particle effect</param>
        /// <param name="container">Parent node for all pooled instances</param>
        /// <param name="initialSize">Number of instances to pre-create</param>
        public ParticlePool(PackedScene prefab, Node3D container, int initialSize = 10)
        {
            _prefab = prefab;
            _container = container;

            // Pre-instantiate initial pool size
            if (_prefab != null)
            {
                for (int i = 0; i < initialSize; i++)
                {
                    CreateInstance();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get an effect instance from the pool.
        /// Creates a new instance if pool is empty.
        /// </summary>
        /// <returns>Node3D particle effect instance, or null if creation failed</returns>
        public Node3D Get()
        {
            Node3D instance;

            // Get from pool or create new
            if (_available.Count > 0)
            {
                instance = _available.Dequeue();
            }
            else
            {
                // Pool exhausted - create new instance dynamically
                instance = CreateInstance();
                if (instance == null)
                {
                    GD.PrintErr("ParticlePool: Failed to create new instance");
                    return null;
                }
            }

            // Mark as in use
            _inUse.Add(instance);

            // Activate the instance
            ActivateInstance(instance);

            return instance;
        }

        /// <summary>
        /// Return an effect instance to the pool for reuse.
        /// </summary>
        /// <param name="instance">The instance to return</param>
        public void Return(Node3D instance)
        {
            if (instance == null || !Node.IsInstanceValid(instance))
            {
                return;
            }

            // Remove from in-use set
            if (!_inUse.Remove(instance))
            {
                // Instance wasn't from this pool
                GD.PrintErr($"ParticlePool: Attempted to return instance not from this pool: {instance.Name}");
                return;
            }

            // Deactivate the instance
            DeactivateInstance(instance);

            // Return to available queue
            _available.Enqueue(instance);
        }

        /// <summary>
        /// Get pool statistics for debugging.
        /// </summary>
        /// <returns>String containing pool usage info</returns>
        public string GetStats()
        {
            int total = _available.Count + _inUse.Count;
            return $"Pool Stats - Total: {total}, Available: {_available.Count}, In Use: {_inUse.Count}";
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create a new instance and add it to the pool.
        /// </summary>
        /// <returns>The created instance, or null if creation failed</returns>
        private Node3D CreateInstance()
        {
            if (_prefab == null)
            {
                return null;
            }

            // Instantiate from prefab
            var instance = _prefab.Instantiate() as Node3D;
            if (instance == null)
            {
                GD.PrintErr("ParticlePool: Failed to instantiate prefab as Node3D");
                return null;
            }

            // Set unique name
            instance.Name = $"{instance.Name}_Pool_{_poolIndex++}";

            // Add to container
            _container.AddChild(instance);

            // Deactivate immediately (will be activated when Get() is called)
            DeactivateInstance(instance);

            // Add to available queue
            _available.Enqueue(instance);

            return instance;
        }

        /// <summary>
        /// Activate an instance (called when getting from pool).
        /// </summary>
        /// <param name="instance">Instance to activate</param>
        private void ActivateInstance(Node3D instance)
        {
            if (instance == null)
            {
                return;
            }

            // Show the instance
            instance.Visible = true;

            // Re-enable particle emission if GpuParticles3D
            if (instance is GpuParticles3D gpuParticles)
            {
                gpuParticles.Emitting = true;
            }
            else if (instance is CpuParticles3D cpuParticles)
            {
                cpuParticles.Emitting = true;
            }

            // Also check children for particle systems
            foreach (var child in instance.GetChildren())
            {
                if (child is GpuParticles3D childGpuParticles)
                {
                    childGpuParticles.Emitting = true;
                }
                else if (child is CpuParticles3D childCpuParticles)
                {
                    childCpuParticles.Emitting = true;
                }
            }
        }

        /// <summary>
        /// Deactivate an instance (called when returning to pool).
        /// </summary>
        /// <param name="instance">Instance to deactivate</param>
        private void DeactivateInstance(Node3D instance)
        {
            if (instance == null)
            {
                return;
            }

            // Hide the instance
            instance.Visible = false;

            // Disable particle emission if GpuParticles3D
            if (instance is GpuParticles3D gpuParticles)
            {
                gpuParticles.Emitting = false;
            }
            else if (instance is CpuParticles3D cpuParticles)
            {
                cpuParticles.Emitting = false;
            }

            // Also check children for particle systems
            foreach (var child in instance.GetChildren())
            {
                if (child is GpuParticles3D childGpuParticles)
                {
                    childGpuParticles.Emitting = false;
                }
                else if (child is CpuParticles3D childCpuParticles)
                {
                    childCpuParticles.Emitting = false;
                }
            }

            // Reset transform to origin
            instance.Position = Vector3.Zero;
            instance.Rotation = Vector3.Zero;
            instance.Scale = Vector3.One;

            // Reparent to container if needed
            if (instance.GetParent() != _container)
            {
                instance.Reparent(_container);
            }
        }

        #endregion
    }
}
