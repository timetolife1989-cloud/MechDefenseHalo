using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Notifications
{
    /// <summary>
    /// UI component for displaying a single mission in the list
    /// </summary>
    public partial class MissionListItem : PanelContainer
    {
        #region Signals

        [Signal]
        public delegate void ClaimRewardPressedEventHandler(string missionId);

        #endregion

        #region Private Fields

        private Mission mission;
        private Label titleLabel;
        private Label descriptionLabel;
        private ProgressBar progressBar;
        private Label progressLabel;
        private Label rewardLabel;
        private Button claimButton;
        private TextureRect iconRect;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get UI elements
            titleLabel = GetNodeOrNull<Label>("HBox/VBox/Title");
            descriptionLabel = GetNodeOrNull<Label>("HBox/VBox/Description");
            progressBar = GetNodeOrNull<ProgressBar>("HBox/VBox/ProgressBar");
            progressLabel = GetNodeOrNull<Label>("HBox/VBox/ProgressLabel");
            rewardLabel = GetNodeOrNull<Label>("HBox/VBox/Reward");
            claimButton = GetNodeOrNull<Button>("HBox/ClaimButton");
            iconRect = GetNodeOrNull<TextureRect>("HBox/Icon");

            if (claimButton != null)
            {
                claimButton.Pressed += OnClaimButtonPressed;
            }

            // Subscribe to mission events
            EventBus.On("mission_progress_updated", OnMissionProgressUpdated);
            EventBus.On("mission_completed", OnMissionCompleted);
        }

        public override void _ExitTree()
        {
            EventBus.Off("mission_progress_updated", OnMissionProgressUpdated);
            EventBus.Off("mission_completed", OnMissionCompleted);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the mission data to display
        /// </summary>
        public void SetMission(Mission missionData)
        {
            mission = missionData;
            UpdateDisplay();
        }

        #endregion

        #region Private Methods

        private void UpdateDisplay()
        {
            if (mission == null) return;

            // Update title
            if (titleLabel != null)
            {
                titleLabel.Text = mission.Title;
            }

            // Update description
            if (descriptionLabel != null)
            {
                descriptionLabel.Text = mission.Description;
            }

            // Update progress bar
            if (progressBar != null)
            {
                progressBar.MaxValue = mission.RequiredProgress;
                progressBar.Value = mission.CurrentProgress;
            }

            // Update progress label
            if (progressLabel != null)
            {
                progressLabel.Text = $"{mission.CurrentProgress}/{mission.RequiredProgress}";
            }

            // Update reward label
            if (rewardLabel != null)
            {
                string rewardText = "";
                if (mission.Rewards.ContainsKey("Credits"))
                    rewardText += $"💰 {mission.Rewards["Credits"]} Credits ";
                if (mission.Rewards.ContainsKey("Cores"))
                    rewardText += $"⚡ {mission.Rewards["Cores"]} Cores ";
                if (mission.Rewards.ContainsKey("XP"))
                    rewardText += $"⭐ {mission.Rewards["XP"]} XP";
                
                rewardLabel.Text = rewardText.Trim();
            }

            // Update claim button
            if (claimButton != null)
            {
                claimButton.Disabled = !mission.IsCompleted || mission.IsRewardClaimed;
                claimButton.Text = mission.IsRewardClaimed ? "Claimed" : "Claim Reward";
            }

            // Set icon based on mission type
            if (iconRect != null)
            {
                // Icon would be set based on mission type
                // For now, we'll just leave it as is
            }
        }

        private void OnClaimButtonPressed()
        {
            if (mission != null)
            {
                EmitSignal(SignalName.ClaimRewardPressed, mission.ID);
            }
        }

        private void OnMissionProgressUpdated(object data)
        {
            if (mission == null) return;

            // Check if this update is for our mission
            if (data is Godot.Collections.Dictionary dict && dict.ContainsKey("MissionID"))
            {
                string missionId = dict["MissionID"].ToString();
                if (missionId == mission.ID && dict.ContainsKey("Progress"))
                {
                    mission.CurrentProgress = (int)dict["Progress"];
                    UpdateDisplay();
                }
            }
        }

        private void OnMissionCompleted(object data)
        {
            if (mission == null || data == null) return;

            string completedMissionId = data.ToString();
            if (completedMissionId == mission.ID)
            {
                mission.IsCompleted = true;
                UpdateDisplay();
            }
        }

        #endregion
    }
}
