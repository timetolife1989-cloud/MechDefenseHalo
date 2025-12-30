using Godot;

namespace MechDefenseHalo.AI.States
{
    public class ChaseState : AIState
    {
        public ChaseState(EnemyAIController controller) : base(controller) { }
        
        public override void Update(float delta)
        {
            // Lost target?
            if (!Controller.HasTarget)
            {
                Controller.ChangeState("Idle");
                return;
            }
            
            // In attack range?
            if (Controller.IsInRange(Controller.AttackRange))
            {
                Controller.ChangeState("Attack");
                return;
            }
            
            // Check if should flee
            var healthComp = Controller.GetParent().GetNodeOrNull<Components.HealthComponent>("HealthComponent");
            if (healthComp != null && healthComp.HealthPercent < Controller.FleeHealthThreshold)
            {
                Controller.ChangeState("Flee");
                return;
            }
            
            // Chase target
            Controller.MoveTowards(Controller.TargetPosition);
        }
    }
}
