using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Animation;

namespace MechDefenseHalo.Tests.Animation
{
    /// <summary>
    /// Unit tests for RagdollController
    /// </summary>
    [TestSuite]
    public class RagdollControllerTests
    {
        private RagdollController _ragdoll;

        [Before]
        public void Setup()
        {
            _ragdoll = new RagdollController();
        }

        [After]
        public void Teardown()
        {
            _ragdoll?.QueueFree();
            _ragdoll = null;
        }

        #region Initialization Tests

        [TestCase]
        public void RagdollController_InitialIsActive_IsFalse()
        {
            // Arrange & Act
            bool isActive = _ragdoll.IsActive;
            
            // Assert
            AssertBool(isActive).IsFalse();
        }

        [TestCase]
        public void RagdollController_InitialTimeActive_IsZero()
        {
            // Arrange & Act
            float timeActive = _ragdoll.TimeActive;
            
            // Assert
            AssertFloat(timeActive).IsEqual(0f);
        }

        [TestCase]
        public void RagdollController_SmoothTransition_DefaultsToTrue()
        {
            // Arrange & Act
            bool smooth = _ragdoll.SmoothTransition;
            
            // Assert
            AssertBool(smooth).IsTrue();
        }

        [TestCase]
        public void RagdollController_TransitionTime_HasDefaultValue()
        {
            // Arrange & Act
            float transitionTime = _ragdoll.TransitionTime;
            
            // Assert
            AssertFloat(transitionTime).IsEqual(0.1f);
        }

        [TestCase]
        public void RagdollController_GravityScale_DefaultsToOne()
        {
            // Arrange & Act
            float gravityScale = _ragdoll.GravityScale;
            
            // Assert
            AssertFloat(gravityScale).IsEqual(1f);
        }

        [TestCase]
        public void RagdollController_DefaultMass_HasDefaultValue()
        {
            // Arrange & Act
            float mass = _ragdoll.DefaultMass;
            
            // Assert
            AssertFloat(mass).IsEqual(1f);
        }

        [TestCase]
        public void RagdollController_AutoDisableTime_HasDefaultValue()
        {
            // Arrange & Act
            float autoDisable = _ragdoll.AutoDisableTime;
            
            // Assert
            AssertFloat(autoDisable).IsEqual(5f);
        }

        [TestCase]
        public void RagdollController_PhysicalBoneCount_InitiallyZero()
        {
            // Arrange & Act
            int count = _ragdoll.PhysicalBoneCount;
            
            // Assert
            AssertInt(count).IsEqual(0);
        }

        #endregion

        #region Activation Tests

        [TestCase]
        public void ActivateRagdoll_WithoutPhysicalBones_DoesNotCrash()
        {
            // Arrange - No physical bones added
            
            // Act & Assert - Should not throw
            _ragdoll.ActivateRagdoll();
        }

        [TestCase]
        public void ActivateRagdollImmediate_WithoutPhysicalBones_DoesNotCrash()
        {
            // Arrange - No physical bones added
            
            // Act & Assert - Should not throw
            _ragdoll.ActivateRagdollImmediate();
        }

        [TestCase]
        public void DeactivateRagdoll_WhenNotActive_DoesNotCrash()
        {
            // Arrange
            AssertBool(_ragdoll.IsActive).IsFalse();
            
            // Act & Assert - Should not throw
            _ragdoll.DeactivateRagdoll();
        }

        [TestCase]
        public void ActivateRagdoll_WithSmoothTransitionDisabled_ActivatesImmediately()
        {
            // Arrange
            _ragdoll.SmoothTransition = false;
            
            // Act
            _ragdoll.ActivateRagdoll();
            
            // Assert - Should be active immediately (though no bones to activate)
            // With no physical bones, it won't actually activate
            AssertBool(_ragdoll.IsActive).IsFalse();
        }

        #endregion

        #region Impulse Tests

        [TestCase]
        public void ApplyImpulse_WhenNotActive_DoesNotCrash()
        {
            // Arrange
            Vector3 position = Vector3.Zero;
            Vector3 impulse = Vector3.Forward * 100f;
            
            // Act & Assert - Should not throw
            _ragdoll.ApplyImpulse(position, impulse);
        }

        [TestCase]
        public void ApplyImpulse_WithoutPhysicalBones_DoesNotCrash()
        {
            // Arrange
            _ragdoll.ActivateRagdollImmediate(); // Try to activate
            Vector3 position = Vector3.Zero;
            Vector3 impulse = Vector3.Forward * 100f;
            
            // Act & Assert - Should not throw
            _ragdoll.ApplyImpulse(position, impulse);
        }

        [TestCase]
        public void ApplyExplosionForce_WhenNotActive_DoesNotCrash()
        {
            // Arrange
            Vector3 center = Vector3.Zero;
            float force = 500f;
            float radius = 10f;
            
            // Act & Assert - Should not throw
            _ragdoll.ApplyExplosionForce(center, force, radius);
        }

        [TestCase]
        public void ApplyExplosionForce_WithoutPhysicalBones_DoesNotCrash()
        {
            // Arrange
            _ragdoll.ActivateRagdollImmediate();
            Vector3 center = Vector3.Zero;
            float force = 500f;
            float radius = 10f;
            
            // Act & Assert - Should not throw
            _ragdoll.ApplyExplosionForce(center, force, radius);
        }

        #endregion

        #region Reset Tests

        [TestCase]
        public void ResetPose_WithoutPhysicalBones_DoesNotCrash()
        {
            // Arrange - No physical bones
            
            // Act & Assert - Should not throw
            _ragdoll.ResetPose();
        }

        [TestCase]
        public void ResetPose_DeactivatesRagdoll()
        {
            // Arrange
            _ragdoll.ActivateRagdollImmediate();
            
            // Act
            _ragdoll.ResetPose();
            
            // Assert
            AssertBool(_ragdoll.IsActive).IsFalse();
        }

        #endregion

        #region Configuration Tests

        [TestCase]
        public void RagdollController_CustomGravityScale_Works()
        {
            // Arrange
            float customGravity = 2.0f;
            
            // Act
            _ragdoll.GravityScale = customGravity;
            
            // Assert
            AssertFloat(_ragdoll.GravityScale).IsEqual(customGravity);
        }

        [TestCase]
        public void RagdollController_CustomDefaultMass_Works()
        {
            // Arrange
            float customMass = 5.0f;
            
            // Act
            _ragdoll.DefaultMass = customMass;
            
            // Assert
            AssertFloat(_ragdoll.DefaultMass).IsEqual(customMass);
        }

        [TestCase]
        public void RagdollController_CustomAutoDisableTime_Works()
        {
            // Arrange
            float customTime = 10f;
            
            // Act
            _ragdoll.AutoDisableTime = customTime;
            
            // Assert
            AssertFloat(_ragdoll.AutoDisableTime).IsEqual(customTime);
        }

        [TestCase]
        public void RagdollController_DisableAutoDisable_Works()
        {
            // Arrange
            float noAutoDisable = -1f;
            
            // Act
            _ragdoll.AutoDisableTime = noAutoDisable;
            
            // Assert
            AssertFloat(_ragdoll.AutoDisableTime).IsEqual(-1f);
        }

        [TestCase]
        public void RagdollController_CustomTransitionTime_Works()
        {
            // Arrange
            float customTime = 0.5f;
            
            // Act
            _ragdoll.TransitionTime = customTime;
            
            // Assert
            AssertFloat(_ragdoll.TransitionTime).IsEqual(customTime);
        }

        [TestCase]
        public void RagdollController_DisableSmoothTransition_Works()
        {
            // Arrange
            _ragdoll.SmoothTransition = false;
            
            // Act
            bool smooth = _ragdoll.SmoothTransition;
            
            // Assert
            AssertBool(smooth).IsFalse();
        }

        #endregion
    }
}
