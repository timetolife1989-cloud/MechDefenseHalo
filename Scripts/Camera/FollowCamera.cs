using Godot;

namespace MechDefenseHalo.Camera
{
    /// <summary>
    /// Dedicated follow camera with smooth tracking and optional camera collision detection.
    /// Provides advanced follow behavior with damping, rotation, and distance control.
    /// </summary>
    public partial class FollowCamera : Camera3D
    {
        #region Exported Properties
        
        [Export] public Node3D Target { get; set; }
        [Export] public Vector3 Offset { get; set; } = new(0, 5, 10);
        [Export] public float FollowSpeed { get; set; } = 5f;
        [Export] public float RotationSpeed { get; set; } = 3f;
        [Export] public bool EnableCollision { get; set; } = true;
        [Export] public float CollisionMargin { get; set; } = 0.5f;
        [Export] public uint CollisionMask { get; set; } = 1;
        
        #endregion
        
        #region Private Fields
        
        private Vector3 _currentVelocity = Vector3.Zero;
        private Vector3 _desiredPosition;
        
        #endregion
        
        #region Godot Lifecycle Methods
        
        public override void _Ready()
        {
            // Initialize camera position if target exists
            if (Target != null)
            {
                _desiredPosition = Target.GlobalPosition + Offset;
                GlobalPosition = _desiredPosition;
            }
        }
        
        public override void _Process(double delta)
        {
            if (Target == null) return;
            
            UpdateFollowPosition(delta);
            UpdateLookAt();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Sets the follow target for the camera.
        /// </summary>
        /// <param name="target">Target node to follow</param>
        public void SetTarget(Node3D target)
        {
            Target = target;
            if (Target != null)
            {
                _desiredPosition = Target.GlobalPosition + Offset;
            }
        }
        
        /// <summary>
        /// Sets the camera offset relative to the target.
        /// </summary>
        /// <param name="offset">Position offset from target</param>
        public void SetOffset(Vector3 offset)
        {
            Offset = offset;
        }
        
        /// <summary>
        /// Gets the current camera offset.
        /// </summary>
        public Vector3 GetOffset()
        {
            return Offset;
        }
        
        /// <summary>
        /// Instantly snaps camera to target position (no interpolation).
        /// </summary>
        public void SnapToTarget()
        {
            if (Target == null) return;
            
            _desiredPosition = Target.GlobalPosition + Offset;
            GlobalPosition = _desiredPosition;
            _currentVelocity = Vector3.Zero;
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateFollowPosition(double delta)
        {
            // Calculate desired position
            _desiredPosition = Target.GlobalPosition + Offset;
            
            // Handle collision detection
            if (EnableCollision)
            {
                _desiredPosition = CheckCollision(Target.GlobalPosition, _desiredPosition);
            }
            
            // Smooth follow with lerp
            GlobalPosition = GlobalPosition.Lerp(_desiredPosition, FollowSpeed * (float)delta);
        }
        
        private void UpdateLookAt()
        {
            if (Target == null) return;
            
            // Only look at target if we're at a different position to avoid exceptions
            float distanceSquared = GlobalPosition.DistanceSquaredTo(Target.GlobalPosition);
            if (distanceSquared > 0.001f)
            {
                // Calculate target rotation
                var targetTransform = GlobalTransform.LookingAt(Target.GlobalPosition, Vector3.Up);
                
                // Smoothly interpolate rotation
                GlobalTransform = new Transform3D(
                    GlobalTransform.Basis.Slerp(targetTransform.Basis, RotationSpeed * (float)GetProcessDeltaTime()),
                    GlobalPosition
                );
            }
        }
        
        private Vector3 CheckCollision(Vector3 targetPos, Vector3 desiredPos)
        {
            var spaceState = GetWorld3D().DirectSpaceState;
            if (spaceState == null) return desiredPos;
            
            // Ray from target to desired camera position
            var query = PhysicsRayQueryParameters3D.Create(targetPos, desiredPos);
            query.CollisionMask = CollisionMask;
            
            var result = spaceState.IntersectRay(query);
            
            if (result.Count > 0)
            {
                // Collision detected, move camera closer to target
                Vector3 hitPosition = (Vector3)result["position"];
                Vector3 normal = (Vector3)result["normal"];
                
                // Apply collision margin
                return hitPosition + normal * CollisionMargin;
            }
            
            return desiredPos;
        }
        
        #endregion
    }
}
