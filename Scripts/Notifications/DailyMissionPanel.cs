using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Notifications
{
    /// <summary>
    /// Main UI panel for displaying daily missions
    /// </summary>
    public partial class DailyMissionPanel : Control
    {
        #region Exported Properties

        [Export] private PackedScene missionListItemScene;

        #endregion

        #region Private Fields

        private Label titleLabel;
        private Label timeRemainingLabel;
        private VBoxContainer missionListContainer;
        private Button refreshInfoButton;
        private Timer updateTimer;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get UI elements
            titleLabel = GetNodeOrNull<Label>("Panel/VBox/Title");
            timeRemainingLabel = GetNodeOrNull<Label>("Panel/VBox/TimeRemaining");
            missionListContainer = GetNodeOrNull<VBoxContainer>("Panel/VBox/Scroll/MissionList");
            refreshInfoButton = GetNodeOrNull<Button>("Panel/VBox/RefreshInfo");

            // Load mission list item scene
            if (missionListItemScene == null)
            {
                missionListItemScene = GD.Load<PackedScene>("res://UI/Notifications/MissionListItem.tscn");
            }

            // Setup update timer
            updateTimer = new Timer();
            AddChild(updateTimer);
            updateTimer.WaitTime = 1.0f;
            updateTimer.Autostart = true;
            updateTimer.Timeout += UpdateTimeRemaining;

            // Setup refresh button
            if (refreshInfoButton != null)
            {
                refreshInfoButton.Pressed += OnRefreshInfoPressed;
            }

            // Subscribe to events
            EventBus.On("daily_missions_refreshed", OnDailyMissionsRefreshed);
            EventBus.On("mission_completed", OnMissionCompleted);

            // Load missions
            RefreshMissionList();
        }

        public override void _ExitTree()
        {
            EventBus.Off("daily_missions_refreshed", OnDailyMissionsRefreshed);
            EventBus.Off("mission_completed", OnMissionCompleted);
        }

        #endregion

        #region Private Methods

        private void RefreshMissionList()
        {
            if (missionListContainer == null || DailyMissionManager.Instance == null)
                return;

            // Clear existing items
            foreach (Node child in missionListContainer.GetChildren())
            {
                child.QueueFree();
            }

            // Get active missions
            var missions = DailyMissionManager.Instance.ActiveMissions;

            // Create UI items for each mission
            foreach (var mission in missions)
            {
                var itemInstance = missionListItemScene.Instantiate<MissionListItem>();
                missionListContainer.AddChild(itemInstance);
                itemInstance.SetMission(mission);

                // Connect claim button signal
                itemInstance.ClaimRewardPressed += OnClaimRewardPressed;
            }

            UpdateTimeRemaining();
        }

        private void UpdateTimeRemaining()
        {
            if (timeRemainingLabel == null || DailyMissionManager.Instance == null)
                return;

            var missions = DailyMissionManager.Instance.ActiveMissions;
            if (missions.Count == 0)
            {
                timeRemainingLabel.Text = "No active missions";
                return;
            }

            var nextReset = SaveManager.GetDateTime("last_daily_reset").AddHours(24);
            var timeRemaining = nextReset - DateTime.Now;

            if (timeRemaining.TotalSeconds <= 0)
            {
                timeRemainingLabel.Text = "Refreshing...";
                DailyMissionManager.Instance?.ForceRefresh();
                return;
            }

            timeRemainingLabel.Text = $"Resets in: {timeRemaining.Hours}h {timeRemaining.Minutes}m";
        }

        private void OnClaimRewardPressed(string missionId)
        {
            DailyMissionManager.Instance?.ClaimMissionReward(missionId);
            RefreshMissionList();
        }

        private void OnRefreshInfoPressed()
        {
            RefreshMissionList();
        }

        private void OnDailyMissionsRefreshed(object data)
        {
            RefreshMissionList();
        }

        private void OnMissionCompleted(object data)
        {
            // Refresh to update UI
            CallDeferred(MethodName.RefreshMissionList);
        }

        #endregion
    }
}
