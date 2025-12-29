using Godot;
using GdUnit4;
using MechDefenseHalo.Tutorial;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Tutorial
{
    /// <summary>
    /// Unit tests for Tutorial system
    /// Tests TutorialStep, TutorialProgressTracker, and basic tutorial flow
    /// </summary>
    [TestSuite]
    public class TutorialTests
    {
        [TestCase]
        public void TutorialStep_IsObjectiveComplete_WithIntValue_ReturnsTrueWhenMet()
        {
            // Arrange
            var step = new TutorialStep
            {
                StepNumber = 1,
                Title = "Test Step",
                ObjectiveType = "test",
                ObjectiveValue = 10,
                CurrentProgress = 10
            };

            // Act
            bool isComplete = step.IsObjectiveComplete();

            // Assert
            AssertBool(isComplete).IsTrue();
        }

        [TestCase]
        public void TutorialStep_IsObjectiveComplete_WithIntValue_ReturnsFalseWhenNotMet()
        {
            // Arrange
            var step = new TutorialStep
            {
                StepNumber = 1,
                Title = "Test Step",
                ObjectiveType = "test",
                ObjectiveValue = 10,
                CurrentProgress = 5
            };

            // Act
            bool isComplete = step.IsObjectiveComplete();

            // Assert
            AssertBool(isComplete).IsFalse();
        }

        [TestCase]
        public void TutorialStep_GetProgressPercentage_ReturnsCorrectPercentage()
        {
            // Arrange
            var step = new TutorialStep
            {
                StepNumber = 1,
                Title = "Test Step",
                ObjectiveType = "test",
                ObjectiveValue = 10,
                CurrentProgress = 5
            };

            // Act
            float percentage = step.GetProgressPercentage();

            // Assert
            AssertFloat(percentage).IsEqual(50f);
        }

        [TestCase]
        public void TutorialStep_GetProgressPercentage_ClampedAt100()
        {
            // Arrange
            var step = new TutorialStep
            {
                StepNumber = 1,
                Title = "Test Step",
                ObjectiveType = "test",
                ObjectiveValue = 10,
                CurrentProgress = 15
            };

            // Act
            float percentage = step.GetProgressPercentage();

            // Assert
            AssertFloat(percentage).IsEqual(100f);
        }

        [TestCase]
        public void TutorialStep_GetObjectiveText_ReturnsFormattedString()
        {
            // Arrange
            var step = new TutorialStep
            {
                StepNumber = 1,
                Title = "Test Step",
                ObjectiveType = "test",
                ObjectiveValue = 10,
                CurrentProgress = 3
            };

            // Act
            string text = step.GetObjectiveText();

            // Assert
            AssertString(text).IsEqual("3/10");
        }

        [TestCase]
        public void TutorialStep_CanSkip_DefaultsToTrue()
        {
            // Arrange
            var step = new TutorialStep
            {
                StepNumber = 1,
                Title = "Test Step"
            };

            // Assert
            AssertBool(step.CanSkip).IsTrue();
        }

        [TestCase]
        public void TutorialStep_WithStringObjective_IsCompleteWhenProgressGreaterThanZero()
        {
            // Arrange
            var step = new TutorialStep
            {
                StepNumber = 1,
                Title = "Test Step",
                ObjectiveType = "ui_opened",
                ObjectiveValue = "inventory",
                CurrentProgress = 1
            };

            // Act
            bool isComplete = step.IsObjectiveComplete();

            // Assert
            AssertBool(isComplete).IsTrue();
        }

        [TestCase]
        public void TutorialSkipHandler_CanSkipStep_ReturnsTrueForSkippableStep()
        {
            // Arrange
            var skipHandler = new TutorialSkipHandler();
            var step = new TutorialStep
            {
                StepNumber = 1,
                Title = "Test Step",
                CanSkip = true
            };

            // Act
            bool canSkip = skipHandler.CanSkipStep(step);

            // Assert
            AssertBool(canSkip).IsTrue();
        }

        [TestCase]
        public void TutorialSkipHandler_CanSkipStep_ReturnsFalseForNonSkippableStep()
        {
            // Arrange
            var skipHandler = new TutorialSkipHandler();
            var step = new TutorialStep
            {
                StepNumber = 1,
                Title = "Test Step",
                CanSkip = false
            };

            // Act
            bool canSkip = skipHandler.CanSkipStep(step);

            // Assert
            AssertBool(canSkip).IsFalse();
        }

        [TestCase]
        public void TutorialSkipHandler_CanSkipStep_ReturnsFalseForNullStep()
        {
            // Arrange
            var skipHandler = new TutorialSkipHandler();

            // Act
            bool canSkip = skipHandler.CanSkipStep(null);

            // Assert
            AssertBool(canSkip).IsFalse();
        }
    }
}
