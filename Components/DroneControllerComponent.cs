using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MechDefenseHalo.Components
{
    /// <summary>
    /// AI controller for drones - handles targeting, movement, and attack behavior.
    /// </summary>
    public partial class DroneControllerComponent : Node
    {
        #region Exported Properties

        [Export] public float OrbitRadius { get; set; } = 3f;
        [Export] public float OrbitSpeed { get; set; } = 2f;
        [Export] public float DetectionRange { get; set; } = 30f;
        [Export] public float AttackRange { get; set; } = 20f;
        [Export] public float MoveSpeed { get; set; } = 8f;
        [Export] public NodePath TargetPath { get; set; } // Usually the player

        #endregion

        #region Public Properties

        public Node3D CurrentTarget { get; private set; }
        public Node3D OrbitTarget { get; private set; }
        public DroneState State { get; private set; } = DroneState.Idle;

        #endregion

        #region Private Fields

        private Node3D _droneBody;
        private float _orbitAngle = 0f;
        private Vector3 _targetPosition;
        private List<Node3D> _detectedEnemies = new List<Node3D>();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            _droneBody = GetParent<Node3D>();
            
            if (TargetPath != null && !TargetPath.IsEmpty)
            {
                OrbitTarget = GetNode<Node3D>(TargetPath);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_droneBody == null)
                return;

            float dt = (float)delta;

            switch (State)
            {
                case DroneState.Idle:
                    UpdateIdle(dt);
                    break;
                case DroneState.Orbiting:
                    UpdateOrbiting(dt);
                    break;
                case DroneState.Attacking:
                    UpdateAttacking(dt);
                    break;
                case DroneState.Returning:
                    UpdateReturning(dt);
                    break;
            }

            // Always look for enemies
            FindNearestEnemy();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the target to orbit around (usually the player)
        /// </summary>
        public void SetOrbitTarget(Node3D target)
        {
            OrbitTarget = target;
            State = DroneState.Orbiting;
        }

        /// <summary>
        /// Set a specific enemy to attack
        /// </summary>
        public void SetAttackTarget(Node3D target)
        {
            CurrentTarget = target;
            State = DroneState.Attacking;
        }

        /// <summary>
        /// Return to orbiting the player
        /// </summary>
        public void ReturnToOrbit()
        {
            State = DroneState.Returning;
        }

        #endregion

        #region Private Methods - State Updates

        private void UpdateIdle(float delta)
        {
            // If we have an orbit target, start orbiting
            if (OrbitTarget != null)
            {
                State = DroneState.Orbiting;
            }
        }

        private void UpdateOrbiting(float delta)
        {
            if (OrbitTarget == null || !IsInstanceValid(OrbitTarget))
            {
                State = DroneState.Idle;
                return;
            }

            // Orbit around target
            _orbitAngle += OrbitSpeed * delta;
            if (_orbitAngle > Mathf.Tau) // Full circle
                _orbitAngle -= Mathf.Tau;

            Vector3 offset = new Vector3(
                Mathf.Cos(_orbitAngle) * OrbitRadius,
                1f, // Height offset
                Mathf.Sin(_orbitAngle) * OrbitRadius
            );

            Vector3 targetPos = OrbitTarget.GlobalPosition + offset;
            _droneBody.GlobalPosition = _droneBody.GlobalPosition.Lerp(targetPos, MoveSpeed * delta);

            // If enemy detected, switch to attacking
            if (CurrentTarget != null && IsInstanceValid(CurrentTarget))
            {
                State = DroneState.Attacking;
            }
        }

        private void UpdateAttacking(float delta)
        {
            // Check if target is still valid
            if (CurrentTarget == null || !IsInstanceValid(CurrentTarget))
            {
                CurrentTarget = null;
                State = DroneState.Returning;
                return;
            }

            // Check if target is in range
            float distanceToTarget = _droneBody.GlobalPosition.DistanceTo(CurrentTarget.GlobalPosition);
            
            if (distanceToTarget > DetectionRange)
            {
                // Target too far, return to orbit
                CurrentTarget = null;
                State = DroneState.Returning;
                return;
            }

            // Move towards target if outside attack range
            if (distanceToTarget > AttackRange)
            {
                Vector3 direction = (CurrentTarget.GlobalPosition - _droneBody.GlobalPosition).Normalized();
                _droneBody.GlobalPosition += direction * MoveSpeed * delta;
            }

            // Look at target
            _droneBody.LookAt(CurrentTarget.GlobalPosition, Vector3.Up);
        }

        private void UpdateReturning(float delta)
        {
            if (OrbitTarget == null)
            {
                State = DroneState.Idle;
                return;
            }

            // Move back to orbit position
            float distanceToOrbit = _droneBody.GlobalPosition.DistanceTo(OrbitTarget.GlobalPosition);
            
            if (distanceToOrbit <= OrbitRadius * 1.5f)
            {
                State = DroneState.Orbiting;
                return;
            }

            Vector3 direction = (OrbitTarget.GlobalPosition - _droneBody.GlobalPosition).Normalized();
            _droneBody.GlobalPosition += direction * MoveSpeed * delta;
        }

        private void FindNearestEnemy()
        {
            if (State == DroneState.Attacking && CurrentTarget != null)
                return; // Already attacking

            // Simple enemy detection - look for nodes in "enemies" group
            var enemies = GetTree().GetNodesInGroup("enemies");
            
            Node3D nearest = null;
            float nearestDistance = DetectionRange;

            foreach (var enemy in enemies)
            {
                if (enemy is Node3D enemy3D && IsInstanceValid(enemy3D))
                {
                    float distance = _droneBody.GlobalPosition.DistanceTo(enemy3D.GlobalPosition);
                    if (distance < nearestDistance)
                    {
                        nearest = enemy3D;
                        nearestDistance = distance;
                    }
                }
            }

            if (nearest != null)
            {
                CurrentTarget = nearest;
            }
        }

        #endregion
    }

    #region Enums

    public enum DroneState
    {
        Idle,
        Orbiting,
        Attacking,
        Returning
    }

    #endregion
}
