using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Animation;

namespace MechDefenseHalo.Tests.Animation
{
    /// <summary>
    /// Unit tests for AnimationBlender
    /// </summary>
    [TestSuite]
    public class AnimationBlenderTests
    {
        private AnimationBlender _blender;
        private AnimationController _animationController;

        [Before]
        public void Setup()
        {
            _blender = new AnimationBlender();
            _animationController = new AnimationController();
            _blender.AnimationController = _animationController;
        }

        [After]
        public void Teardown()
        {
            _blender?.QueueFree();
            _animationController?.QueueFree();
            _blender = null;
            _animationController = null;
        }

        #region Initialization Tests

        [TestCase]
        public void AnimationBlender_DefaultBlendTime_HasValue()
        {
            // Arrange & Act
            float blendTime = _blender.DefaultBlendTime;
            
            // Assert
            AssertFloat(blendTime).IsEqual(0.2f);
        }

        [TestCase]
        public void AnimationBlender_SmoothParameterTransitions_DefaultsToTrue()
        {
            // Arrange & Act
            bool smooth = _blender.SmoothParameterTransitions;
            
            // Assert
            AssertBool(smooth).IsTrue();
        }

        [TestCase]
        public void AnimationBlender_ParameterInterpolationSpeed_HasDefaultValue()
        {
            // Arrange & Act
            float speed = _blender.ParameterInterpolationSpeed;
            
            // Assert
            AssertFloat(speed).IsEqual(5f);
        }

        [TestCase]
        public void AnimationBlender_InitialBlendProgress_IsOne()
        {
            // Arrange & Act
            float progress = _blender.CurrentBlendProgress;
            
            // Assert
            AssertFloat(progress).IsEqual(1f);
        }

        [TestCase]
        public void AnimationBlender_InitialIsBlending_IsFalse()
        {
            // Arrange & Act
            bool isBlending = _blender.IsBlending;
            
            // Assert
            AssertBool(isBlending).IsFalse();
        }

        #endregion

        #region State Blending Tests

        [TestCase]
        public void BlendToState_NullStateName_DoesNotCrash()
        {
            // Arrange
            string stateName = null;
            
            // Act & Assert - Should not throw
            _blender.BlendToState(stateName);
        }

        [TestCase]
        public void BlendToState_EmptyStateName_DoesNotCrash()
        {
            // Arrange
            string stateName = "";
            
            // Act & Assert - Should not throw
            _blender.BlendToState(stateName);
        }

        [TestCase]
        public void BlendToState_ValidState_StartsBlending()
        {
            // Arrange
            string stateName = "walk";
            float blendTime = 0.5f;
            
            // Act
            _blender.BlendToState(stateName, blendTime);
            
            // Assert
            AssertBool(_blender.IsBlending).IsTrue();
        }

        [TestCase]
        public void BlendToState_ZeroBlendTime_CompletesImmediately()
        {
            // Arrange
            string stateName = "walk";
            float blendTime = 0f;
            
            // Act
            _blender.BlendToState(stateName, blendTime);
            
            // Assert
            AssertBool(_blender.IsBlending).IsFalse();
            AssertFloat(_blender.CurrentBlendProgress).IsEqual(1f);
        }

        [TestCase]
        public void BlendToState_NegativeBlendTime_UsesDefaultBlendTime()
        {
            // Arrange
            string stateName = "walk";
            float blendTime = -1f; // Use default
            
            // Act
            _blender.BlendToState(stateName, blendTime);
            
            // Assert - Should use default blend time and start blending
            AssertBool(_blender.IsBlending).IsTrue();
        }

        [TestCase]
        public void SnapToState_ValidState_CompletesImmediately()
        {
            // Arrange
            string stateName = "idle";
            
            // Act
            _blender.SnapToState(stateName);
            
            // Assert
            AssertBool(_blender.IsBlending).IsFalse();
        }

        #endregion

        #region Layer Management Tests

        [TestCase]
        public void SetLayerWeight_ValidLayer_DoesNotCrash()
        {
            // Arrange
            string layerName = "upper_body";
            float weight = 0.8f;
            
            // Act & Assert - Should not throw
            _blender.SetLayerWeight(layerName, weight);
        }

        [TestCase]
        public void SetLayerWeight_ClampsBelowZero()
        {
            // Arrange
            string layerName = "test_layer";
            float weight = -1f;
            
            // Act
            _blender.SetLayerWeight(layerName, weight);
            float result = _blender.GetLayerWeight(layerName);
            
            // Assert - Should clamp to 0
            AssertFloat(result).IsGreaterEqual(0f);
        }

        [TestCase]
        public void SetLayerWeight_ClampsAboveOne()
        {
            // Arrange
            string layerName = "test_layer";
            float weight = 2f;
            
            // Act
            _blender.SetLayerWeight(layerName, weight);
            float result = _blender.GetLayerWeight(layerName);
            
            // Assert - Should clamp to 1
            AssertFloat(result).IsLessEqual(1f);
        }

        [TestCase]
        public void GetLayerWeight_NonexistentLayer_ReturnsZero()
        {
            // Arrange
            string layerName = "nonexistent_layer";
            
            // Act
            float weight = _blender.GetLayerWeight(layerName);
            
            // Assert
            AssertFloat(weight).IsEqual(0f);
        }

        [TestCase]
        public void SetLayerWeight_WithoutSmooth_SetsImmediately()
        {
            // Arrange
            string layerName = "test_layer";
            float weight = 0.5f;
            
            // Act
            _blender.SetLayerWeight(layerName, weight, smooth: false);
            float result = _blender.GetLayerWeight(layerName);
            
            // Assert
            AssertFloat(result).IsEqual(0.5f);
        }

        #endregion

        #region Parameter Management Tests

        [TestCase]
        public void SetParameter_ValidParameter_DoesNotCrash()
        {
            // Arrange
            string paramName = "speed";
            float value = 1.5f;
            
            // Act & Assert - Should not throw
            _blender.SetParameter(paramName, value);
        }

        [TestCase]
        public void GetParameter_NonexistentParameter_ReturnsZero()
        {
            // Arrange
            string paramName = "nonexistent_param";
            
            // Act
            float value = _blender.GetParameter(paramName);
            
            // Assert
            AssertFloat(value).IsEqual(0f);
        }

        [TestCase]
        public void SetParameter_WithoutSmooth_SetsImmediately()
        {
            // Arrange
            string paramName = "test_param";
            float value = 0.75f;
            
            // Act
            _blender.SetParameter(paramName, value, smooth: false);
            float result = _blender.GetParameter(paramName);
            
            // Assert
            AssertFloat(result).IsEqual(0.75f);
        }

        [TestCase]
        public void SetBlendSpace2D_ValidParameter_DoesNotCrash()
        {
            // Arrange
            string paramName = "movement";
            Vector2 value = new Vector2(1f, 0.5f);
            
            // Act & Assert - Should not throw
            _blender.SetBlendSpace2D(paramName, value);
        }

        #endregion

        #region Configuration Tests

        [TestCase]
        public void AnimationBlender_CustomDefaultBlendTime_Works()
        {
            // Arrange
            float customTime = 1.0f;
            
            // Act
            _blender.DefaultBlendTime = customTime;
            
            // Assert
            AssertFloat(_blender.DefaultBlendTime).IsEqual(customTime);
        }

        [TestCase]
        public void AnimationBlender_DisableSmoothTransitions_Works()
        {
            // Arrange
            _blender.SmoothParameterTransitions = false;
            
            // Act
            bool smooth = _blender.SmoothParameterTransitions;
            
            // Assert
            AssertBool(smooth).IsFalse();
        }

        [TestCase]
        public void AnimationBlender_CustomInterpolationSpeed_Works()
        {
            // Arrange
            float customSpeed = 10f;
            
            // Act
            _blender.ParameterInterpolationSpeed = customSpeed;
            
            // Assert
            AssertFloat(_blender.ParameterInterpolationSpeed).IsEqual(customSpeed);
        }

        #endregion
    }
}
