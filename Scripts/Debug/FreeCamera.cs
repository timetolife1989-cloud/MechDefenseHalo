using Godot;
using System;

namespace MechDefenseHalo.Debug
{
    /// <summary>
    /// Free camera for debug inspection and scene exploration
    /// Toggle with F7 key
    /// </summary>
    public partial class FreeCamera : Camera3D
    {
        #region Private Fields

        private bool _isActive = false;
        private float _moveSpeed = 10f;
        private float _fastMultiplier = 3f;
        private Vector2 _mouseSensitivity = new Vector2(0.1f, 0.1f);
        private Camera3D _originalCamera;
        private Input.MouseModeEnum _originalMouseMode;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Only enable in debug builds
            #if !DEBUG && !TOOLS
            QueueFree();
            return;
            #endif

            // Start disabled
            Current = false;
            ProcessMode = ProcessModeEnum.Always;
            
            GD.Print("FreeCamera ready - Press F7 to toggle");
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.F7)
                {
                    ToggleFreeCamera();
                    GetViewport().SetInputAsHandled();
                }
            }
            
            if (_isActive && @event is InputEventMouseMotion mouseMotion)
            {
                RotateY(-mouseMotion.Relative.X * _mouseSensitivity.X * 0.01f);
                RotateX(-mouseMotion.Relative.Y * _mouseSensitivity.Y * 0.01f);
                
                // Clamp vertical rotation
                Vector3 rot = RotationDegrees;
                rot.X = Mathf.Clamp(rot.X, -90, 90);
                RotationDegrees = rot;
            }
        }

        public override void _Process(double delta)
        {
            if (!_isActive) return;
            
            Vector3 velocity = Vector3.Zero;
            float speed = _moveSpeed;
            
            if (Input.IsKeyPressed(Key.Shift))
            {
                speed *= _fastMultiplier;
            }
            
            if (Input.IsKeyPressed(Key.W))
                velocity -= Transform.Basis.Z;
            if (Input.IsKeyPressed(Key.S))
                velocity += Transform.Basis.Z;
            if (Input.IsKeyPressed(Key.A))
                velocity -= Transform.Basis.X;
            if (Input.IsKeyPressed(Key.D))
                velocity += Transform.Basis.X;
            if (Input.IsKeyPressed(Key.Q))
                velocity -= Transform.Basis.Y;
            if (Input.IsKeyPressed(Key.E))
                velocity += Transform.Basis.Y;
            
            if (velocity.Length() > 0)
            {
                GlobalPosition += velocity.Normalized() * speed * (float)delta;
            }
        }

        #endregion

        #region Private Methods

        private void ToggleFreeCamera()
        {
            _isActive = !_isActive;
            Current = _isActive;
            
            if (_isActive)
            {
                // Save original camera and mouse mode
                _originalCamera = GetViewport().GetCamera3D();
                _originalMouseMode = Input.MouseMode;
                
                // Position at current camera location
                if (_originalCamera != null && _originalCamera != this)
                {
                    GlobalTransform = _originalCamera.GlobalTransform;
                }
                
                Input.MouseMode = Input.MouseModeEnum.Captured;
                GD.Print("Free camera activated - WASD to move, QE for up/down, Shift for speed boost");
            }
            else
            {
                // Restore original camera
                if (_originalCamera != null && IsInstanceValid(_originalCamera))
                {
                    _originalCamera.Current = true;
                }
                
                Input.MouseMode = _originalMouseMode;
                GD.Print("Free camera deactivated");
            }
        }

        #endregion
    }
}
