using Godot;
using GdUnit4;
using MechDefenseHalo.Abilities;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Abilities
{
    /// <summary>
    /// Unit tests for AbilitySystem
    /// Tests ability registration, usage, energy management, and upgrades
    /// </summary>
    [TestSuite]
    public class AbilitySystemTests
    {
        private AbilitySystem _abilitySystem;
        private Node3D _mockUser;
        private Node _testRoot;

        [Before]
        public void Setup()
        {
            _testRoot = new Node();
            _mockUser = new Node3D { Name = "MockPlayer" };
            _testRoot.AddChild(_mockUser);
            
            _abilitySystem = new AbilitySystem();
            _mockUser.AddChild(_abilitySystem);
            
            // Manually call _Ready since we're not in a running scene
            _abilitySystem._Ready();
        }

        [After]
        public void Teardown()
        {
            if (_testRoot != null)
            {
                _testRoot.QueueFree();
            }
            _abilitySystem = null;
            _mockUser = null;
        }

        [TestCase]
        public void AbilitySystem_ShouldRegisterFourAbilities()
        {
            // Act
            int count = _abilitySystem.GetAbilityCount();

            // Assert
            AssertInt(count).IsEqual(4);
        }

        [TestCase]
        public void GetAbility_WithValidIndex_ShouldReturnAbility()
        {
            // Act
            var ability = _abilitySystem.GetAbility(0);

            // Assert
            AssertObject(ability).IsNotNull();
            AssertString(ability.AbilityId).IsNotEmpty();
        }

        [TestCase]
        public void GetAbility_WithInvalidIndex_ShouldReturnNull()
        {
            // Act
            var ability = _abilitySystem.GetAbility(999);

            // Assert
            AssertObject(ability).IsNull();
        }

        [TestCase]
        public void GetAbilityById_WithValidId_ShouldReturnAbility()
        {
            // Act
            var ability = _abilitySystem.GetAbilityById("dash");

            // Assert
            AssertObject(ability).IsNotNull();
            AssertString(ability.AbilityName).IsEqual("Tactical Dash");
        }

        [TestCase]
        public void GetAbilityById_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var ability = _abilitySystem.GetAbilityById("nonexistent");

            // Assert
            AssertObject(ability).IsNull();
        }

        [TestCase]
        public void GetAbilityReady_InitialState_ShouldBeTrue()
        {
            // Act
            bool isReady = _abilitySystem.GetAbilityReady(0);

            // Assert
            AssertBool(isReady).IsTrue();
        }

        [TestCase]
        public void GetAbilityCooldownProgress_NoCooldown_ShouldReturnOne()
        {
            // Act
            float progress = _abilitySystem.GetAbilityCooldownProgress(0);

            // Assert
            AssertFloat(progress).IsEqual(1f);
        }

        [TestCase]
        public void GetAbilityRemainingCooldown_NoCooldown_ShouldReturnZero()
        {
            // Act
            float remaining = _abilitySystem.GetAbilityRemainingCooldown(0);

            // Assert
            AssertFloat(remaining).IsEqual(0f);
        }

        [TestCase]
        public void UpgradeAbility_ShouldIncreaseLevel()
        {
            // Arrange
            var ability = _abilitySystem.GetAbility(0);
            int initialLevel = ability.UpgradeLevel;

            // Act
            bool success = _abilitySystem.UpgradeAbility(0);

            // Assert
            AssertBool(success).IsTrue();
            AssertInt(ability.UpgradeLevel).IsEqual(initialLevel + 1);
        }

        [TestCase]
        public void UpgradeAbility_WithInvalidIndex_ShouldReturnFalse()
        {
            // Act
            bool success = _abilitySystem.UpgradeAbility(999);

            // Assert
            AssertBool(success).IsFalse();
        }

        [TestCase]
        public void UpgradeAbilityById_WithValidId_ShouldIncreaseLevel()
        {
            // Arrange
            var ability = _abilitySystem.GetAbilityById("shield");
            int initialLevel = ability.UpgradeLevel;

            // Act
            bool success = _abilitySystem.UpgradeAbilityById("shield");

            // Assert
            AssertBool(success).IsTrue();
            AssertInt(ability.UpgradeLevel).IsEqual(initialLevel + 1);
        }

        [TestCase]
        public void GetAllAbilities_ShouldReturnAllRegisteredAbilities()
        {
            // Act
            var abilities = _abilitySystem.GetAllAbilities();

            // Assert
            AssertInt(abilities.Count).IsEqual(4);
            AssertThat(abilities).ContainsExactly(
                _abilitySystem.GetAbility(0),
                _abilitySystem.GetAbility(1),
                _abilitySystem.GetAbility(2),
                _abilitySystem.GetAbility(3)
            );
        }

        [TestCase]
        public void AllAbilities_ShouldHaveUniqueIds()
        {
            // Act
            var abilities = _abilitySystem.GetAllAbilities();
            var ids = new System.Collections.Generic.List<string>();
            
            foreach (var ability in abilities)
            {
                ids.Add(ability.AbilityId);
            }

            // Assert
            var uniqueIds = new System.Collections.Generic.HashSet<string>(ids);
            AssertInt(uniqueIds.Count).IsEqual(abilities.Count);
        }
    }
}
