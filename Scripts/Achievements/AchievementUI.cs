using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Achievements
{
    /// <summary>
    /// UI for displaying all achievements with categories, progress, and filters.
    /// 
    /// REQUIRED SCENE STRUCTURE (create manually in Godot):
    /// 
    /// Control (AchievementUI) - Script: AchievementUI.cs
    /// ├─ Panel (Background)
    /// │  ├─ Label (Title) - text: "ACHIEVEMENTS"
    /// │  ├─ Label (ProgressLabel) - text: "0/50 (0%)"
    /// │  ├─ ProgressBar (CompletionBar)
    /// │  ├─ HBoxContainer (FilterButtons)
    /// │  │  ├─ Button (AllButton) - text: "All"
    /// │  │  ├─ Button (CombatButton) - text: "Combat"
    /// │  │  ├─ Button (CollectionButton) - text: "Collection"
    /// │  │  ├─ Button (ProgressionButton) - text: "Progression"
    /// │  │  ├─ Button (BossButton) - text: "Boss"
    /// │  │  └─ Button (SecretButton) - text: "Secret"
    /// │  ├─ ScrollContainer (AchievementList)
    /// │  │  └─ VBoxContainer (AchievementContainer)
    /// │  └─ Button (CloseButton) - text: "Close"
    /// </summary>
    public partial class AchievementUI : Control
    {
        #region Export Variables

        [Export] public Label ProgressLabel { get; set; }
        [Export] public ProgressBar CompletionBar { get; set; }
        [Export] public VBoxContainer AchievementContainer { get; set; }
        [Export] public Button CloseButton { get; set; }
        
        // Filter buttons
        [Export] public Button AllButton { get; set; }
        [Export] public Button CombatButton { get; set; }
        [Export] public Button CollectionButton { get; set; }
        [Export] public Button ProgressionButton { get; set; }
        [Export] public Button BossButton { get; set; }
        [Export] public Button SecretButton { get; set; }

        [Export] public PackedScene AchievementItemPrefab { get; set; }

        #endregion

        #region Private Fields

        private AchievementManager _achievementManager;
        private string _currentFilter = "All";
        private List<Control> _achievementItems = new List<Control>();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            _achievementManager = GetNode<AchievementManager>("/root/AchievementManager");

            if (_achievementManager == null)
            {
                GD.PrintErr("AchievementUI: AchievementManager not found!");
                return;
            }

            // Connect buttons
            if (CloseButton != null)
                CloseButton.Pressed += OnClosePressed;

            if (AllButton != null)
                AllButton.Pressed += () => OnFilterPressed("All");
            if (CombatButton != null)
                CombatButton.Pressed += () => OnFilterPressed("Combat");
            if (CollectionButton != null)
                CollectionButton.Pressed += () => OnFilterPressed("Collection");
            if (ProgressionButton != null)
                ProgressionButton.Pressed += () => OnFilterPressed("Progression");
            if (BossButton != null)
                BossButton.Pressed += () => OnFilterPressed("Boss");
            if (SecretButton != null)
                SecretButton.Pressed += () => OnFilterPressed("Secret");

            // Listen for achievement unlock events
            EventBus.On(EventBus.AchievementUnlocked, OnAchievementUnlocked);

            // Initial display
            RefreshDisplay();

            // Hide by default
            Hide();

            GD.Print("AchievementUI initialized");
        }

        public override void _Input(InputEvent @event)
        {
            // Toggle with 'A' key
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.A && !keyEvent.Echo)
            {
                ToggleVisibility();
                GetViewport().SetInputAsHandled();
            }
        }

        public override void _ExitTree()
        {
            EventBus.Off(EventBus.AchievementUnlocked, OnAchievementUnlocked);
        }

        #endregion

        #region Event Handlers

        private void OnClosePressed()
        {
            Hide();
        }

        private void OnFilterPressed(string filter)
        {
            _currentFilter = filter;
            RefreshDisplay();
        }

        private void OnAchievementUnlocked(object data)
        {
            // Refresh display when achievement is unlocked
            RefreshDisplay();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Toggle UI visibility
        /// </summary>
        public void ToggleVisibility()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Refresh the entire display
        /// </summary>
        public void RefreshDisplay()
        {
            UpdateProgress();
            UpdateAchievementList();
        }

        #endregion

        #region Private Methods - Display Updates

        private void UpdateProgress()
        {
            int total = _achievementManager.TotalAchievements;
            int completed = _achievementManager.CompletedAchievements;
            float percentage = _achievementManager.CompletionPercentage;

            if (ProgressLabel != null)
            {
                ProgressLabel.Text = $"{completed}/{total} ({percentage:F1}%)";
            }

            if (CompletionBar != null)
            {
                CompletionBar.MaxValue = total;
                CompletionBar.Value = completed;
            }
        }

        private void UpdateAchievementList()
        {
            // Clear existing items
            ClearAchievementItems();

            // Get filtered achievements
            List<Achievement> achievements;
            if (_currentFilter == "All")
            {
                achievements = _achievementManager.GetAllAchievements();
            }
            else
            {
                achievements = _achievementManager.GetAchievementsByCategory(_currentFilter);
            }

            // Sort: Completed last, then by progress
            achievements = achievements
                .OrderBy(a => a.IsCompleted)
                .ThenByDescending(a => a.GetCompletionPercent())
                .ToList();

            // Create UI items for each achievement
            foreach (var achievement in achievements)
            {
                CreateAchievementItem(achievement);
            }
        }

        private void ClearAchievementItems()
        {
            foreach (var item in _achievementItems)
            {
                item.QueueFree();
            }
            _achievementItems.Clear();
        }

        private void CreateAchievementItem(Achievement achievement)
        {
            if (AchievementContainer == null) return;

            Control item;

            // If we have a prefab, instantiate it
            if (AchievementItemPrefab != null)
            {
                item = AchievementItemPrefab.Instantiate<Control>();
                // Assume the prefab has a method to set achievement data
                if (item.HasMethod("SetAchievement"))
                {
                    item.Call("SetAchievement", achievement);
                }
            }
            else
            {
                // Create simple item
                item = CreateSimpleAchievementItem(achievement);
            }

            AchievementContainer.AddChild(item);
            _achievementItems.Add(item);
        }

        private Control CreateSimpleAchievementItem(Achievement achievement)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(0, 80);

            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 10);
            margin.AddThemeConstantOverride("margin_right", 10);
            margin.AddThemeConstantOverride("margin_top", 10);
            margin.AddThemeConstantOverride("margin_bottom", 10);
            panel.AddChild(margin);

            var hbox = new HBoxContainer();
            margin.AddChild(hbox);

            // Status icon
            var statusLabel = new Label();
            statusLabel.Text = achievement.IsCompleted ? "✅" : "⬜";
            statusLabel.CustomMinimumSize = new Vector2(40, 0);
            hbox.AddChild(statusLabel);

            // Achievement info
            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hbox.AddChild(vbox);

            var nameLabel = new Label();
            nameLabel.Text = achievement.GetDisplayName();
            nameLabel.AddThemeColorOverride("font_color", achievement.IsCompleted ? Colors.Gold : Colors.White);
            vbox.AddChild(nameLabel);

            var descLabel = new Label();
            descLabel.Text = achievement.GetDisplayDescription();
            descLabel.AddThemeColorOverride("font_color", Colors.Gray);
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            vbox.AddChild(descLabel);

            // Progress bar (if not completed and not secret)
            if (!achievement.IsCompleted && achievement.RequiredProgress > 1 && (!achievement.IsSecret))
            {
                var progressBar = new ProgressBar();
                progressBar.MaxValue = achievement.RequiredProgress;
                progressBar.Value = achievement.Progress;
                progressBar.ShowPercentage = true;
                vbox.AddChild(progressBar);

                var progressText = new Label();
                progressText.Text = $"{achievement.Progress}/{achievement.RequiredProgress}";
                progressText.AddThemeFontSizeOverride("font_size", 10);
                vbox.AddChild(progressText);
            }

            // Rewards (if completed)
            if (achievement.IsCompleted && achievement.Rewards != null && achievement.Rewards.Count > 0)
            {
                var rewardLabel = new Label();
                rewardLabel.Text = "Rewards: " + FormatRewards(achievement);
                rewardLabel.AddThemeColorOverride("font_color", Colors.LightGreen);
                rewardLabel.AddThemeFontSizeOverride("font_size", 11);
                vbox.AddChild(rewardLabel);
            }

            return panel;
        }

        private string FormatRewards(Achievement achievement)
        {
            var rewards = new List<string>();

            foreach (var reward in achievement.Rewards)
            {
                switch (reward.Key.ToLower())
                {
                    case "credits":
                    case "reward_credits":
                        rewards.Add($"{reward.Value} Credits");
                        break;
                    case "cores":
                    case "reward_cores":
                        rewards.Add($"{reward.Value} Cores");
                        break;
                    case "xp":
                    case "reward_xp":
                        rewards.Add($"{reward.Value} XP");
                        break;
                    case "legendary":
                    case "reward_legendary":
                        rewards.Add($"{reward.Value} Legendary");
                        break;
                }
            }

            return string.Join(", ", rewards);
        }

        #endregion
    }
}
