using Godot;

namespace MechDefenseHalo.AI.States
{
    public class IdleState : AIState
    {
        private float _idleTime;
        private float _maxIdleTime = 2f;
        
        public IdleState(EnemyAIController controller) : base(controller) { }
        
        public override void Enter()
        {
            Controller.Stop();
            _idleTime = 0;
        }
        
        public override void Update(float delta)
        {
            _idleTime += delta;
            
            // Check for target
            if (Controller.HasTarget)
            {
                Controller.ChangeState("Chase");
                return;
            }
            
            // Switch to patrol after idle time
            if (_idleTime >= _maxIdleTime)
            {
                Controller.ChangeState("Patrol");
            }
        }
    }
}
