namespace MechDefenseHalo.AI.States
{
    public class DeadState : AIState
    {
        public DeadState(EnemyAIController controller) : base(controller) { }
        
        public override void Enter()
        {
            Controller.Stop();
            
            // Disable physics/collision
            var body = Controller.GetParent<CharacterBody3D>();
            if (body != null)
            {
                body.SetPhysicsProcess(false);
            }
            
            // TODO: Play death animation
            // TODO: Drop loot
            
            // Queue free after delay
            Controller.GetTree().CreateTimer(3.0).Timeout += () =>
            {
                Controller.GetParent().QueueFree();
            };
        }
    }
}
