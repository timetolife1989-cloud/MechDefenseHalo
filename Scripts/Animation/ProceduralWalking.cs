using Godot;
using System;

namespace MechDefenseHalo.Animation
{
    /// <summary>
    /// Procedural walking system that animates mech feet without baked animations.
    /// Implements natural stepping with arc motion based on body movement.
    /// </summary>
    public partial class ProceduralWalking : Node
    {
        #region Exported Properties
        
        [Export] private float stepHeight = 0.3f;
        [Export] private float stepDistance = 1.0f;
        [Export] private float stepSpeed = 5.0f;
        
        #endregion
        
        #region Private Fields
        
        private bool isLeftFootMoving = false;
        private Vector3 leftFootTarget;
        private Vector3 rightFootTarget;
        
        private float leftFootProgress = 1.0f;
        private float rightFootProgress = 1.0f;
        
        #endregion
        
        #region Public Methods
        
        public void UpdateFootTargets(Node3D leftFoot, Node3D rightFoot, float delta)
        {
            if (leftFoot == null || rightFoot == null)
                return;
                
            Vector3 bodyPosition = GetParent<Node3D>().GlobalPosition;
            Vector3 bodyVelocity = GetBodyVelocity();
            
            // Only move feet when moving
            if (bodyVelocity.Length() < 0.1f)
            {
                return;
            }
            
            // Check if foot needs to step
            if (leftFootProgress >= 1.0f && ShouldStep(leftFoot, bodyPosition))
            {
                StartStep(leftFoot, bodyPosition, bodyVelocity, true);
            }
            
            if (rightFootProgress >= 1.0f && ShouldStep(rightFoot, bodyPosition))
            {
                StartStep(rightFoot, bodyPosition, bodyVelocity, false);
            }
            
            // Update stepping animation
            if (leftFootProgress < 1.0f)
            {
                leftFootProgress += delta * stepSpeed;
                leftFoot.GlobalPosition = CalculateStepPosition(leftFootTarget, leftFootProgress);
            }
            
            if (rightFootProgress < 1.0f)
            {
                rightFootProgress += delta * stepSpeed;
                rightFoot.GlobalPosition = CalculateStepPosition(rightFootTarget, rightFootProgress);
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private bool ShouldStep(Node3D foot, Vector3 bodyPosition)
        {
            float distance = foot.GlobalPosition.DistanceTo(bodyPosition);
            return distance > stepDistance;
        }
        
        private void StartStep(Node3D foot, Vector3 bodyPosition, Vector3 velocity, bool isLeftFoot)
        {
            Vector3 targetPos = bodyPosition + velocity.Normalized() * stepDistance;
            targetPos += isLeftFoot ? Vector3.Left * 0.5f : Vector3.Right * 0.5f;
            
            if (isLeftFoot)
            {
                leftFootTarget = targetPos;
                leftFootProgress = 0f;
            }
            else
            {
                rightFootTarget = targetPos;
                rightFootProgress = 0f;
            }
        }
        
        private Vector3 CalculateStepPosition(Vector3 target, float progress)
        {
            Vector3 currentPos = GetParent<Node3D>().GlobalPosition;
            Vector3 linearPos = currentPos.Lerp(target, progress);
            
            // Add arc (step up and down)
            float heightOffset = Mathf.Sin(progress * Mathf.Pi) * stepHeight;
            linearPos.Y += heightOffset;
            
            return linearPos;
        }
        
        private Vector3 GetBodyVelocity()
        {
            var body = GetParent<CharacterBody3D>();
            return body?.Velocity ?? Vector3.Zero;
        }
        
        #endregion
    }
}
