using Godot;

namespace MechDefenseHalo.Camera
{
    /// <summary>
    /// Camera shake effect system with intensity-based random offsets.
    /// Emits ShakeUpdated signal with current offset for camera positioning.
    /// </summary>
    public partial class CameraShake : Node
    {
        #region Signals
        
        [Signal] public delegate void ShakeUpdatedEventHandler(Vector3 offset);
        
        #endregion
        
        #region Private Fields
        
        private float _intensity;
        private float _duration;
        private float _timer;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initiates a shake effect with specified parameters.
        /// </summary>
        /// <param name="intensity">Shake magnitude</param>
        /// <param name="duration">Duration in seconds</param>
        public void StartShake(float intensity, float duration)
        {
            _intensity = intensity;
            _duration = duration;
            _timer = duration;
        }
        
        #endregion
        
        #region Godot Lifecycle Methods
        
        public override void _Process(double delta)
        {
            if (_timer <= 0) return;
            
            _timer -= (float)delta;
            float progress = _timer / _duration;
            
            Vector3 offset = new Vector3(
                GD.Randf() * 2 - 1,
                GD.Randf() * 2 - 1,
                0
            ) * _intensity * progress;
            
            EmitSignal(SignalName.ShakeUpdated, offset);
        }
        
        #endregion
    }
}
