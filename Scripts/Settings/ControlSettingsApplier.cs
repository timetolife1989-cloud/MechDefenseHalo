using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.Settings
{
    /// <summary>
    /// Applies control settings and manages key bindings
    /// </summary>
    public static class ControlSettingsApplier
    {
        public static Dictionary<string, int> DefaultKeyBindings = new Dictionary<string, int>
        {
            {"move_forward", (int)Key.W},
            {"move_backward", (int)Key.S},
            {"move_left", (int)Key.A},
            {"move_right", (int)Key.D},
            {"sprint", (int)Key.Shift},
            {"fire", (int)MouseButton.Left},
            {"reload", (int)Key.R},
            {"ability", (int)Key.E},
            {"shield", (int)Key.Q},
            {"weapon_switch", (int)Key.Tab},
            {"weapon_1", (int)Key.Key1},
            {"weapon_2", (int)Key.Key2},
            {"weapon_3", (int)Key.Key3}
        };
        
        // Global storage for runtime values
        public static float MouseSensitivity { get; set; } = 1.0f;
        public static bool InvertY { get; set; } = false;
        public static float ControllerSensitivity { get; set; } = 1.0f;
        public static int ControllerDeadzone { get; set; } = 15;
        
        public static void Apply(ControlSettingsData settings)
        {
            // If no custom bindings, use defaults
            if (settings.KeyBindings == null || settings.KeyBindings.Count == 0)
            {
                settings.KeyBindings = new Dictionary<string, int>(DefaultKeyBindings);
            }
            
            // Apply key bindings to InputMap
            foreach (var binding in settings.KeyBindings)
            {
                if (InputMap.HasAction(binding.Key))
                {
                    InputMap.ActionEraseEvents(binding.Key);
                    
                    // Check if this is a mouse button (1-9) or keyboard key
                    // Mouse buttons are typically in range 1-9, keyboard keys start higher
                    if (binding.Value >= (int)MouseButton.Left && binding.Value <= 9)
                    {
                        var inputMouse = new InputEventMouseButton();
                        inputMouse.ButtonIndex = (MouseButton)binding.Value;
                        InputMap.ActionAddEvent(binding.Key, inputMouse);
                    }
                    else
                    {
                        var inputKey = new InputEventKey();
                        inputKey.Keycode = (Key)binding.Value;
                        InputMap.ActionAddEvent(binding.Key, inputKey);
                    }
                }
            }
            
            // Store global settings
            MouseSensitivity = settings.MouseSensitivity;
            InvertY = settings.InvertY;
            ControllerSensitivity = settings.ControllerSensitivity;
            ControllerDeadzone = settings.ControllerDeadzone;
            
            GD.Print($"Applied control settings: MouseSensitivity={settings.MouseSensitivity}, " +
                     $"InvertY={settings.InvertY}, KeyBindings={settings.KeyBindings.Count}");
        }
    }
}
