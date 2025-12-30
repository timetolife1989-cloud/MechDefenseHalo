using System.Collections.Generic;

namespace MechDefenseHalo.Quests
{
    /// <summary>
    /// Defines rewards granted upon quest completion
    /// </summary>
    public class QuestRewards
    {
        public int Credits { get; set; }
        public int Experience { get; set; }
        public List<string> Items { get; set; } = new();
    }
}
