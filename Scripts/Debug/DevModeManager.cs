using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Debug
{
    /// <summary>
    /// Manages debug mode and hotkey shortcuts
    /// Provides quick access to common debug functions
    /// </summary>
    public partial class DevModeManager : Node
    {
        #region Private Fields

        private bool _devModeEnabled = false;
        private Dictionary<Key, Action> _hotkeys;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Only enable in debug builds
            #if !DEBUG && !TOOLS
            QueueFree();
            return;
            #endif

            RegisterHotkeys();
            GD.Print("DevModeManager initialized - Debug hotkeys active");
            PrintHotkeys();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (_hotkeys.ContainsKey(keyEvent.Keycode))
                {
                    _hotkeys[keyEvent.Keycode]();
                    GetViewport().SetInputAsHandled();
                }
            }
        }

        #endregion

        #region Hotkey Registration

        private void RegisterHotkeys()
        {
            _hotkeys = new Dictionary<Key, Action>
            {
                [Key.F1] = ToggleGodMode,
                [Key.F2] = QuickLevelUp,
                [Key.F3] = SpawnEnemyAtCursor,
                [Key.F4] = GiveTestItems,
                [Key.F5] = SkipWave,
                [Key.F6] = ToggleSlowMotion,
                // F7 and F8 are handled by FreeCamera and PerformanceProfiler
            };
        }

        private void PrintHotkeys()
        {
            GD.Print("=== Debug Hotkeys ===");
            GD.Print("F1 - Toggle God Mode");
            GD.Print("F2 - Level Up (+1)");
            GD.Print("F3 - Spawn Enemy at Cursor");
            GD.Print("F4 - Give Test Items");
            GD.Print("F5 - Skip to Next Wave");
            GD.Print("F6 - Toggle Slow Motion");
            GD.Print("F7 - Free Camera");
            GD.Print("F8 - Performance Stats");
            GD.Print("~ (Tilde) - Debug Console");
            GD.Print("====================");
        }

        #endregion

        #region Hotkey Actions

        private void ToggleGodMode()
        {
            var commands = GetDebugCommands();
            if (commands != null)
            {
                commands.Execute("god");
            }
        }

        private void QuickLevelUp()
        {
            var commands = GetDebugCommands();
            if (commands != null)
            {
                commands.Execute("levelup 1");
            }
        }

        private void SpawnEnemyAtCursor()
        {
            var commands = GetDebugCommands();
            if (commands != null)
            {
                // Spawn a random enemy type
                string[] enemyTypes = { "Grunt", "Tank", "Flyer", "Swarm", "Shooter" };
                string randomType = enemyTypes[GD.Randi() % enemyTypes.Length];
                commands.Execute($"spawn {randomType}");
            }
        }

        private void GiveTestItems()
        {
            var commands = GetDebugCommands();
            if (commands != null)
            {
                commands.Execute("credits 5000");
                commands.Execute("cores 500");
                GD.Print("Gave test resources");
            }
        }

        private void SkipWave()
        {
            var commands = GetDebugCommands();
            if (commands != null)
            {
                commands.Execute("skipwave");
            }
        }

        private void ToggleSlowMotion()
        {
            if (Engine.TimeScale == 1.0)
            {
                Engine.TimeScale = 0.3;
                GD.Print("Slow motion enabled (0.3x)");
            }
            else
            {
                Engine.TimeScale = 1.0;
                GD.Print("Normal time scale");
            }
        }

        #endregion

        #region Helper Methods

        private DebugCommands GetDebugCommands()
        {
            return GetTree().Root.GetNodeOrNull<DebugCommands>("DebugCommands");
        }

        #endregion
    }
}
