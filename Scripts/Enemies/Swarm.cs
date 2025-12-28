using Godot;
using System;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Fast, weak swarm enemy - appears in large numbers.
    /// </summary>
    public partial class Swarm : EnemyBase
    {
        #region Constructor

        public Swarm()
        {
            EnemyName = "Swarm";
            MaxHealth = 20f;
            MoveSpeed = 6f;
            AttackDamage = 5f;
            AttackRange = 1.5f;
            AttackCooldown = 0.8f;
            DetectionRange = 40f;
        }

        #endregion

        #region Protected Methods

        protected override void UpdateBehavior(float delta)
        {
            if (Target == null || !IsInstanceValid(Target))
                return;

            // Aggressive behavior: Rush at target, attack rapidly
            if (IsInAttackRange())
            {
                // Circle around target while attacking
                Vector3 tangent = GlobalPosition.Cross(Vector3.Up).Normalized();
                
                if (_movement != null)
                {
                    _movement.SetDirection(tangent * 0.3f);
                }
                
                TryAttack();
            }
            else
            {
                // Rush towards target
                MoveTowardsTarget(delta);
            }
        }

        protected override void OnDeath()
        {
            GD.Print($"{EnemyName} defeated!");
            // TODO: Small explosion effect
        }

        #endregion
    }
}
