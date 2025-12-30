using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Animation
{
    /// <summary>
    /// Manages death animations with multiple variants and transitions to ragdoll.
    /// Provides different death animation types based on damage source and direction.
    /// 
    /// USAGE:
    /// DeathAnimations deathAnim = GetNode<DeathAnimations>("DeathAnimations");
    /// deathAnim.PlayDeathAnimation(hitDirection, damageType);
    /// 
    /// SETUP (Godot):
    /// 1. Add DeathAnimations node to character with AnimationController
    /// 2. Optionally assign RagdollController for physics ragdoll
    /// 3. Configure death animation variants in inspector
    /// 
    /// SCENE STRUCTURE:
    /// Character (CharacterBody3D)
    /// ├── AnimationController
    /// ├── DeathAnimations (this script)
    /// └── RagdollController (optional)
    /// 
    /// DEATH TYPES:
    /// - forward: Hit from front, falls backward
    /// - backward: Hit from back, falls forward
    /// - explosion: Dramatic explosion death
    /// - headshot: Special headshot death
    /// - fire: Burning death animation
    /// - electrocution: Electrical death
    /// </summary>
    public partial class DeathAnimations : Node
    {
        #region Death Types Enum

        /// <summary>
        /// Types of death animations available.
        /// </summary>
        public enum DeathType
        {
            Forward,
            Backward,
            Left,
            Right,
            Explosion,
            Headshot,
            Fire,
            Electrocution,
            Default
        }

        #endregion

        #region Exported Properties

        /// <summary>
        /// Reference to the AnimationController.
        /// </summary>
        [Export] public AnimationController AnimationController { get; set; }

        /// <summary>
        /// Reference to the RagdollController (optional).
        /// </summary>
        [Export] public RagdollController RagdollController { get; set; }

        /// <summary>
        /// Enable ragdoll physics after death animation.
        /// </summary>
        [Export] public bool EnableRagdoll { get; set; } = true;

        /// <summary>
        /// Delay in seconds before activating ragdoll after death animation starts.
        /// </summary>
        [Export] public float RagdollDelay { get; set; } = 0.5f;

        /// <summary>
        /// Whether to select random death variants when available.
        /// </summary>
        [Export] public bool UseRandomVariants { get; set; } = true;

        #endregion

        #region Public Properties

        /// <summary>
        /// Whether a death animation is currently playing.
        /// </summary>
        public bool IsPlayingDeathAnimation { get; private set; } = false;

        /// <summary>
        /// Current death type being played.
        /// </summary>
        public DeathType CurrentDeathType { get; private set; } = DeathType.Default;

        #endregion

        #region Private Fields

        private Dictionary<DeathType, List<string>> _deathAnimationVariants;
        private Timer _ragdollTimer;
        private Random _random = new Random();

        #endregion

        #region Signals

        /// <summary>
        /// Emitted when a death animation starts.
        /// </summary>
        [Signal]
        public delegate void DeathAnimationStartedEventHandler(string deathType);

        /// <summary>
        /// Emitted when a death animation finishes.
        /// </summary>
        [Signal]
        public delegate void DeathAnimationFinishedEventHandler(string deathType);

        /// <summary>
        /// Emitted when ragdoll is activated.
        /// </summary>
        [Signal]
        public delegate void RagdollActivatedEventHandler();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            InitializeDeathAnimations();
            SetupRagdollTimer();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize death animation variants dictionary.
        /// </summary>
        private void InitializeDeathAnimations()
        {
            // Try to find AnimationController if not assigned
            if (AnimationController == null)
            {
                AnimationController = GetParent().GetNodeOrNull<AnimationController>("AnimationController");
                if (AnimationController == null)
                {
                    GD.PushWarning($"DeathAnimations: AnimationController not found on {GetParent().Name}");
                }
            }

            // Try to find RagdollController if not assigned
            if (RagdollController == null && EnableRagdoll)
            {
                RagdollController = GetParent().GetNodeOrNull<RagdollController>("RagdollController");
            }

            // Initialize animation variants
            _deathAnimationVariants = new Dictionary<DeathType, List<string>>()
            {
                { DeathType.Forward, new List<string> { "death_forward", "death_forward_01", "death_forward_02" } },
                { DeathType.Backward, new List<string> { "death_backward", "death_backward_01", "death_backward_02" } },
                { DeathType.Left, new List<string> { "death_left", "death_left_01" } },
                { DeathType.Right, new List<string> { "death_right", "death_right_01" } },
                { DeathType.Explosion, new List<string> { "death_explosion", "death_explosion_01" } },
                { DeathType.Headshot, new List<string> { "death_headshot" } },
                { DeathType.Fire, new List<string> { "death_fire" } },
                { DeathType.Electrocution, new List<string> { "death_electrocution" } },
                { DeathType.Default, new List<string> { "death_default", "death_forward" } }
            };

            GD.Print($"DeathAnimations: Initialized on {GetParent().Name}");
        }

        /// <summary>
        /// Setup the ragdoll activation timer.
        /// </summary>
        private void SetupRagdollTimer()
        {
            _ragdollTimer = new Timer();
            _ragdollTimer.Name = "RagdollTimer";
            _ragdollTimer.OneShot = true;
            _ragdollTimer.Timeout += OnRagdollTimerTimeout;
            AddChild(_ragdollTimer);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Play a death animation based on hit direction and damage type.
        /// </summary>
        /// <param name="hitDirection">Direction the hit came from</param>
        /// <param name="damageType">Type of damage (used to select special death animations)</param>
        public void PlayDeathAnimation(Vector3 hitDirection, string damageType = "")
        {
            if (IsPlayingDeathAnimation)
                return;

            if (AnimationController == null)
            {
                GD.PushError("DeathAnimations: Cannot play death animation - AnimationController is null");
                return;
            }

            // Determine death type based on damage type first
            DeathType deathType = DetermineDeathType(hitDirection, damageType);
            PlayDeathAnimationType(deathType);
        }

        /// <summary>
        /// Play a specific death animation type.
        /// </summary>
        /// <param name="deathType">Type of death animation to play</param>
        public void PlayDeathAnimationType(DeathType deathType)
        {
            if (IsPlayingDeathAnimation)
                return;

            if (AnimationController == null)
            {
                GD.PushError("DeathAnimations: Cannot play death animation - AnimationController is null");
                return;
            }

            IsPlayingDeathAnimation = true;
            CurrentDeathType = deathType;

            // Get animation name from variants
            string animationName = GetDeathAnimationName(deathType);

            // Play the animation
            AnimationController.PlayAnimation(animationName);
            EmitSignal(SignalName.DeathAnimationStarted, deathType.ToString());

            GD.Print($"DeathAnimations: Playing {animationName} for death type {deathType}");

            // Schedule ragdoll activation if enabled
            if (EnableRagdoll && RagdollController != null && RagdollDelay > 0)
            {
                _ragdollTimer.Start(RagdollDelay);
            }
            else if (EnableRagdoll && RagdollController != null && RagdollDelay <= 0)
            {
                ActivateRagdoll();
            }

            // Connect to animation finished if available
            if (AnimationController != null)
            {
                AnimationController.AnimationFinished += OnDeathAnimationFinished;
            }
        }

        /// <summary>
        /// Immediately activate ragdoll physics.
        /// </summary>
        public void ActivateRagdoll()
        {
            if (RagdollController != null)
            {
                RagdollController.ActivateRagdoll();
                EmitSignal(SignalName.RagdollActivated);
                GD.Print("DeathAnimations: Ragdoll activated");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determine which death type to use based on hit direction and damage type.
        /// </summary>
        private DeathType DetermineDeathType(Vector3 hitDirection, string damageType)
        {
            // Check for special damage types first
            if (!string.IsNullOrEmpty(damageType))
            {
                switch (damageType.ToLower())
                {
                    case "explosion":
                    case "explosive":
                        return DeathType.Explosion;
                    case "headshot":
                        return DeathType.Headshot;
                    case "fire":
                    case "burn":
                        return DeathType.Fire;
                    case "electric":
                    case "shock":
                    case "electrocution":
                        return DeathType.Electrocution;
                }
            }

            // Determine based on hit direction
            if (hitDirection.Length() < 0.01f)
                return DeathType.Default;

            Node3D parentNode = GetParent() as Node3D;
            if (parentNode == null)
                return DeathType.Default;

            Vector3 forward = -parentNode.GlobalTransform.Basis.Z;
            Vector3 right = parentNode.GlobalTransform.Basis.X;
            Vector3 normalized = hitDirection.Normalized();

            float forwardDot = forward.Dot(normalized);
            float rightDot = right.Dot(normalized);

            // Determine primary direction
            if (Mathf.Abs(forwardDot) > Mathf.Abs(rightDot))
            {
                return forwardDot > 0 ? DeathType.Forward : DeathType.Backward;
            }
            else
            {
                return rightDot > 0 ? DeathType.Right : DeathType.Left;
            }
        }

        /// <summary>
        /// Get the animation name for a death type, with optional random variant.
        /// </summary>
        private string GetDeathAnimationName(DeathType deathType)
        {
            if (!_deathAnimationVariants.ContainsKey(deathType))
            {
                GD.PushWarning($"DeathAnimations: No variants found for {deathType}, using default");
                deathType = DeathType.Default;
            }

            List<string> variants = _deathAnimationVariants[deathType];
            if (variants.Count == 0)
            {
                return "death_default";
            }

            if (UseRandomVariants && variants.Count > 1)
            {
                int index = _random.Next(variants.Count);
                return variants[index];
            }

            return variants[0];
        }

        /// <summary>
        /// Called when the ragdoll timer times out.
        /// </summary>
        private void OnRagdollTimerTimeout()
        {
            ActivateRagdoll();
        }

        /// <summary>
        /// Called when a death animation finishes.
        /// </summary>
        private void OnDeathAnimationFinished(string animName)
        {
            if (animName.Contains("death"))
            {
                EmitSignal(SignalName.DeathAnimationFinished, CurrentDeathType.ToString());
                IsPlayingDeathAnimation = false;

                // Disconnect signal
                if (AnimationController != null)
                {
                    AnimationController.AnimationFinished -= OnDeathAnimationFinished;
                }
            }
        }

        #endregion

        #region Cleanup

        public override void _ExitTree()
        {
            if (_ragdollTimer != null)
            {
                _ragdollTimer.Timeout -= OnRagdollTimerTimeout;
            }

            if (AnimationController != null)
            {
                AnimationController.AnimationFinished -= OnDeathAnimationFinished;
            }
        }

        #endregion
    }
}
