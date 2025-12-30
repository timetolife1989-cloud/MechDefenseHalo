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

        #endregion

        #region Components

        [ExportGroup("Components")]
        [Export] public NodePath HealthSystemPath { get; set; }

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
                healthSystem = GetNodeOrNull<HealthSystem>("HealthSystem");
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
            // Try to find player node
            var root = GetTree().Root;
            target = root.FindChild("Player*", true, false) as Node3D;

            if (target == null)
            {
                target = root.FindChild("PlayerMech", true, false) as Node3D;
            }

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

            if (distanceToTarget > DetectionRange * 1.5f)
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

            if (distanceToTarget > AttackRange * 1.2f)
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
            var playerHealth = target.GetNodeOrNull<HealthSystem>("HealthSystem");
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Damage, "melee");
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

            // Death animation delay (placeholder)
            await ToSignal(GetTree().CreateTimer(2.0f), "timeout");

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

#if DEBUG
        public override void _Draw()
        {
            // TODO: Add debug sphere for detection/attack range in 3D viewport
        }
#endif

        #endregion
    }
}
