using Godot;

namespace MechDefenseHalo.AI.States
{
    public class AttackState : AIState
    {
        private float _attackCooldown = 0;
        
        public AttackState(EnemyAIController controller) : base(controller) { }
        
        public override void Update(float delta)
        {
            // Lost target?
            if (!Controller.HasTarget)
            {
                Controller.ChangeState("Idle");
                return;
            }
            
            // Target out of range?
            if (!Controller.IsInRange(Controller.AttackRange))
            {
                Controller.ChangeState("Chase");
                return;
            }
            
            // Stop and attack
            Controller.Stop();
            
            _attackCooldown -= delta;
            if (_attackCooldown <= 0)
            {
                PerformAttack();
                
                // Get attack cooldown from enemy stats
                var enemyBase = Controller.GetParent() as Enemies.EnemyBase;
                _attackCooldown = enemyBase?.AttackCooldown ?? 1.5f;
            }
        }
        
        private void PerformAttack()
        {
            var enemyBase = Controller.GetParent() as Enemies.EnemyBase;
            if (enemyBase != null)
            {
                enemyBase.TryAttack();
            }
        }
    }
}
