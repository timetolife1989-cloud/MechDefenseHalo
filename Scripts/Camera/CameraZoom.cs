using Godot;

namespace MechDefenseHalo.Camera
{
    /// <summary>
    /// Camera zoom system with smooth FOV transitions.
    /// Supports zoom in/out with configurable speed and FOV limits.
    /// </summary>
    public partial class CameraZoom : Node
    {
        #region Exported Properties
        
        [Export] public float MinFov { get; set; } = 40f;
        [Export] public float MaxFov { get; set; } = 90f;
        [Export] public float DefaultFov { get; set; } = 75f;
        [Export] public float ZoomSpeed { get; set; } = 5f;
        
        #endregion
        
        #region Private Fields
        
        private Camera3D _camera;
        private float _targetFov;
        
        #endregion
        
        #region Godot Lifecycle Methods
        
        public override void _Ready()
        {
            _camera = GetParent<Camera3D>();
            if (_camera != null)
            {
                _camera.Fov = DefaultFov;
                _targetFov = DefaultFov;
            }
        }
        
        public override void _Process(double delta)
        {
            if (_camera == null) return;
            
            _camera.Fov = Mathf.Lerp(_camera.Fov, _targetFov, ZoomSpeed * (float)delta);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Zooms the camera in by reducing FOV.
        /// </summary>
        /// <param name="amount">Amount to zoom (positive values zoom in)</param>
        public void ZoomIn(float amount)
        {
            _targetFov = Mathf.Clamp(_targetFov - amount, MinFov, MaxFov);
        }
        
        /// <summary>
        /// Zooms the camera out by increasing FOV.
        /// </summary>
        /// <param name="amount">Amount to zoom (positive values zoom out)</param>
        public void ZoomOut(float amount)
        {
            _targetFov = Mathf.Clamp(_targetFov + amount, MinFov, MaxFov);
        }
        
        /// <summary>
        /// Sets the target FOV directly.
        /// </summary>
        /// <param name="fov">Target field of view</param>
        public void SetTargetFov(float fov)
        {
            _targetFov = Mathf.Clamp(fov, MinFov, MaxFov);
        }
        
        /// <summary>
        /// Resets zoom to default FOV.
        /// </summary>
        public void ResetZoom()
        {
            _targetFov = DefaultFov;
        }
        
        /// <summary>
        /// Gets the current FOV of the camera.
        /// </summary>
        public float GetCurrentFov()
        {
            return _camera?.Fov ?? DefaultFov;
        }
        
        #endregion
    }
}
