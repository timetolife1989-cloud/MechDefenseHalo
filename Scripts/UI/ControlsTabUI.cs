using Godot;
using System.Collections.Generic;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Controls settings tab UI
    /// </summary>
    public partial class ControlsTabUI : Control
    {
        #region Export Variables
        
        [Export] public HSlider MouseSensitivitySlider { get; set; }
        [Export] public Label MouseSensitivityLabel { get; set; }
        [Export] public CheckBox InvertYCheckbox { get; set; }
        
        [Export] public HSlider ControllerSensitivitySlider { get; set; }
        [Export] public Label ControllerSensitivityLabel { get; set; }
        [Export] public HSlider ControllerDeadzoneSlider { get; set; }
        [Export] public Label ControllerDeadzoneLabel { get; set; }
        
        [Export] public ScrollContainer KeyBindingScrollContainer { get; set; }
        [Export] public VBoxContainer KeyBindingList { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private ControlSettingsData _currentSettings;
        private Dictionary<string, Button> _keyBindingButtons = new Dictionary<string, Button>();
        private string _waitingForKey = null;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            ConnectSignals();
            SetupKeyBindingList();
        }
        
        public override void _Input(InputEvent @event)
        {
            if (_waitingForKey != null)
            {
                if (@event is InputEventKey keyEvent && keyEvent.Pressed)
                {
                    UpdateKeyBinding(_waitingForKey, (int)keyEvent.Keycode);
                    _waitingForKey = null;
                    AcceptEvent();
                }
                else if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
                {
                    UpdateKeyBinding(_waitingForKey, (int)mouseEvent.ButtonIndex);
                    _waitingForKey = null;
                    AcceptEvent();
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        public void LoadSettings(ControlSettingsData settings)
        {
            _currentSettings = settings;
            
            if (MouseSensitivitySlider != null)
            {
                MouseSensitivitySlider.Value = settings.MouseSensitivity;
                UpdateSensitivityLabel(MouseSensitivityLabel, settings.MouseSensitivity);
            }
            
            if (InvertYCheckbox != null)
                InvertYCheckbox.ButtonPressed = settings.InvertY;
            
            if (ControllerSensitivitySlider != null)
            {
                ControllerSensitivitySlider.Value = settings.ControllerSensitivity;
                UpdateSensitivityLabel(ControllerSensitivityLabel, settings.ControllerSensitivity);
            }
            
            if (ControllerDeadzoneSlider != null)
            {
                ControllerDeadzoneSlider.Value = settings.ControllerDeadzone;
                UpdateDeadzoneLabel(settings.ControllerDeadzone);
            }
            
            // Update key binding display
            UpdateKeyBindingDisplay(settings.KeyBindings);
        }
        
        public void SaveToSettings(ControlSettingsData settings)
        {
            if (MouseSensitivitySlider != null)
                settings.MouseSensitivity = (float)MouseSensitivitySlider.Value;
            
            if (InvertYCheckbox != null)
                settings.InvertY = InvertYCheckbox.ButtonPressed;
            
            if (ControllerSensitivitySlider != null)
                settings.ControllerSensitivity = (float)ControllerSensitivitySlider.Value;
            
            if (ControllerDeadzoneSlider != null)
                settings.ControllerDeadzone = (int)ControllerDeadzoneSlider.Value;
            
            // Key bindings are already updated in real-time
        }
        
        #endregion
        
        #region Private Methods
        
        private void ConnectSignals()
        {
            if (MouseSensitivitySlider != null)
                MouseSensitivitySlider.ValueChanged += (value) => UpdateSensitivityLabel(MouseSensitivityLabel, value);
            
            if (ControllerSensitivitySlider != null)
                ControllerSensitivitySlider.ValueChanged += (value) => UpdateSensitivityLabel(ControllerSensitivityLabel, value);
            
            if (ControllerDeadzoneSlider != null)
                ControllerDeadzoneSlider.ValueChanged += (value) => UpdateDeadzoneLabel((int)value);
        }
        
        private void SetupKeyBindingList()
        {
            if (KeyBindingList == null) return;
            
            // Create key binding rows for each action
            foreach (var action in ControlSettingsApplier.DefaultKeyBindings.Keys)
            {
                var row = new HBoxContainer();
                
                // Action label
                var label = new Label();
                label.Text = FormatActionName(action);
                label.CustomMinimumSize = new Vector2(200, 0);
                row.AddChild(label);
                
                // Key button
                var button = new Button();
                button.Text = "Click to bind";
                button.CustomMinimumSize = new Vector2(150, 0);
                button.Pressed += () => OnKeyBindingButtonPressed(action);
                row.AddChild(button);
                
                // Reset button
                var resetBtn = new Button();
                resetBtn.Text = "Reset";
                resetBtn.Pressed += () => ResetKeyBinding(action);
                row.AddChild(resetBtn);
                
                KeyBindingList.AddChild(row);
                _keyBindingButtons[action] = button;
            }
        }
        
        private void OnKeyBindingButtonPressed(string action)
        {
            _waitingForKey = action;
            
            if (_keyBindingButtons.ContainsKey(action))
            {
                _keyBindingButtons[action].Text = "Press key...";
            }
        }
        
        private void UpdateKeyBinding(string action, int keycode)
        {
            if (_currentSettings != null && _currentSettings.KeyBindings != null)
            {
                _currentSettings.KeyBindings[action] = keycode;
                
                if (_keyBindingButtons.ContainsKey(action))
                {
                    _keyBindingButtons[action].Text = GetKeyName(keycode);
                }
            }
        }
        
        private void ResetKeyBinding(string action)
        {
            if (ControlSettingsApplier.DefaultKeyBindings.ContainsKey(action))
            {
                int defaultKey = ControlSettingsApplier.DefaultKeyBindings[action];
                UpdateKeyBinding(action, defaultKey);
            }
        }
        
        private void UpdateKeyBindingDisplay(Dictionary<string, int> bindings)
        {
            if (bindings == null) return;
            
            foreach (var kvp in bindings)
            {
                if (_keyBindingButtons.ContainsKey(kvp.Key))
                {
                    _keyBindingButtons[kvp.Key].Text = GetKeyName(kvp.Value);
                }
            }
        }
        
        private void UpdateSensitivityLabel(Label label, double value)
        {
            if (label != null)
                label.Text = $"{value:F2}";
        }
        
        private void UpdateDeadzoneLabel(int value)
        {
            if (ControllerDeadzoneLabel != null)
                ControllerDeadzoneLabel.Text = $"{value}%";
        }
        
        private string FormatActionName(string action)
        {
            // Convert snake_case to Title Case
            string[] parts = action.Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
                }
            }
            return string.Join(" ", parts);
        }
        
        private string GetKeyName(int keycode)
        {
            // Check if it's a mouse button (1-9)
            if (keycode >= (int)MouseButton.Left && keycode <= 9)
            {
                return ((MouseButton)keycode).ToString();
            }
            
            // Otherwise it's a keyboard key
            return ((Key)keycode).ToString();
        }
        
        #endregion
    }
}
