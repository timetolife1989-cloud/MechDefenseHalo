using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Tutorial
{
    /// <summary>
    /// Represents a single step in the tutorial sequence
    /// </summary>
    public class TutorialStep
    {
        #region Public Properties

        public int StepNumber { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ObjectiveType { get; set; } = "";
        public object ObjectiveValue { get; set; }
        public int CurrentProgress { get; set; } = 0;
        
        public List<string> HighlightKeys { get; set; } = new List<string>();
        public string HighlightUI { get; set; } = "";
        public List<string> SpawnEnemies { get; set; } = new List<string>();
        public int SpawnWave { get; set; } = 0;
        
        public bool CanSkip { get; set; } = true;

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if the objective for this step is complete
        /// </summary>
        /// <returns>True if objective is met</returns>
        public bool IsObjectiveComplete()
        {
            if (ObjectiveValue is int targetValue)
            {
                return CurrentProgress >= targetValue;
            }
            else if (ObjectiveValue is string targetString)
            {
                return CurrentProgress > 0; // Simple completion check for string-based objectives
            }
            
            return false;
        }

        /// <summary>
        /// Get progress as a percentage
        /// </summary>
        /// <returns>Progress percentage (0-100)</returns>
        public float GetProgressPercentage()
        {
            if (ObjectiveValue is int targetValue && targetValue > 0)
            {
                return Mathf.Clamp((float)CurrentProgress / targetValue * 100f, 0f, 100f);
            }
            
            return CurrentProgress > 0 ? 100f : 0f;
        }

        /// <summary>
        /// Get formatted objective text
        /// </summary>
        /// <returns>Human-readable objective progress</returns>
        public string GetObjectiveText()
        {
            if (ObjectiveValue is int targetValue)
            {
                return $"{CurrentProgress}/{targetValue}";
            }
            else if (ObjectiveValue is string)
            {
                return CurrentProgress > 0 ? "Complete" : "Incomplete";
            }
            
            return "";
        }

        #endregion
    }
}
