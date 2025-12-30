using Godot;
using System;

namespace MechDefenseHalo.Mobile
{
    /// <summary>
    /// Main controller for touch-based input on mobile devices
    /// </summary>
    public partial class TouchController : Control
    {
        private VirtualJoystick leftJoystick;
        private TouchFireButton fireButton;
        
        public Vector2 MovementInput { get; private set; }
        public bool IsFirePressed { get; private set; }
        
        public override void _Ready()
        {
            leftJoystick = GetNode<VirtualJoystick>("LeftJoystick");
            fireButton = GetNode<TouchFireButton>("FireButton");
            
            if (leftJoystick != null)
            {
                leftJoystick.JoystickMoved += OnJoystickMoved;
            }
            
            if (fireButton != null)
            {
                fireButton.Pressed += OnFirePressed;
                fireButton.Released += OnFireReleased;
            }
            
            GD.Print("TouchController initialized");
        }
        
        private void OnJoystickMoved(Vector2 direction)
        {
            MovementInput = direction;
        }
        
        private void OnFirePressed()
        {
            IsFirePressed = true;
        }
        
        private void OnFireReleased()
        {
            IsFirePressed = false;
        }
        
        public override void _ExitTree()
        {
            // Cleanup signal connections
            if (leftJoystick != null)
            {
                leftJoystick.JoystickMoved -= OnJoystickMoved;
            }
            
            if (fireButton != null)
            {
                fireButton.Pressed -= OnFirePressed;
                fireButton.Released -= OnFireReleased;
            }
        }
    }
}
