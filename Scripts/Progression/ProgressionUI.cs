using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Progression
{
    /// <summary>
    /// Manages progression UI elements including XP bar, level display, and level-up notifications
    /// </summary>
    public partial class ProgressionUI : Control
    {
        #region Exported Properties

        [Export] public NodePath XPBarPath { get; set; }
        [Export] public NodePath LevelLabelPath { get; set; }
        [Export] public NodePath XPLabelPath { get; set; }
        [Export] public NodePath LevelUpPanelPath { get; set; }
        [Export] public NodePath RewardLabelPath { get; set; }

        #endregion

        #region Private Fields

        private ProgressBar _xpBar;
        private Label _levelLabel;
        private Label _xpLabel;
        private Panel _levelUpPanel;
        private Label _rewardLabel;
        private AnimationPlayer _animationPlayer;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get UI elements
            _xpBar = GetNodeOrNull<ProgressBar>(XPBarPath);
            _levelLabel = GetNodeOrNull<Label>(LevelLabelPath);
            _xpLabel = GetNodeOrNull<Label>(XPLabelPath);
            _levelUpPanel = GetNodeOrNull<Panel>(LevelUpPanelPath);
            _rewardLabel = GetNodeOrNull<Label>(RewardLabelPath);

            // Try to find animation player
            _animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

            // Hide level up panel initially
            if (_levelUpPanel != null)
            {
                _levelUpPanel.Visible = false;
            }

            // Subscribe to events
            EventBus.On("xp_gained", OnXPGained);
            EventBus.On("player_leveled_up", OnPlayerLeveledUp);
            EventBus.On("level_reward_granted", OnLevelRewardGranted);

            // Update UI with current values
            UpdateUI();

            GD.Print("ProgressionUI initialized");
        }

        public override void _ExitTree()
        {
            // Unsubscribe from events
            EventBus.Off("xp_gained", OnXPGained);
            EventBus.Off("player_leveled_up", OnPlayerLeveledUp);
            EventBus.Off("level_reward_granted", OnLevelRewardGranted);
        }

        #endregion

        #region Event Handlers

        private void OnXPGained(object data)
        {
            UpdateUI();
        }

        private void OnPlayerLeveledUp(object data)
        {
            UpdateUI();
            ShowLevelUpNotification(data);
        }

        private void OnLevelRewardGranted(object data)
        {
            if (data is LevelRewardData rewardData)
            {
                UpdateRewardDisplay(rewardData);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update all UI elements with current progression data
        /// </summary>
        private void UpdateUI()
        {
            if (PlayerLevel.Instance == null) return;

            // Update XP bar
            if (_xpBar != null)
            {
                _xpBar.Value = PlayerLevel.GetProgressToNextLevel() * 100;
            }

            // Update level label
            if (_levelLabel != null)
            {
                int prestige = PrestigeSystem.Instance?.PrestigeLevel ?? 0;
                if (prestige > 0)
                {
                    _levelLabel.Text = $"Level {PlayerLevel.Instance.CurrentLevel} [P{prestige}]";
                }
                else
                {
                    _levelLabel.Text = $"Level {PlayerLevel.Instance.CurrentLevel}";
                }
            }

            // Update XP label
            if (_xpLabel != null)
            {
                if (PlayerLevel.Instance.CurrentLevel >= 100)
                {
                    _xpLabel.Text = "MAX LEVEL";
                }
                else
                {
                    _xpLabel.Text = $"{PlayerLevel.Instance.CurrentXP} / {PlayerLevel.Instance.XPToNextLevel} XP";
                }
            }
        }

        /// <summary>
        /// Show level up notification with animation
        /// </summary>
        private void ShowLevelUpNotification(object data)
        {
            if (_levelUpPanel == null) return;

            _levelUpPanel.Visible = true;

            // Play animation if available
            if (_animationPlayer != null && _animationPlayer.HasAnimation("level_up"))
            {
                _animationPlayer.Play("level_up");
            }

            // Auto-hide after 3 seconds using async/await
            HideLevelUpPanelAfterDelay();
        }

        private async void HideLevelUpPanelAfterDelay()
        {
            await ToSignal(GetTree().CreateTimer(3.0), SceneTreeTimer.SignalName.Timeout);
            
            if (_levelUpPanel != null && IsInstanceValid(_levelUpPanel))
            {
                _levelUpPanel.Visible = false;
            }
        }

        /// <summary>
        /// Update reward display text
        /// </summary>
        private void UpdateRewardDisplay(LevelRewardData rewardData)
        {
            if (_rewardLabel == null) return;

            string rewardText = $"Level {rewardData.Level} Rewards:\n";
            rewardText += $"+{rewardData.Credits} Credits\n";
            rewardText += $"+{rewardData.Cores} Cores";

            if (rewardData.IsMilestone)
            {
                var milestone = LevelRewards.GetMilestoneReward(rewardData.Level);
                if (milestone != null)
                {
                    rewardText += $"\n\n★ {milestone.Description} ★";
                }
            }

            _rewardLabel.Text = rewardText;
        }

        #endregion
    }
}
