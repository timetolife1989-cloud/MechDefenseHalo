using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Tutorial
{
    /// <summary>
    /// Handles UI highlighting and key indicators for tutorial
    /// </summary>
    public partial class TutorialHighlight : Control
    {
        #region Constants

        private const int KeyIndicatorBorderWidth = 2;
        private const int KeyIndicatorCornerRadius = 5;
        private const int KeyIndicatorMinSize = 60;

        #endregion

        #region Node References

        private ColorRect _overlay;
        private Panel _highlightFrame;
        private VBoxContainer _keyIndicatorContainer;

        #endregion

        #region Private Fields

        private Dictionary<string, Control> _activeKeyIndicators = new Dictionary<string, Control>();
        private Tween _pulseTween;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get or create overlay
            _overlay = GetNodeOrNull<ColorRect>("Overlay");
            if (_overlay == null)
            {
                _overlay = new ColorRect();
                _overlay.Name = "Overlay";
                _overlay.SetAnchorsPreset(LayoutPreset.FullRect);
                _overlay.Color = new Color(0, 0, 0, 0.5f);
                _overlay.MouseFilter = MouseFilterEnum.Ignore;
                _overlay.Visible = false;
                AddChild(_overlay);
            }

            // Get or create highlight frame
            _highlightFrame = GetNodeOrNull<Panel>("HighlightFrame");
            if (_highlightFrame == null)
            {
                _highlightFrame = new Panel();
                _highlightFrame.Name = "HighlightFrame";
                
                // Create a StyleBox for the highlight
                var styleBox = new StyleBoxFlat();
                styleBox.BorderColor = new Color(1, 1, 0, 1); // Yellow
                styleBox.BorderWidthLeft = 3;
                styleBox.BorderWidthRight = 3;
                styleBox.BorderWidthTop = 3;
                styleBox.BorderWidthBottom = 3;
                styleBox.BgColor = new Color(1, 1, 0, 0.2f);
                
                _highlightFrame.AddThemeStyleboxOverride("panel", styleBox);
                _highlightFrame.MouseFilter = MouseFilterEnum.Ignore;
                _highlightFrame.Visible = false;
                AddChild(_highlightFrame);
            }

            // Get or create key indicator container
            _keyIndicatorContainer = GetNodeOrNull<VBoxContainer>("KeyIndicators");
            if (_keyIndicatorContainer == null)
            {
                _keyIndicatorContainer = new VBoxContainer();
                _keyIndicatorContainer.Name = "KeyIndicators";
                _keyIndicatorContainer.SetAnchorsPreset(LayoutPreset.CenterBottom);
                _keyIndicatorContainer.Position = new Vector2(0, -100);
                AddChild(_keyIndicatorContainer);
            }

            GD.Print("TutorialHighlight initialized");
        }

        #endregion

        #region Public Methods - UI Highlighting

        /// <summary>
        /// Highlight a UI element by name
        /// </summary>
        /// <param name="elementName">Name of the UI element to highlight</param>
        public void HighlightElement(string elementName)
        {
            var element = GetUIElement(elementName);
            
            if (element == null)
            {
                GD.PrintErr($"UI element not found: {elementName}");
                return;
            }

            // Show dark overlay
            if (_overlay != null)
            {
                _overlay.Visible = true;
            }

            // Position highlight frame around element
            if (_highlightFrame != null)
            {
                _highlightFrame.GlobalPosition = element.GlobalPosition - new Vector2(10, 10);
                _highlightFrame.Size = element.Size + new Vector2(20, 20);
                _highlightFrame.Visible = true;

                // Start pulse animation
                StartPulseAnimation();
            }

            GD.Print($"Highlighting UI element: {elementName}");
        }

        /// <summary>
        /// Highlight keyboard keys
        /// </summary>
        /// <param name="keys">List of keys to highlight</param>
        public void HighlightKeys(List<string> keys)
        {
            ClearKeyIndicators();

            foreach (var key in keys)
            {
                ShowKeyIndicator(key);
            }

            GD.Print($"Highlighting keys: {string.Join(", ", keys)}");
        }

        /// <summary>
        /// Clear all highlights
        /// </summary>
        public void ClearHighlights()
        {
            if (_overlay != null)
            {
                _overlay.Visible = false;
            }

            if (_highlightFrame != null)
            {
                _highlightFrame.Visible = false;
            }

            StopPulseAnimation();
            ClearKeyIndicators();

            GD.Print("Cleared all highlights");
        }

        #endregion

        #region Public Methods - Key Indicators

        /// <summary>
        /// Show a key indicator on screen
        /// </summary>
        /// <param name="key">Key to show</param>
        public void ShowKeyIndicator(string key)
        {
            if (_activeKeyIndicators.ContainsKey(key))
            {
                return; // Already showing
            }

            var keyLabel = new Label();
            keyLabel.Text = key;
            keyLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
            keyLabel.AddThemeFontSizeOverride("font_size", 32);
            
            var panel = new Panel();
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            styleBox.BorderColor = new Color(1, 1, 0, 1);
            styleBox.BorderWidthAll = KeyIndicatorBorderWidth;
            styleBox.CornerRadiusAll = KeyIndicatorCornerRadius;
            panel.AddThemeStyleboxOverride("panel", styleBox);
            panel.CustomMinimumSize = new Vector2(KeyIndicatorMinSize, KeyIndicatorMinSize);
            
            keyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            keyLabel.VerticalAlignment = VerticalAlignment.Center;
            keyLabel.SetAnchorsPreset(LayoutPreset.FullRect);
            
            panel.AddChild(keyLabel);
            _keyIndicatorContainer.AddChild(panel);
            _activeKeyIndicators[key] = panel;
        }

        /// <summary>
        /// Clear all key indicators
        /// </summary>
        public void ClearKeyIndicators()
        {
            foreach (var indicator in _activeKeyIndicators.Values)
            {
                indicator.QueueFree();
            }
            _activeKeyIndicators.Clear();
        }

        #endregion

        #region Private Methods

        private Control GetUIElement(string elementName)
        {
            // Try to find the element in the scene tree
            var root = GetTree().Root;
            return FindNodeByName(root, elementName);
        }

        private Control FindNodeByName(Node node, string name)
        {
            if (node.Name == name && node is Control control)
            {
                return control;
            }

            foreach (Node child in node.GetChildren())
            {
                var result = FindNodeByName(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void StartPulseAnimation()
        {
            StopPulseAnimation();

            if (_highlightFrame == null) return;

            _pulseTween = CreateTween();
            _pulseTween.SetLoops();
            _pulseTween.TweenProperty(_highlightFrame, "modulate:a", 0.5f, 0.5f);
            _pulseTween.TweenProperty(_highlightFrame, "modulate:a", 1.0f, 0.5f);
        }

        private void StopPulseAnimation()
        {
            if (_pulseTween != null)
            {
                _pulseTween.Kill();
                _pulseTween = null;
            }

            if (_highlightFrame != null)
            {
                _highlightFrame.Modulate = new Color(1, 1, 1, 1);
            }
        }

        #endregion

        #region Godot Lifecycle

        public override void _ExitTree()
        {
            StopPulseAnimation();
            ClearKeyIndicators();
        }

        #endregion
    }
}
