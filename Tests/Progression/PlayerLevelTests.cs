using Godot;
using GdUnit4;
using MechDefenseHalo.Progression;
using MechDefenseHalo.Economy;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Progression
{
    /// <summary>
    /// Unit tests for PlayerLevel system
    /// Tests XP gain, level progression, and level-up mechanics
    /// </summary>
    [TestSuite]
    public class PlayerLevelTests
    {
        private PlayerLevel _playerLevel;

        [Before]
        public void Setup()
        {
            _playerLevel = new PlayerLevel();
            // Initialize currency manager for reward tests
            var currencyManager = new CurrencyManager();
            CurrencyManager.SetCredits(0);
            CurrencyManager.SetCores(0);
            
            // Set to level 1 with 0 XP
            PlayerLevel.SetLevel(1, 0);
        }

        [After]
        public void Teardown()
        {
            _playerLevel = null;
            CurrencyManager.ResetCurrencies();
        }

        [TestCase]
        public void InitialLevel_ShouldBe1()
        {
            // Assert
            AssertInt(_playerLevel.CurrentLevel).IsEqual(1);
        }

        [TestCase]
        public void InitialXP_ShouldBe0()
        {
            // Assert
            AssertInt(_playerLevel.CurrentXP).IsEqual(0);
        }

        [TestCase]
        public void AddXP_SmallAmount_ShouldNotLevelUp()
        {
            // Act
            PlayerLevel.AddXP(50, "test");

            // Assert
            AssertInt(_playerLevel.CurrentLevel).IsEqual(1);
            AssertInt(_playerLevel.CurrentXP).IsEqual(50);
        }

        [TestCase]
        public void AddXP_ExactAmount_ShouldLevelUp()
        {
            // Act
            PlayerLevel.AddXP(100, "test");

            // Assert
            AssertInt(_playerLevel.CurrentLevel).IsEqual(2);
            AssertInt(_playerLevel.CurrentXP).IsEqual(0);
        }

        [TestCase]
        public void AddXP_ExcessAmount_ShouldCarryOverXP()
        {
            // Act
            PlayerLevel.AddXP(150, "test");

            // Assert
            AssertInt(_playerLevel.CurrentLevel).IsEqual(2);
            AssertInt(_playerLevel.CurrentXP).IsEqual(50);
        }

        [TestCase]
        public void AddXP_MultipleLevels_ShouldLevelUpMultipleTimes()
        {
            // Act - Add enough XP for 3 levels (100 + 100 + 100 = 300)
            PlayerLevel.AddXP(300, "test");

            // Assert - Should be at level 4
            AssertInt(_playerLevel.CurrentLevel).IsEqual(4);
        }

        [TestCase]
        public void AddXP_ZeroAmount_ShouldNotChange()
        {
            // Act
            PlayerLevel.AddXP(0, "test");

            // Assert
            AssertInt(_playerLevel.CurrentLevel).IsEqual(1);
            AssertInt(_playerLevel.CurrentXP).IsEqual(0);
        }

        [TestCase]
        public void AddXP_NegativeAmount_ShouldNotChange()
        {
            // Arrange
            PlayerLevel.AddXP(50, "setup");

            // Act
            PlayerLevel.AddXP(-25, "test");

            // Assert
            AssertInt(_playerLevel.CurrentXP).IsEqual(50);
        }

        [TestCase]
        public void SetLevel_ValidLevel_ShouldUpdateLevel()
        {
            // Act
            PlayerLevel.SetLevel(5, 50);

            // Assert
            AssertInt(_playerLevel.CurrentLevel).IsEqual(5);
            AssertInt(_playerLevel.CurrentXP).IsEqual(50);
        }

        [TestCase]
        public void SetLevel_Above100_ShouldClampTo100()
        {
            // Act
            PlayerLevel.SetLevel(150, 0);

            // Assert
            AssertInt(_playerLevel.CurrentLevel).IsEqual(100);
        }

        [TestCase]
        public void SetLevel_BelowZero_ShouldClampTo1()
        {
            // Act
            PlayerLevel.SetLevel(0, 0);

            // Assert
            AssertInt(_playerLevel.CurrentLevel).IsEqual(1);
        }

        [TestCase]
        public void GetProgressToNextLevel_NoXP_ShouldBeZero()
        {
            // Act
            float progress = PlayerLevel.GetProgressToNextLevel();

            // Assert
            AssertFloat(progress).IsEqual(0f);
        }

        [TestCase]
        public void GetProgressToNextLevel_HalfXP_ShouldBeHalf()
        {
            // Act
            PlayerLevel.AddXP(50, "test");
            float progress = PlayerLevel.GetProgressToNextLevel();

            // Assert
            AssertFloat(progress).IsEqual(0.5f);
        }

        [TestCase]
        public void GetProgressToNextLevel_Level100_ShouldBe1()
        {
            // Act
            PlayerLevel.SetLevel(100, 0);
            float progress = PlayerLevel.GetProgressToNextLevel();

            // Assert
            AssertFloat(progress).IsEqual(1f);
        }

        [TestCase]
        public void CanPrestige_BelowLevel100_ShouldBeFalse()
        {
            // Arrange
            PlayerLevel.SetLevel(50, 0);

            // Act & Assert
            AssertBool(PlayerLevel.CanPrestige()).IsFalse();
        }

        [TestCase]
        public void CanPrestige_AtLevel100_ShouldBeTrue()
        {
            // Arrange
            PlayerLevel.SetLevel(100, 0);

            // Act & Assert
            AssertBool(PlayerLevel.CanPrestige()).IsTrue();
        }

        [TestCase]
        public void XPToNextLevel_Level1_ShouldBe100()
        {
            // Assert
            AssertInt(_playerLevel.XPToNextLevel).IsEqual(100);
        }

        [TestCase]
        public void TotalXP_Level1With50XP_ShouldBe50()
        {
            // Arrange
            PlayerLevel.AddXP(50, "test");

            // Act & Assert
            AssertInt(_playerLevel.TotalXP).IsEqual(50);
        }

        [TestCase]
        public void TotalXP_Level2With0XP_ShouldBe100()
        {
            // Arrange
            PlayerLevel.AddXP(100, "test");

            // Act & Assert
            AssertInt(_playerLevel.TotalXP).IsEqual(100);
        }
    }
}
