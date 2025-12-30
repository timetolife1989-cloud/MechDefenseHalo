using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.AI;

namespace MechDefenseHalo.Tests.AI
{
    /// <summary>
    /// Unit tests for AdaptiveDifficultyController
    /// </summary>
    [TestSuite]
    public class AdaptiveDifficultyControllerTests
    {
        private AdaptiveDifficultyController _controller;

        [Before]
        public void Setup()
        {
            _controller = new AdaptiveDifficultyController();
        }

        [TestCase]
        public void SetDifficultyLevel_ValidValue_SetsDifficulty()
        {
            // Act
            _controller.SetDifficultyLevel(0.7f);

            // Assert
            AssertFloat(_controller.GetDifficultyLevel()).IsEqual(0.7f);
        }

        [TestCase]
        public void SetDifficultyLevel_BelowMin_ClampedToMin()
        {
            // Arrange
            _controller.MinDifficulty = 0.2f;

            // Act
            _controller.SetDifficultyLevel(0.1f);

            // Assert
            AssertFloat(_controller.GetDifficultyLevel()).IsGreaterEqual(0.2f);
        }

        [TestCase]
        public void SetDifficultyLevel_AboveMax_ClampedToMax()
        {
            // Arrange
            _controller.MaxDifficulty = 1.0f;

            // Act
            _controller.SetDifficultyLevel(1.5f);

            // Assert
            AssertFloat(_controller.GetDifficultyLevel()).IsLessEqual(1.0f);
        }

        [TestCase]
        public void GetSpawnRateMultiplier_MinDifficulty_ReturnsLowMultiplier()
        {
            // Arrange
            _controller.SetDifficultyLevel(0.0f);

            // Act
            float result = _controller.GetSpawnRateMultiplier();

            // Assert - Should be around 0.5
            AssertFloat(result).IsGreater(0.4f);
            AssertFloat(result).IsLess(0.6f);
        }

        [TestCase]
        public void GetSpawnRateMultiplier_MaxDifficulty_ReturnsHighMultiplier()
        {
            // Arrange
            _controller.SetDifficultyLevel(1.0f);

            // Act
            float result = _controller.GetSpawnRateMultiplier();

            // Assert - Should be around 2.0
            AssertFloat(result).IsGreater(1.9f);
        }

        [TestCase]
        public void GetEnemyHealthMultiplier_MinDifficulty_ReturnsLowMultiplier()
        {
            // Arrange
            _controller.SetDifficultyLevel(0.0f);

            // Act
            float result = _controller.GetEnemyHealthMultiplier();

            // Assert - Should be around 0.7
            AssertFloat(result).IsGreater(0.6f);
            AssertFloat(result).IsLess(0.8f);
        }

        [TestCase]
        public void GetEnemyHealthMultiplier_MaxDifficulty_ReturnsHighMultiplier()
        {
            // Arrange
            _controller.SetDifficultyLevel(1.0f);

            // Act
            float result = _controller.GetEnemyHealthMultiplier();

            // Assert - Should be around 1.5
            AssertFloat(result).IsGreater(1.4f);
        }
    }
}
