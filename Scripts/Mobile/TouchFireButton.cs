using Godot;
using System;

namespace MechDefenseHalo.Mobile
{
    /// <summary>
    /// Touch-based fire button for mobile controls
    /// </summary>
    public partial class TouchFireButton : Control
    {
        [Signal] public delegate void PressedEventHandler();
        [Signal] public delegate void ReleasedEventHandler();
        
        private TextureRect buttonTexture;
        private bool isTouching = false;
        private int currentTouchIndex = -1;
        
        public override void _Ready()
        {
            buttonTexture = GetNode<TextureRect>("ButtonTexture");
            
            // Make sure the control is visible and can receive input
            MouseFilter = MouseFilterEnum.Stop;
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventScreenTouch touch)
            {
                Vector2 localPos = touch.Position - GlobalPosition;
                bool isInside = GetRect().HasPoint(localPos);
                
                if (touch.Pressed && isInside && !isTouching)
                {
                    isTouching = true;
                    currentTouchIndex = touch.Index;
                    EmitSignal(SignalName.Pressed);
                    
                    // Visual feedback
                    if (buttonTexture != null)
                    {
                        buttonTexture.Modulate = new Color(0.7f, 0.7f, 0.7f, 1.0f);
                    }
                }
                else if (!touch.Pressed && touch.Index == currentTouchIndex)
                {
                    isTouching = false;
                    currentTouchIndex = -1;
                    EmitSignal(SignalName.Released);
                    
                    // Reset visual feedback
                    if (buttonTexture != null)
                    {
                        buttonTexture.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    }
                }
            }
        }
    }
}
