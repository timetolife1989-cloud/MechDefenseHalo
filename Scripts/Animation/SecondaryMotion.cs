using Godot;
using System;

namespace MechDefenseHalo.Animation
{
    /// <summary>
    /// Secondary motion system for cables, antennas, and other dangling parts.
    /// Applies lag and drag effects based on mech acceleration for realistic physics.
    /// </summary>
    public partial class SecondaryMotion : Node
    {
        #region Exported Properties
        
        [Export] private Godot.Collections.Array<Node3D> secondaryBones;
        
        #endregion
        
        #region Private Fields
        
        private Vector3 lastPosition;
        private Vector3 lastVelocity;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            if (GetParent() is Node3D parent)
            {
                lastPosition = parent.GlobalPosition;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        public void UpdateSecondaryBones(float delta)
        {
            if (!(GetParent() is Node3D parent))
                return;
                
            Vector3 currentPosition = parent.GlobalPosition;
            
            // Prevent division by zero
            if (delta <= 0)
                return;
                
            Vector3 velocity = (currentPosition - lastPosition) / delta;
            Vector3 acceleration = (velocity - lastVelocity) / delta;
            
            if (secondaryBones != null)
            {
                foreach (var bone in secondaryBones)
                {
                    if (bone != null)
                    {
                        // Apply drag/lag based on acceleration
                        Vector3 drag = -acceleration * 0.01f;
                        bone.Rotation += new Vector3(drag.Z, drag.X, 0);
                        
                        // Damping
                        bone.Rotation = bone.Rotation.Lerp(Vector3.Zero, delta * 5f);
                    }
                }
            }
            
            lastPosition = currentPosition;
            lastVelocity = velocity;
        }
        
        #endregion
    }
}
