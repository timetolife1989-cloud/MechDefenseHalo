using Godot;
using System;

namespace MechDefenseHalo.Hangar
{
    /// <summary>
    /// Handles model rotation in the 3D viewer
    /// Supports manual rotation and auto-rotate mode
    /// </summary>
    public partial class ModelRotator : Node
    {
        [Export] private float rotationSpeed = 0.5f;
        [Export] private bool autoRotate = false;
        [Export] private float autoRotateSpeed = 30f;
        
        private Node3D targetModel;
        private float currentRotationY = 0f;
        private float currentRotationX = 0f;
        
        public void SetTarget(Node3D model)
        {
            targetModel = model;
            currentRotationY = 0f;
            currentRotationX = 0f;
        }
        
        public void RotateModel(Vector2 mouseDelta)
        {
            if (targetModel == null) return;
            
            currentRotationY += mouseDelta.X * rotationSpeed;
            currentRotationX += mouseDelta.Y * rotationSpeed;
            
            // Clamp vertical rotation
            currentRotationX = Mathf.Clamp(currentRotationX, -80f, 80f);
            
            ApplyRotation();
        }
        
        public override void _Process(double delta)
        {
            if (autoRotate && targetModel != null)
            {
                currentRotationY += autoRotateSpeed * (float)delta;
                ApplyRotation();
            }
        }
        
        private void ApplyRotation()
        {
            if (targetModel == null) return;
            
            targetModel.RotationDegrees = new Vector3(
                currentRotationX,
                currentRotationY,
                0
            );
        }
        
        public void ResetRotation()
        {
            currentRotationY = 0f;
            currentRotationX = 0f;
            ApplyRotation();
        }
    }
}
