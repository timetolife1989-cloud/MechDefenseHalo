using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Animation
{
    /// <summary>
    /// Camera reaction system for weapon recoil and hit feedback.
    /// Provides procedural camera shake for firing and taking damage.
    /// </summary>
    public partial class CameraReaction : Camera3D
    {
        #region Exported Properties
        
        [Export] private float recoilStrength = 0.1f;
        [Export] private float returnSpeed = 10f;
        
        #endregion
        
        #region Private Fields
        
        private Vector3 recoilOffset = Vector3.Zero;
        private Vector3 originalPosition = Vector3.Zero;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            originalPosition = Position;
            EventBus.On(EventBus.WeaponFired, OnWeaponFired);
            EventBus.On(EventBus.PlayerHit, OnPlayerHit);
        }
        
        public override void _ExitTree()
        {
            EventBus.Off(EventBus.WeaponFired, OnWeaponFired);
            EventBus.Off(EventBus.PlayerHit, OnPlayerHit);
        }
        
        public override void _Process(double delta)
        {
            // Smooth return to zero
            recoilOffset = recoilOffset.Lerp(Vector3.Zero, (float)delta * returnSpeed);
            
            // Apply offset
            Position = originalPosition + recoilOffset;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnWeaponFired(object data)
        {
            // Recoil kick
            recoilOffset += new Vector3(
                GD.Randf() * recoilStrength - recoilStrength / 2,
                recoilStrength,
                -recoilStrength * 0.5f
            );
        }
        
        private void OnPlayerHit(object data)
        {
            // Hit shake
            recoilOffset += new Vector3(
                GD.Randf() * 0.2f - 0.1f,
                GD.Randf() * 0.2f - 0.1f,
                0
            );
        }
        
        #endregion
    }
}
