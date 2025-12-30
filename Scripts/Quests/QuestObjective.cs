using Godot;

namespace MechDefenseHalo.Quests
{
    /// <summary>
    /// Represents a single objective within a quest
    /// Tracks progress towards completion
    /// </summary>
    public class QuestObjective
    {
        public string Description { get; set; }
        public int RequiredCount { get; set; } = 1;
        public int CurrentCount { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Get the progress percentage of this objective
        /// </summary>
        public float GetProgress()
        {
            if (RequiredCount <= 0)
                return 0f;
            
            return Mathf.Clamp((float)CurrentCount / RequiredCount, 0f, 1f);
        }
    }
}
