using Godot;
using GdUnit4;
using MechDefenseHalo.Economy;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Economy
{
    /// <summary>
    /// Unit tests for CurrencyManager system
    /// Tests currency operations like add, spend, and balance checks
    /// </summary>
    [TestSuite]
    public class CurrencyManagerTests
    {
        private CurrencyManager _currencyManager;

        [Before]
        public void Setup()
        {
            _currencyManager = new CurrencyManager();
            // Initialize as singleton instance
            CurrencyManager.SetCredits(0);
            CurrencyManager.SetCores(0);
        }

        [After]
        public void Teardown()
        {
            CurrencyManager.ResetCurrencies();
            _currencyManager = null;
        }

        [TestCase]
        public void InitialCredits_ShouldBeZero()
        {
            // Assert
            AssertInt(_currencyManager.Credits).IsEqual(0);
        }

        [TestCase]
        public void InitialCores_ShouldBeZero()
        {
            // Assert
            AssertInt(_currencyManager.Cores).IsEqual(0);
        }

        [TestCase]
        public void AddCredits_PositiveAmount_ShouldIncreaseBalance()
        {
            // Act
            CurrencyManager.AddCredits(100, "test");

            // Assert
            AssertInt(_currencyManager.Credits).IsEqual(100);
        }

        [TestCase]
        public void AddCredits_ZeroAmount_ShouldReturnFalse()
        {
            // Act
            var result = CurrencyManager.AddCredits(0, "test");

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void AddCredits_NegativeAmount_ShouldReturnFalse()
        {
            // Act
            var result = CurrencyManager.AddCredits(-50, "test");

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void AddCredits_Multiple_ShouldAccumulate()
        {
            // Act
            CurrencyManager.AddCredits(50, "test1");
            CurrencyManager.AddCredits(75, "test2");
            CurrencyManager.AddCredits(25, "test3");

            // Assert
            AssertInt(_currencyManager.Credits).IsEqual(150);
        }

        [TestCase]
        public void SpendCredits_SufficientBalance_ShouldReturnTrue()
        {
            // Arrange
            CurrencyManager.AddCredits(100, "test");

            // Act
            var result = CurrencyManager.SpendCredits(50, "purchase");

            // Assert
            AssertBool(result).IsTrue();
            AssertInt(_currencyManager.Credits).IsEqual(50);
        }

        [TestCase]
        public void SpendCredits_InsufficientBalance_ShouldReturnFalse()
        {
            // Arrange
            CurrencyManager.AddCredits(50, "test");

            // Act
            var result = CurrencyManager.SpendCredits(100, "purchase");

            // Assert
            AssertBool(result).IsFalse();
            AssertInt(_currencyManager.Credits).IsEqual(50);
        }

        [TestCase]
        public void SpendCredits_ExactBalance_ShouldSucceed()
        {
            // Arrange
            CurrencyManager.AddCredits(100, "test");

            // Act
            var result = CurrencyManager.SpendCredits(100, "purchase");

            // Assert
            AssertBool(result).IsTrue();
            AssertInt(_currencyManager.Credits).IsEqual(0);
        }

        [TestCase]
        public void SpendCredits_ZeroAmount_ShouldReturnFalse()
        {
            // Arrange
            CurrencyManager.AddCredits(100, "test");

            // Act
            var result = CurrencyManager.SpendCredits(0, "purchase");

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void HasCredits_SufficientBalance_ShouldReturnTrue()
        {
            // Arrange
            CurrencyManager.AddCredits(100, "test");

            // Act & Assert
            AssertBool(CurrencyManager.HasCredits(50)).IsTrue();
        }

        [TestCase]
        public void HasCredits_InsufficientBalance_ShouldReturnFalse()
        {
            // Arrange
            CurrencyManager.AddCredits(50, "test");

            // Act & Assert
            AssertBool(CurrencyManager.HasCredits(100)).IsFalse();
        }

        [TestCase]
        public void AddCores_PositiveAmount_ShouldIncreaseBalance()
        {
            // Act
            CurrencyManager.AddCores(10, "test");

            // Assert
            AssertInt(_currencyManager.Cores).IsEqual(10);
        }

        [TestCase]
        public void AddCores_ZeroAmount_ShouldReturnFalse()
        {
            // Act
            var result = CurrencyManager.AddCores(0, "test");

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void AddCores_NegativeAmount_ShouldReturnFalse()
        {
            // Act
            var result = CurrencyManager.AddCores(-5, "test");

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void SpendCores_SufficientBalance_ShouldReturnTrue()
        {
            // Arrange
            CurrencyManager.AddCores(20, "test");

            // Act
            var result = CurrencyManager.SpendCores(10, "purchase");

            // Assert
            AssertBool(result).IsTrue();
            AssertInt(_currencyManager.Cores).IsEqual(10);
        }

        [TestCase]
        public void SpendCores_InsufficientBalance_ShouldReturnFalse()
        {
            // Arrange
            CurrencyManager.AddCores(5, "test");

            // Act
            var result = CurrencyManager.SpendCores(10, "purchase");

            // Assert
            AssertBool(result).IsFalse();
            AssertInt(_currencyManager.Cores).IsEqual(5);
        }

        [TestCase]
        public void HasCores_SufficientBalance_ShouldReturnTrue()
        {
            // Arrange
            CurrencyManager.AddCores(20, "test");

            // Act & Assert
            AssertBool(CurrencyManager.HasCores(10)).IsTrue();
        }

        [TestCase]
        public void HasCores_InsufficientBalance_ShouldReturnFalse()
        {
            // Arrange
            CurrencyManager.AddCores(5, "test");

            // Act & Assert
            AssertBool(CurrencyManager.HasCores(10)).IsFalse();
        }

        [TestCase]
        public void SetCredits_ShouldUpdateBalance()
        {
            // Act
            CurrencyManager.SetCredits(500);

            // Assert
            AssertInt(_currencyManager.Credits).IsEqual(500);
        }

        [TestCase]
        public void SetCredits_NegativeValue_ShouldSetToZero()
        {
            // Act
            CurrencyManager.SetCredits(-100);

            // Assert
            AssertInt(_currencyManager.Credits).IsEqual(0);
        }

        [TestCase]
        public void SetCores_ShouldUpdateBalance()
        {
            // Act
            CurrencyManager.SetCores(50);

            // Assert
            AssertInt(_currencyManager.Cores).IsEqual(50);
        }

        [TestCase]
        public void SetCores_NegativeValue_ShouldSetToZero()
        {
            // Act
            CurrencyManager.SetCores(-10);

            // Assert
            AssertInt(_currencyManager.Cores).IsEqual(0);
        }

        [TestCase]
        public void ResetCurrencies_ShouldSetBothToZero()
        {
            // Arrange
            CurrencyManager.AddCredits(100, "test");
            CurrencyManager.AddCores(20, "test");

            // Act
            CurrencyManager.ResetCurrencies();

            // Assert
            AssertInt(_currencyManager.Credits).IsEqual(0);
            AssertInt(_currencyManager.Cores).IsEqual(0);
        }

        [TestCase]
        public void MixedOperations_CreditsAndCores_ShouldNotInterfere()
        {
            // Act
            CurrencyManager.AddCredits(100, "test");
            CurrencyManager.AddCores(10, "test");
            CurrencyManager.SpendCredits(30, "purchase");
            CurrencyManager.SpendCores(5, "purchase");

            // Assert
            AssertInt(_currencyManager.Credits).IsEqual(70);
            AssertInt(_currencyManager.Cores).IsEqual(5);
        }
    }
}
