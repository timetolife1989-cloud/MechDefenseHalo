using Godot;

namespace MechDefenseHalo.Camera
{
    /// <summary>
    /// Main camera controller with smooth follow, shake support, and look-at functionality.
    /// Handles camera positioning, target following, and integrates with camera shake system.
    /// </summary>
    public partial class CameraController : Camera3D
    {
        #region Exported Properties
        
        [Export] public Node3D Target { get; set; }
        [Export] public Vector3 Offset { get; set; } = new(0, 5, 10);
        [Export] public float FollowSpeed { get; set; } = 5f;
        
        #endregion
        
        #region Private Fields
        
        private CameraShake _shake;
        private Vector3 _shakeOffset;
        
        #endregion
        
        #region Godot Lifecycle Methods
        
        public override void _Ready()
        {
            _shake = new CameraShake();
            AddChild(_shake);
            
            _shake.ShakeUpdated += OnShakeUpdated;
        }
        
        public override void _Process(double delta)
        {
            if (Target == null) return;
            
            Vector3 targetPos = Target.GlobalPosition + Offset + _shakeOffset;
            GlobalPosition = GlobalPosition.Lerp(targetPos, FollowSpeed * (float)delta);
            
            // Only look at target if we're at a different position to avoid exceptions
            if (GlobalPosition.DistanceSquaredTo(Target.GlobalPosition) > 0.001f)
            {
                LookAt(Target.GlobalPosition, Vector3.Up);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Triggers a camera shake effect with specified intensity and duration.
        /// </summary>
        /// <param name="intensity">Shake magnitude</param>
        /// <param name="duration">Duration in seconds</param>
        public void Shake(float intensity, float duration)
        {
            _shake.StartShake(intensity, duration);
        }
        
        #endregion
        
        #region Private Methods
        
        private void OnShakeUpdated(Vector3 offset)
        {
            _shakeOffset = offset;
        }
        
        #endregion
    }
}
