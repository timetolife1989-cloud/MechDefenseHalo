using Godot;

namespace MechDefenseHalo.AI.States
{
    public class DeadState : AIState
    {
        private const float DEATH_CLEANUP_DELAY = 3.0f;
        
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
            Controller.GetTree().CreateTimer(DEATH_CLEANUP_DELAY).Timeout += () =>
            {
                Controller.GetParent().QueueFree();
            };
        }
    }
}
