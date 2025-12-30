using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Animation
{
    /// <summary>
    /// Manages ragdoll physics for character death and physics-based reactions.
    /// Switches between animated and physics-based control of character body parts.
    /// 
    /// USAGE:
    /// RagdollController ragdoll = GetNode<RagdollController>("RagdollController");
    /// ragdoll.ActivateRagdoll();
    /// ragdoll.ApplyImpulse(hitPosition, impulseForce);
    /// 
    /// SETUP (Godot):
    /// 1. Create a Skeleton3D with PhysicalBone3D nodes for each bone
    /// 2. Add RagdollController to character root
    /// 3. Assign Skeleton3D reference in inspector
    /// 4. Configure ragdoll parameters
    /// 
    /// SCENE STRUCTURE:
    /// Character (CharacterBody3D)
    /// ├── RagdollController (this script)
    /// ├── Skeleton3D
    /// │   ├── PhysicalBone3D (hip)
    /// │   ├── PhysicalBone3D (spine)
    /// │   ├── PhysicalBone3D (head)
    /// │   └── ... (other physical bones)
    /// └── AnimationController
    /// 
    /// FEATURES:
    /// - Smooth transition from animation to ragdoll
    /// - Physics impulse application
    /// - Configurable ragdoll constraints
    /// - Auto-disable after rest
    /// - Explosion force support
    /// </summary>
    public partial class RagdollController : Node
    {
        #region Exported Properties

        /// <summary>
        /// Reference to the Skeleton3D node.
        /// </summary>
        [Export] public Skeleton3D Skeleton { get; set; }

        /// <summary>
        /// Enable smooth transition from animation to ragdoll.
        /// </summary>
        [Export] public bool SmoothTransition { get; set; } = true;

        /// <summary>
        /// Transition time in seconds from animation to ragdoll.
        /// </summary>
        [Export] public float TransitionTime { get; set; } = 0.1f;

        /// <summary>
        /// Whether the ragdoll is currently active.
        /// </summary>
        [Export] public bool IsActive { get; private set; } = false;

        /// <summary>
        /// Gravity scale for ragdoll physics.
        /// </summary>
        [Export] public float GravityScale { get; set; } = 1f;

        /// <summary>
        /// Default mass for physical bones.
        /// </summary>
        [Export] public float DefaultMass { get; set; } = 1f;

        /// <summary>
        /// Auto-disable ragdoll after this many seconds at rest (-1 = never).
        /// </summary>
        [Export] public float AutoDisableTime { get; set; } = 5f;

        #endregion

        #region Public Properties

        /// <summary>
        /// Time since ragdoll was activated.
        /// </summary>
        public float TimeActive { get; private set; } = 0f;

        /// <summary>
        /// Number of physical bones in the ragdoll.
        /// </summary>
        public int PhysicalBoneCount => _physicalBones.Count;

        #endregion

        #region Private Fields

        private List<PhysicalBone3D> _physicalBones = new();
        private Dictionary<PhysicalBone3D, Transform3D> _originalTransforms = new();
        private float _transitionTimer = 0f;
        private bool _isTransitioning = false;
        private float _restTimer = 0f;

        #endregion

        #region Signals

        /// <summary>
        /// Emitted when ragdoll is activated.
        /// </summary>
        [Signal]
        public delegate void RagdollActivatedEventHandler();

        /// <summary>
        /// Emitted when ragdoll is deactivated.
        /// </summary>
        [Signal]
        public delegate void RagdollDeactivatedEventHandler();

        /// <summary>
        /// Emitted when an impulse is applied to the ragdoll.
        /// </summary>
        [Signal]
        public delegate void ImpulseAppliedEventHandler(Vector3 position, Vector3 impulse);

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            InitializeRagdoll();
        }

        public override void _Process(double delta)
        {
            if (IsActive)
            {
                TimeActive += (float)delta;
                UpdateRagdoll((float)delta);
            }

            if (_isTransitioning)
            {
                UpdateTransition((float)delta);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize ragdoll system and find all physical bones.
        /// </summary>
        private void InitializeRagdoll()
        {
            // Try to find Skeleton if not assigned
            if (Skeleton == null)
            {
                Skeleton = FindSkeletonInChildren(GetParent());
                if (Skeleton == null)
                {
                    GD.PushWarning($"RagdollController: Skeleton3D not found on {GetParent().Name}");
                    return;
                }
            }

            // Find all PhysicalBone3D children
            FindPhysicalBones(Skeleton);

            if (_physicalBones.Count == 0)
            {
                GD.PushWarning($"RagdollController: No PhysicalBone3D nodes found in skeleton");
                return;
            }

            // Store original transforms and disable physics initially
            foreach (var bone in _physicalBones)
            {
                _originalTransforms[bone] = bone.Transform;
                bone.SetPhysicsProcess(false);
            }

            GD.Print($"RagdollController: Initialized with {_physicalBones.Count} physical bones");
        }

        /// <summary>
        /// Recursively find Skeleton3D in node hierarchy.
        /// </summary>
        private Skeleton3D FindSkeletonInChildren(Node node)
        {
            if (node is Skeleton3D skeleton)
            {
                return skeleton;
            }

            foreach (Node child in node.GetChildren())
            {
                Skeleton3D found = FindSkeletonInChildren(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Recursively find all PhysicalBone3D nodes.
        /// </summary>
        private void FindPhysicalBones(Node node)
        {
            if (node is PhysicalBone3D physicalBone)
            {
                _physicalBones.Add(physicalBone);
            }

            foreach (Node child in node.GetChildren())
            {
                FindPhysicalBones(child);
            }
        }

        #endregion

        #region Ragdoll Control

        /// <summary>
        /// Activate ragdoll physics.
        /// </summary>
        public void ActivateRagdoll()
        {
            if (IsActive || _physicalBones.Count == 0)
                return;

            if (SmoothTransition)
            {
                StartTransition();
            }
            else
            {
                ActivateRagdollImmediate();
            }
        }

        /// <summary>
        /// Immediately activate ragdoll without transition.
        /// </summary>
        public void ActivateRagdollImmediate()
        {
            if (_physicalBones.Count == 0)
                return;

            foreach (var bone in _physicalBones)
            {
                bone.SetPhysicsProcess(true);
                ConfigurePhysicalBone(bone);
            }

            IsActive = true;
            TimeActive = 0f;
            _restTimer = 0f;
            EmitSignal(SignalName.RagdollActivated);
            GD.Print("RagdollController: Ragdoll activated");
        }

        /// <summary>
        /// Deactivate ragdoll and return to animated control.
        /// </summary>
        public void DeactivateRagdoll()
        {
            if (!IsActive)
                return;

            foreach (var bone in _physicalBones)
            {
                bone.SetPhysicsProcess(false);
            }

            IsActive = false;
            TimeActive = 0f;
            EmitSignal(SignalName.RagdollDeactivated);
            GD.Print("RagdollController: Ragdoll deactivated");
        }

        /// <summary>
        /// Apply an impulse to the ragdoll at a specific position.
        /// </summary>
        /// <param name="worldPosition">Position in world space to apply impulse</param>
        /// <param name="impulse">Impulse vector to apply</param>
        public void ApplyImpulse(Vector3 worldPosition, Vector3 impulse)
        {
            if (!IsActive || _physicalBones.Count == 0)
                return;

            // Find the closest bone to the hit position
            PhysicalBone3D closestBone = FindClosestBone(worldPosition);
            if (closestBone != null)
            {
                closestBone.ApplyImpulse(impulse, worldPosition - closestBone.GlobalPosition);
                EmitSignal(SignalName.ImpulseApplied, worldPosition, impulse);
                GD.Print($"RagdollController: Applied impulse {impulse} at {worldPosition}");
            }
        }

        /// <summary>
        /// Apply an explosion force to all ragdoll bones.
        /// </summary>
        /// <param name="explosionCenter">Center of the explosion</param>
        /// <param name="explosionForce">Force of the explosion</param>
        /// <param name="explosionRadius">Radius of the explosion</param>
        public void ApplyExplosionForce(Vector3 explosionCenter, float explosionForce, float explosionRadius)
        {
            if (!IsActive || _physicalBones.Count == 0)
                return;

            foreach (var bone in _physicalBones)
            {
                Vector3 direction = bone.GlobalPosition - explosionCenter;
                float distance = direction.Length();

                if (distance < explosionRadius && distance > 0.01f)
                {
                    // Calculate falloff
                    float falloff = 1f - (distance / explosionRadius);
                    Vector3 force = direction.Normalized() * explosionForce * falloff;
                    
                    bone.ApplyImpulse(force);
                }
            }

            GD.Print($"RagdollController: Applied explosion force {explosionForce} at {explosionCenter}");
        }

        /// <summary>
        /// Reset ragdoll to original pose.
        /// </summary>
        public void ResetPose()
        {
            if (_physicalBones.Count == 0)
                return;

            DeactivateRagdoll();

            foreach (var bone in _physicalBones)
            {
                if (_originalTransforms.ContainsKey(bone))
                {
                    bone.Transform = _originalTransforms[bone];
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Start smooth transition to ragdoll.
        /// </summary>
        private void StartTransition()
        {
            _isTransitioning = true;
            _transitionTimer = 0f;
        }

        /// <summary>
        /// Update the transition from animation to ragdoll.
        /// </summary>
        private void UpdateTransition(float delta)
        {
            _transitionTimer += delta;
            float progress = Mathf.Clamp(_transitionTimer / TransitionTime, 0f, 1f);

            if (progress >= 1f)
            {
                _isTransitioning = false;
                ActivateRagdollImmediate();
            }
        }

        /// <summary>
        /// Update ragdoll state each frame.
        /// </summary>
        private void UpdateRagdoll(float delta)
        {
            // Check if ragdoll is at rest for auto-disable
            if (AutoDisableTime > 0)
            {
                bool allAtRest = true;
                foreach (var bone in _physicalBones)
                {
                    if (bone.LinearVelocity.Length() > 0.1f || bone.AngularVelocity.Length() > 0.1f)
                    {
                        allAtRest = false;
                        break;
                    }
                }

                if (allAtRest)
                {
                    _restTimer += delta;
                    if (_restTimer >= AutoDisableTime)
                    {
                        DeactivateRagdoll();
                    }
                }
                else
                {
                    _restTimer = 0f;
                }
            }
        }

        /// <summary>
        /// Configure physics properties for a physical bone.
        /// </summary>
        private void ConfigurePhysicalBone(PhysicalBone3D bone)
        {
            bone.Mass = DefaultMass;
            // Note: PhysicalBone3D uses global gravity settings in Godot 4.x
            // GravityScale is not directly available, gravity is controlled via project settings
        }

        /// <summary>
        /// Find the physical bone closest to a world position.
        /// </summary>
        private PhysicalBone3D FindClosestBone(Vector3 worldPosition)
        {
            PhysicalBone3D closest = null;
            float closestDistance = float.MaxValue;

            foreach (var bone in _physicalBones)
            {
                float distance = bone.GlobalPosition.DistanceTo(worldPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = bone;
                }
            }

            return closest;
        }

        #endregion
    }
}
