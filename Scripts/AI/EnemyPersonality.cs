using Godot;

namespace MechDefenseHalo.AI
{
    /// <summary>
    /// Defines behavioral characteristics for individual enemies.
    /// Used by AI Director to create diverse enemy behaviors.
    /// </summary>
    public class EnemyPersonality
    {
        public float Speed; // 0.0 - 1.0
        public float Aggression; // 0.0 = cautious, 1.0 = reckless
        public float Caution; // 0.0 = suicidal, 1.0 = retreats early
        public float Teamwork; // 0.0 = lone wolf, 1.0 = coordinated
        public float Range; // 0.0 = melee, 1.0 = long range
        
        /// <summary>
        /// Calculate ideal distance to maintain from player based on personality
        /// </summary>
        public float GetIdealDistanceToPlayer()
        {
            // Melee personalities want close, ranged want far
            return Mathf.Lerp(3f, 20f, Range);
        }
        
        /// <summary>
        /// Determine if enemy should retreat based on health and allies
        /// </summary>
        public bool ShouldRetreat(float healthPercent, int nearbyAllies)
        {
            float retreatThreshold = Mathf.Lerp(0.1f, 0.6f, Caution);
            
            if (healthPercent < retreatThreshold)
            {
                // High teamwork = retreat to allies
                if (Teamwork > 0.5f && nearbyAllies > 0)
                {
                    return true;
                }
                // Low teamwork = fight to death if aggressive
                else if (Aggression < 0.5f)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Get attack delay based on aggression level
        /// </summary>
        public float GetAttackDelay()
        {
            // Aggressive enemies attack faster
            return Mathf.Lerp(2f, 0.5f, Aggression);
        }
    }
}
