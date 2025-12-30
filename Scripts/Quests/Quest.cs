using System.Collections.Generic;

namespace MechDefenseHalo.Quests
{
    /// <summary>
    /// Quest status enumeration
    /// </summary>
    public enum QuestStatus
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }

    /// <summary>
    /// Represents a quest with objectives, rewards, and progression tracking
    /// </summary>
    public class Quest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<QuestObjective> Objectives { get; set; }
        public QuestRewards Rewards { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.NotStarted;

        /// <summary>
        /// Get the overall progress of the quest (0.0 to 1.0)
        /// </summary>
        public float GetProgress()
        {
            if (Objectives == null || Objectives.Count == 0)
                return 0f;

            float totalProgress = 0f;
            foreach (var objective in Objectives)
            {
                totalProgress += objective.GetProgress();
            }

            return totalProgress / Objectives.Count;
        }

        /// <summary>
        /// Check if all objectives are completed
        /// </summary>
        public bool AreAllObjectivesCompleted()
        {
            if (Objectives == null || Objectives.Count == 0)
                return false;

            foreach (var objective in Objectives)
            {
                if (!objective.IsCompleted)
                    return false;
            }

            return true;
        }
    }
}
