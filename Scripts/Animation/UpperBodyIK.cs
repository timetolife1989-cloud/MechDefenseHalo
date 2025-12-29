using Godot;
using System;

namespace MechDefenseHalo.Animation
{
    /// <summary>
    /// Upper body IK system for smooth aim tracking.
    /// Rotates spine and head to follow crosshair/aim target independently.
    /// </summary>
    public partial class UpperBodyIK : Node
    {
        #region Exported Properties
        
        [Export] private Node3D spine;
        [Export] private Node3D head;
        [Export] private Node3D leftShoulder;
        [Export] private Node3D rightShoulder;
        
        [Export] private float aimSmoothness = 5.0f;
        
        #endregion
        
        #region Private Fields
        
        private Vector3 currentAimTarget;
        
        #endregion
        
        #region Public Methods
        
        public void UpdateAimTarget(float delta)
        {
            Vector3 targetPosition = GetCrosshairWorldPosition();
            currentAimTarget = currentAimTarget.Lerp(targetPosition, delta * aimSmoothness);
            
            // Rotate spine towards target
            if (spine != null)
            {
                Vector3 direction = (currentAimTarget - spine.GlobalPosition).Normalized();
                spine.LookAt(spine.GlobalPosition + direction, Vector3.Up);
            }
            
            // Rotate head independently
            if (head != null)
            {
                Vector3 headDirection = (currentAimTarget - head.GlobalPosition).Normalized();
                head.LookAt(head.GlobalPosition + headDirection, Vector3.Up);
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private Vector3 GetCrosshairWorldPosition()
        {
            var camera = GetViewport().GetCamera3D();
            if (camera == null)
                return Vector3.Zero;
                
            var screenCenter = GetViewport().GetVisibleRect().Size / 2;
            
            var from = camera.ProjectRayOrigin(screenCenter);
            var to = from + camera.ProjectRayNormal(screenCenter) * 1000f;
            
            var spaceState = GetWorld3D().DirectSpaceState;
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            var result = spaceState.IntersectRay(query);
            
            if (result.Count > 0)
            {
                return (Vector3)result["position"];
            }
            
            return to;
        }
        
        #endregion
    }
}
