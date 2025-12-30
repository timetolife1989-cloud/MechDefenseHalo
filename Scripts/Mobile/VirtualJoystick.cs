using Godot;
using System;

namespace MechDefenseHalo.Mobile
{
    /// <summary>
    /// Virtual joystick for mobile touch controls
    /// </summary>
    public partial class VirtualJoystick : Control
    {
        [Signal] public delegate void JoystickMovedEventHandler(Vector2 direction);
        
        [Export] public float MaxDistance { get; set; } = 50f;
        
        private TextureRect background;
        private TextureRect knob;
        private Vector2 knobCenter;
        private bool isTouching = false;
        private int currentTouchIndex = -1;
        
        public override void _Ready()
        {
            background = GetNode<TextureRect>("Background");
            knob = GetNode<TextureRect>("Knob");
            
            if (background != null)
            {
                knobCenter = background.Size / 2;
            }
            
            // Make sure the control is visible and can receive input
            MouseFilter = MouseFilterEnum.Stop;
            
            ResetKnob();
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
                    UpdateKnobPosition(touch.Position);
                }
                else if (!touch.Pressed && touch.Index == currentTouchIndex)
                {
                    isTouching = false;
                    currentTouchIndex = -1;
                    ResetKnob();
                }
            }
            
            if (@event is InputEventScreenDrag drag && isTouching && drag.Index == currentTouchIndex)
            {
                UpdateKnobPosition(drag.Position);
            }
        }
        
        private void UpdateKnobPosition(Vector2 touchPosition)
        {
            if (background == null || knob == null)
                return;
            
            // Convert from global screen coordinates to center-relative joystick coordinates:
            // 1. Subtract GlobalPosition to get position relative to joystick's top-left
            // 2. Subtract knobCenter to make coordinates relative to joystick center
            // Result: (0,0) is joystick center, positive X is right, positive Y is down
            Vector2 localPos = touchPosition - GlobalPosition - knobCenter;
            float distance = localPos.Length();
            
            // Clamp the knob position to stay within the max radius
            if (distance > MaxDistance)
            {
                localPos = localPos.Normalized() * MaxDistance;
            }
            
            // Update knob visual position
            knob.Position = knobCenter + localPos - knob.Size / 2;
            
            // Calculate direction and strength for input
            // Handle case where touch is exactly at center (distance == 0)
            Vector2 direction = distance > 0 ? localPos.Normalized() : Vector2.Zero;
            float strength = Mathf.Clamp(distance / MaxDistance, 0f, 1f);
            
            EmitSignal(SignalName.JoystickMoved, direction * strength);
        }
        
        private void ResetKnob()
        {
            if (knob != null && background != null)
            {
                knob.Position = knobCenter - knob.Size / 2;
                EmitSignal(SignalName.JoystickMoved, Vector2.Zero);
            }
        }
    }
}
