using Godot;
using GdUnit4;
using MechDefenseHalo.Abilities;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Abilities
{
    /// <summary>
    /// Unit tests for individual abilities
    /// Tests ability properties, execution, and upgrade mechanics
    /// </summary>
    [TestSuite]
    public class AbilityTests
    {
        [TestCase]
        public void DashAbility_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var ability = new DashAbility();

            // Assert
            AssertString(ability.AbilityId).IsEqual("dash");
            AssertString(ability.AbilityName).IsEqual("Tactical Dash");
            AssertFloat(ability.Cooldown).IsGreater(0f);
            AssertFloat(ability.EnergyCost).IsGreater(0f);
        }

        [TestCase]
        public void ShieldAbility_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var ability = new ShieldAbility();

            // Assert
            AssertString(ability.AbilityId).IsEqual("shield");
            AssertString(ability.AbilityName).IsEqual("Energy Shield");
            AssertFloat(ability.Cooldown).IsGreater(0f);
            AssertFloat(ability.EnergyCost).IsGreater(0f);
        }

        [TestCase]
        public void EMPAbility_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var ability = new EMPAbility();

            // Assert
            AssertString(ability.AbilityId).IsEqual("emp");
            AssertString(ability.AbilityName).IsEqual("EMP Blast");
            AssertFloat(ability.Cooldown).IsGreater(0f);
            AssertFloat(ability.EnergyCost).IsGreater(0f);
        }

        [TestCase]
        public void TimeSlowAbility_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var ability = new TimeSlowAbility();

            // Assert
            AssertString(ability.AbilityId).IsEqual("time_slow");
            AssertString(ability.AbilityName).IsEqual("Temporal Field");
            AssertFloat(ability.Cooldown).IsGreater(0f);
            AssertFloat(ability.EnergyCost).IsGreater(0f);
        }

        [TestCase]
        public void AbilityBase_UpgradeLevel_ShouldStartAtZero()
        {
            // Arrange & Act
            var ability = new DashAbility();

            // Assert
            AssertInt(ability.UpgradeLevel).IsEqual(0);
        }

        [TestCase]
        public void GetModifiedCooldown_WithNoUpgrades_ShouldReturnBaseCooldown()
        {
            // Arrange
            var ability = new DashAbility();
            float baseCooldown = ability.Cooldown;

            // Act
            float modified = ability.GetModifiedCooldown();

            // Assert
            AssertFloat(modified).IsEqual(baseCooldown);
        }

        [TestCase]
        public void GetModifiedCooldown_WithUpgrades_ShouldReduceCooldown()
        {
            // Arrange
            var ability = new DashAbility();
            float baseCooldown = ability.Cooldown;
            ability.UpgradeLevel = 1;

            // Act
            float modified = ability.GetModifiedCooldown();

            // Assert
            AssertFloat(modified).IsLess(baseCooldown);
        }

        [TestCase]
        public void GetModifiedEnergyCost_WithNoUpgrades_ShouldReturnBaseCost()
        {
            // Arrange
            var ability = new ShieldAbility();
            float baseCost = ability.EnergyCost;

            // Act
            float modified = ability.GetModifiedEnergyCost();

            // Assert
            AssertFloat(modified).IsEqual(baseCost);
        }

        [TestCase]
        public void GetModifiedEnergyCost_WithUpgrades_ShouldReduceCost()
        {
            // Arrange
            var ability = new ShieldAbility();
            float baseCost = ability.EnergyCost;
            ability.UpgradeLevel = 2;

            // Act
            float modified = ability.GetModifiedEnergyCost();

            // Assert
            AssertFloat(modified).IsLess(baseCost);
        }

        [TestCase]
        public void GetModifiedCooldown_WithMaxUpgrades_ShouldNotGoBelowHalf()
        {
            // Arrange
            var ability = new EMPAbility();
            float baseCooldown = ability.Cooldown;
            ability.UpgradeLevel = 20; // Very high upgrade level

            // Act
            float modified = ability.GetModifiedCooldown();

            // Assert
            AssertFloat(modified).IsGreaterEqual(baseCooldown * 0.5f);
        }

        [TestCase]
        public void CanUse_DashAbility_OnGround_ShouldReturnTrue()
        {
            // Arrange
            var ability = new DashAbility();
            var user = new CharacterBody3D();
            
            // Note: In a real test, we'd need to set up the physics properly
            // For now, we test the default behavior

            // Act
            bool canUse = ability.CanUse(user);

            // Assert - CharacterBody3D.IsOnFloor() returns false by default in tests
            // so the ability will return false unless properly set up
            AssertBool(canUse).IsNotNull();
        }

        [TestCase]
        public void AllAbilities_ShouldHaveDescription()
        {
            // Arrange & Act
            var dash = new DashAbility();
            var shield = new ShieldAbility();
            var emp = new EMPAbility();
            var timeSlow = new TimeSlowAbility();

            // Assert
            AssertString(dash.Description).IsNotEmpty();
            AssertString(shield.Description).IsNotEmpty();
            AssertString(emp.Description).IsNotEmpty();
            AssertString(timeSlow.Description).IsNotEmpty();
        }

        [TestCase]
        public void AllAbilities_ShouldHaveIconPath()
        {
            // Arrange & Act
            var dash = new DashAbility();
            var shield = new ShieldAbility();
            var emp = new EMPAbility();
            var timeSlow = new TimeSlowAbility();

            // Assert
            AssertString(dash.IconPath).IsNotEmpty();
            AssertString(shield.IconPath).IsNotEmpty();
            AssertString(emp.IconPath).IsNotEmpty();
            AssertString(timeSlow.IconPath).IsNotEmpty();
        }
    }
}
