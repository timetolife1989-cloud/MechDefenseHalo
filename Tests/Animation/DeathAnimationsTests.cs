using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Animation;

namespace MechDefenseHalo.Tests.Animation
{
    /// <summary>
    /// Unit tests for DeathAnimations
    /// </summary>
    [TestSuite]
    public class DeathAnimationsTests
    {
        private DeathAnimations _deathAnimations;
        private AnimationController _animationController;

        [Before]
        public void Setup()
        {
            _deathAnimations = new DeathAnimations();
            _animationController = new AnimationController();
            _deathAnimations.AnimationController = _animationController;
        }

        [After]
        public void Teardown()
        {
            _deathAnimations?.QueueFree();
            _animationController?.QueueFree();
            _deathAnimations = null;
            _animationController = null;
        }

        #region Initialization Tests

        [TestCase]
        public void DeathAnimations_InitialState_NotPlayingDeathAnimation()
        {
            // Arrange & Act - Created in Setup
            
            // Assert
            AssertBool(_deathAnimations.IsPlayingDeathAnimation).IsFalse();
        }

        [TestCase]
        public void DeathAnimations_InitialDeathType_IsDefault()
        {
            // Arrange & Act
            var currentType = _deathAnimations.CurrentDeathType;
            
            // Assert
            AssertThat(currentType).IsEqual(DeathAnimations.DeathType.Default);
        }

        [TestCase]
        public void DeathAnimations_EnableRagdoll_DefaultsToTrue()
        {
            // Arrange & Act
            bool enableRagdoll = _deathAnimations.EnableRagdoll;
            
            // Assert
            AssertBool(enableRagdoll).IsTrue();
        }

        [TestCase]
        public void DeathAnimations_RagdollDelay_HasDefaultValue()
        {
            // Arrange & Act
            float delay = _deathAnimations.RagdollDelay;
            
            // Assert
            AssertFloat(delay).IsEqual(0.5f);
        }

        #endregion

        #region Death Type Tests

        [TestCase]
        public void PlayDeathAnimationType_Forward_SetsCorrectDeathType()
        {
            // Arrange
            var deathType = DeathAnimations.DeathType.Forward;
            
            // Act
            _deathAnimations.PlayDeathAnimationType(deathType);
            
            // Assert
            AssertThat(_deathAnimations.CurrentDeathType).IsEqual(deathType);
        }

        [TestCase]
        public void PlayDeathAnimationType_Explosion_SetsCorrectDeathType()
        {
            // Arrange
            var deathType = DeathAnimations.DeathType.Explosion;
            
            // Act
            _deathAnimations.PlayDeathAnimationType(deathType);
            
            // Assert
            AssertThat(_deathAnimations.CurrentDeathType).IsEqual(deathType);
        }

        [TestCase]
        public void PlayDeathAnimationType_Headshot_SetsCorrectDeathType()
        {
            // Arrange
            var deathType = DeathAnimations.DeathType.Headshot;
            
            // Act
            _deathAnimations.PlayDeathAnimationType(deathType);
            
            // Assert
            AssertThat(_deathAnimations.CurrentDeathType).IsEqual(deathType);
        }

        [TestCase]
        public void PlayDeathAnimationType_Fire_SetsCorrectDeathType()
        {
            // Arrange
            var deathType = DeathAnimations.DeathType.Fire;
            
            // Act
            _deathAnimations.PlayDeathAnimationType(deathType);
            
            // Assert
            AssertThat(_deathAnimations.CurrentDeathType).IsEqual(deathType);
        }

        [TestCase]
        public void PlayDeathAnimationType_WhilePlayingDeath_DoesNotRestart()
        {
            // Arrange
            _deathAnimations.PlayDeathAnimationType(DeathAnimations.DeathType.Forward);
            var firstType = _deathAnimations.CurrentDeathType;
            
            // Act - Try to play another death animation
            _deathAnimations.PlayDeathAnimationType(DeathAnimations.DeathType.Backward);
            
            // Assert - Should still be the first death type
            AssertThat(_deathAnimations.CurrentDeathType).IsEqual(firstType);
        }

        #endregion

        #region Direction-Based Death Tests

        [TestCase]
        public void PlayDeathAnimation_ZeroDirection_UsesDefaultType()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Zero;
            
            // Act
            _deathAnimations.PlayDeathAnimation(hitDirection);
            
            // Assert - Should not crash and should be playing
            AssertBool(_deathAnimations.IsPlayingDeathAnimation).IsTrue();
        }

        [TestCase]
        public void PlayDeathAnimation_ExplosionDamageType_UsesExplosionType()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Forward;
            string damageType = "explosion";
            
            // Act
            _deathAnimations.PlayDeathAnimation(hitDirection, damageType);
            
            // Assert
            AssertThat(_deathAnimations.CurrentDeathType).IsEqual(DeathAnimations.DeathType.Explosion);
        }

        [TestCase]
        public void PlayDeathAnimation_HeadshotDamageType_UsesHeadshotType()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Forward;
            string damageType = "headshot";
            
            // Act
            _deathAnimations.PlayDeathAnimation(hitDirection, damageType);
            
            // Assert
            AssertThat(_deathAnimations.CurrentDeathType).IsEqual(DeathAnimations.DeathType.Headshot);
        }

        [TestCase]
        public void PlayDeathAnimation_FireDamageType_UsesFireType()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Forward;
            string damageType = "fire";
            
            // Act
            _deathAnimations.PlayDeathAnimation(hitDirection, damageType);
            
            // Assert
            AssertThat(_deathAnimations.CurrentDeathType).IsEqual(DeathAnimations.DeathType.Fire);
        }

        [TestCase]
        public void PlayDeathAnimation_ElectricDamageType_UsesElectrocutionType()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Forward;
            string damageType = "electric";
            
            // Act
            _deathAnimations.PlayDeathAnimation(hitDirection, damageType);
            
            // Assert
            AssertThat(_deathAnimations.CurrentDeathType).IsEqual(DeathAnimations.DeathType.Electrocution);
        }

        #endregion

        #region Ragdoll Tests

        [TestCase]
        public void ActivateRagdoll_WithoutRagdollController_DoesNotCrash()
        {
            // Arrange
            _deathAnimations.RagdollController = null;
            
            // Act & Assert - Should not throw
            _deathAnimations.ActivateRagdoll();
        }

        [TestCase]
        public void PlayDeathAnimationType_WithZeroRagdollDelay_DoesNotCrash()
        {
            // Arrange
            _deathAnimations.RagdollDelay = 0f;
            _deathAnimations.RagdollController = null; // No controller to avoid null ref
            
            // Act & Assert - Should not throw
            _deathAnimations.PlayDeathAnimationType(DeathAnimations.DeathType.Forward);
        }

        #endregion

        #region Configuration Tests

        [TestCase]
        public void DeathAnimations_UseRandomVariants_DefaultsToTrue()
        {
            // Arrange & Act
            bool useRandom = _deathAnimations.UseRandomVariants;
            
            // Assert
            AssertBool(useRandom).IsTrue();
        }

        [TestCase]
        public void DeathAnimations_DisableRandomVariants_AllowsSettingToFalse()
        {
            // Arrange
            _deathAnimations.UseRandomVariants = false;
            
            // Act
            bool useRandom = _deathAnimations.UseRandomVariants;
            
            // Assert
            AssertBool(useRandom).IsFalse();
        }

        #endregion
    }
}
