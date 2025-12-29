using Godot;
using System;

namespace MechDefenseHalo.Debug
{
    /// <summary>
    /// Real-time performance profiler and monitor
    /// Toggle with F8 key
    /// </summary>
    public partial class PerformanceProfiler : Control
    {
        #region Private Fields

        private Label _fpsLabel;
        private Label _frameTimeLabel;
        private Label _drawCallsLabel;
        private Label _verticesLabel;
        private Label _memoryLabel;
        private Label _enemyCountLabel;
        private Label _physicsTimeLabel;
        private Label _renderTimeLabel;
        
        private bool _isVisible = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Only enable in debug/editor builds
            #if !DEBUG
            #if !TOOLS
            QueueFree();
            return;
            #endif
            #endif

            // Get label references
            var vbox = GetNode<VBoxContainer>("Panel/VBoxContainer");
            _fpsLabel = vbox.GetNode<Label>("FPS");
            _frameTimeLabel = vbox.GetNode<Label>("FrameTime");
            _drawCallsLabel = vbox.GetNode<Label>("DrawCalls");
            _verticesLabel = vbox.GetNode<Label>("Vertices");
            _memoryLabel = vbox.GetNode<Label>("Memory");
            _enemyCountLabel = vbox.GetNode<Label>("EnemyCount");
            _physicsTimeLabel = vbox.GetNode<Label>("PhysicsTime");
            _renderTimeLabel = vbox.GetNode<Label>("RenderTime");
            
            // Start hidden
            Visible = false;
            
            GD.Print("PerformanceProfiler ready - Press F8 to toggle");
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.F8)
                {
                    ToggleProfiler();
                    GetViewport().SetInputAsHandled();
                }
            }
        }

        public override void _Process(double delta)
        {
            if (!_isVisible) return;

            UpdateStats(delta);
        }

        #endregion

        #region Private Methods

        private void ToggleProfiler()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                GD.Print("Performance profiler activated");
            }
            else
            {
                GD.Print("Performance profiler deactivated");
            }
        }

        private void UpdateStats(double delta)
        {
            // FPS
            int fps = Engine.GetFramesPerSecond();
            _fpsLabel.Text = $"FPS: {fps}";
            
            // Color code FPS
            if (fps >= 60)
                _fpsLabel.Modulate = Colors.Green;
            else if (fps >= 30)
                _fpsLabel.Modulate = Colors.Yellow;
            else
                _fpsLabel.Modulate = Colors.Red;
            
            // Frame time
            float frameTime = (float)delta * 1000f;
            _frameTimeLabel.Text = $"Frame: {frameTime:F2}ms";
            
            // Draw calls
            long drawCalls = (long)Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame);
            _drawCallsLabel.Text = $"Draw Calls: {drawCalls}";
            
            // Vertices
            long vertices = (long)Performance.GetMonitor(Performance.Monitor.RenderVerticesInFrame);
            _verticesLabel.Text = $"Vertices: {vertices:N0}";
            
            // Memory
            long memoryUsage = (long)OS.GetStaticMemoryUsage() / 1048576;
            _memoryLabel.Text = $"Memory: {memoryUsage} MB";
            
            // Enemy count
            int enemyCount = GetTree().GetNodesInGroup("enemies").Count;
            _enemyCountLabel.Text = $"Enemies: {enemyCount}";
            
            // Physics time
            float physicsTime = (float)Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess) * 1000f;
            _physicsTimeLabel.Text = $"Physics: {physicsTime:F2}ms";
            
            // Render time
            float renderTime = (float)Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000f;
            _renderTimeLabel.Text = $"Render: {renderTime:F2}ms";
        }

        #endregion
    }
}
