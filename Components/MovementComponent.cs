using Godot;
using System;

namespace MechDefenseHalo.Components
{
    /// <summary>
    /// Reusable movement component for characters and entities.
    /// Handles velocity-based movement with acceleration and deceleration.
    /// </summary>
    public partial class MovementComponent : Node
    {
        #region Exported Properties

        [Export] public float MaxSpeed { get; set; } = 5f;
        [Export] public float Acceleration { get; set; } = 20f;
        [Export] public float Deceleration { get; set; } = 15f;
        [Export] public float RotationSpeed { get; set; } = 5f;
        [Export] public bool UseGravity { get; set; } = true;

        #endregion

        #region Public Properties

        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public Vector3 DesiredDirection { get; set; } = Vector3.Zero;

        #endregion

        #region Private Fields

        private CharacterBody3D _body;
        private float _gravity;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            _body = GetParent<CharacterBody3D>();
            if (_body == null)
            {
                GD.PrintErr($"MovementComponent on {GetParent().Name} requires CharacterBody3D parent!");
            }

            _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update movement physics (call in _PhysicsProcess)
        /// </summary>
        /// <param name="delta">Physics delta time</param>
        public void UpdateMovement(float delta)
        {
            if (_body == null) return;

            // Apply gravity
            if (UseGravity && !_body.IsOnFloor())
            {
                Velocity = new Vector3(Velocity.X, Velocity.Y - _gravity * delta, Velocity.Z);
            }

            // Get status effect multiplier if available
            float speedMultiplier = 1f;
            var statusEffect = GetParent().GetNodeOrNull<StatusEffectComponent>("StatusEffectComponent");
            if (statusEffect != null)
            {
                speedMultiplier = statusEffect.GetMovementMultiplier();
            }

            // Calculate target velocity
            Vector3 targetVelocity = DesiredDirection * MaxSpeed * speedMultiplier;

            // Interpolate current velocity towards target
            float accel = DesiredDirection.Length() > 0.1f ? Acceleration : Deceleration;
            
            Velocity = new Vector3(
                Mathf.MoveToward(Velocity.X, targetVelocity.X, accel * delta),
                Velocity.Y, // Keep Y velocity (gravity)
                Mathf.MoveToward(Velocity.Z, targetVelocity.Z, accel * delta)
            );

            // Apply velocity to body
            _body.Velocity = Velocity;
            _body.MoveAndSlide();
            Velocity = _body.Velocity;
        }

        /// <summary>
        /// Set movement direction (will be normalized)
        /// </summary>
        public void SetDirection(Vector3 direction)
        {
            DesiredDirection = new Vector3(direction.X, 0, direction.Z).Normalized();
        }

        /// <summary>
        /// Rotate towards a target position smoothly
        /// </summary>
        public void LookAtTarget(Vector3 targetPosition, float delta)
        {
            if (_body == null) return;

            Vector3 direction = (targetPosition - _body.GlobalPosition).Normalized();
            direction.Y = 0; // Keep rotation on horizontal plane

            if (direction.LengthSquared() > 0.01f)
            {
                float targetAngle = Mathf.Atan2(direction.X, direction.Z);
                Vector3 currentRotation = _body.Rotation;
                currentRotation.Y = Mathf.LerpAngle(currentRotation.Y, targetAngle, RotationSpeed * delta);
                _body.Rotation = currentRotation;
            }
        }

        /// <summary>
        /// Stop all movement immediately
        /// </summary>
        public void Stop()
        {
            Velocity = new Vector3(0, Velocity.Y, 0);
            DesiredDirection = Vector3.Zero;
        }

        #endregion
    }
}
