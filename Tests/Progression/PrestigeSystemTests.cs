using Godot;
using GdUnit4;
using MechDefenseHalo.Progression;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Progression
{
    /// <summary>
    /// Unit tests for PrestigeSystem
    /// Tests prestige mechanics, bonuses, and level resets
    /// </summary>
    [TestSuite]
    public class PrestigeSystemTests
    {
        private PrestigeSystem _prestigeSystem;
        private PlayerLevel _playerLevel;

        [Before]
        public void Setup()
        {
            _playerLevel = new PlayerLevel();
            _prestigeSystem = new PrestigeSystem();
            
            // Reset to level 1
            PlayerLevel.SetLevel(1, 0);
            PrestigeSystem.SetPrestigeLevel(0);
        }

        [After]
        public void Teardown()
        {
            _prestigeSystem = null;
            _playerLevel = null;
        }

        [TestCase]
        public void InitialPrestigeLevel_ShouldBe0()
        {
            // Assert
            AssertInt(_prestigeSystem.PrestigeLevel).IsEqual(0);
        }

        [TestCase]
        public void InitialStatBonus_ShouldBe0()
        {
            // Assert
            AssertFloat(_prestigeSystem.TotalStatBonus).IsEqual(0f);
        }

        [TestCase]
        public void CanPrestige_BelowLevel100_ShouldBeFalse()
        {
            // Arrange
            PlayerLevel.SetLevel(50, 0);

            // Act & Assert
            AssertBool(_prestigeSystem.CanPrestige).IsFalse();
        }

        [TestCase]
        public void CanPrestige_AtLevel100_ShouldBeTrue()
        {
            // Arrange
            PlayerLevel.SetLevel(100, 0);

            // Act & Assert
            AssertBool(_prestigeSystem.CanPrestige).IsTrue();
        }

        [TestCase]
        public void Prestige_BelowLevel100_ShouldReturnFalse()
        {
            // Arrange
            PlayerLevel.SetLevel(50, 0);

            // Act
            bool result = PrestigeSystem.Prestige();

            // Assert
            AssertBool(result).IsFalse();
            AssertInt(_prestigeSystem.PrestigeLevel).IsEqual(0);
        }

        [TestCase]
        public void Prestige_AtLevel100_ShouldIncreasePrestigeLevel()
        {
            // Arrange
            PlayerLevel.SetLevel(100, 0);

            // Act
            bool result = PrestigeSystem.Prestige();

            // Assert
            AssertBool(result).IsTrue();
            AssertInt(_prestigeSystem.PrestigeLevel).IsEqual(1);
        }

        [TestCase]
        public void Prestige_AtLevel100_ShouldResetToLevel1()
        {
            // Arrange
            PlayerLevel.SetLevel(100, 0);

            // Act
            PrestigeSystem.Prestige();

            // Assert
            AssertInt(_playerLevel.CurrentLevel).IsEqual(1);
            AssertInt(_playerLevel.CurrentXP).IsEqual(0);
        }

        [TestCase]
        public void Prestige_First_ShouldGive5PercentBonus()
        {
            // Arrange
            PlayerLevel.SetLevel(100, 0);

            // Act
            PrestigeSystem.Prestige();

            // Assert
            AssertFloat(_prestigeSystem.TotalStatBonus).IsEqual(0.05f);
            AssertInt(PrestigeSystem.GetBonusPercentage()).IsEqual(5);
        }

        [TestCase]
        public void Prestige_Multiple_ShouldStackBonuses()
        {
            // Arrange
            PlayerLevel.SetLevel(100, 0);

            // Act
            PrestigeSystem.Prestige();
            PlayerLevel.SetLevel(100, 0);
            PrestigeSystem.Prestige();
            PlayerLevel.SetLevel(100, 0);
            PrestigeSystem.Prestige();

            // Assert
            AssertInt(_prestigeSystem.PrestigeLevel).IsEqual(3);
            AssertFloat(_prestigeSystem.TotalStatBonus).IsEqual(0.15f);
            AssertInt(PrestigeSystem.GetBonusPercentage()).IsEqual(15);
        }

        [TestCase]
        public void Prestige_AtMax_ShouldNotAllowMore()
        {
            // Arrange
            PrestigeSystem.SetPrestigeLevel(10);
            PlayerLevel.SetLevel(100, 0);

            // Act
            bool result = PrestigeSystem.Prestige();

            // Assert
            AssertBool(result).IsFalse();
            AssertInt(_prestigeSystem.PrestigeLevel).IsEqual(10);
        }

        [TestCase]
        public void GetStatMultiplier_NoPrestige_ShouldBe1()
        {
            // Act & Assert
            AssertFloat(PrestigeSystem.GetStatMultiplier()).IsEqual(1.0f);
        }

        [TestCase]
        public void GetStatMultiplier_OnePrestige_ShouldBe1Point05()
        {
            // Arrange
            PrestigeSystem.SetPrestigeLevel(1);

            // Act & Assert
            AssertFloat(PrestigeSystem.GetStatMultiplier()).IsEqual(1.05f);
        }

        [TestCase]
        public void GetStatMultiplier_MaxPrestige_ShouldBe1Point5()
        {
            // Arrange
            PrestigeSystem.SetPrestigeLevel(10);

            // Act & Assert
            AssertFloat(PrestigeSystem.GetStatMultiplier()).IsEqual(1.5f);
        }

        [TestCase]
        public void GetBonusPercentage_NoPrestige_ShouldBe0()
        {
            // Act & Assert
            AssertInt(PrestigeSystem.GetBonusPercentage()).IsEqual(0);
        }

        [TestCase]
        public void GetBonusPercentage_MaxPrestige_ShouldBe50()
        {
            // Arrange
            PrestigeSystem.SetPrestigeLevel(10);

            // Act & Assert
            AssertInt(PrestigeSystem.GetBonusPercentage()).IsEqual(50);
        }

        [TestCase]
        public void GetRemainingPrestiges_NoPrestige_ShouldBe10()
        {
            // Act & Assert
            AssertInt(PrestigeSystem.GetRemainingPrestiges()).IsEqual(10);
        }

        [TestCase]
        public void GetRemainingPrestiges_FivePrestiges_ShouldBe5()
        {
            // Arrange
            PrestigeSystem.SetPrestigeLevel(5);

            // Act & Assert
            AssertInt(PrestigeSystem.GetRemainingPrestiges()).IsEqual(5);
        }

        [TestCase]
        public void GetRemainingPrestiges_MaxPrestige_ShouldBe0()
        {
            // Arrange
            PrestigeSystem.SetPrestigeLevel(10);

            // Act & Assert
            AssertInt(PrestigeSystem.GetRemainingPrestiges()).IsEqual(0);
        }

        [TestCase]
        public void SetPrestigeLevel_ValidLevel_ShouldUpdate()
        {
            // Act
            PrestigeSystem.SetPrestigeLevel(5);

            // Assert
            AssertInt(_prestigeSystem.PrestigeLevel).IsEqual(5);
        }

        [TestCase]
        public void SetPrestigeLevel_Above10_ShouldClampTo10()
        {
            // Act
            PrestigeSystem.SetPrestigeLevel(15);

            // Assert
            AssertInt(_prestigeSystem.PrestigeLevel).IsEqual(10);
        }

        [TestCase]
        public void SetPrestigeLevel_Negative_ShouldClampTo0()
        {
            // Act
            PrestigeSystem.SetPrestigeLevel(-5);

            // Assert
            AssertInt(_prestigeSystem.PrestigeLevel).IsEqual(0);
        }
    }
}
