using Godot;
using System;

namespace MechDefenseHalo.Tutorial
{
    /// <summary>
    /// Wrapper class for objective tracking functionality.
    /// Provides a simplified API for tracking tutorial objectives.
    /// Internally uses TutorialProgressTracker.
    /// </summary>
    public partial class ObjectiveTracker : Node
    {
        #region Private Fields

        private TutorialProgressTracker _progressTracker;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get or create progress tracker
            _progressTracker = GetNodeOrNull<TutorialProgressTracker>("../TutorialProgressTracker");
            
            if (_progressTracker == null)
            {
                GD.PrintErr("ObjectiveTracker: TutorialProgressTracker not found. Ensure TutorialManager is properly initialized.");
            }
            
            GD.Print("ObjectiveTracker initialized");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start tracking objectives for a tutorial step
        /// </summary>
        /// <param name="step">The tutorial step to track</param>
        public void StartTracking(TutorialStep step)
        {
            if (_progressTracker != null)
            {
                _progressTracker.StartTracking(step);
            }
            else
            {
                GD.PrintErr("ObjectiveTracker: Cannot start tracking - TutorialProgressTracker not available");
            }
        }

        /// <summary>
        /// Stop tracking current objectives
        /// </summary>
        public void StopTracking()
        {
            if (_progressTracker != null)
            {
                _progressTracker.StopTracking();
            }
        }

        /// <summary>
        /// Manually update progress for current objective
        /// </summary>
        /// <param name="progress">New progress value</param>
        public void SetProgress(int progress)
        {
            if (_progressTracker != null)
            {
                _progressTracker.SetProgress(progress);
            }
            else
            {
                GD.PrintErr("ObjectiveTracker: Cannot set progress - TutorialProgressTracker not available");
            }
        }

        /// <summary>
        /// Get current step being tracked
        /// </summary>
        /// <returns>Current tutorial step or null</returns>
        public TutorialStep GetCurrentStep()
        {
            return _progressTracker?.CurrentStep;
        }

        /// <summary>
        /// Check if an objective is complete
        /// </summary>
        /// <returns>True if current objective is complete</returns>
        public bool IsObjectiveComplete()
        {
            if (_progressTracker?.CurrentStep != null)
            {
                return _progressTracker.CurrentStep.IsObjectiveComplete();
            }
            return false;
        }

        /// <summary>
        /// Get current progress percentage
        /// </summary>
        /// <returns>Progress percentage (0-100)</returns>
        public float GetProgressPercentage()
        {
            if (_progressTracker?.CurrentStep != null)
            {
                return _progressTracker.CurrentStep.GetProgressPercentage();
            }
            return 0f;
        }

        #endregion
    }
}
