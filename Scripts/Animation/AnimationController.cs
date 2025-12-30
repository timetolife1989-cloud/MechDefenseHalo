using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Animation
{
    /// <summary>
    /// Main animation controller for character animation management.
    /// Handles AnimationTree state machine transitions, animation playback, and animation events.
    /// 
    /// USAGE:
    /// AnimationController controller = GetNode<AnimationController>("AnimationController");
    /// controller.PlayAnimation("walk");
    /// controller.PlayDeathAnimation("forward");
    /// controller.PlayHitReaction(hitDirection);
    /// 
    /// SETUP (Godot):
    /// 1. Add AnimationController node to your character
    /// 2. Assign AnimationTree and AnimationPlayer references in the inspector
    /// 3. Configure AnimationTree with proper state machine
    /// 
    /// SCENE STRUCTURE:
    /// Character (CharacterBody3D)
    /// ├── AnimationController (this script)
    /// ├── AnimationTree (with state machine)
    /// └── AnimationPlayer
    /// 
    /// FEATURES:
    /// - State machine-based animation transitions
    /// - Death animation variants (forward, backward, explosion)
    /// - Directional hit reactions
    /// - Animation event callbacks
    /// - Speed and blend parameter control
    /// </summary>
    public partial class AnimationController : Node
    {
        #region Exported Properties

        /// <summary>
        /// Reference to the AnimationTree node for state machine control.
        /// </summary>
        [Export] public AnimationTree AnimationTree { get; set; }

        /// <summary>
        /// Reference to the AnimationPlayer node for direct animation playback.
        /// </summary>
        [Export] public AnimationPlayer AnimationPlayer { get; set; }

        /// <summary>
        /// Enable animation event callbacks.
        /// </summary>
        [Export] public bool EnableAnimationEvents { get; set; } = true;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current animation state name.
        /// </summary>
        public string CurrentState { get; private set; } = "";

        /// <summary>
        /// Whether the animation system is ready and functional.
        /// </summary>
        public bool IsReady { get; private set; } = false;

        #endregion

        #region Private Fields

        private AnimationNodeStateMachinePlayback _stateMachine;
        private Dictionary<string, Action> _animationEventCallbacks = new();

        #endregion

        #region Signals

        /// <summary>
        /// Emitted when an animation starts playing.
        /// </summary>
        [Signal]
        public delegate void AnimationStartedEventHandler(string animationName);

        /// <summary>
        /// Emitted when an animation finishes playing.
        /// </summary>
        [Signal]
        public delegate void AnimationFinishedEventHandler(string animationName);

        /// <summary>
        /// Emitted for custom animation events.
        /// </summary>
        [Signal]
        public delegate void AnimationEventTriggeredEventHandler(string eventName);

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            InitializeAnimationSystem();
        }

        public override void _Process(double delta)
        {
            if (!IsReady)
                return;

            UpdateCurrentState();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the animation system and connect signals.
        /// </summary>
        private void InitializeAnimationSystem()
        {
            if (AnimationTree == null)
            {
                GD.PushWarning($"AnimationController: AnimationTree not assigned on {Name}");
                return;
            }

            if (AnimationPlayer == null)
            {
                GD.PushWarning($"AnimationController: AnimationPlayer not assigned on {Name}");
                return;
            }

            // Get state machine playback
            _stateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");
            if (_stateMachine == null)
            {
                GD.PushError($"AnimationController: Failed to get state machine playback on {Name}");
                return;
            }

            // Connect animation signals if events are enabled
            if (EnableAnimationEvents && AnimationPlayer != null)
            {
                AnimationPlayer.AnimationStarted += OnAnimationStarted;
                AnimationPlayer.AnimationFinished += OnAnimationFinished;
            }

            IsReady = true;
            GD.Print($"AnimationController: Initialized successfully on {Name}");
        }

        #endregion

        #region Animation Control

        /// <summary>
        /// Play an animation by name using the state machine.
        /// </summary>
        /// <param name="animName">Name of the animation state to transition to</param>
        public void PlayAnimation(string animName)
        {
            if (!IsReady)
            {
                GD.PushWarning($"AnimationController: Cannot play animation '{animName}' - system not ready");
                return;
            }

            if (string.IsNullOrEmpty(animName))
            {
                GD.PushWarning("AnimationController: Animation name is null or empty");
                return;
            }

            _stateMachine?.Travel(animName);
            CurrentState = animName;
        }

        /// <summary>
        /// Play a death animation based on the death type.
        /// </summary>
        /// <param name="deathType">Type of death (forward, backward, explosion, etc.)</param>
        public void PlayDeathAnimation(string deathType)
        {
            string animName = $"death_{deathType}";
            PlayAnimation(animName);
            GD.Print($"AnimationController: Playing death animation '{animName}'");
        }

        /// <summary>
        /// Play a hit reaction animation based on hit direction.
        /// </summary>
        /// <param name="hitDirection">Direction of the hit in world space</param>
        public void PlayHitReaction(Vector3 hitDirection)
        {
            if (!IsReady)
                return;

            // Determine hit direction relative to character
            string directionSuffix = DetermineHitDirection(hitDirection);
            string animName = $"hit_reaction_{directionSuffix}";

            PlayAnimation(animName);
            GD.Print($"AnimationController: Playing hit reaction '{animName}'");
        }

        /// <summary>
        /// Set animation speed multiplier.
        /// </summary>
        /// <param name="speed">Speed multiplier (1.0 = normal speed)</param>
        public void SetAnimationSpeed(float speed)
        {
            if (AnimationTree != null)
            {
                AnimationTree.Set("parameters/TimeScale/scale", speed);
            }
        }

        /// <summary>
        /// Set blend parameter for blending between animations.
        /// </summary>
        /// <param name="parameterName">Name of the blend parameter</param>
        /// <param name="value">Blend value (typically 0.0 to 1.0)</param>
        public void SetBlendParameter(string parameterName, float value)
        {
            if (AnimationTree != null)
            {
                AnimationTree.Set($"parameters/{parameterName}", value);
            }
        }

        /// <summary>
        /// Immediately stop all animations.
        /// </summary>
        public void StopAnimation()
        {
            if (AnimationPlayer != null)
            {
                AnimationPlayer.Stop();
            }
            CurrentState = "";
        }

        #endregion

        #region Animation Events

        /// <summary>
        /// Register a callback for a specific animation event.
        /// </summary>
        /// <param name="eventName">Name of the animation event</param>
        /// <param name="callback">Callback action to invoke</param>
        public void RegisterAnimationEvent(string eventName, Action callback)
        {
            if (!_animationEventCallbacks.ContainsKey(eventName))
            {
                _animationEventCallbacks[eventName] = callback;
            }
            else
            {
                _animationEventCallbacks[eventName] += callback;
            }
        }

        /// <summary>
        /// Unregister a callback for a specific animation event.
        /// </summary>
        /// <param name="eventName">Name of the animation event</param>
        /// <param name="callback">Callback action to remove</param>
        public void UnregisterAnimationEvent(string eventName, Action callback)
        {
            if (_animationEventCallbacks.ContainsKey(eventName))
            {
                _animationEventCallbacks[eventName] -= callback;
            }
        }

        /// <summary>
        /// Trigger a custom animation event manually.
        /// This can be called from animation tracks as a method call.
        /// </summary>
        /// <param name="eventName">Name of the event to trigger</param>
        public void TriggerAnimationEvent(string eventName)
        {
            if (_animationEventCallbacks.ContainsKey(eventName))
            {
                _animationEventCallbacks[eventName]?.Invoke();
            }

            EmitSignal(SignalName.AnimationEventTriggered, eventName);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update the current animation state from the state machine.
        /// </summary>
        private void UpdateCurrentState()
        {
            if (_stateMachine != null)
            {
                string currentState = _stateMachine.GetCurrentNode();
                if (currentState != CurrentState)
                {
                    CurrentState = currentState;
                }
            }
        }

        /// <summary>
        /// Determine hit direction suffix based on hit direction vector.
        /// </summary>
        /// <param name="hitDirection">Direction of the hit</param>
        /// <returns>Direction suffix (front, back, left, right)</returns>
        private string DetermineHitDirection(Vector3 hitDirection)
        {
            if (hitDirection.Length() < 0.01f)
                return "front"; // Default to front if no direction

            // Get character's forward direction (assuming -Z is forward in Godot)
            Node3D parentNode = GetParent() as Node3D;
            if (parentNode == null)
                return "front";

            Vector3 forward = -parentNode.GlobalTransform.Basis.Z;
            Vector3 right = parentNode.GlobalTransform.Basis.X;

            // Normalize hit direction
            Vector3 normalized = hitDirection.Normalized();

            // Calculate dot products to determine direction
            float forwardDot = forward.Dot(normalized);
            float rightDot = right.Dot(normalized);

            // Determine primary direction
            if (Mathf.Abs(forwardDot) > Mathf.Abs(rightDot))
            {
                return forwardDot > 0 ? "front" : "back";
            }
            else
            {
                return rightDot > 0 ? "right" : "left";
            }
        }

        /// <summary>
        /// Called when an animation starts.
        /// </summary>
        private void OnAnimationStarted(StringName animName)
        {
            EmitSignal(SignalName.AnimationStarted, animName.ToString());
        }

        /// <summary>
        /// Called when an animation finishes.
        /// </summary>
        private void OnAnimationFinished(StringName animName)
        {
            EmitSignal(SignalName.AnimationFinished, animName.ToString());
        }

        #endregion

        #region Cleanup

        public override void _ExitTree()
        {
            // Disconnect signals
            if (AnimationPlayer != null && EnableAnimationEvents)
            {
                AnimationPlayer.AnimationStarted -= OnAnimationStarted;
                AnimationPlayer.AnimationFinished -= OnAnimationFinished;
            }

            _animationEventCallbacks.Clear();
        }

        #endregion
    }
}
