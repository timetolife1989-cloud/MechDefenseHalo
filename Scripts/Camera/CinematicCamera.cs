using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.Camera
{
    /// <summary>
    /// Cinematic camera system for scripted camera sequences.
    /// Supports keyframe-based animation with position, rotation, and FOV transitions.
    /// </summary>
    public partial class CinematicCamera : Camera3D
    {
        #region Exported Properties
        
        [Export] public float TransitionSpeed { get; set; } = 2f;
        [Export] public bool AutoPlay { get; set; } = false;
        
        #endregion
        
        #region Signals
        
        [Signal] public delegate void SequenceStartedEventHandler();
        [Signal] public delegate void SequenceCompletedEventHandler();
        [Signal] public delegate void KeyframeReachedEventHandler(int keyframeIndex);
        
        #endregion
        
        #region Private Fields
        
        private List<CinematicKeyframe> _keyframes = new List<CinematicKeyframe>();
        private int _currentKeyframeIndex = -1;
        private bool _isPlaying = false;
        private float _transitionProgress = 0f;
        
        #endregion
        
        #region Godot Lifecycle Methods
        
        public override void _Ready()
        {
            if (AutoPlay && _keyframes.Count > 0)
            {
                PlaySequence();
            }
        }
        
        public override void _Process(double delta)
        {
            if (!_isPlaying || _currentKeyframeIndex < 0 || _currentKeyframeIndex >= _keyframes.Count)
                return;
            
            var currentKeyframe = _keyframes[_currentKeyframeIndex];
            _transitionProgress += (float)delta * TransitionSpeed;
            
            // Interpolate position
            GlobalPosition = GlobalPosition.Lerp(currentKeyframe.Position, _transitionProgress);
            
            // Interpolate rotation
            var targetBasis = Basis.FromEuler(currentKeyframe.Rotation);
            GlobalTransform = new Transform3D(
                GlobalTransform.Basis.Slerp(targetBasis, _transitionProgress),
                GlobalPosition
            );
            
            // Interpolate FOV
            Fov = Mathf.Lerp(Fov, currentKeyframe.Fov, _transitionProgress);
            
            // Check if we reached the keyframe
            if (_transitionProgress >= 1f)
            {
                EmitSignal(SignalName.KeyframeReached, _currentKeyframeIndex);
                
                // Wait at keyframe
                if (currentKeyframe.WaitTime > 0)
                {
                    _transitionProgress = -currentKeyframe.WaitTime;
                }
                else
                {
                    MoveToNextKeyframe();
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Adds a keyframe to the cinematic sequence.
        /// </summary>
        public void AddKeyframe(Vector3 position, Vector3 rotation, float fov = 75f, float waitTime = 0f)
        {
            _keyframes.Add(new CinematicKeyframe
            {
                Position = position,
                Rotation = rotation,
                Fov = fov,
                WaitTime = waitTime
            });
        }
        
        /// <summary>
        /// Adds a keyframe based on a Node3D's transform.
        /// </summary>
        public void AddKeyframeFromNode(Node3D node, float fov = 75f, float waitTime = 0f)
        {
            AddKeyframe(node.GlobalPosition, node.GlobalRotation, fov, waitTime);
        }
        
        /// <summary>
        /// Clears all keyframes from the sequence.
        /// </summary>
        public void ClearKeyframes()
        {
            _keyframes.Clear();
            _currentKeyframeIndex = -1;
        }
        
        /// <summary>
        /// Starts playing the cinematic sequence.
        /// </summary>
        public void PlaySequence()
        {
            if (_keyframes.Count == 0) return;
            
            _isPlaying = true;
            _currentKeyframeIndex = 0;
            _transitionProgress = 0f;
            
            EmitSignal(SignalName.SequenceStarted);
        }
        
        /// <summary>
        /// Stops the cinematic sequence.
        /// </summary>
        public void StopSequence()
        {
            _isPlaying = false;
            _currentKeyframeIndex = -1;
        }
        
        /// <summary>
        /// Checks if a sequence is currently playing.
        /// </summary>
        public bool IsPlaying()
        {
            return _isPlaying;
        }
        
        /// <summary>
        /// Gets the total number of keyframes.
        /// </summary>
        public int GetKeyframeCount()
        {
            return _keyframes.Count;
        }
        
        #endregion
        
        #region Private Methods
        
        private void MoveToNextKeyframe()
        {
            _currentKeyframeIndex++;
            _transitionProgress = 0f;
            
            if (_currentKeyframeIndex >= _keyframes.Count)
            {
                _isPlaying = false;
                _currentKeyframeIndex = -1;
                EmitSignal(SignalName.SequenceCompleted);
            }
        }
        
        #endregion
        
        #region Nested Classes
        
        private class CinematicKeyframe
        {
            public Vector3 Position { get; set; }
            public Vector3 Rotation { get; set; }
            public float Fov { get; set; }
            public float WaitTime { get; set; }
        }
        
        #endregion
    }
}
