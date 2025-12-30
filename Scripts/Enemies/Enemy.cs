using Godot;
using System;
using MechDefenseHalo.Components;
using MechDefenseHalo.Managers;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Base Enemy class that handles AI behavior, navigation, combat, and integration with HealthSystem.
    /// Can be inherited for specialized enemies (Boss, FastEnemy, TankEnemy, etc.)
    /// </summary>
    public partial class Enemy : CharacterBody3D
    {
        #region Signals

        [Signal] public delegate void AttackedEventHandler(Node target);
        [Signal] public delegate void DiedEventHandler();
        [Signal] public delegate void StateChangedEventHandler(EnemyState newState);

        #endregion

        #region Enemy Stats

        [ExportGroup("Enemy Stats")]
        [Export] public float MaxHealth { get; set; } = 100f;
        [Export] public float MoveSpeed { get; set; } = 5f;
        [Export] public float RotationSpeed { get; set; } = 5f;
        [Export] public float Damage { get; set; } = 10f;

        #endregion

        #region Combat

        [ExportGroup("Combat")]
        [Export] public float AttackRange { get; set; } = 2f;
        [Export] public float AttackCooldown { get; set; } = 1.5f;
        [Export] public float DetectionRange { get; set; } = 20f;
        [Export] public float DeathAnimationDuration { get; set; } = 2.0f;

        #endregion

        #region Components

        [ExportGroup("Components")]
        [Export] public NodePath HealthSystemPath { get; set; }
        [Export] public string PlayerNodeName { get; set; } = "PlayerMech";
        [Export] public string DamageType { get; set; } = "melee";

        private const string DefaultHealthSystemNodeName = "HealthSystem";
        
        private HealthSystem healthSystem;
        private Node3D target; // Player reference
        private float attackTimer = 0f;

        #endregion

        #region Enemy State

        public enum EnemyState
        {
            Idle,
            Chase,
            Attack,
            Dead
        }

        private EnemyState currentState = EnemyState.Idle;

        #endregion

        #region Initialization

        public override void _Ready()
        {
            // Setup HealthSystem
            if (HealthSystemPath != null)
            {
                healthSystem = GetNode<HealthSystem>(HealthSystemPath);
            }
            else
            {
                healthSystem = GetNodeOrNull<HealthSystem>(DefaultHealthSystemNodeName);
            }

            if (healthSystem != null)
            {
                healthSystem.MaxHealth = MaxHealth;
                healthSystem.Connect(HealthSystem.SignalName.Died, new Callable(this, nameof(OnDeath)));
            }
            else
            {
                GD.PrintErr($"{Name}: HealthSystem not found!");
            }

            // Find player
            FindPlayer();
        }

        private void FindPlayer()
        {
            // Try to find player node by group first (most reliable)
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0 && players[0] is Node3D player)
            {
                target = player;
                return;
            }

            // Fallback: search by configurable name
            var root = GetTree().Root;
            target = root.FindChild(PlayerNodeName, true, false) as Node3D;

            if (target == null)
            {
                GD.PrintErr($"{Name}: Player not found!");
            }
        }

        #endregion

        #region State Machine

        private void ChangeState(EnemyState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            EmitSignal(SignalName.StateChanged, newState);
            GD.Print($"{Name} state changed to: {newState}");
        }

        public override void _Process(double delta)
        {
            if (currentState == EnemyState.Dead) return;

            attackTimer -= (float)delta;

            switch (currentState)
            {
                case EnemyState.Idle:
                    UpdateIdleState();
                    break;
                case EnemyState.Chase:
                    UpdateChaseState(delta);
                    break;
                case EnemyState.Attack:
                    UpdateAttackState(delta);
                    break;
            }
        }

        private void UpdateIdleState()
        {
            if (target == null) return;

            float distanceToTarget = GlobalPosition.DistanceTo(target.GlobalPosition);

            if (distanceToTarget <= DetectionRange)
            {
                ChangeState(EnemyState.Chase);
            }
        }

        private void UpdateChaseState(double delta)
        {
            if (target == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }

            float distanceToTarget = GlobalPosition.DistanceTo(target.GlobalPosition);

            if (distanceToTarget <= AttackRange)
            {
                ChangeState(EnemyState.Attack);
                return;
            }

            // Use hysteresis to prevent state flickering at detection range edge
            float loseTargetDistance = DetectionRange * 1.5f;
            if (distanceToTarget > loseTargetDistance)
            {
                ChangeState(EnemyState.Idle);
                return;
            }

            // Move towards target
            MoveTowardsTarget(delta);
        }

        private void UpdateAttackState(double delta)
        {
            if (target == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }

            float distanceToTarget = GlobalPosition.DistanceTo(target.GlobalPosition);

            // Use hysteresis to prevent state flickering at attack range edge
            float exitAttackDistance = AttackRange * 1.2f;
            if (distanceToTarget > exitAttackDistance)
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            // Rotate towards target
            LookAtTarget(delta);

            // Attack if cooldown ready
            if (attackTimer <= 0)
            {
                PerformAttack();
                attackTimer = AttackCooldown;
            }
        }

        #endregion

        #region Movement

        private void MoveTowardsTarget(double delta)
        {
            if (target == null) return;

            Vector3 direction = (target.GlobalPosition - GlobalPosition).Normalized();
            direction.Y = 0; // Keep movement horizontal

            // Rotate towards target
            LookAtTarget(delta);

            // Move forward
            Velocity = direction * MoveSpeed;
            MoveAndSlide();
        }

        private void LookAtTarget(double delta)
        {
            if (target == null) return;

            Vector3 targetPos = target.GlobalPosition;
            targetPos.Y = GlobalPosition.Y; // Keep rotation horizontal

            Vector3 direction = (targetPos - GlobalPosition).Normalized();

            if (direction.LengthSquared() > 0.01f)
            {
                float targetAngle = Mathf.Atan2(direction.X, direction.Z);
                float currentAngle = Rotation.Y;
                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, RotationSpeed * (float)delta);

                Rotation = new Vector3(Rotation.X, newAngle, Rotation.Z);
            }
        }

        #endregion

        #region Combat

        private void PerformAttack()
        {
            if (target == null) return;

            GD.Print($"{Name} attacks for {Damage} damage!");

            // Try to damage player's HealthSystem
            var playerHealth = target.GetNodeOrNull<HealthSystem>(DefaultHealthSystemNodeName);
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Damage, DamageType);
            }

            EmitSignal(SignalName.Attacked, target);
        }

        public void TakeDamage(float amount, string damageType = "normal")
        {
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(amount, damageType);
            }
        }

        #endregion

        #region Death Handling

        private async void OnDeath()
        {
            ChangeState(EnemyState.Dead);

            GD.Print($"{Name} died!");
            EmitSignal(SignalName.Died);

            // Notify spawner
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.OnEnemyDied(this);
            }

            // Death animation delay (configurable per enemy type)
            await ToSignal(GetTree().CreateTimer(DeathAnimationDuration), SceneTreeTimer.SignalName.Timeout);

            QueueFree();
        }

        #endregion

        #region Utility Methods

        public void SetTarget(Node3D newTarget)
        {
            target = newTarget;
        }

        public EnemyState GetState()
        {
            return currentState;
        }

        public bool IsAlive()
        {
            return currentState != EnemyState.Dead;
        }

        public float GetHealthPercent()
        {
            if (healthSystem != null)
            {
                return healthSystem.GetHealthPercent();
            }
            return 0f;
        }

        #endregion

        #region Debug

        // Note: For 3D debug visualization of detection/attack ranges,
        // use the Godot editor's gizmo system or create debug MeshInstance3D nodes
        // in a separate debug tool script, as _Draw() is only for 2D rendering

        #endregion
    }
}
