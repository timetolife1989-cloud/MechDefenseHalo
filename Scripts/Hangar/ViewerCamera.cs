using Godot;
using System;

namespace MechDefenseHalo.Hangar
{
    /// <summary>
    /// Camera controller for the 3D viewer
    /// Handles zoom and positioning
    /// </summary>
    public partial class ViewerCamera : Camera3D
    {
        [Export] private float minDistance = 2f;
        [Export] private float maxDistance = 20f;
        [Export] private float zoomSpeed = 0.5f;
        
        private float currentDistance = 5f;
        
        public void SetDistance(float distance)
        {
            currentDistance = Mathf.Clamp(distance, minDistance, maxDistance);
            UpdatePosition();
        }
        
        public void ZoomIn()
        {
            currentDistance -= zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            UpdatePosition();
        }
        
        public void ZoomOut()
        {
            currentDistance += zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            UpdatePosition();
        }
        
        private void UpdatePosition()
        {
            Position = new Vector3(0, 0, currentDistance);
            LookAt(Vector3.Zero, Vector3.Up);
        }
    }
}
