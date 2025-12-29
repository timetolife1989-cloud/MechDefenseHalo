using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Achievements
{
    /// <summary>
    /// Displays toast notification when achievements are unlocked.
    /// 
    /// REQUIRED SCENE STRUCTURE (create manually in Godot):
    /// 
    /// Control (AchievementNotification) - Script: AchievementNotification.cs
    /// └─ Panel (Container)
    ///    ├─ HBoxContainer
    ///    │  ├─ TextureRect (Icon)
    ///    │  └─ VBoxContainer
    ///    │     ├─ Label (TitleLabel) - text: "Achievement Unlocked!"
    ///    │     ├─ Label (NameLabel) - text: "Achievement Name"
    ///    │     └─ Label (RewardLabel) - text: "Rewards"
    ///    └─ AnimationPlayer (Animator)
    /// </summary>
    public partial class AchievementNotification : Control
    {
        #region Export Variables

        [Export] public Label TitleLabel { get; set; }
        [Export] public Label NameLabel { get; set; }
        [Export] public Label RewardLabel { get; set; }
        [Export] public TextureRect Icon { get; set; }
        [Export] public AnimationPlayer Animator { get; set; }
        [Export] public AudioStreamPlayer SoundPlayer { get; set; }

        [Export] public float DisplayDuration { get; set; } = 3.0f;

        #endregion

        #region Private Fields

        private Timer _displayTimer;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Create display timer
            _displayTimer = new Timer();
            _displayTimer.OneShot = true;
            _displayTimer.Timeout += OnDisplayTimeout;
            AddChild(_displayTimer);

            // Subscribe to achievement unlock events
            EventBus.On(EventBus.AchievementUnlocked, OnAchievementUnlocked);

            // Start hidden
            Modulate = new Color(1, 1, 1, 0);
            Hide();

            GD.Print("AchievementNotification initialized");
        }

        public override void _ExitTree()
        {
            EventBus.Off(EventBus.AchievementUnlocked, OnAchievementUnlocked);
        }

        #endregion

        #region Event Handlers

        private void OnAchievementUnlocked(object data)
        {
            if (data is Achievement achievement)
            {
                ShowNotification(achievement);
            }
        }

        private void OnDisplayTimeout()
        {
            HideNotification();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show notification for an achievement
        /// </summary>
        public void ShowNotification(Achievement achievement)
        {
            if (achievement == null) return;

            // Set text
            if (TitleLabel != null)
                TitleLabel.Text = "🏆 Achievement Unlocked!";

            if (NameLabel != null)
                NameLabel.Text = achievement.Name;

            if (RewardLabel != null)
                RewardLabel.Text = FormatRewards(achievement);

            // Show the notification
            Show();
            
            // Play sound
            if (SoundPlayer != null)
                SoundPlayer.Play();

            // Animate in
            if (Animator != null && Animator.HasAnimation("slide_in"))
            {
                Animator.Play("slide_in");
            }
            else
            {
                // Fallback: simple fade in
                var tween = CreateTween();
                tween.TweenProperty(this, "modulate:a", 1.0, 0.3);
            }

            // Start display timer
            _displayTimer.Start(DisplayDuration);

            GD.Print($"Showing notification for: {achievement.Name}");
        }

        /// <summary>
        /// Hide the notification
        /// </summary>
        public void HideNotification()
        {
            // Animate out
            if (Animator != null && Animator.HasAnimation("slide_out"))
            {
                Animator.Play("slide_out");
                Animator.AnimationFinished += (animName) =>
                {
                    if (animName == "slide_out")
                        Hide();
                };
            }
            else
            {
                // Fallback: simple fade out
                var tween = CreateTween();
                tween.TweenProperty(this, "modulate:a", 0.0, 0.3);
                tween.TweenCallback(Callable.From(() => Hide()));
            }
        }

        #endregion

        #region Private Methods

        private string FormatRewards(Achievement achievement)
        {
            if (achievement.Rewards == null || achievement.Rewards.Count == 0)
                return "";

            var rewards = new System.Collections.Generic.List<string>();

            foreach (var reward in achievement.Rewards)
            {
                string rewardText = "";
                
                switch (reward.Key.ToLower())
                {
                    case "credits":
                    case "reward_credits":
                        rewardText = $"💰 {reward.Value} Credits";
                        break;
                    case "cores":
                    case "reward_cores":
                        rewardText = $"💎 {reward.Value} Cores";
                        break;
                    case "xp":
                    case "reward_xp":
                        rewardText = $"⭐ {reward.Value} XP";
                        break;
                    case "legendary":
                    case "reward_legendary":
                        rewardText = $"🎁 {reward.Value} Legendary Item(s)";
                        break;
                }

                if (!string.IsNullOrEmpty(rewardText))
                    rewards.Add(rewardText);
            }

            return string.Join(", ", rewards);
        }

        #endregion
    }
}
