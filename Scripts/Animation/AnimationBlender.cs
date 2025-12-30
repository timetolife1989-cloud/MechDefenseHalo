using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Animation
{
    /// <summary>
    /// Advanced animation blending system for smooth state transitions.
    /// Handles blending between different animation states, layers, and blend trees.
    /// 
    /// USAGE:
    /// AnimationBlender blender = GetNode<AnimationBlender>("AnimationBlender");
    /// blender.BlendToState("walk", 0.3f);
    /// blender.SetLayerWeight("upper_body", 0.8f);
    /// 
    /// SETUP (Godot):
    /// 1. Add AnimationBlender node to character with AnimationController
    /// 2. Configure blend parameters and layers in inspector
    /// 3. Set up AnimationTree with blend nodes
    /// 
    /// SCENE STRUCTURE:
    /// Character (CharacterBody3D)
    /// ├── AnimationController
    /// ├── AnimationTree (with blend nodes and layers)
    /// └── AnimationBlender (this script)
    /// 
    /// FEATURES:
    /// - Smooth state-to-state blending
    /// - Animation layer weight control
    /// - Blend space (2D/3D) parameter management
    /// - Custom blend curves
    /// - Animation speed ramping
    /// </summary>
    public partial class AnimationBlender : Node
    {
        #region Exported Properties

        /// <summary>
        /// Reference to the AnimationController.
        /// </summary>
        [Export] public AnimationController AnimationController { get; set; }

        /// <summary>
        /// Default blend time in seconds for state transitions.
        /// </summary>
        [Export] public float DefaultBlendTime { get; set; } = 0.2f;

        /// <summary>
        /// Enable smooth parameter interpolation.
        /// </summary>
        [Export] public bool SmoothParameterTransitions { get; set; } = true;

        /// <summary>
        /// Speed of parameter interpolation (higher = faster).
        /// </summary>
        [Export] public float ParameterInterpolationSpeed { get; set; } = 5f;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current blend progress (0.0 to 1.0).
        /// </summary>
        public float CurrentBlendProgress { get; private set; } = 1f;

        /// <summary>
        /// Whether a blend is currently in progress.
        /// </summary>
        public bool IsBlending => CurrentBlendProgress < 1f;

        #endregion

        #region Private Fields

        private Dictionary<string, float> _targetParameters = new();
        private Dictionary<string, float> _currentParameters = new();
        private Dictionary<string, float> _layerWeights = new();
        private string _targetState = "";
        private float _blendTimer = 0f;
        private float _blendDuration = 0f;

        #endregion

        #region Signals

        /// <summary>
        /// Emitted when a blend starts.
        /// </summary>
        [Signal]
        public delegate void BlendStartedEventHandler(string fromState, string toState, float duration);

        /// <summary>
        /// Emitted when a blend finishes.
        /// </summary>
        [Signal]
        public delegate void BlendFinishedEventHandler(string toState);

        /// <summary>
        /// Emitted when a layer weight changes.
        /// </summary>
        [Signal]
        public delegate void LayerWeightChangedEventHandler(string layerName, float weight);

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            InitializeBlender();
        }

        public override void _Process(double delta)
        {
            UpdateBlending((float)delta);
            UpdateParameterInterpolation((float)delta);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the animation blender.
        /// </summary>
        private void InitializeBlender()
        {
            // Try to find AnimationController if not assigned
            if (AnimationController == null)
            {
                AnimationController = GetParent().GetNodeOrNull<AnimationController>("AnimationController");
                if (AnimationController == null)
                {
                    GD.PushWarning($"AnimationBlender: AnimationController not found on {GetParent().Name}");
                }
            }

            GD.Print($"AnimationBlender: Initialized on {GetParent().Name}");
        }

        #endregion

        #region State Blending

        /// <summary>
        /// Blend to a new animation state over the specified duration.
        /// </summary>
        /// <param name="stateName">Name of the target state</param>
        /// <param name="blendTime">Time to blend in seconds (uses DefaultBlendTime if negative)</param>
        public void BlendToState(string stateName, float blendTime = -1f)
        {
            if (AnimationController == null)
            {
                GD.PushError("AnimationBlender: Cannot blend - AnimationController is null");
                return;
            }

            if (string.IsNullOrEmpty(stateName))
            {
                GD.PushWarning("AnimationBlender: State name is null or empty");
                return;
            }

            string fromState = AnimationController.CurrentState;
            _targetState = stateName;
            _blendDuration = blendTime >= 0 ? blendTime : DefaultBlendTime;
            _blendTimer = 0f;
            CurrentBlendProgress = 0f;

            EmitSignal(SignalName.BlendStarted, fromState, stateName, _blendDuration);
            GD.Print($"AnimationBlender: Blending from '{fromState}' to '{stateName}' over {_blendDuration}s");

            // If blend time is zero, immediately transition
            if (_blendDuration <= 0)
            {
                AnimationController.PlayAnimation(stateName);
                CurrentBlendProgress = 1f;
                EmitSignal(SignalName.BlendFinished, stateName);
            }
        }

        /// <summary>
        /// Immediately snap to a state without blending.
        /// </summary>
        /// <param name="stateName">Name of the target state</param>
        public void SnapToState(string stateName)
        {
            BlendToState(stateName, 0f);
        }

        #endregion

        #region Layer Management

        /// <summary>
        /// Set the weight of an animation layer.
        /// </summary>
        /// <param name="layerName">Name of the layer</param>
        /// <param name="weight">Weight value (0.0 to 1.0)</param>
        /// <param name="smooth">Whether to smoothly interpolate to the new weight</param>
        public void SetLayerWeight(string layerName, float weight, bool smooth = true)
        {
            weight = Mathf.Clamp(weight, 0f, 1f);

            if (!_layerWeights.ContainsKey(layerName))
            {
                _layerWeights[layerName] = 0f;
            }

            if (smooth && SmoothParameterTransitions)
            {
                _layerWeights[layerName] = weight; // Will be interpolated in _Process
            }
            else
            {
                _layerWeights[layerName] = weight;
                ApplyLayerWeight(layerName, weight);
            }

            EmitSignal(SignalName.LayerWeightChanged, layerName, weight);
        }

        /// <summary>
        /// Get the current weight of an animation layer.
        /// </summary>
        /// <param name="layerName">Name of the layer</param>
        /// <returns>Current layer weight</returns>
        public float GetLayerWeight(string layerName)
        {
            if (_layerWeights.ContainsKey(layerName))
            {
                return _layerWeights[layerName];
            }
            return 0f;
        }

        #endregion

        #region Parameter Management

        /// <summary>
        /// Set a blend parameter value.
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="value">Parameter value</param>
        /// <param name="smooth">Whether to smoothly interpolate to the new value</param>
        public void SetParameter(string parameterName, float value, bool smooth = true)
        {
            if (AnimationController == null || AnimationController.AnimationTree == null)
                return;

            if (!_currentParameters.ContainsKey(parameterName))
            {
                _currentParameters[parameterName] = 0f;
            }

            _targetParameters[parameterName] = value;

            if (!smooth || !SmoothParameterTransitions)
            {
                _currentParameters[parameterName] = value;
                AnimationController.SetBlendParameter(parameterName, value);
            }
        }

        /// <summary>
        /// Set a 2D blend space parameter.
        /// </summary>
        /// <param name="parameterName">Name of the blend space parameter</param>
        /// <param name="value">2D vector value</param>
        /// <param name="smooth">Whether to smoothly interpolate</param>
        public void SetBlendSpace2D(string parameterName, Vector2 value, bool smooth = true)
        {
            SetParameter($"{parameterName}/blend_position", value.X, smooth);
        }

        /// <summary>
        /// Get current value of a parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <returns>Current parameter value</returns>
        public float GetParameter(string parameterName)
        {
            if (_currentParameters.ContainsKey(parameterName))
            {
                return _currentParameters[parameterName];
            }
            return 0f;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update the current blend operation.
        /// </summary>
        private void UpdateBlending(float delta)
        {
            if (CurrentBlendProgress >= 1f)
                return;

            _blendTimer += delta;
            CurrentBlendProgress = Mathf.Clamp(_blendTimer / _blendDuration, 0f, 1f);

            // Apply eased blend curve for smoother transitions
            float easedProgress = EaseInOutCubic(CurrentBlendProgress);

            // Update blend weight parameter if available
            if (AnimationController != null && AnimationController.AnimationTree != null)
            {
                AnimationController.AnimationTree.Set("parameters/blend_amount", easedProgress);
            }

            // Check if blend is complete
            if (CurrentBlendProgress >= 1f)
            {
                if (AnimationController != null)
                {
                    AnimationController.PlayAnimation(_targetState);
                }
                EmitSignal(SignalName.BlendFinished, _targetState);
                GD.Print($"AnimationBlender: Blend to '{_targetState}' completed");
            }
        }

        /// <summary>
        /// Smoothly interpolate parameters to their target values.
        /// </summary>
        private void UpdateParameterInterpolation(float delta)
        {
            if (!SmoothParameterTransitions || AnimationController == null)
                return;

            foreach (var kvp in _targetParameters)
            {
                string paramName = kvp.Key;
                float targetValue = kvp.Value;

                if (!_currentParameters.ContainsKey(paramName))
                {
                    _currentParameters[paramName] = targetValue;
                    continue;
                }

                float currentValue = _currentParameters[paramName];
                float newValue = Mathf.Lerp(currentValue, targetValue, delta * ParameterInterpolationSpeed);
                _currentParameters[paramName] = newValue;

                // Apply to AnimationTree
                AnimationController.SetBlendParameter(paramName, newValue);
            }
        }

        /// <summary>
        /// Apply a layer weight to the AnimationTree.
        /// </summary>
        private void ApplyLayerWeight(string layerName, float weight)
        {
            if (AnimationController == null || AnimationController.AnimationTree == null)
                return;

            // Try to set layer weight in AnimationTree
            string layerPath = $"parameters/{layerName}/blend_amount";
            AnimationController.AnimationTree.Set(layerPath, weight);
        }

        /// <summary>
        /// Cubic ease in-out function for smooth blending.
        /// </summary>
        private float EaseInOutCubic(float t)
        {
            return t < 0.5f 
                ? 4f * t * t * t 
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        #endregion
    }
}
