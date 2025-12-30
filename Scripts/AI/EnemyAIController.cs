using Godot;
using System;
using MechDefenseHalo.Components;
using MechDefenseHalo.AI.States;

namespace MechDefenseHalo.AI
{
    public partial class EnemyAIController : Node
    {
        #region Exported Properties
        
        [Export] public float DetectionRange { get; set; } = 30f;
        [Export] public float AttackRange { get; set; } = 5f;
        [Export] public float FleeHealthThreshold { get; set; } = 0.2f; // Flee below 20% HP
        [Export] public bool UsePathfinding { get; set; } = true;
        [Export] public float PathUpdateInterval { get; set; } = 0.5f;
        
        #endregion
        
        #region Public Properties
        
        public Node3D Target { get; private set; }
        public Vector3 TargetPosition { get; private set; }
        public bool HasTarget => Target != null && IsInstanceValid(Target);
        public float DistanceToTarget => HasTarget ? GlobalPosition.DistanceTo(Target.GlobalPosition) : float.MaxValue;
        
        #endregion
        
        #region Private Fields
        
        private AIStateMachine _stateMachine;
        private NavigationAgent3D _navAgent;
        private HealthComponent _healthComponent;
        private CharacterBody3D _body;
        private float _pathUpdateTimer;
        private Vector3 GlobalPosition => GetParent<Node3D>().GlobalPosition;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            _body = GetParent<CharacterBody3D>();
            _healthComponent = GetParent().GetNodeOrNull<HealthComponent>("HealthComponent");
            
            SetupNavigationAgent();
            SetupStateMachine();
            
            if (_healthComponent != null)
            {
                _healthComponent.Died += OnDeath;
            }
        }
        
        public override void _PhysicsProcess(double delta)
        {
            float deltaF = (float)delta;
            
            // Update target detection
            UpdateTargetDetection();
            
            // Update pathfinding
            if (UsePathfinding && HasTarget)
            {
                _pathUpdateTimer -= deltaF;
                if (_pathUpdateTimer <= 0)
                {
                    UpdatePath();
                    _pathUpdateTimer = PathUpdateInterval;
                }
            }
            
            // Update state machine
            _stateMachine?.Update(deltaF);
        }
        
        #endregion
        
        #region Setup Methods
        
        private void SetupNavigationAgent()
        {
            _navAgent = new NavigationAgent3D();
            _navAgent.PathDesiredDistance = 0.5f;
            _navAgent.TargetDesiredDistance = 0.5f;
            GetParent().AddChild(_navAgent);
            
            // Wait for navigation map
            CallDeferred(nameof(ActorSetup));
        }
        
        private void ActorSetup()
        {
            // Wait one frame for navigation map to be ready
            GetTree().CreateTimer(0.0).Timeout += () => _navAgent.NavigationFinished += OnNavigationFinished;
        }
        
        private void SetupStateMachine()
        {
            _stateMachine = new AIStateMachine();
            
            // Create states
            var idleState = new IdleState(this);
            var patrolState = new PatrolState(this);
            var chaseState = new ChaseState(this);
            var attackState = new AttackState(this);
            var fleeState = new FleeState(this);
            var deadState = new DeadState(this);
            
            // Add states
            _stateMachine.AddState("Idle", idleState);
            _stateMachine.AddState("Patrol", patrolState);
            _stateMachine.AddState("Chase", chaseState);
            _stateMachine.AddState("Attack", attackState);
            _stateMachine.AddState("Flee", fleeState);
            _stateMachine.AddState("Dead", deadState);
            
            // Set initial state
            _stateMachine.ChangeState("Idle");
        }
        
        #endregion
        
        #region Public Methods
        
        public void MoveTowards(Vector3 targetPosition)
        {
            if (_navAgent == null || _body == null)
                return;
            
            if (UsePathfinding)
            {
                _navAgent.TargetPosition = targetPosition;
                
                if (_navAgent.IsNavigationFinished())
                    return;
                
                Vector3 nextPosition = _navAgent.GetNextPathPosition();
                Vector3 direction = (nextPosition - GlobalPosition).Normalized();
                
                MoveInDirection(direction);
            }
            else
            {
                // Direct movement (no pathfinding)
                Vector3 direction = (targetPosition - GlobalPosition).Normalized();
                MoveInDirection(direction);
            }
        }
        
        public void MoveInDirection(Vector3 direction)
        {
            if (_body == null)
                return;
            
            var enemyBase = GetParent() as Enemies.EnemyBase;
            float speed = enemyBase?.MoveSpeed ?? 5f;
            
            Vector3 velocity = direction * speed;
            velocity.Y = _body.Velocity.Y; // Preserve vertical velocity
            
            _body.Velocity = velocity;
            _body.MoveAndSlide();
            
            // Face movement direction
            if (direction.Length() > 0.01f)
            {
                _body.LookAt(GlobalPosition + direction, Vector3.Up);
            }
        }
        
        public void Stop()
        {
            if (_body == null)
                return;
            
            _body.Velocity = new Vector3(0, _body.Velocity.Y, 0);
        }
        
        public bool IsInRange(float range)
        {
            return DistanceToTarget <= range;
        }
        
        public void ChangeState(string stateName)
        {
            _stateMachine?.ChangeState(stateName);
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateTargetDetection()
        {
            // Find player (or closest target)
            var player = GetTree().GetFirstNodeInGroup("player") as Node3D;
            
            if (player != null && IsInstanceValid(player))
            {
                float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
                
                if (distance <= DetectionRange)
                {
                    Target = player;
                    TargetPosition = player.GlobalPosition;
                }
                else if (Target == player)
                {
                    // Lost target
                    Target = null;
                }
            }
        }
        
        private void UpdatePath()
        {
            if (_navAgent != null && HasTarget)
            {
                _navAgent.TargetPosition = Target.GlobalPosition;
            }
        }
        
        private void OnNavigationFinished()
        {
            // Reached destination
        }
        
        private void OnDeath()
        {
            _stateMachine?.ChangeState("Dead");
        }
        
        #endregion
    }
}
