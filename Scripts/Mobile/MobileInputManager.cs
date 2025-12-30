using Godot;
using System;

namespace MechDefenseHalo.Mobile
{
    /// <summary>
    /// Manages mobile input and automatically enables touch controls on mobile platforms
    /// </summary>
    public partial class MobileInputManager : Node
    {
        private TouchController touchController;
        private bool isMobilePlatform = false;
        
        public override void _Ready()
        {
            string osName = OS.GetName();
            isMobilePlatform = osName == "Android" || osName == "iOS";
            
            GD.Print($"MobileInputManager: Platform = {osName}, IsMobile = {isMobilePlatform}");
            
            if (isMobilePlatform)
            {
                EnableTouchControls();
            }
        }
        
        private void EnableTouchControls()
        {
            // Load the touch controls scene
            var touchScene = ResourceLoader.Load<PackedScene>("res://Scenes/Mobile/TouchControls.tscn");
            if (touchScene != null)
            {
                touchController = touchScene.Instantiate<TouchController>();
                GetTree().Root.AddChild(touchController);
                GD.Print("Touch controls enabled");
            }
            else
            {
                GD.PrintErr("Failed to load TouchControls.tscn");
            }
        }
        
        /// <summary>
        /// Get movement input from touch controls or keyboard
        /// </summary>
        public Vector2 GetMovementInput()
        {
            if (touchController != null)
            {
                return touchController.MovementInput;
            }
            
            // Fallback to keyboard input
            return Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        }
        
        /// <summary>
        /// Check if fire button is pressed (touch or mouse)
        /// </summary>
        public bool IsFirePressed()
        {
            if (touchController != null)
            {
                return touchController.IsFirePressed;
            }
            
            // Fallback to mouse/keyboard input
            return Input.IsActionPressed("fire");
        }
        
        /// <summary>
        /// Check if this is a mobile platform
        /// </summary>
        public bool IsMobilePlatform()
        {
            return isMobilePlatform;
        }
    }
}
