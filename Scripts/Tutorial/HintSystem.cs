using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Tutorial
{
    /// <summary>
    /// Wrapper class for hint and visual feedback functionality.
    /// Provides a simplified API for showing hints, highlights, and key indicators.
    /// Internally uses TutorialHighlight.
    /// </summary>
    public partial class HintSystem : Node
    {
        #region Private Fields

        private TutorialHighlight _highlightUI;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get or create highlight UI
            _highlightUI = GetNodeOrNull<TutorialHighlight>("../TutorialHighlight");
            
            if (_highlightUI == null)
            {
                GD.PrintErr("HintSystem: TutorialHighlight not found. Ensure TutorialManager is properly initialized.");
            }
            
            GD.Print("HintSystem initialized");
        }

        #endregion

        #region Public Methods - UI Highlighting

        /// <summary>
        /// Show a hint by highlighting a UI element
        /// </summary>
        /// <param name="elementName">Name of the UI element to highlight</param>
        public void ShowHint(string elementName)
        {
            if (_highlightUI != null)
            {
                _highlightUI.HighlightElement(elementName);
                GD.Print($"HintSystem: Showing hint for {elementName}");
            }
            else
            {
                GD.PrintErr("HintSystem: Cannot show hint - TutorialHighlight not available");
            }
        }

        /// <summary>
        /// Show keyboard key hints
        /// </summary>
        /// <param name="keys">List of keys to show</param>
        public void ShowKeyHints(List<string> keys)
        {
            if (_highlightUI != null)
            {
                _highlightUI.HighlightKeys(keys);
                GD.Print($"HintSystem: Showing key hints: {string.Join(", ", keys)}");
            }
            else
            {
                GD.PrintErr("HintSystem: Cannot show key hints - TutorialHighlight not available");
            }
        }

        /// <summary>
        /// Show a single keyboard key hint
        /// </summary>
        /// <param name="key">Key to show</param>
        public void ShowKeyHint(string key)
        {
            ShowKeyHints(new List<string> { key });
        }

        /// <summary>
        /// Clear all active hints and highlights
        /// </summary>
        public void ClearHints()
        {
            if (_highlightUI != null)
            {
                _highlightUI.ClearHighlights();
                GD.Print("HintSystem: Cleared all hints");
            }
        }

        /// <summary>
        /// Highlight a specific UI element with custom message
        /// </summary>
        /// <param name="elementName">Name of the UI element</param>
        /// <param name="message">Hint message to display (currently logged)</param>
        public void ShowHintWithMessage(string elementName, string message)
        {
            if (_highlightUI != null)
            {
                _highlightUI.HighlightElement(elementName);
                GD.Print($"HintSystem: {message}");
            }
            else
            {
                GD.PrintErr("HintSystem: Cannot show hint with message - TutorialHighlight not available");
            }
        }

        #endregion

        #region Public Methods - Key Indicators

        /// <summary>
        /// Show a single key indicator
        /// </summary>
        /// <param name="key">Key to show indicator for</param>
        public void ShowKeyIndicator(string key)
        {
            if (_highlightUI != null)
            {
                _highlightUI.ShowKeyIndicator(key);
            }
            else
            {
                GD.PrintErr("HintSystem: Cannot show key indicator - TutorialHighlight not available");
            }
        }

        /// <summary>
        /// Clear all key indicators
        /// </summary>
        public void ClearKeyIndicators()
        {
            if (_highlightUI != null)
            {
                _highlightUI.ClearKeyIndicators();
            }
        }

        #endregion
    }
}
