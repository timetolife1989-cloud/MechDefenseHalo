using Godot;
using System;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Basic melee enemy - rushes player and attacks.
    /// </summary>
    public partial class Grunt : EnemyBase
    {
        #region Constructor

        public Grunt()
        {
            EnemyName = "Grunt";
            MaxHealth = 50f;
            MoveSpeed = 4f;
            AttackDamage = 8f;
            AttackRange = 2f;
            AttackCooldown = 1.5f;
            DetectionRange = 35f;
        }

        #endregion

        #region Protected Methods

        protected override void UpdateBehavior(float delta)
        {
            if (Target == null || !IsInstanceValid(Target))
                return;

            // Simple behavior: Move towards target and attack when in range
            if (IsInAttackRange())
            {
                // Stop and attack
                if (_movement != null)
                {
                    _movement.Stop();
                }
                TryAttack();
            }
            else
            {
                // Chase target
                MoveTowardsTarget(delta);
            }
        }

        protected override void OnDeath()
        {
            GD.Print($"{EnemyName} defeated!");
            // TODO: Drop loot, play death animation
        }

        #endregion
    }
}
