using Godot;

namespace MechDefenseHalo.AI.States
{
    public class PatrolState : AIState
    {
        private Vector3 _patrolTarget;
        private float _patrolRadius = 10f;
        
        public PatrolState(EnemyAIController controller) : base(controller) { }
        
        public override void Enter()
        {
            PickNewPatrolPoint();
        }
        
        public override void Update(float delta)
        {
            // Check for target
            if (Controller.HasTarget)
            {
                Controller.ChangeState("Chase");
                return;
            }
            
            // Move to patrol point
            Controller.MoveTowards(_patrolTarget);
            
            // Check if reached patrol point
            var body = Controller.GetParent<CharacterBody3D>();
            if (body != null && body.GlobalPosition.DistanceTo(_patrolTarget) < 2f)
            {
                Controller.ChangeState("Idle");
            }
        }
        
        private void PickNewPatrolPoint()
        {
            var body = Controller.GetParent<Node3D>();
            Vector3 randomOffset = new Vector3(
                GD.Randf() * _patrolRadius * 2 - _patrolRadius,
                0,
                GD.Randf() * _patrolRadius * 2 - _patrolRadius
            );
            
            _patrolTarget = body.GlobalPosition + randomOffset;
        }
    }
}
