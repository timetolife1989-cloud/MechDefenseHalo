using Godot;
using System;

namespace MechDefenseHalo.Mobile
{
    /// <summary>
    /// Performance monitoring overlay for mobile debugging
    /// </summary>
    public partial class PerformanceMonitor : Control
    {
        private Label fpsLabel;
        private Label memoryLabel;
        private Label drawCallsLabel;
        
        [Export] public bool ShowMonitor { get; set; } = true;
        
        public override void _Ready()
        {
            fpsLabel = GetNode<Label>("FPSLabel");
            memoryLabel = GetNode<Label>("MemoryLabel");
            drawCallsLabel = GetNode<Label>("DrawCallsLabel");
            
            Visible = ShowMonitor;
            
            // Position in top-left corner
            Position = new Vector2(10, 10);
            
            GD.Print("PerformanceMonitor initialized");
        }
        
        public override void _Process(double delta)
        {
            if (!Visible || !ShowMonitor)
                return;
                
            if (fpsLabel != null)
            {
                fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
            }
            
            if (memoryLabel != null)
            {
                long memoryBytes = (long)OS.GetStaticMemoryUsage();
                float memoryMB = memoryBytes / 1048576.0f;
                memoryLabel.Text = $"MEM: {memoryMB:F1} MB";
            }
            
            if (drawCallsLabel != null)
            {
                int drawCalls = (int)Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame);
                drawCallsLabel.Text = $"Draw: {drawCalls}";
            }
        }
        
        public void ToggleMonitor()
        {
            ShowMonitor = !ShowMonitor;
            Visible = ShowMonitor;
        }
    }
}
