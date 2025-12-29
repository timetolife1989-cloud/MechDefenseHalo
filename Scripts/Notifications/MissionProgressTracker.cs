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

        private float surviveTimeAccumulator = 0f;
        private bool isGameActive = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Subscribe to game state events
            Core.EventBus.On(Core.EventBus.GameStarted, OnGameStarted);
            Core.EventBus.On(Core.EventBus.GameOver, OnGameOver);
            Core.EventBus.On(Core.EventBus.GamePaused, OnGamePaused);
        }

        public override void _ExitTree()
        {
            Core.EventBus.Off(Core.EventBus.GameStarted, OnGameStarted);
            Core.EventBus.Off(Core.EventBus.GameOver, OnGameOver);
            Core.EventBus.Off(Core.EventBus.GamePaused, OnGamePaused);
        }

        public override void _Process(double delta)
        {
            // Track survive time missions only during active gameplay
            if (isGameActive)
            {
                TrackSurviveTime((float)delta);
            }
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

        private void OnGameStarted(object data)
        {
            isGameActive = true;
            surviveTimeAccumulator = 0f;
        }

        private void OnGameOver(object data)
        {
            isGameActive = false;
        }

        private void OnGamePaused(object data)
        {
            if (data is bool isPaused)
            {
                isGameActive = !isPaused;
            }
        }

        #endregion
    }
}
