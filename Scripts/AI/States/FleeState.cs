using Godot;

namespace MechDefenseHalo.AI.States
{
    public class FleeState : AIState
    {
        public FleeState(EnemyAIController controller) : base(controller) { }
        
        public override void Update(float delta)
        {
            if (!Controller.HasTarget)
            {
                Controller.ChangeState("Idle");
                return;
            }
            
            // Flee in opposite direction
            var body = Controller.GetParent<Node3D>();
            if (body == null)
            {
                Controller.ChangeState("Idle");
                return;
            }
            
            Vector3 fleeDirection = (body.GlobalPosition - Controller.TargetPosition).Normalized();
            Vector3 fleeTarget = body.GlobalPosition + fleeDirection * 20f;
            
            Controller.MoveTowards(fleeTarget);
            
            // Check if safe (recovered health or far enough)
            var healthComp = Controller.GetParent().GetNodeOrNull<Components.HealthComponent>("HealthComponent");
            if (healthComp != null && healthComp.HealthPercent > 0.5f)
            {
                Controller.ChangeState("Chase");
            }
            else if (Controller.DistanceToTarget > Controller.DetectionRange * 2)
            {
                Controller.ChangeState("Idle");
            }
        }
    }
}
