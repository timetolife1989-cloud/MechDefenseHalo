using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Tutorial
{
    /// <summary>
    /// Wrapper class for tracking overall tutorial progression.
    /// Provides a simplified API for monitoring tutorial completion status.
    /// Works alongside TutorialManager and TutorialProgressTracker.
    /// </summary>
    public partial class ProgressTracker : Node
    {
        #region Private Fields

        private TutorialManager _tutorialManager;
        private int _totalSteps = 0;
        private int _completedSteps = 0;

        #endregion

        #region Public Properties

        /// <summary>
        /// Total number of tutorial steps
        /// </summary>
        public int TotalSteps => _totalSteps;

        /// <summary>
        /// Number of completed tutorial steps
        /// </summary>
        public int CompletedSteps => _completedSteps;

        /// <summary>
        /// Tutorial completion percentage (0-100)
        /// </summary>
        public float CompletionPercentage => _totalSteps > 0 ? ((float)_completedSteps / _totalSteps) * 100f : 0f;

        /// <summary>
        /// Check if tutorial is complete
        /// </summary>
        public bool IsTutorialComplete => SaveManager.GetBool("tutorial_completed");

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get tutorial manager
            _tutorialManager = GetNodeOrNull<TutorialManager>("..");
            
            if (_tutorialManager == null)
            {
                GD.PrintErr("ProgressTracker: TutorialManager not found");
            }
            
            // Listen for tutorial events
            EventBus.On(EventBus.TutorialStarted, OnTutorialStarted);
            EventBus.On(EventBus.TutorialStepStarted, OnTutorialStepStarted);
            EventBus.On(EventBus.TutorialObjectiveComplete, OnObjectiveComplete);
            EventBus.On(EventBus.TutorialCompleted, OnTutorialCompleted);
            
            GD.Print("ProgressTracker initialized");
        }

        public override void _ExitTree()
        {
            // Clean up event listeners
            EventBus.Off(EventBus.TutorialStarted, OnTutorialStarted);
            EventBus.Off(EventBus.TutorialStepStarted, OnTutorialStepStarted);
            EventBus.Off(EventBus.TutorialObjectiveComplete, OnObjectiveComplete);
            EventBus.Off(EventBus.TutorialCompleted, OnTutorialCompleted);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get current tutorial step index
        /// </summary>
        /// <returns>Current step index or -1 if not available</returns>
        public int GetCurrentStepIndex()
        {
            return _tutorialManager?.CurrentStepIndex ?? -1;
        }

        /// <summary>
        /// Check if tutorial is currently active
        /// </summary>
        /// <returns>True if tutorial is active</returns>
        public bool IsTutorialActive()
        {
            return _tutorialManager?.IsTutorialActive ?? false;
        }

        /// <summary>
        /// Get formatted progress string
        /// </summary>
        /// <returns>Progress string (e.g., "3/10 steps completed")</returns>
        public string GetProgressString()
        {
            return $"{_completedSteps}/{_totalSteps} steps completed";
        }

        /// <summary>
        /// Get formatted percentage string
        /// </summary>
        /// <returns>Percentage string (e.g., "30%")</returns>
        public string GetPercentageString()
        {
            return $"{CompletionPercentage:F0}%";
        }

        /// <summary>
        /// Reset progress tracking (for testing or replay)
        /// </summary>
        public void ResetProgress()
        {
            _completedSteps = 0;
            GD.Print("ProgressTracker: Progress reset");
        }

        /// <summary>
        /// Set total number of steps (usually called by TutorialManager)
        /// </summary>
        /// <param name="total">Total number of steps</param>
        public void SetTotalSteps(int total)
        {
            _totalSteps = total;
            GD.Print($"ProgressTracker: Total steps set to {total}");
        }

        #endregion

        #region Event Handlers

        private void OnTutorialStarted(object data)
        {
            _completedSteps = 0;
            GD.Print("ProgressTracker: Tutorial started, progress reset");
        }

        private void OnTutorialStepStarted(object data)
        {
            // Optional: Could track step starts here
            GD.Print($"ProgressTracker: Step started - Current progress: {GetProgressString()}");
        }

        private void OnObjectiveComplete(object data)
        {
            _completedSteps++;
            GD.Print($"ProgressTracker: Objective complete - {GetProgressString()} ({GetPercentageString()})");
            
            // Emit progress update event
            EventBus.Emit("tutorial_progress_updated", new Dictionary<string, object>
            {
                { "completed", _completedSteps },
                { "total", _totalSteps },
                { "percentage", CompletionPercentage }
            });
        }

        private void OnTutorialCompleted(object data)
        {
            _completedSteps = _totalSteps;
            GD.Print($"ProgressTracker: Tutorial completed - {GetProgressString()}");
        }

        #endregion
    }
}
