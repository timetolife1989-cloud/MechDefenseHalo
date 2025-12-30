using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Animation
{
    /// <summary>
    /// Weapon swap animator that animates the entire mech body during weapon changes.
    /// Provides natural weight shift, torso twist, arm movement, and camera tilt.
    /// </summary>
    public partial class WeaponSwapAnimator : Node
    {
        #region Exported Properties
        
        [Export] private Node3D mechBody;
        [Export] private Node3D rightArm;
        [Export] private Camera3D camera;
        
        [Export] private float swapDuration = 0.5f;
        
        #endregion
        
        #region Private Fields
        
        private bool isSwapping = false;
        private float swapProgress = 0f;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            EventBus.On(EventBus.WeaponSwitched, OnWeaponSwap);
        }
        
        public override void _ExitTree()
        {
            EventBus.Off(EventBus.WeaponSwitched, OnWeaponSwap);
        }
        
        public override void _Process(double delta)
        {
            if (isSwapping)
            {
                swapProgress += (float)delta / swapDuration;
                
                // Animate entire mech body
                AnimateBodyDuringSwap(swapProgress);
                
                // Animate arm
                AnimateArmDuringSwap(swapProgress);
                
                // Animate camera
                AnimateCameraDuringSwap(swapProgress);
                
                if (swapProgress >= 1.0f)
                {
                    isSwapping = false;
                    ResetTransforms();
                }
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnWeaponSwap(object data)
        {
            if (!isSwapping)
            {
                isSwapping = true;
                swapProgress = 0f;
            }
        }
        
        #endregion
        
        #region Animation Methods
        
        private void AnimateBodyDuringSwap(float progress)
        {
            if (mechBody == null)
                return;
                
            // Body weight shift
            float weightShift = Mathf.Sin(progress * Mathf.Pi) * 0.1f;
            mechBody.Position = new Vector3(0, weightShift, 0);
            
            // Torso twist
            float twist = Mathf.Sin(progress * Mathf.Pi) * 15f;
            mechBody.RotationDegrees = new Vector3(0, twist, 0);
        }
        
        private void AnimateArmDuringSwap(float progress)
        {
            if (rightArm == null)
                return;
                
            // Arm raise and rotate
            float raise = Mathf.Sin(progress * Mathf.Pi) * 30f;
            rightArm.RotationDegrees = new Vector3(-raise, 0, 0);
        }
        
        private void AnimateCameraDuringSwap(float progress)
        {
            if (camera == null)
                return;
                
            // Camera tilt
            float tilt = Mathf.Sin(progress * Mathf.Pi) * 5f;
            camera.Rotation = new Vector3(Mathf.DegToRad(tilt), camera.Rotation.Y, camera.Rotation.Z);
        }
        
        private void ResetTransforms()
        {
            if (mechBody != null)
            {
                mechBody.Position = Vector3.Zero;
                mechBody.RotationDegrees = Vector3.Zero;
            }
            
            if (rightArm != null)
            {
                rightArm.RotationDegrees = Vector3.Zero;
            }
            
            if (camera != null)
            {
                camera.Rotation = new Vector3(0, camera.Rotation.Y, 0);
            }
        }
        
        #endregion
    }
}
