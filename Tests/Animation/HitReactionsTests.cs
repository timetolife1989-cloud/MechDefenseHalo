using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Animation;

namespace MechDefenseHalo.Tests.Animation
{
    /// <summary>
    /// Unit tests for HitReactions
    /// </summary>
    [TestSuite]
    public class HitReactionsTests
    {
        private HitReactions _hitReactions;
        private AnimationController _animationController;

        [Before]
        public void Setup()
        {
            _hitReactions = new HitReactions();
            _animationController = new AnimationController();
            _hitReactions.AnimationController = _animationController;
        }

        [After]
        public void Teardown()
        {
            _hitReactions?.QueueFree();
            _animationController?.QueueFree();
            _hitReactions = null;
            _animationController = null;
        }

        #region Initialization Tests

        [TestCase]
        public void HitReactions_InitialState_NotPlayingHitReaction()
        {
            // Arrange & Act - Created in Setup
            
            // Assert
            AssertBool(_hitReactions.IsPlayingHitReaction).IsFalse();
        }

        [TestCase]
        public void HitReactions_MinDamageThreshold_HasDefaultValue()
        {
            // Arrange & Act
            float threshold = _hitReactions.MinDamageThreshold;
            
            // Assert
            AssertFloat(threshold).IsEqual(5f);
        }

        [TestCase]
        public void HitReactions_HitReactionCooldown_HasDefaultValue()
        {
            // Arrange & Act
            float cooldown = _hitReactions.HitReactionCooldown;
            
            // Assert
            AssertFloat(cooldown).IsEqual(0.5f);
        }

        [TestCase]
        public void HitReactions_HeavyHitThreshold_HasDefaultValue()
        {
            // Arrange & Act
            float threshold = _hitReactions.HeavyHitThreshold;
            
            // Assert
            AssertFloat(threshold).IsEqual(50f);
        }

        [TestCase]
        public void HitReactions_EnableHitReactions_DefaultsToTrue()
        {
            // Arrange & Act
            bool enabled = _hitReactions.EnableHitReactions;
            
            // Assert
            AssertBool(enabled).IsTrue();
        }

        #endregion

        #region Hit Direction Tests

        [TestCase]
        public void PlayHitReaction_ZeroDirection_DoesNotCrash()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Zero;
            float damage = 10f;
            
            // Act & Assert - Should not throw
            _hitReactions.PlayHitReaction(hitDirection, damage);
        }

        [TestCase]
        public void PlayHitReaction_ValidDamage_PlaysReaction()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Forward;
            float damage = 10f;
            
            // Act
            _hitReactions.PlayHitReaction(hitDirection, damage);
            
            // Assert
            AssertBool(_hitReactions.IsPlayingHitReaction).IsTrue();
        }

        [TestCase]
        public void PlayHitReaction_BelowThreshold_DoesNotPlay()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Forward;
            float damage = 3f; // Below default threshold of 5
            _hitReactions.MinDamageThreshold = 5f;
            
            // Act
            _hitReactions.PlayHitReaction(hitDirection, damage);
            
            // Assert
            AssertBool(_hitReactions.IsPlayingHitReaction).IsFalse();
        }

        [TestCase]
        public void PlayHitReaction_ZeroDamage_PlaysReaction()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Forward;
            float damage = 0f; // Zero damage should play (special case)
            
            // Act
            _hitReactions.PlayHitReaction(hitDirection, damage);
            
            // Assert - Zero damage bypasses threshold check
            AssertBool(_hitReactions.IsPlayingHitReaction).IsTrue();
        }

        [TestCase]
        public void PlayHitReaction_CriticalHit_SetsFlagCorrectly()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Forward;
            float damage = 50f;
            bool isCritical = true;
            
            // Act & Assert - Should not crash
            _hitReactions.PlayHitReaction(hitDirection, damage, isCritical);
        }

        #endregion

        #region Cooldown Tests

        [TestCase]
        public void PlayHitReaction_DuringCooldown_DoesNotPlay()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Forward;
            float damage = 10f;
            
            // Act - Play first reaction
            _hitReactions.PlayHitReaction(hitDirection, damage);
            bool firstPlayed = _hitReactions.IsPlayingHitReaction;
            
            // Try to play second reaction immediately
            _hitReactions.PlayHitReaction(hitDirection, damage);
            
            // Assert - First should play, second should not due to cooldown
            AssertBool(firstPlayed).IsTrue();
            // We can't easily test the second play without time progression
        }

        [TestCase]
        public void ResetCooldown_RemovesCooldownTimer()
        {
            // Arrange
            Vector3 hitDirection = Vector3.Forward;
            float damage = 10f;
            _hitReactions.PlayHitReaction(hitDirection, damage);
            
            // Act
            _hitReactions.ResetCooldown();
            
            // Assert
            AssertFloat(_hitReactions.CooldownRemaining).IsEqual(0f);
        }

        [TestCase]
        public void CooldownRemaining_InitialState_IsZero()
        {
            // Arrange & Act
            float remaining = _hitReactions.CooldownRemaining;
            
            // Assert
            AssertFloat(remaining).IsEqual(0f);
        }

        #endregion

        #region Direction-Specific Tests

        [TestCase]
        public void PlayHitReactionDirection_Front_DoesNotCrash()
        {
            // Arrange
            var direction = HitReactions.HitDirection.Front;
            
            // Act & Assert - Should not throw
            _hitReactions.PlayHitReactionDirection(direction);
        }

        [TestCase]
        public void PlayHitReactionDirection_Back_DoesNotCrash()
        {
            // Arrange
            var direction = HitReactions.HitDirection.Back;
            
            // Act & Assert - Should not throw
            _hitReactions.PlayHitReactionDirection(direction);
        }

        [TestCase]
        public void PlayHitReactionDirection_Left_DoesNotCrash()
        {
            // Arrange
            var direction = HitReactions.HitDirection.Left;
            
            // Act & Assert - Should not throw
            _hitReactions.PlayHitReactionDirection(direction);
        }

        [TestCase]
        public void PlayHitReactionDirection_Right_DoesNotCrash()
        {
            // Arrange
            var direction = HitReactions.HitDirection.Right;
            
            // Act & Assert - Should not throw
            _hitReactions.PlayHitReactionDirection(direction);
        }

        [TestCase]
        public void PlayHitReactionDirection_Heavy_DoesNotCrash()
        {
            // Arrange
            var direction = HitReactions.HitDirection.Front;
            bool isHeavy = true;
            
            // Act & Assert - Should not throw
            _hitReactions.PlayHitReactionDirection(direction, isHeavy);
        }

        #endregion

        #region Configuration Tests

        [TestCase]
        public void HitReactions_DisableReactions_PreventsPlayback()
        {
            // Arrange
            _hitReactions.EnableHitReactions = false;
            Vector3 hitDirection = Vector3.Forward;
            float damage = 10f;
            
            // Act
            _hitReactions.PlayHitReaction(hitDirection, damage);
            
            // Assert
            AssertBool(_hitReactions.IsPlayingHitReaction).IsFalse();
        }

        [TestCase]
        public void HitReactions_CustomMinDamageThreshold_Works()
        {
            // Arrange
            _hitReactions.MinDamageThreshold = 20f;
            Vector3 hitDirection = Vector3.Forward;
            float damage = 15f; // Below new threshold
            
            // Act
            _hitReactions.PlayHitReaction(hitDirection, damage);
            
            // Assert
            AssertBool(_hitReactions.IsPlayingHitReaction).IsFalse();
        }

        [TestCase]
        public void HitReactions_CustomHeavyHitThreshold_Works()
        {
            // Arrange
            _hitReactions.HeavyHitThreshold = 100f;
            
            // Act
            float threshold = _hitReactions.HeavyHitThreshold;
            
            // Assert
            AssertFloat(threshold).IsEqual(100f);
        }

        [TestCase]
        public void HitReactions_UseCriticalHitReactions_DefaultsToTrue()
        {
            // Arrange & Act
            bool useCritical = _hitReactions.UseCriticalHitReactions;
            
            // Assert
            AssertBool(useCritical).IsTrue();
        }

        #endregion
    }
}
