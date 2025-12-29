using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.WaveSystem;

namespace MechDefenseHalo.Tests.WaveSystem
{
    /// <summary>
    /// Unit tests for DifficultyScaler
    /// </summary>
    [TestSuite]
    public class DifficultyScalerTests
    {
        #region HP Scaling Tests

        [TestCase]
        public void ScaleEnemyHP_Wave1_ReturnsBaseHPPlus50()
        {
            // Arrange
            int baseHP = 100;
            int waveNumber = 1;

            // Act
            int result = DifficultyScaler.ScaleEnemyHP(baseHP, waveNumber);

            // Assert
            AssertInt(result).IsEqual(150); // 100 + (1 * 50)
        }

        [TestCase]
        public void ScaleEnemyHP_Wave10_ReturnsBaseHPPlus500()
        {
            // Arrange
            int baseHP = 100;
            int waveNumber = 10;

            // Act
            int result = DifficultyScaler.ScaleEnemyHP(baseHP, waveNumber);

            // Assert
            AssertInt(result).IsEqual(600); // 100 + (10 * 50)
        }

        [TestCase]
        public void ScaleEnemyHP_Wave11_UsesExponentialScaling()
        {
            // Arrange
            int baseHP = 100;
            int waveNumber = 11;

            // Act
            int result = DifficultyScaler.ScaleEnemyHP(baseHP, waveNumber);

            // Assert - Should be 100 * (1 + (11-10) * 0.15) = 115
            AssertInt(result).IsEqual(115);
        }

        [TestCase]
        public void ScaleEnemyHP_Wave20_UsesExponentialScaling()
        {
            // Arrange
            int baseHP = 100;
            int waveNumber = 20;

            // Act
            int result = DifficultyScaler.ScaleEnemyHP(baseHP, waveNumber);

            // Assert - Should be 100 * (1 + (20-10) * 0.15) = 250
            AssertInt(result).IsEqual(250);
        }

        [TestCase]
        public void ScaleEnemyHP_Wave0_ReturnsBaseHP()
        {
            // Arrange
            int baseHP = 100;
            int waveNumber = 0;

            // Act
            int result = DifficultyScaler.ScaleEnemyHP(baseHP, waveNumber);

            // Assert
            AssertInt(result).IsEqual(100);
        }

        [TestCase]
        public void ScaleEnemyHP_NegativeWave_ReturnsBaseHP()
        {
            // Arrange
            int baseHP = 100;
            int waveNumber = -5;

            // Act
            int result = DifficultyScaler.ScaleEnemyHP(baseHP, waveNumber);

            // Assert
            AssertInt(result).IsEqual(100);
        }

        #endregion

        #region Damage Scaling Tests

        [TestCase]
        public void ScaleEnemyDamage_Wave1_ReturnsBaseDamage()
        {
            // Arrange
            int baseDamage = 10;
            int waveNumber = 1;

            // Act
            int result = DifficultyScaler.ScaleEnemyDamage(baseDamage, waveNumber);

            // Assert
            AssertInt(result).IsEqual(10);
        }

        [TestCase]
        public void ScaleEnemyDamage_Wave10_ReturnsBaseDamage()
        {
            // Arrange
            int baseDamage = 10;
            int waveNumber = 10;

            // Act
            int result = DifficultyScaler.ScaleEnemyDamage(baseDamage, waveNumber);

            // Assert
            AssertInt(result).IsEqual(10);
        }

        [TestCase]
        public void ScaleEnemyDamage_Wave11_AppliesScaling()
        {
            // Arrange
            int baseDamage = 10;
            int waveNumber = 11;

            // Act
            int result = DifficultyScaler.ScaleEnemyDamage(baseDamage, waveNumber);

            // Assert - Should be 10 * (1 + (11-10) * 0.10) = 11
            AssertInt(result).IsEqual(11);
        }

        [TestCase]
        public void ScaleEnemyDamage_Wave20_AppliesScaling()
        {
            // Arrange
            int baseDamage = 10;
            int waveNumber = 20;

            // Act
            int result = DifficultyScaler.ScaleEnemyDamage(baseDamage, waveNumber);

            // Assert - Should be 10 * (1 + (20-10) * 0.10) = 20
            AssertInt(result).IsEqual(20);
        }

        #endregion

        #region Count Scaling Tests

        [TestCase]
        public void ScaleEnemyCount_Wave1_ReturnsBaseCount()
        {
            // Arrange
            int baseCount = 10;
            int waveNumber = 1;

            // Act
            int result = DifficultyScaler.ScaleEnemyCount(baseCount, waveNumber);

            // Assert - Wave 1 is < 5, so no scaling
            AssertInt(result).IsEqual(10);
        }

        [TestCase]
        public void ScaleEnemyCount_Wave5_AppliesScaling()
        {
            // Arrange
            int baseCount = 10;
            int waveNumber = 5;

            // Act
            int result = DifficultyScaler.ScaleEnemyCount(baseCount, waveNumber);

            // Assert - Floor(5/5) = 1, multiplier = 1.1, result = 11
            AssertInt(result).IsEqual(11);
        }

        [TestCase]
        public void ScaleEnemyCount_Wave10_AppliesScaling()
        {
            // Arrange
            int baseCount = 10;
            int waveNumber = 10;

            // Act
            int result = DifficultyScaler.ScaleEnemyCount(baseCount, waveNumber);

            // Assert - Floor(10/5) = 2, multiplier = 1.2, result = 12
            AssertInt(result).IsEqual(12);
        }

        #endregion

        #region Reward Calculation Tests

        [TestCase]
        public void CalculateCreditsReward_Wave1_Returns50()
        {
            // Arrange & Act
            int result = DifficultyScaler.CalculateCreditsReward(1);

            // Assert
            AssertInt(result).IsEqual(50);
        }

        [TestCase]
        public void CalculateCreditsReward_Wave10_Returns500()
        {
            // Arrange & Act
            int result = DifficultyScaler.CalculateCreditsReward(10);

            // Assert
            AssertInt(result).IsEqual(500);
        }

        [TestCase]
        public void CalculateXPReward_Wave1_Returns100()
        {
            // Arrange & Act
            int result = DifficultyScaler.CalculateXPReward(1);

            // Assert
            AssertInt(result).IsEqual(100);
        }

        [TestCase]
        public void CalculateXPReward_Wave10_Returns1000()
        {
            // Arrange & Act
            int result = DifficultyScaler.CalculateXPReward(10);

            // Assert
            AssertInt(result).IsEqual(1000);
        }

        #endregion

        #region Elite Wave Tests

        [TestCase]
        public void IsEliteWave_Wave30_ReturnsFalse()
        {
            // Arrange & Act
            bool result = DifficultyScaler.IsEliteWave(30);

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void IsEliteWave_Wave31_ReturnsTrue()
        {
            // Arrange & Act
            bool result = DifficultyScaler.IsEliteWave(31);

            // Assert
            AssertBool(result).IsTrue();
        }

        [TestCase]
        public void IsEliteWave_Wave50_ReturnsTrue()
        {
            // Arrange & Act
            bool result = DifficultyScaler.IsEliteWave(50);

            // Assert
            AssertBool(result).IsTrue();
        }

        [TestCase]
        public void GetEliteHPMultiplier_Wave30_Returns1()
        {
            // Arrange & Act
            float result = DifficultyScaler.GetEliteHPMultiplier(30);

            // Assert
            AssertFloat(result).IsEqual(1.0f);
        }

        [TestCase]
        public void GetEliteHPMultiplier_Wave31_Returns2()
        {
            // Arrange & Act
            float result = DifficultyScaler.GetEliteHPMultiplier(31);

            // Assert
            AssertFloat(result).IsEqual(2.0f);
        }

        [TestCase]
        public void GetEliteDamageMultiplier_Wave30_Returns1()
        {
            // Arrange & Act
            float result = DifficultyScaler.GetEliteDamageMultiplier(30);

            // Assert
            AssertFloat(result).IsEqual(1.0f);
        }

        [TestCase]
        public void GetEliteDamageMultiplier_Wave31_Returns1Point5()
        {
            // Arrange & Act
            float result = DifficultyScaler.GetEliteDamageMultiplier(31);

            // Assert
            AssertFloat(result).IsEqual(1.5f);
        }

        #endregion
    }
}
