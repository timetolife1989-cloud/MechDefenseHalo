using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Notifications
{
    /// <summary>
    /// Badge to show number of unclaimed mission rewards
    /// </summary>
    public partial class NotificationBadge : Control
    {
        #region Private Fields

        private Label countLabel;
        private Panel backgroundPanel;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            countLabel = GetNodeOrNull<Label>("Panel/Count");
            backgroundPanel = GetNodeOrNull<Panel>("Panel");

            // Subscribe to events
            EventBus.On("mission_completed", OnMissionStateChanged);
            EventBus.On("mission_reward_claimed", OnMissionStateChanged);
            EventBus.On("daily_missions_refreshed", OnMissionStateChanged);

            UpdateBadge();
        }

        public override void _ExitTree()
        {
            EventBus.Off("mission_completed", OnMissionStateChanged);
            EventBus.Off("mission_reward_claimed", OnMissionStateChanged);
            EventBus.Off("daily_missions_refreshed", OnMissionStateChanged);
        }

        #endregion

        #region Private Methods

        private void UpdateBadge()
        {
            if (DailyMissionManager.Instance == null) return;

            int unclaimedCount = 0;
            foreach (var mission in DailyMissionManager.Instance.ActiveMissions)
            {
                if (mission.IsCompleted && !mission.IsRewardClaimed)
                {
                    unclaimedCount++;
                }
            }

            // Update count label
            if (countLabel != null)
            {
                countLabel.Text = unclaimedCount.ToString();
            }

            // Show/hide badge
            Visible = unclaimedCount > 0;
        }

        private void OnMissionStateChanged(object data)
        {
            CallDeferred(MethodName.UpdateBadge);
        }

        #endregion
    }
}
