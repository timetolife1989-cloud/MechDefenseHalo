using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.Quests
{
    /// <summary>
    /// Main UI for quest management
    /// Displays available, active, and completed quests
    /// </summary>
    public partial class QuestUI : Control
    {
        #region Exports

        [Export] public NodePath QuestListPath { get; set; }
        [Export] public NodePath QuestDetailPath { get; set; }
        [Export] public NodePath TabContainerPath { get; set; }

        #endregion

        #region Private Fields

        private VBoxContainer _questList;
        private Control _questDetail;
        private TabContainer _tabContainer;
        private Quest _selectedQuest;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get UI nodes
            if (QuestListPath != null)
                _questList = GetNodeOrNull<VBoxContainer>(QuestListPath);
            
            if (QuestDetailPath != null)
                _questDetail = GetNodeOrNull<Control>(QuestDetailPath);
            
            if (TabContainerPath != null)
                _tabContainer = GetNodeOrNull<TabContainer>(TabContainerPath);

            // If nodes don't exist, create basic structure
            if (_questList == null || _questDetail == null)
            {
                SetupUI();
            }

            // Subscribe to quest events
            if (Core.EventBus.Instance != null)
            {
                Core.EventBus.Instance.On("quest_started", OnQuestUpdated);
                Core.EventBus.Instance.On("quest_completed", OnQuestUpdated);
                Core.EventBus.Instance.On("quest_failed", OnQuestUpdated);
            }

            // Initial update
            RefreshQuestList();
        }

        public override void _ExitTree()
        {
            // Unsubscribe from events
            if (Core.EventBus.Instance != null)
            {
                Core.EventBus.Instance.Off("quest_started", OnQuestUpdated);
                Core.EventBus.Instance.Off("quest_completed", OnQuestUpdated);
                Core.EventBus.Instance.Off("quest_failed", OnQuestUpdated);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show the quest UI
        /// </summary>
        public void ShowQuestUI()
        {
            Visible = true;
            RefreshQuestList();
        }

        /// <summary>
        /// Hide the quest UI
        /// </summary>
        public void HideQuestUI()
        {
            Visible = false;
        }

        /// <summary>
        /// Refresh the quest list display
        /// </summary>
        public void RefreshQuestList()
        {
            if (QuestManager.Instance == null || _questList == null)
                return;

            // Clear existing list
            foreach (Node child in _questList.GetChildren())
            {
                child.QueueFree();
            }

            // Get current tab selection if available
            QuestStatus filterStatus = QuestStatus.Active;
            if (_tabContainer != null)
            {
                filterStatus = _tabContainer.CurrentTab switch
                {
                    0 => QuestStatus.Active,
                    1 => QuestStatus.NotStarted,
                    2 => QuestStatus.Completed,
                    _ => QuestStatus.Active
                };
            }

            // Display quests based on filter
            var quests = QuestManager.Instance.GetQuestsByStatus(filterStatus);
            foreach (var quest in quests)
            {
                CreateQuestListItem(quest);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Setup basic UI structure if nodes not provided
        /// </summary>
        private void SetupUI()
        {
            var hbox = new HBoxContainer();
            AddChild(hbox);

            // Left side - Quest list
            var leftPanel = new PanelContainer();
            leftPanel.CustomMinimumSize = new Vector2(300, 0);
            hbox.AddChild(leftPanel);

            _questList = new VBoxContainer();
            leftPanel.AddChild(_questList);

            // Right side - Quest detail
            var rightPanel = new PanelContainer();
            rightPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            hbox.AddChild(rightPanel);

            _questDetail = new VBoxContainer();
            rightPanel.AddChild(_questDetail);
        }

        /// <summary>
        /// Create a quest list item button
        /// </summary>
        private void CreateQuestListItem(Quest quest)
        {
            var button = new Button();
            button.Text = quest.Name;
            button.Name = $"QuestButton_{quest.Id}";
            
            // Set button color based on status
            Color buttonColor = quest.Status switch
            {
                QuestStatus.Active => new Color(1, 1, 0), // Yellow
                QuestStatus.Completed => new Color(0, 1, 0), // Green
                QuestStatus.Failed => new Color(1, 0, 0), // Red
                _ => new Color(1, 1, 1) // White
            };
            button.AddThemeColorOverride("font_color", buttonColor);

            button.Pressed += () => OnQuestSelected(quest);
            _questList.AddChild(button);
        }

        /// <summary>
        /// Called when a quest is selected
        /// </summary>
        private void OnQuestSelected(Quest quest)
        {
            _selectedQuest = quest;
            ShowQuestDetail(quest);
        }

        /// <summary>
        /// Display detailed information about a quest
        /// </summary>
        private void ShowQuestDetail(Quest quest)
        {
            if (_questDetail == null)
                return;

            // Clear existing detail view
            foreach (Node child in _questDetail.GetChildren())
            {
                child.QueueFree();
            }

            // Quest name
            var nameLabel = new Label();
            nameLabel.Text = quest.Name;
            nameLabel.AddThemeFontSizeOverride("font_size", 24);
            _questDetail.AddChild(nameLabel);

            // Status
            var statusLabel = new Label();
            statusLabel.Text = $"Status: {quest.Status}";
            _questDetail.AddChild(statusLabel);

            // Description
            var descLabel = new Label();
            descLabel.Text = quest.Description;
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _questDetail.AddChild(descLabel);

            // Spacer
            var spacer1 = new Control();
            spacer1.CustomMinimumSize = new Vector2(0, 20);
            _questDetail.AddChild(spacer1);

            // Objectives header
            var objHeader = new Label();
            objHeader.Text = "Objectives:";
            objHeader.AddThemeFontSizeOverride("font_size", 18);
            _questDetail.AddChild(objHeader);

            // Objectives list
            foreach (var objective in quest.Objectives)
            {
                var objLabel = new Label();
                string status = objective.IsCompleted ? "[✓]" : $"[{objective.CurrentCount}/{objective.RequiredCount}]";
                objLabel.Text = $"{status} {objective.Description}";
                
                if (objective.IsCompleted)
                {
                    objLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
                }
                
                _questDetail.AddChild(objLabel);
            }

            // Spacer
            var spacer2 = new Control();
            spacer2.CustomMinimumSize = new Vector2(0, 20);
            _questDetail.AddChild(spacer2);

            // Rewards header
            var rewardHeader = new Label();
            rewardHeader.Text = "Rewards:";
            rewardHeader.AddThemeFontSizeOverride("font_size", 18);
            _questDetail.AddChild(rewardHeader);

            // Rewards list
            if (quest.Rewards != null)
            {
                if (quest.Rewards.Credits > 0)
                {
                    var creditLabel = new Label();
                    creditLabel.Text = $"• {quest.Rewards.Credits} Credits";
                    _questDetail.AddChild(creditLabel);
                }

                if (quest.Rewards.Experience > 0)
                {
                    var xpLabel = new Label();
                    xpLabel.Text = $"• {quest.Rewards.Experience} XP";
                    _questDetail.AddChild(xpLabel);
                }

                if (quest.Rewards.Items != null && quest.Rewards.Items.Count > 0)
                {
                    foreach (var item in quest.Rewards.Items)
                    {
                        var itemLabel = new Label();
                        itemLabel.Text = $"• {item}";
                        _questDetail.AddChild(itemLabel);
                    }
                }
            }

            // Action button
            if (quest.Status == QuestStatus.NotStarted)
            {
                var startButton = new Button();
                startButton.Text = "Start Quest";
                startButton.Pressed += () => OnStartQuestPressed(quest.Id);
                _questDetail.AddChild(startButton);
            }
        }

        #endregion

        #region Event Handlers

        private void OnQuestUpdated(object data)
        {
            RefreshQuestList();
            
            // Refresh detail view if selected quest was updated
            if (_selectedQuest != null && data is QuestEventData eventData)
            {
                if (_selectedQuest.Id == eventData.QuestId)
                {
                    ShowQuestDetail(_selectedQuest);
                }
            }
        }

        private void OnStartQuestPressed(string questId)
        {
            QuestManager.Instance?.StartQuest(questId);
        }

        #endregion
    }
}
