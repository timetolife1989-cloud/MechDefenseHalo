using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.Quests
{
    /// <summary>
    /// Tracks active quests and displays progress
    /// UI component for quest tracking HUD
    /// </summary>
    public partial class QuestTracker : Control
    {
        #region Exports

        [Export] public int MaxDisplayedQuests { get; set; } = 3;
        [Export] public bool ShowObjectiveProgress { get; set; } = true;

        #endregion

        #region Private Fields

        private VBoxContainer _questListContainer;
        private Label _noQuestsLabel;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Create UI structure
            SetupUI();

            // Subscribe to quest events only if QuestManager is available
            if (QuestManager.Instance != null && Core.EventBus.Instance != null)
            {
                Core.EventBus.Instance.On("quest_started", OnQuestStarted);
                Core.EventBus.Instance.On("quest_completed", OnQuestCompleted);
                Core.EventBus.Instance.On("quest_failed", OnQuestFailed);
                Core.EventBus.Instance.On("quest_objective_completed", OnObjectiveCompleted);
            }
            else
            {
                GD.PrintErr("QuestTracker: QuestManager or EventBus not initialized!");
            }

            // Initial update
            UpdateQuestDisplay();
        }

        public override void _ExitTree()
        {
            // Unsubscribe from events
            if (Core.EventBus.Instance != null)
            {
                Core.EventBus.Instance.Off("quest_started", OnQuestStarted);
                Core.EventBus.Instance.Off("quest_completed", OnQuestCompleted);
                Core.EventBus.Instance.Off("quest_failed", OnQuestFailed);
                Core.EventBus.Instance.Off("quest_objective_completed", OnObjectiveCompleted);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Setup the UI structure
        /// </summary>
        private void SetupUI()
        {
            // Create container for quest list
            _questListContainer = new VBoxContainer();
            _questListContainer.Name = "QuestListContainer";
            AddChild(_questListContainer);

            // Create "no quests" label
            _noQuestsLabel = new Label();
            _noQuestsLabel.Text = "No Active Quests";
            _noQuestsLabel.Name = "NoQuestsLabel";
            _noQuestsLabel.Visible = false;
            AddChild(_noQuestsLabel);
        }

        /// <summary>
        /// Update the quest display with current active quests
        /// </summary>
        private void UpdateQuestDisplay()
        {
            if (QuestManager.Instance == null)
                return;

            // Clear existing quest displays
            foreach (Node child in _questListContainer.GetChildren())
            {
                child.QueueFree();
            }

            var activeQuests = QuestManager.Instance.ActiveQuests;

            if (activeQuests.Count == 0)
            {
                _noQuestsLabel.Visible = true;
                return;
            }

            _noQuestsLabel.Visible = false;

            // Display up to MaxDisplayedQuests
            int displayCount = Mathf.Min(activeQuests.Count, MaxDisplayedQuests);
            for (int i = 0; i < displayCount; i++)
            {
                var quest = activeQuests[i];
                CreateQuestDisplay(quest);
            }
        }

        /// <summary>
        /// Create a display element for a quest
        /// </summary>
        private void CreateQuestDisplay(Quest quest)
        {
            var questPanel = new PanelContainer();
            questPanel.Name = $"Quest_{quest.Id}";

            var vbox = new VBoxContainer();
            questPanel.AddChild(vbox);

            // Quest title
            var titleLabel = new Label();
            titleLabel.Text = quest.Name;
            titleLabel.AddThemeColorOverride("font_color", new Color(1, 1, 0)); // Yellow
            vbox.AddChild(titleLabel);

            // Objectives
            if (ShowObjectiveProgress)
            {
                foreach (var objective in quest.Objectives)
                {
                    var objectiveLabel = new Label();
                    string checkmark = objective.IsCompleted ? "[✓]" : "[ ]";
                    objectiveLabel.Text = $"{checkmark} {objective.Description} ({objective.CurrentCount}/{objective.RequiredCount})";
                    
                    if (objective.IsCompleted)
                    {
                        objectiveLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0)); // Green
                    }
                    
                    vbox.AddChild(objectiveLabel);
                }
            }

            _questListContainer.AddChild(questPanel);
        }

        #endregion

        #region Event Handlers

        private void OnQuestStarted(object data)
        {
            UpdateQuestDisplay();
        }

        private void OnQuestCompleted(object data)
        {
            UpdateQuestDisplay();
        }

        private void OnQuestFailed(object data)
        {
            UpdateQuestDisplay();
        }

        private void OnObjectiveCompleted(object data)
        {
            UpdateQuestDisplay();
        }

        #endregion
    }
}
