using Godot;
using System;

namespace MechDefenseHalo.Debug
{
    /// <summary>
    /// Debug console for in-game command execution
    /// Toggled with tilde key (~)
    /// </summary>
    public partial class DebugConsole : Control
    {
        #region Private Fields

        private LineEdit _commandInput;
        private RichTextLabel _outputLog;
        private DebugCommands _commandExecutor;
        private bool _isVisible = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Only enable in debug/editor builds (disabled in release builds)
            #if !DEBUG && !TOOLS
            QueueFree();
            return;
            #endif

            _commandInput = GetNode<LineEdit>("Panel/VBoxContainer/CommandInput");
            _outputLog = GetNode<RichTextLabel>("Panel/VBoxContainer/ScrollContainer/OutputLog");
            
            // Get or create command executor
            _commandExecutor = GetNodeOrNull<DebugCommands>("/root/DebugCommands");
            if (_commandExecutor == null)
            {
                _commandExecutor = new DebugCommands();
                _commandExecutor.Name = "DebugCommands";
                GetTree().Root.AddChild(_commandExecutor);
            }
            
            _commandInput.TextSubmitted += OnCommandSubmitted;
            
            // Start hidden
            Visible = false;
            
            Log("[color=cyan]Debug Console Ready[/color]");
            Log("Press ~ (tilde) to open/close console");
            Log("Type 'help' for available commands");
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.Quoteleft) // Tilde key
                {
                    ToggleConsole();
                    GetViewport().SetInputAsHandled();
                }
            }
        }

        #endregion

        #region Private Methods

        private void ToggleConsole()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                _commandInput.GrabFocus();
            }
            else
            {
                _commandInput.ReleaseFocus();
            }
        }

        private void OnCommandSubmitted(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            Log($"[color=gray]> {command}[/color]");
            
            string result = _commandExecutor.Execute(command);
            Log(result);
            
            _commandInput.Clear();
        }

        public void Log(string message)
        {
            _outputLog.AppendText(message + "\n");
            
            // Auto-scroll to bottom
            CallDeferred(nameof(ScrollToBottom));
        }

        private void ScrollToBottom()
        {
            var scrollContainer = _outputLog.GetParent<ScrollContainer>();
            if (scrollContainer != null)
            {
                scrollContainer.ScrollVertical = (int)scrollContainer.GetVScrollBar().MaxValue;
            }
        }

        #endregion
    }
}
