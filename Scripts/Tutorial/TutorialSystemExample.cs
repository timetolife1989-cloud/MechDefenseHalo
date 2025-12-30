using Godot;
using MechDefenseHalo.Tutorial;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Examples
{
    /// <summary>
    /// Example demonstrating how to use the Tutorial System
    /// This file shows various usage patterns and best practices
    /// </summary>
    public partial class TutorialSystemExample : Node
    {
        private TutorialManager _tutorialManager;
        private ObjectiveTracker _objectiveTracker;
        private HintSystem _hintSystem;
        private ProgressTracker _progressTracker;

        public override void _Ready()
        {
            // Example 1: Get existing TutorialManager (usually in the scene)
            _tutorialManager = GetNode<TutorialManager>("/root/TutorialManager");
            
            // Example 2: Access the child components
            _objectiveTracker = _tutorialManager.GetNode<ObjectiveTracker>("ObjectiveTracker");
            _hintSystem = _tutorialManager.GetNode<HintSystem>("HintSystem");
            _progressTracker = _tutorialManager.GetNode<ProgressTracker>("ProgressTracker");
            
            // Example 3: Listen for tutorial events
            SetupEventListeners();
        }

        private void SetupEventListeners()
        {
            EventBus.On(EventBus.TutorialStarted, OnTutorialStarted);
            EventBus.On(EventBus.TutorialStepStarted, OnTutorialStepStarted);
            EventBus.On(EventBus.TutorialObjectiveComplete, OnObjectiveComplete);
            EventBus.On(EventBus.TutorialCompleted, OnTutorialCompleted);
        }

        // Example event handlers
        private void OnTutorialStarted(object data)
        {
            GD.Print("Tutorial started - Pausing game systems");
            // Pause enemy spawning, disable certain UI, etc.
        }

        private void OnTutorialStepStarted(object data)
        {
            GD.Print($"Tutorial step started - Current progress: {_progressTracker.GetProgressString()}");
            // Update UI to show current step info
        }

        private void OnObjectiveComplete(object data)
        {
            GD.Print("Tutorial objective completed!");
            // Play completion sound, show visual feedback
            PlayCompletionFeedback();
        }

        private void OnTutorialCompleted(object data)
        {
            GD.Print("Tutorial completed - Resuming normal gameplay");
            // Resume game systems, hide tutorial UI, show rewards screen
        }

        // Example: Manual tutorial control
        public void StartTutorial()
        {
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorial();
            }
        }

        public void SkipTutorial()
        {
            if (_tutorialManager != null)
            {
                _tutorialManager.SkipTutorial();
            }
        }

        public void RestartTutorial()
        {
            if (_tutorialManager != null)
            {
                _tutorialManager.RestartTutorial();
            }
        }

        // Example: Using ObjectiveTracker directly
        public void CheckObjectiveProgress()
        {
            if (_objectiveTracker != null)
            {
                var currentStep = _objectiveTracker.GetCurrentStep();
                if (currentStep != null)
                {
                    float progress = _objectiveTracker.GetProgressPercentage();
                    GD.Print($"Current objective: {currentStep.Title} - {progress}% complete");
                }
            }
        }

        // Example: Using HintSystem to show hints
        public void ShowMovementHints()
        {
            if (_hintSystem != null)
            {
                // Show keyboard keys
                _hintSystem.ShowKeyHints(new System.Collections.Generic.List<string> 
                { 
                    "W", "A", "S", "D" 
                });
            }
        }

        public void ShowInventoryHint()
        {
            if (_hintSystem != null)
            {
                // Highlight a UI element
                _hintSystem.ShowHint("InventoryButton");
            }
        }

        public void ClearAllHints()
        {
            if (_hintSystem != null)
            {
                _hintSystem.ClearHints();
            }
        }

        // Example: Using ProgressTracker to monitor progress
        public void DisplayProgressInfo()
        {
            if (_progressTracker != null)
            {
                int completed = _progressTracker.CompletedSteps;
                int total = _progressTracker.TotalSteps;
                float percentage = _progressTracker.CompletionPercentage;
                
                GD.Print($"Tutorial Progress: {completed}/{total} ({percentage:F1}%)");
                GD.Print($"Is Active: {_progressTracker.IsTutorialActive()}");
                GD.Print($"Is Complete: {_progressTracker.IsTutorialComplete}");
            }
        }

        // Example: Creating a custom tutorial step programmatically
        public TutorialStep CreateCustomStep()
        {
            return new TutorialStep
            {
                StepNumber = 1,
                Title = "Custom Step",
                Description = "This is a programmatically created tutorial step",
                ObjectiveType = "enemy_kills",
                ObjectiveValue = 5,
                HighlightKeys = new System.Collections.Generic.List<string> { "LMB" },
                CanSkip = true
            };
        }

        // Example: Manually tracking a custom step
        public void TrackCustomStep()
        {
            var customStep = CreateCustomStep();
            
            if (_objectiveTracker != null)
            {
                _objectiveTracker.StartTracking(customStep);
            }
        }

        // Example: Manually updating progress (for testing)
        public void SimulateProgress(int amount)
        {
            if (_objectiveTracker != null)
            {
                _objectiveTracker.SetProgress(amount);
                GD.Print($"Simulated progress update: {amount}");
            }
        }

        // Example: Visual feedback methods
        private void PlayCompletionFeedback()
        {
            // Play sound effect
            // Show particle effect
            // Brief screen flash
            GD.Print("✓ Objective Complete!");
        }

        // Example: Check if tutorial should be shown
        public bool ShouldShowTutorial()
        {
            // Check if first launch
            bool isFirstLaunch = SaveManager.GetBool("is_first_launch");
            
            // Check if tutorial already completed
            bool tutorialCompleted = SaveManager.GetBool("tutorial_completed");
            
            return isFirstLaunch && !tutorialCompleted;
        }

        // Example: Force show tutorial (for testing or settings menu)
        public void ShowTutorialFromSettings()
        {
            // Reset tutorial completion flag
            SaveManager.SetBool("tutorial_completed", false);
            
            // Start tutorial
            if (_tutorialManager != null)
            {
                _tutorialManager.RestartTutorial();
            }
        }

        // Cleanup
        public override void _ExitTree()
        {
            EventBus.Off(EventBus.TutorialStarted, OnTutorialStarted);
            EventBus.Off(EventBus.TutorialStepStarted, OnTutorialStepStarted);
            EventBus.Off(EventBus.TutorialObjectiveComplete, OnObjectiveComplete);
            EventBus.Off(EventBus.TutorialCompleted, OnTutorialCompleted);
        }
    }
}
