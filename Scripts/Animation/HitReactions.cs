using Godot;
using System;

namespace MechDefenseHalo.Animation
{
    /// <summary>
    /// Manages hit reaction animations based on damage direction and type.
    /// Provides directional hit feedback and integrates with the animation system.
    /// 
    /// USAGE:
    /// HitReactions hitReactions = GetNode<HitReactions>("HitReactions");
    /// hitReactions.PlayHitReaction(hitDirection, damageAmount);
    /// 
    /// SETUP (Godot):
    /// 1. Add HitReactions node to character with AnimationController
    /// 2. Configure hit reaction parameters in inspector
    /// 3. Optionally connect to damage events
    /// 
    /// SCENE STRUCTURE:
    /// Character (CharacterBody3D)
    /// ├── AnimationController
    /// ├── HitReactions (this script)
    /// └── HealthComponent (optional, for auto-triggering)
    /// 
    /// FEATURES:
    /// - Directional hit reactions (front, back, left, right)
    /// - Damage-based reaction intensity
    /// - Hit reaction cooldown to prevent spam
    /// - Optional camera shake integration
    /// - Critical hit reactions
    /// </summary>
    public partial class HitReactions : Node
    {
        #region Hit Direction Enum

        /// <summary>
        /// Direction of the hit relative to the character.
        /// </summary>
        public enum HitDirection
        {
            Front,
            Back,
            Left,
            Right
        }

        #endregion

        #region Exported Properties

        /// <summary>
        /// Reference to the AnimationController.
        /// </summary>
        [Export] public AnimationController AnimationController { get; set; }

        /// <summary>
        /// Minimum damage required to trigger a hit reaction.
        /// </summary>
        [Export] public float MinDamageThreshold { get; set; } = 5f;

        /// <summary>
        /// Cooldown between hit reactions in seconds.
        /// </summary>
        [Export] public float HitReactionCooldown { get; set; } = 0.5f;

        /// <summary>
        /// Damage threshold for heavy hit reactions.
        /// </summary>
        [Export] public float HeavyHitThreshold { get; set; } = 50f;

        /// <summary>
        /// Enable hit reactions (can be disabled for bosses or certain states).
        /// </summary>
        [Export] public bool EnableHitReactions { get; set; } = true;

        /// <summary>
        /// Play different animations for critical hits.
        /// </summary>
        [Export] public bool UseCriticalHitReactions { get; set; } = true;

        #endregion

        #region Public Properties

        /// <summary>
        /// Whether a hit reaction is currently playing.
        /// </summary>
        public bool IsPlayingHitReaction { get; private set; } = false;

        /// <summary>
        /// Time remaining on hit reaction cooldown.
        /// </summary>
        public float CooldownRemaining => _cooldownTimer > 0 ? _cooldownTimer : 0f;

        #endregion

        #region Private Fields

        private float _cooldownTimer = 0f;
        private HitDirection _lastHitDirection = HitDirection.Front;

        #endregion

        #region Signals

        /// <summary>
        /// Emitted when a hit reaction starts.
        /// </summary>
        [Signal]
        public delegate void HitReactionStartedEventHandler(string direction, float damage);

        /// <summary>
        /// Emitted when a hit reaction finishes.
        /// </summary>
        [Signal]
        public delegate void HitReactionFinishedEventHandler();

        /// <summary>
        /// Emitted when a critical hit reaction plays.
        /// </summary>
        [Signal]
        public delegate void CriticalHitReactionEventHandler(string direction);

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            InitializeHitReactions();
        }

        public override void _Process(double delta)
        {
            UpdateCooldown((float)delta);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize hit reaction system.
        /// </summary>
        private void InitializeHitReactions()
        {
            // Try to find AnimationController if not assigned
            if (AnimationController == null)
            {
                AnimationController = GetParent().GetNodeOrNull<AnimationController>("AnimationController");
                if (AnimationController == null)
                {
                    GD.PushWarning($"HitReactions: AnimationController not found on {GetParent().Name}");
                }
            }

            GD.Print($"HitReactions: Initialized on {GetParent().Name}");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Play a hit reaction based on hit direction and damage amount.
        /// </summary>
        /// <param name="hitDirection">Direction the hit came from in world space</param>
        /// <param name="damageAmount">Amount of damage taken</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        public void PlayHitReaction(Vector3 hitDirection, float damageAmount = 0f, bool isCritical = false)
        {
            if (!EnableHitReactions || AnimationController == null)
                return;

            // Check cooldown
            if (_cooldownTimer > 0)
            {
                return;
            }

            // Check damage threshold
            if (damageAmount < MinDamageThreshold && damageAmount > 0)
            {
                return;
            }

            // Determine hit direction
            HitDirection direction = DetermineHitDirection(hitDirection);
            _lastHitDirection = direction;

            // Select animation based on damage and critical status
            string animationName = GetHitReactionAnimation(direction, damageAmount, isCritical);

            // Play the animation
            AnimationController.PlayAnimation(animationName);
            IsPlayingHitReaction = true;

            // Start cooldown
            _cooldownTimer = HitReactionCooldown;

            // Emit signals
            EmitSignal(SignalName.HitReactionStarted, direction.ToString(), damageAmount);
            if (isCritical && UseCriticalHitReactions)
            {
                EmitSignal(SignalName.CriticalHitReaction, direction.ToString());
            }

            GD.Print($"HitReactions: Playing {animationName} (damage: {damageAmount}, critical: {isCritical})");

            // Connect to animation finished (disconnect first to avoid multiple subscriptions)
            if (AnimationController != null)
            {
                AnimationController.AnimationFinished -= OnHitReactionFinished;
                AnimationController.AnimationFinished += OnHitReactionFinished;
            }
        }

        /// <summary>
        /// Play a hit reaction for a specific direction.
        /// </summary>
        /// <param name="direction">Hit direction</param>
        /// <param name="isHeavy">Whether this is a heavy hit</param>
        public void PlayHitReactionDirection(HitDirection direction, bool isHeavy = false)
        {
            if (!EnableHitReactions || AnimationController == null)
                return;

            if (_cooldownTimer > 0)
                return;

            _lastHitDirection = direction;
            float simulatedDamage = isHeavy ? HeavyHitThreshold : MinDamageThreshold;
            
            string animationName = GetHitReactionAnimation(direction, simulatedDamage, false);
            AnimationController.PlayAnimation(animationName);
            IsPlayingHitReaction = true;
            _cooldownTimer = HitReactionCooldown;

            EmitSignal(SignalName.HitReactionStarted, direction.ToString(), simulatedDamage);

            if (AnimationController != null)
            {
                AnimationController.AnimationFinished -= OnHitReactionFinished;
                AnimationController.AnimationFinished += OnHitReactionFinished;
            }
        }

        /// <summary>
        /// Force reset the hit reaction cooldown.
        /// </summary>
        public void ResetCooldown()
        {
            _cooldownTimer = 0f;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update the hit reaction cooldown timer.
        /// </summary>
        private void UpdateCooldown(float delta)
        {
            if (_cooldownTimer > 0)
            {
                _cooldownTimer -= delta;
            }
        }

        /// <summary>
        /// Determine hit direction relative to character.
        /// </summary>
        private HitDirection DetermineHitDirection(Vector3 hitDirection)
        {
            if (hitDirection.Length() < 0.01f)
                return HitDirection.Front;

            Node3D parentNode = GetParent() as Node3D;
            if (parentNode == null)
                return HitDirection.Front;

            Vector3 forward = -parentNode.GlobalTransform.Basis.Z;
            Vector3 right = parentNode.GlobalTransform.Basis.X;
            Vector3 normalized = hitDirection.Normalized();

            float forwardDot = forward.Dot(normalized);
            float rightDot = right.Dot(normalized);

            // Determine primary direction
            if (Mathf.Abs(forwardDot) > Mathf.Abs(rightDot))
            {
                return forwardDot > 0 ? HitDirection.Front : HitDirection.Back;
            }
            else
            {
                return rightDot > 0 ? HitDirection.Right : HitDirection.Left;
            }
        }

        /// <summary>
        /// Get the appropriate hit reaction animation name.
        /// </summary>
        private string GetHitReactionAnimation(HitDirection direction, float damageAmount, bool isCritical)
        {
            string baseAnim = "hit_reaction";
            string directionSuffix = direction.ToString().ToLower();
            
            // Check for critical hit animation
            if (isCritical && UseCriticalHitReactions)
            {
                return $"{baseAnim}_critical_{directionSuffix}";
            }

            // Check for heavy hit animation
            if (damageAmount >= HeavyHitThreshold)
            {
                return $"{baseAnim}_heavy_{directionSuffix}";
            }

            // Default hit reaction
            return $"{baseAnim}_{directionSuffix}";
        }

        /// <summary>
        /// Called when a hit reaction animation finishes.
        /// </summary>
        private void OnHitReactionFinished(string animName)
        {
            if (animName.Contains("hit_reaction"))
            {
                IsPlayingHitReaction = false;
                EmitSignal(SignalName.HitReactionFinished);

                // Disconnect signal
                if (AnimationController != null)
                {
                    AnimationController.AnimationFinished -= OnHitReactionFinished;
                }
            }
        }

        #endregion

        #region Cleanup

        public override void _ExitTree()
        {
            if (AnimationController != null)
            {
                AnimationController.AnimationFinished -= OnHitReactionFinished;
            }
        }

        #endregion
    }
}
