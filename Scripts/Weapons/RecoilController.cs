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
        
        private Vector3 _currentRecoil = Vector3.Zero;
        private Vector3 _targetRecoil = Vector3.Zero;
        
        public override void _Ready()
        {
            if (TargetCamera == null)
            {
                TargetCamera = GetViewport().GetCamera3D();
            }
            
            // Subscribe to recoil events
            EventBus.On(EventBus.WeaponRecoil, OnWeaponRecoil);
        }
        
        public override void _Process(double delta)
        {
            float deltaF = (float)delta;
            
            // Smoothly recover from recoil
            _currentRecoil = _currentRecoil.Lerp(_targetRecoil, RecoilRecoverySpeed * deltaF);
            
            // Apply recoil to camera
            if (TargetCamera != null && _currentRecoil != Vector3.Zero)
            {
                TargetCamera.RotationDegrees = new Vector3(
                    TargetCamera.RotationDegrees.X + _currentRecoil.X,
                    TargetCamera.RotationDegrees.Y + _currentRecoil.Y,
                    TargetCamera.RotationDegrees.Z + _currentRecoil.Z
                );
            }
            
            // Reset target recoil
            _targetRecoil = Vector3.Zero;
        }
        
        public override void _ExitTree()
        {
            EventBus.Off(EventBus.WeaponRecoil, OnWeaponRecoil);
        }
        
        private void OnWeaponRecoil(object data)
        {
            float intensity = data is float f ? f : 0.01f;
            
            // Add random recoil
            float recoilX = Mathf.Min(-intensity * 10f, -MaxRecoilAngle);
            float recoilY = (GD.Randf() - 0.5f) * intensity * 5f;
            
            _currentRecoil += new Vector3(recoilX, recoilY, 0);
        }
    }
}
