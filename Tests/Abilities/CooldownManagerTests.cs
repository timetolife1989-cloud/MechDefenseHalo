using Godot;
using GdUnit4;
using MechDefenseHalo.Abilities;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Abilities
{
    /// <summary>
    /// Unit tests for CooldownManager
    /// Tests cooldown tracking, progress calculation, and reset functionality
    /// </summary>
    [TestSuite]
    public class CooldownManagerTests
    {
        private CooldownManager _cooldownManager;
        private SceneTree _sceneTree;
        private Node _testRoot;

        [Before]
        public void Setup()
        {
            _testRoot = new Node();
            _cooldownManager = new CooldownManager();
            _testRoot.AddChild(_cooldownManager);
            
            // Note: For testing, we need to manually process the cooldown manager
            // since we're not in a running scene tree
        }

        [After]
        public void Teardown()
        {
            if (_testRoot != null)
            {
                _testRoot.QueueFree();
            }
            _cooldownManager = null;
        }

        [TestCase]
        public void StartCooldown_ShouldPutAbilityOnCooldown()
        {
            // Arrange
            string abilityId = "test_ability";
            float duration = 5.0f;

            // Act
            _cooldownManager.StartCooldown(abilityId, duration);

            // Assert
            AssertBool(_cooldownManager.IsOnCooldown(abilityId)).IsTrue();
        }

        [TestCase]
        public void IsOnCooldown_WithNoActiveCooldown_ShouldReturnFalse()
        {
            // Arrange
            string abilityId = "test_ability";

            // Act & Assert
            AssertBool(_cooldownManager.IsOnCooldown(abilityId)).IsFalse();
        }

        [TestCase]
        public void GetRemainingCooldown_ShouldReturnCorrectValue()
        {
            // Arrange
            string abilityId = "test_ability";
            float duration = 10.0f;

            // Act
            _cooldownManager.StartCooldown(abilityId, duration);
            float remaining = _cooldownManager.GetRemainingCooldown(abilityId);

            // Assert
            AssertFloat(remaining).IsGreaterEqual(9.9f); // Should be close to 10
            AssertFloat(remaining).IsLessEqual(10.0f);
        }

        [TestCase]
        public void GetRemainingCooldown_WithNoCooldown_ShouldReturnZero()
        {
            // Arrange
            string abilityId = "test_ability";

            // Act
            float remaining = _cooldownManager.GetRemainingCooldown(abilityId);

            // Assert
            AssertFloat(remaining).IsEqual(0f);
        }

        [TestCase]
        public void GetCooldownProgress_InitialStart_ShouldBeZero()
        {
            // Arrange
            string abilityId = "test_ability";
            float duration = 10.0f;

            // Act
            _cooldownManager.StartCooldown(abilityId, duration);
            float progress = _cooldownManager.GetCooldownProgress(abilityId);

            // Assert
            AssertFloat(progress).IsLessEqual(0.1f); // Should be very close to 0
        }

        [TestCase]
        public void GetCooldownProgress_NoCooldown_ShouldReturnOne()
        {
            // Arrange
            string abilityId = "test_ability";

            // Act
            float progress = _cooldownManager.GetCooldownProgress(abilityId);

            // Assert
            AssertFloat(progress).IsEqual(1f);
        }

        [TestCase]
        public void ResetCooldown_ShouldMakeAbilityAvailable()
        {
            // Arrange
            string abilityId = "test_ability";
            _cooldownManager.StartCooldown(abilityId, 10.0f);

            // Act
            _cooldownManager.ResetCooldown(abilityId);

            // Assert
            AssertBool(_cooldownManager.IsOnCooldown(abilityId)).IsFalse();
        }

        [TestCase]
        public void ResetAllCooldowns_ShouldClearAllCooldowns()
        {
            // Arrange
            _cooldownManager.StartCooldown("ability1", 5.0f);
            _cooldownManager.StartCooldown("ability2", 10.0f);
            _cooldownManager.StartCooldown("ability3", 15.0f);

            // Act
            _cooldownManager.ResetAllCooldowns();

            // Assert
            AssertBool(_cooldownManager.IsOnCooldown("ability1")).IsFalse();
            AssertBool(_cooldownManager.IsOnCooldown("ability2")).IsFalse();
            AssertBool(_cooldownManager.IsOnCooldown("ability3")).IsFalse();
        }

        [TestCase]
        public void MultipleCooldowns_ShouldBeTrackedIndependently()
        {
            // Arrange
            string ability1 = "dash";
            string ability2 = "shield";
            
            // Act
            _cooldownManager.StartCooldown(ability1, 5.0f);
            _cooldownManager.StartCooldown(ability2, 10.0f);

            // Assert
            AssertBool(_cooldownManager.IsOnCooldown(ability1)).IsTrue();
            AssertBool(_cooldownManager.IsOnCooldown(ability2)).IsTrue();
            
            float remaining1 = _cooldownManager.GetRemainingCooldown(ability1);
            float remaining2 = _cooldownManager.GetRemainingCooldown(ability2);
            
            AssertFloat(remaining1).IsLess(remaining2);
        }
    }
}
