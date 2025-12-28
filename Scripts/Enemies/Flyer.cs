using Godot;
using System;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Aerial flying enemy that attacks from above.
    /// </summary>
    public partial class Flyer : EnemyBase
    {
        #region Exported Properties

        [Export] public float FlyHeight { get; set; } = 5f;
        [Export] public float DiveSpeed { get; set; } = 8f;

        #endregion

        #region Private Fields

        private bool _isDiving = false;
        private Vector3 _flyPosition;

        #endregion

        #region Constructor

        public Flyer()
        {
            EnemyName = "Flyer";
            MaxHealth = 35f;
            MoveSpeed = 5f;
            AttackDamage = 15f;
            AttackRange = 2f;
            AttackCooldown = 3f;
            DetectionRange = 45f;
        }

        #endregion

        #region Protected Methods

        protected override void OnReady()
        {
            base.OnReady();
            
            // Disable gravity for flying
            if (_movement != null)
            {
                _movement.UseGravity = false;
            }

            // Start at flying height
            GlobalPosition = new Vector3(GlobalPosition.X, GlobalPosition.Y + FlyHeight, GlobalPosition.Z);
        }

        protected override void UpdateBehavior(float delta)
        {
            if (Target == null || !IsInstanceValid(Target))
                return;

            if (!_isDiving)
            {
                // Fly above target
                _flyPosition = Target.GlobalPosition + new Vector3(0, FlyHeight, 0);
                
                Vector3 direction = (_flyPosition - GlobalPosition).Normalized();
                
                if (_movement != null)
                {
                    _movement.SetDirection(direction);
                }

                // Check if we should dive
                float horizontalDistance = new Vector2(
                    GlobalPosition.X - Target.GlobalPosition.X,
                    GlobalPosition.Z - Target.GlobalPosition.Z
                ).Length();

                if (horizontalDistance < 5f && _attackTimer <= 0)
                {
                    StartDive();
                }
            }
            else
            {
                // Diving towards target
                Vector3 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
                
                if (_movement != null)
                {
                    _movement.Velocity = direction * DiveSpeed;
                }

                // Check if we hit the ground or target
                if (GlobalPosition.Y <= Target.GlobalPosition.Y + 1f || IsInAttackRange())
                {
                    EndDive();
                    TryAttack();
                }
            }
        }

        protected override void OnDeath()
        {
            GD.Print($"{EnemyName} shot down!");
            // TODO: Falling animation
        }

        #endregion

        #region Private Methods

        private void StartDive()
        {
            _isDiving = true;
            GD.Print($"{EnemyName} diving!");
        }

        private void EndDive()
        {
            _isDiving = false;
            _attackTimer = AttackCooldown;
            
            // Return to flying height
            GlobalPosition = new Vector3(
                GlobalPosition.X,
                Target.GlobalPosition.Y + FlyHeight,
                GlobalPosition.Z
            );
        }

        #endregion
    }
}
