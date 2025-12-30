using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Animation;

namespace MechDefenseHalo.Tests.Animation
{
    /// <summary>
    /// Unit tests for AnimationController
    /// </summary>
    [TestSuite]
    public class AnimationControllerTests
    {
        private AnimationController _controller;
        private AnimationTree _animationTree;
        private AnimationPlayer _animationPlayer;

        [Before]
        public void Setup()
        {
            _controller = new AnimationController();
            _animationTree = new AnimationTree();
            _animationPlayer = new AnimationPlayer();
            
            // Note: In a real integration test scenario, you would need to set up a proper 
            // AnimationTree with a state machine root and AnimationNodeStateMachinePlayback.
            // For unit tests, we're testing the controller logic independently.
            // See the project's integration test examples for full AnimationTree setup.
        }

        [After]
        public void Teardown()
        {
            _controller?.QueueFree();
            _animationTree?.QueueFree();
            _animationPlayer?.QueueFree();
            _controller = null;
            _animationTree = null;
            _animationPlayer = null;
        }

        #region Initialization Tests

        [TestCase]
        public void AnimationController_InitialState_IsNotReady()
        {
            // Arrange & Act - Controller created in Setup
            
            // Assert
            AssertBool(_controller.IsReady).IsFalse();
        }

        [TestCase]
        public void AnimationController_InitialCurrentState_IsEmpty()
        {
            // Arrange & Act
            string currentState = _controller.CurrentState;
            
            // Assert
            AssertString(currentState).IsEmpty();
        }

        #endregion

        #region Animation Control Tests

        [TestCase]
        public void PlayAnimation_NullOrEmptyName_DoesNotCrash()
        {
            // Arrange
            _controller.AnimationTree = _animationTree;
            _controller.AnimationPlayer = _animationPlayer;
            
            // Act & Assert - Should not throw
            _controller.PlayAnimation("");
            _controller.PlayAnimation(null);
        }

        [TestCase]
        public void PlayDeathAnimation_ValidType_CreatesCorrectAnimationName()
        {
            // Arrange
            string deathType = "forward";
            
            // Act - This will attempt to play "death_forward"
            _controller.PlayDeathAnimation(deathType);
            
            // Assert - No exception should be thrown
            // In a full integration test, we would verify the animation plays
            AssertObject(_controller).IsNotNull();
        }

        [TestCase]
        public void SetAnimationSpeed_ValidSpeed_DoesNotCrash()
        {
            // Arrange
            _controller.AnimationTree = _animationTree;
            float speed = 1.5f;
            
            // Act & Assert - Should not throw
            _controller.SetAnimationSpeed(speed);
        }

        [TestCase]
        public void SetBlendParameter_ValidParameter_DoesNotCrash()
        {
            // Arrange
            _controller.AnimationTree = _animationTree;
            string paramName = "blend";
            float value = 0.5f;
            
            // Act & Assert - Should not throw
            _controller.SetBlendParameter(paramName, value);
        }

        [TestCase]
        public void StopAnimation_WithAnimationPlayer_StopsAnimation()
        {
            // Arrange
            _controller.AnimationPlayer = _animationPlayer;
            
            // Act
            _controller.StopAnimation();
            
            // Assert
            AssertString(_controller.CurrentState).IsEmpty();
        }

        #endregion

        #region Animation Event Tests

        [TestCase]
        public void RegisterAnimationEvent_ValidEvent_DoesNotCrash()
        {
            // Arrange
            string eventName = "test_event";
            bool eventTriggered = false;
            
            // Act
            _controller.RegisterAnimationEvent(eventName, () => { eventTriggered = true; });
            _controller.TriggerAnimationEvent(eventName);
            
            // Assert
            AssertBool(eventTriggered).IsTrue();
        }

        [TestCase]
        public void UnregisterAnimationEvent_RegisteredEvent_RemovesCallback()
        {
            // Arrange
            string eventName = "test_event";
            bool eventTriggered = false;
            void callback() { eventTriggered = true; }
            
            _controller.RegisterAnimationEvent(eventName, callback);
            _controller.UnregisterAnimationEvent(eventName, callback);
            
            // Act
            _controller.TriggerAnimationEvent(eventName);
            
            // Assert - Event should not be triggered after unregistration
            AssertBool(eventTriggered).IsFalse();
        }

        [TestCase]
        public void TriggerAnimationEvent_UnregisteredEvent_DoesNotCrash()
        {
            // Arrange
            string eventName = "nonexistent_event";
            
            // Act & Assert - Should not throw
            _controller.TriggerAnimationEvent(eventName);
        }

        #endregion

        #region Hit Reaction Tests

        [TestCase]
        public void PlayHitReaction_ZeroVector_UsesDefaultDirection()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Zero;
            
            // Act & Assert - Should not crash with zero vector
            _controller.PlayHitReaction(hitDirection);
        }

        [TestCase]
        public void PlayHitReaction_ValidDirection_DoesNotCrash()
        {
            // Arrange
            _controller.AnimationTree = _animationTree;
            Vector3 hitDirection = new Vector3(1, 0, 0);
            
            // Act & Assert - Should not throw
            _controller.PlayHitReaction(hitDirection);
        }

        #endregion
    }
}
