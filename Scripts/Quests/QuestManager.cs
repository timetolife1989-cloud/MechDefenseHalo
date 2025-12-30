using Godot;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Progression;

namespace MechDefenseHalo.Quests
{
    /// <summary>
    /// Manages all quests in the game
    /// Handles quest registration, progression, and completion
    /// Singleton pattern for global access
    /// </summary>
    public partial class QuestManager : Node
    {
        #region Singleton

        private static QuestManager _instance;
        
        public static QuestManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("QuestManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Public Properties

        public IReadOnlyList<Quest> ActiveQuests => _activeQuests.AsReadOnly();
        public IReadOnlyDictionary<string, Quest> AllQuests => _quests;

        #endregion

        #region Private Fields

        private Dictionary<string, Quest> _quests = new();
        private List<Quest> _activeQuests = new();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple QuestManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            RegisterQuests();
            LoadProgress();

            GD.Print("QuestManager initialized with quest system");
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start a quest by ID
        /// </summary>
        public void StartQuest(string questId)
        {
            if (!_quests.ContainsKey(questId))
            {
                GD.PrintErr($"Quest not found: {questId}");
                return;
            }

            var quest = _quests[questId];
            if (quest.Status != QuestStatus.NotStarted)
            {
                GD.PrintErr($"Quest {questId} cannot be started (status: {quest.Status})");
                return;
            }

            quest.Status = QuestStatus.Active;
            _activeQuests.Add(quest);

            // Emit quest started event
            EventBus.Emit("quest_started", new QuestEventData
            {
                QuestId = questId,
                QuestName = quest.Name
            });

            GD.Print($"Quest started: {quest.Name}");
            SaveProgress();
        }

        /// <summary>
        /// Update a specific objective in a quest
        /// </summary>
        public void UpdateObjective(string questId, int objectiveIndex, int progress)
        {
            if (!_quests.ContainsKey(questId))
            {
                GD.PrintErr($"Quest not found: {questId}");
                return;
            }

            var quest = _quests[questId];
            if (quest.Status != QuestStatus.Active)
            {
                return;
            }

            if (objectiveIndex < 0 || objectiveIndex >= quest.Objectives.Count)
            {
                GD.PrintErr($"Invalid objective index: {objectiveIndex}");
                return;
            }

            var objective = quest.Objectives[objectiveIndex];
            objective.CurrentCount += progress;

            if (objective.CurrentCount >= objective.RequiredCount && !objective.IsCompleted)
            {
                objective.IsCompleted = true;
                
                // Emit objective completed event
                EventBus.Emit("quest_objective_completed", new QuestObjectiveEventData
                {
                    QuestId = questId,
                    ObjectiveIndex = objectiveIndex,
                    ObjectiveDescription = objective.Description
                });

                GD.Print($"Objective completed: {objective.Description}");
            }

            // Check if all objectives completed
            if (quest.AreAllObjectivesCompleted())
            {
                CompleteQuest(questId);
            }

            SaveProgress();
        }

        /// <summary>
        /// Complete a quest and grant rewards
        /// </summary>
        public void CompleteQuest(string questId)
        {
            if (!_quests.ContainsKey(questId))
            {
                GD.PrintErr($"Quest not found: {questId}");
                return;
            }

            var quest = _quests[questId];
            if (quest.Status != QuestStatus.Active)
            {
                GD.PrintErr($"Quest {questId} is not active");
                return;
            }

            quest.Status = QuestStatus.Completed;
            _activeQuests.Remove(quest);

            GiveRewards(quest.Rewards);

            // Emit quest completed event
            EventBus.Emit("quest_completed", new QuestEventData
            {
                QuestId = questId,
                QuestName = quest.Name
            });

            GD.Print($"Quest completed: {quest.Name}");
            SaveProgress();
        }

        /// <summary>
        /// Fail a quest
        /// </summary>
        public void FailQuest(string questId)
        {
            if (!_quests.ContainsKey(questId))
            {
                GD.PrintErr($"Quest not found: {questId}");
                return;
            }

            var quest = _quests[questId];
            if (quest.Status != QuestStatus.Active)
            {
                return;
            }

            quest.Status = QuestStatus.Failed;
            _activeQuests.Remove(quest);

            // Emit quest failed event
            EventBus.Emit("quest_failed", new QuestEventData
            {
                QuestId = questId,
                QuestName = quest.Name
            });

            GD.Print($"Quest failed: {quest.Name}");
            SaveProgress();
        }

        /// <summary>
        /// Get a quest by ID
        /// </summary>
        public Quest GetQuest(string questId)
        {
            return _quests.ContainsKey(questId) ? _quests[questId] : null;
        }

        /// <summary>
        /// Get all quests with a specific status
        /// </summary>
        public List<Quest> GetQuestsByStatus(QuestStatus status)
        {
            return _quests.Values.Where(q => q.Status == status).ToList();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Register all available quests in the game
        /// </summary>
        private void RegisterQuests()
        {
            AddQuest(new Quest
            {
                Id = "tutorial_quest",
                Name = "Basic Training",
                Description = "Complete the tutorial",
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { Description = "Kill 5 enemies", RequiredCount = 5 },
                    new QuestObjective { Description = "Survive 3 waves", RequiredCount = 3 }
                },
                Rewards = new QuestRewards
                {
                    Credits = 500,
                    Experience = 100
                }
            });

            AddQuest(new Quest
            {
                Id = "veteran_quest",
                Name = "Veteran Status",
                Description = "Prove your worth",
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { Description = "Reach wave 10", RequiredCount = 1 },
                    new QuestObjective { Description = "Kill 100 enemies", RequiredCount = 100 }
                },
                Rewards = new QuestRewards
                {
                    Credits = 2000,
                    Experience = 500,
                    Items = new List<string> { "rare_weapon_token" }
                }
            });

            GD.Print($"Registered {_quests.Count} quests");
        }

        /// <summary>
        /// Add a quest to the registry
        /// </summary>
        private void AddQuest(Quest quest)
        {
            if (string.IsNullOrEmpty(quest.Id))
            {
                GD.PrintErr("Cannot add quest with empty ID");
                return;
            }

            _quests[quest.Id] = quest;
        }

        /// <summary>
        /// Give rewards to the player
        /// </summary>
        private void GiveRewards(QuestRewards rewards)
        {
            if (rewards == null)
                return;

            // Give credits
            if (rewards.Credits > 0)
            {
                CurrencyManager.AddCredits(rewards.Credits, "quest_reward");
            }

            // Give experience
            if (rewards.Experience > 0)
            {
                PlayerLevel.AddXP(rewards.Experience, "quest_reward");
            }

            // TODO: Give items when inventory system is integrated
            if (rewards.Items != null && rewards.Items.Count > 0)
            {
                GD.Print($"Quest rewards include {rewards.Items.Count} items (item system integration pending)");
            }

            GD.Print($"Rewards granted: {rewards.Credits} credits, {rewards.Experience} XP");
        }

        /// <summary>
        /// Save quest progress to SaveManager
        /// </summary>
        private void SaveProgress()
        {
            // TODO: Integrate with SaveManager when quest save data structure is defined
            // For now, just log that we would save
            GD.Print("Quest progress saved (SaveManager integration pending)");
        }

        /// <summary>
        /// Load quest progress from SaveManager
        /// </summary>
        private void LoadProgress()
        {
            // TODO: Integrate with SaveManager when quest save data structure is defined
            // For now, just log that we would load
            GD.Print("Quest progress loaded (SaveManager integration pending)");
        }

        #endregion
    }

    #region Event Data Classes

    /// <summary>
    /// Event data for quest events
    /// </summary>
    public class QuestEventData
    {
        public string QuestId { get; set; }
        public string QuestName { get; set; }
    }

    /// <summary>
    /// Event data for quest objective events
    /// </summary>
    public class QuestObjectiveEventData
    {
        public string QuestId { get; set; }
        public int ObjectiveIndex { get; set; }
        public string ObjectiveDescription { get; set; }
    }

    #endregion
}
