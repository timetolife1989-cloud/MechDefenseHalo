using Godot;
using GdUnit4;
using MechDefenseHalo.Achievements;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Achievements
{
    /// <summary>
    /// Unit tests for Achievement class
    /// </summary>
    [TestSuite]
    public class AchievementTests
    {
        [TestCase]
        public void Achievement_NewInstance_HasDefaultValues()
        {
            // Arrange & Act
            var achievement = new Achievement
            {
                ID = "test_achievement",
                Name = "Test",
                Description = "Test Description",
                RequiredProgress = 10
            };

            // Assert
            AssertThat(achievement.Progress).IsEqual(0);
            AssertBool(achievement.IsCompleted).IsFalse();
            AssertObject(achievement.Rewards).IsNotNull();
        }

        [TestCase]
        public void GetCompletionPercent_ReturnsCorrectValue()
        {
            // Arrange
            var achievement = new Achievement
            {
                RequiredProgress = 100,
                Progress = 50
            };

            // Act
            float percent = achievement.GetCompletionPercent();

            // Assert
            AssertThat(percent).IsEqual(50.0f);
        }

        [TestCase]
        public void GetCompletionPercent_WhenCompleted_Returns100()
        {
            // Arrange
            var achievement = new Achievement
            {
                RequiredProgress = 100,
                Progress = 100,
                IsCompleted = true
            };

            // Act
            float percent = achievement.GetCompletionPercent();

            // Assert
            AssertThat(percent).IsEqual(100.0f);
        }

        [TestCase]
        public void CanUnlock_WhenProgressMet_ReturnsTrue()
        {
            // Arrange
            var achievement = new Achievement
            {
                RequiredProgress = 10,
                Progress = 10,
                IsCompleted = false
            };

            // Act
            bool canUnlock = achievement.CanUnlock();

            // Assert
            AssertBool(canUnlock).IsTrue();
        }

        [TestCase]
        public void CanUnlock_WhenAlreadyCompleted_ReturnsFalse()
        {
            // Arrange
            var achievement = new Achievement
            {
                RequiredProgress = 10,
                Progress = 10,
                IsCompleted = true
            };

            // Act
            bool canUnlock = achievement.CanUnlock();

            // Assert
            AssertBool(canUnlock).IsFalse();
        }

        [TestCase]
        public void AddProgress_IncreasesProgress()
        {
            // Arrange
            var achievement = new Achievement
            {
                RequiredProgress = 100,
                Progress = 0
            };

            // Act
            achievement.AddProgress(50);

            // Assert
            AssertThat(achievement.Progress).IsEqual(50);
        }

        [TestCase]
        public void AddProgress_WhenCompletedRequirement_ReturnsTrue()
        {
            // Arrange
            var achievement = new Achievement
            {
                RequiredProgress = 100,
                Progress = 90
            };

            // Act
            bool canUnlock = achievement.AddProgress(10);

            // Assert
            AssertBool(canUnlock).IsTrue();
            AssertThat(achievement.Progress).IsEqual(100);
        }

        [TestCase]
        public void AddProgress_WhenAlreadyCompleted_DoesNotIncrease()
        {
            // Arrange
            var achievement = new Achievement
            {
                RequiredProgress = 100,
                Progress = 100,
                IsCompleted = true
            };

            // Act
            bool result = achievement.AddProgress(10);

            // Assert
            AssertBool(result).IsFalse();
            AssertThat(achievement.Progress).IsEqual(100);
        }

        [TestCase]
        public void Complete_SetsCompletedAndProgress()
        {
            // Arrange
            var achievement = new Achievement
            {
                RequiredProgress = 100,
                Progress = 75
            };

            // Act
            achievement.Complete();

            // Assert
            AssertBool(achievement.IsCompleted).IsTrue();
            AssertThat(achievement.Progress).IsEqual(100);
            AssertObject(achievement.UnlockDate).IsNotNull();
        }

        [TestCase]
        public void GetDisplayName_WhenSecretAndNotCompleted_ReturnsHidden()
        {
            // Arrange
            var achievement = new Achievement
            {
                Name = "Secret Achievement",
                IsSecret = true,
                IsCompleted = false
            };

            // Act
            string displayName = achievement.GetDisplayName();

            // Assert
            AssertThat(displayName).IsEqual("???");
        }

        [TestCase]
        public void GetDisplayName_WhenSecretAndCompleted_ReturnsName()
        {
            // Arrange
            var achievement = new Achievement
            {
                Name = "Secret Achievement",
                IsSecret = true,
                IsCompleted = true
            };

            // Act
            string displayName = achievement.GetDisplayName();

            // Assert
            AssertThat(displayName).IsEqual("Secret Achievement");
        }

        [TestCase]
        public void GetDisplayName_WhenNotSecret_ReturnsName()
        {
            // Arrange
            var achievement = new Achievement
            {
                Name = "Regular Achievement",
                IsSecret = false,
                IsCompleted = false
            };

            // Act
            string displayName = achievement.GetDisplayName();

            // Assert
            AssertThat(displayName).IsEqual("Regular Achievement");
        }

        [TestCase]
        public void GetDisplayDescription_WhenSecretAndNotCompleted_ReturnsHidden()
        {
            // Arrange
            var achievement = new Achievement
            {
                Description = "Secret Description",
                IsSecret = true,
                IsCompleted = false
            };

            // Act
            string displayDesc = achievement.GetDisplayDescription();

            // Assert
            AssertThat(displayDesc).IsEqual("Hidden Achievement");
        }

        [TestCase]
        public void AddProgress_WithNegativeAmount_DoesNotGoBelowZero()
        {
            // Arrange
            var achievement = new Achievement
            {
                RequiredProgress = 100,
                Progress = 10
            };

            // Act
            achievement.AddProgress(-20);

            // Assert
            AssertThat(achievement.Progress).IsEqual(0);
        }
    }
}
