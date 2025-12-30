using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Weapons
{
    /// <summary>
    /// Handles camera recoil and weapon kickback effects.
    /// </summary>
    public partial class RecoilController : Node
    {
        #region Exported Properties
        
        [Export] public Camera3D TargetCamera { get; set; }
        [Export] public float RecoilRecoverySpeed { get; set; } = 5f;
        [Export] public float MaxRecoilAngle { get; set; } = 2f;
        
        #endregion
        
        private Vector3 _recoilOffset = Vector3.Zero;
        private Vector3 _originalRotation = Vector3.Zero;
        
        public override void _Ready()
        {
            if (TargetCamera == null)
            {
                TargetCamera = GetViewport().GetCamera3D();
            }
            
            if (TargetCamera != null)
            {
                _originalRotation = TargetCamera.RotationDegrees;
            }
            
            // Subscribe to recoil events
            EventBus.On(EventBus.WeaponRecoil, OnWeaponRecoil);
        }
        
        public override void _Process(double delta)
        {
            float deltaF = (float)delta;
            
            // Smoothly recover from recoil
            _recoilOffset = _recoilOffset.Lerp(Vector3.Zero, RecoilRecoverySpeed * deltaF);
            
            // Apply recoil offset to camera
            if (TargetCamera != null)
            {
                TargetCamera.RotationDegrees = _originalRotation + _recoilOffset;
                
                // Update original rotation if recoil is negligible
                if (_recoilOffset.LengthSquared() < 0.01f)
                {
                    _originalRotation = TargetCamera.RotationDegrees;
                    _recoilOffset = Vector3.Zero;
                }
            }
        }
        
        public override void _ExitTree()
        {
            EventBus.Off(EventBus.WeaponRecoil, OnWeaponRecoil);
        }
        
        private void OnWeaponRecoil(object data)
        {
            float intensity = data is float f ? f : 0.01f;
            
            // Add random recoil as offset
            float recoilX = Mathf.Min(-intensity * 10f, -MaxRecoilAngle);
            float recoilY = (GD.Randf() - 0.5f) * intensity * 5f;
            
            _recoilOffset += new Vector3(recoilX, recoilY, 0);
            
            // Clamp recoil offset
            _recoilOffset.X = Mathf.Clamp(_recoilOffset.X, -MaxRecoilAngle, MaxRecoilAngle);
            _recoilOffset.Y = Mathf.Clamp(_recoilOffset.Y, -MaxRecoilAngle, MaxRecoilAngle);
        }
    }
}
