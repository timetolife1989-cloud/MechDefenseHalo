namespace MechDefenseHalo.AI
{
    public abstract class AIState
    {
        protected EnemyAIController Controller;
        
        public AIState(EnemyAIController controller)
        {
            Controller = controller;
        }
        
        public virtual void Enter() { }
        public virtual void Update(float delta) { }
        public virtual void Exit() { }
    }
}
