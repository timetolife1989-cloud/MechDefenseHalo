using Godot;
using System;

namespace MechDefenseHalo.Notifications
{
    /// <summary>
    /// Tracks progress for mission objectives in real-time
    /// This is a utility class that listens for game events and updates mission progress
    /// </summary>
    public partial class MissionProgressTracker : Node
    {
        #region Private Fields

        private DailyMissionManager missionManager;
        private float surviveTimeAccumulator = 0f;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            missionManager = GetParent<DailyMissionManager>();
            if (missionManager == null)
            {
                GD.PrintErr("MissionProgressTracker must be a child of DailyMissionManager!");
            }
        }

        public override void _Process(double delta)
        {
            // Track survive time missions
            TrackSurviveTime((float)delta);
        }

        #endregion

        #region Private Methods

        private void TrackSurviveTime(float delta)
        {
            surviveTimeAccumulator += delta;

            // Update every second
            if (surviveTimeAccumulator >= 1.0f)
            {
                int secondsElapsed = (int)surviveTimeAccumulator;
                surviveTimeAccumulator -= secondsElapsed;

                // Emit event for survive time tracking
                Core.EventBus.Emit("survive_time_tick", secondsElapsed);
            }
        }

        #endregion
    }
}
