using Godot;
using System;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Slow, high HP tank enemy.
    /// </summary>
    public partial class Tank : EnemyBase
    {
        #region Constructor

        public Tank()
        {
            EnemyName = "Tank";
            MaxHealth = 200f;
            MoveSpeed = 2f;
            AttackDamage = 25f;
            AttackRange = 3f;
            AttackCooldown = 2f;
            DetectionRange = 30f;
        }

        #endregion

        #region Protected Methods

        protected override void OnReady()
        {
            // Add resistances to make tank tougher
            if (_resistance == null)
            {
                _resistance = new ElementalResistanceComponent();
                _resistance.Name = "ElementalResistanceComponent";
                _resistance.PhysicalResistance = 0.7f; // Takes 30% less physical damage
                _resistance.FireResistance = 0.8f;     // Takes 20% less fire damage
                _resistance.IceResistance = 1.2f;      // Takes 20% more ice damage (weakness)
                AddChild(_resistance);
            }
        }

        protected override void UpdateBehavior(float delta)
        {
            if (Target == null || !IsInstanceValid(Target))
                return;

            // Simple behavior: Slowly advance and smash
            if (IsInAttackRange())
            {
                if (_movement != null)
                {
                    _movement.Stop();
                }
                TryAttack();
            }
            else
            {
                MoveTowardsTarget(delta);
            }
        }

        protected override void OnAttackPerformed()
        {
            // TODO: Play heavy attack animation with screen shake
            GD.Print($"{EnemyName} performed heavy attack!");
        }

        protected override void OnDeath()
        {
            GD.Print($"{EnemyName} defeated! (big explosion)");
            // TODO: Spawn explosion effect
        }

        #endregion
    }
}
