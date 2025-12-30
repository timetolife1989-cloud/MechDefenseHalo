using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.UI.Settings
{
    /// <summary>
    /// Control settings component
    /// Manages key bindings, mouse sensitivity, and controller settings
    /// </summary>
    public partial class ControlSettings : Control
    {
        #region Export Variables
        
        [Export] private HSlider mouseSensitivitySlider;
        [Export] private Label mouseSensitivityLabel;
        [Export] private CheckButton invertYCheck;
        
        [Export] private HSlider controllerSensitivitySlider;
        [Export] private Label controllerSensitivityLabel;
        [Export] private HSlider controllerDeadzoneSlider;
        [Export] private Label controllerDeadzoneLabel;
        
        [Export] private VBoxContainer keyBindingContainer;
        
        #endregion
        
        #region Private Fields
        
        private Dictionary<string, Button> keyBindingButtons = new Dictionary<string, Button>();
        private string waitingForKey = null;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            ConnectSignals();
            SetupKeyBindings();
        }
        
        public override void _Input(InputEvent @event)
        {
            if (waitingForKey != null)
            {
                if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
                {
                    UpdateKeyBinding(waitingForKey, (int)keyEvent.Keycode);
                    waitingForKey = null;
                    AcceptEvent();
                }
                else if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
                {
                    UpdateKeyBinding(waitingForKey, (int)mouseEvent.ButtonIndex);
                    waitingForKey = null;
                    AcceptEvent();
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        public void LoadSettings()
        {
            var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
            if (settingsManager == null || settingsManager.CurrentSettings == null)
            {
                GD.PrintErr("SettingsManager not available!");
                return;
            }
            
            var controls = settingsManager.CurrentSettings.Controls;
            
            // Mouse sensitivity
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.Value = controls.MouseSensitivity;
                UpdateSensitivityLabel(mouseSensitivityLabel, controls.MouseSensitivity);
            }
            
            // Invert Y
            if (invertYCheck != null)
            {
                invertYCheck.ButtonPressed = controls.InvertY;
            }
            
            // Controller sensitivity
            if (controllerSensitivitySlider != null)
            {
                controllerSensitivitySlider.Value = controls.ControllerSensitivity;
                UpdateSensitivityLabel(controllerSensitivityLabel, controls.ControllerSensitivity);
            }
            
            // Controller deadzone
            if (controllerDeadzoneSlider != null)
            {
                controllerDeadzoneSlider.Value = controls.ControllerDeadzone;
                UpdateDeadzoneLabel(controls.ControllerDeadzone);
            }
            
            // Update key binding display
            UpdateKeyBindingDisplay(controls.KeyBindings);
        }
        
        public void ApplySettings()
        {
            var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
            if (settingsManager == null || settingsManager.CurrentSettings == null)
            {
                GD.PrintErr("SettingsManager not available!");
                return;
            }
            
            var controls = settingsManager.CurrentSettings.Controls;
            
            // Mouse sensitivity
            if (mouseSensitivitySlider != null)
            {
                controls.MouseSensitivity = (float)mouseSensitivitySlider.Value;
            }
            
            // Invert Y
            if (invertYCheck != null)
            {
                controls.InvertY = invertYCheck.ButtonPressed;
            }
            
            // Controller sensitivity
            if (controllerSensitivitySlider != null)
            {
                controls.ControllerSensitivity = (float)controllerSensitivitySlider.Value;
            }
            
            // Controller deadzone
            if (controllerDeadzoneSlider != null)
            {
                controls.ControllerDeadzone = (int)controllerDeadzoneSlider.Value;
            }
            
            // Apply control settings through the applier
            ControlSettingsApplier.Apply(controls);
            
            GD.Print("Control settings applied");
        }
        
        public void ResetToDefaults()
        {
            var defaults = new ControlSettingsData();
            
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.Value = defaults.MouseSensitivity;
                UpdateSensitivityLabel(mouseSensitivityLabel, defaults.MouseSensitivity);
            }
            
            if (invertYCheck != null)
            {
                invertYCheck.ButtonPressed = defaults.InvertY;
            }
            
            if (controllerSensitivitySlider != null)
            {
                controllerSensitivitySlider.Value = defaults.ControllerSensitivity;
                UpdateSensitivityLabel(controllerSensitivityLabel, defaults.ControllerSensitivity);
            }
            
            if (controllerDeadzoneSlider != null)
            {
                controllerDeadzoneSlider.Value = defaults.ControllerDeadzone;
                UpdateDeadzoneLabel(defaults.ControllerDeadzone);
            }
            
            // Reset key bindings to defaults
            UpdateKeyBindingDisplay(ControlSettingsApplier.DefaultKeyBindings);
            
            // Update the actual settings
            var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
            if (settingsManager != null && settingsManager.CurrentSettings != null)
            {
                settingsManager.CurrentSettings.Controls.KeyBindings = new Dictionary<string, int>(ControlSettingsApplier.DefaultKeyBindings);
            }
            
            GD.Print("Control settings reset to defaults");
        }
        
        #endregion
        
        #region Private Methods
        
        private void ConnectSignals()
        {
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.ValueChanged += (value) => UpdateSensitivityLabel(mouseSensitivityLabel, (float)value);
            }
            
            if (controllerSensitivitySlider != null)
            {
                controllerSensitivitySlider.ValueChanged += (value) => UpdateSensitivityLabel(controllerSensitivityLabel, (float)value);
            }
            
            if (controllerDeadzoneSlider != null)
            {
                controllerDeadzoneSlider.ValueChanged += (value) => UpdateDeadzoneLabel((int)value);
            }
        }
        
        private void SetupKeyBindings()
        {
            if (keyBindingContainer == null) return;
            
            // Create key binding rows for each action
            foreach (var action in ControlSettingsApplier.DefaultKeyBindings.Keys)
            {
                var row = new HBoxContainer();
                row.CustomMinimumSize = new Vector2(0, 40);
                
                // Action label
                var label = new Label();
                label.Text = FormatActionName(action);
                label.CustomMinimumSize = new Vector2(200, 0);
                label.VerticalAlignment = VerticalAlignment.Center;
                row.AddChild(label);
                
                // Spacer
                var spacer = new Control();
                spacer.CustomMinimumSize = new Vector2(20, 0);
                spacer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                row.AddChild(spacer);
                
                // Key button
                var button = new Button();
                button.Text = "Click to bind";
                button.CustomMinimumSize = new Vector2(150, 0);
                button.Pressed += () => OnKeyBindingButtonPressed(action);
                row.AddChild(button);
                
                // Reset button
                var resetBtn = new Button();
                resetBtn.Text = "Reset";
                resetBtn.CustomMinimumSize = new Vector2(80, 0);
                resetBtn.Pressed += () => ResetKeyBinding(action);
                row.AddChild(resetBtn);
                
                keyBindingContainer.AddChild(row);
                keyBindingButtons[action] = button;
            }
        }
        
        private void OnKeyBindingButtonPressed(string action)
        {
            waitingForKey = action;
            
            if (keyBindingButtons.ContainsKey(action))
            {
                keyBindingButtons[action].Text = "Press key...";
            }
        }
        
        private void UpdateKeyBinding(string action, int keycode)
        {
            var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
            if (settingsManager != null && settingsManager.CurrentSettings != null)
            {
                var controls = settingsManager.CurrentSettings.Controls;
                
                if (controls.KeyBindings == null)
                {
                    controls.KeyBindings = new Dictionary<string, int>(ControlSettingsApplier.DefaultKeyBindings);
                }
                
                controls.KeyBindings[action] = keycode;
                
                if (keyBindingButtons.ContainsKey(action))
                {
                    keyBindingButtons[action].Text = GetKeyName(keycode);
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
                if (keyBindingButtons.ContainsKey(kvp.Key))
                {
                    keyBindingButtons[kvp.Key].Text = GetKeyName(kvp.Value);
                }
            }
        }
        
        private void UpdateSensitivityLabel(Label label, float value)
        {
            if (label != null)
            {
                label.Text = $"{value:F2}";
            }
        }
        
        private void UpdateDeadzoneLabel(int value)
        {
            if (controllerDeadzoneLabel != null)
            {
                controllerDeadzoneLabel.Text = $"{value}%";
            }
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
                return $"Mouse {keycode}";
            }
            
            // Otherwise it's a keyboard key
            return ((Key)keycode).ToString().Replace("Key", "");
        }
        
        #endregion
    }
}
