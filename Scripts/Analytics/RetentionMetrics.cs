using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Analytics
{
    /// <summary>
    /// Tracks player retention metrics related to monetization.
    /// Monitors how ad frequency affects player retention.
    /// </summary>
    public partial class RetentionMetrics : Node
    {
        #region Singleton

        private static RetentionMetrics _instance;

        public static RetentionMetrics Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("RetentionMetrics accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Private Fields

        private DateTime _firstSessionDate;
        private DateTime _lastSessionDate;
        private int _totalSessions = 0;
        private int _totalPlaytimeMinutes = 0;
        private Dictionary<int, bool> _dayRetention = new Dictionary<int, bool>();

        #endregion

        #region Public Properties

        public int DaysSinceInstall => (DateTime.Today - _firstSessionDate.Date).Days;
        public bool IsDay1Retained => _dayRetention.ContainsKey(1) && _dayRetention[1];
        public bool IsDay7Retained => _dayRetention.ContainsKey(7) && _dayRetention[7];
        public bool IsDay30Retained => _dayRetention.ContainsKey(30) && _dayRetention[30];

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple RetentionMetrics instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;

            LoadRetentionData();
            RecordSession();

            GD.Print($"RetentionMetrics initialized - Day {DaysSinceInstall} player");
        }

        public override void _ExitTree()
        {
            SaveRetentionData();

            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Record a new session
        /// </summary>
        public static void RecordSession()
        {
            if (Instance == null)
                return;

            Instance._totalSessions++;
            Instance._lastSessionDate = DateTime.Now;

            // Update retention tracking
            int daysSinceInstall = Instance.DaysSinceInstall;
            if (!Instance._dayRetention.ContainsKey(daysSinceInstall))
            {
                Instance._dayRetention[daysSinceInstall] = true;
            }

            GD.Print($"Session recorded - Total sessions: {Instance._totalSessions}, Day {daysSinceInstall}");

            // Emit analytics event
            EventBus.Emit("session_started", new SessionData
            {
                SessionNumber = Instance._totalSessions,
                DaysSinceInstall = daysSinceInstall
            });
        }

        /// <summary>
        /// Record playtime in minutes
        /// </summary>
        /// <param name="minutes">Minutes played</param>
        public static void RecordPlaytime(int minutes)
        {
            if (Instance == null)
                return;

            Instance._totalPlaytimeMinutes += minutes;
            GD.Print($"Playtime recorded: +{minutes} minutes (Total: {Instance._totalPlaytimeMinutes})");
        }

        /// <summary>
        /// Print retention summary
        /// </summary>
        public static void PrintRetentionSummary()
        {
            if (Instance == null)
                return;

            GD.Print("=== RETENTION METRICS SUMMARY ===");
            GD.Print($"Days Since Install: {Instance.DaysSinceInstall}");
            GD.Print($"Total Sessions: {Instance._totalSessions}");
            GD.Print($"Total Playtime: {Instance._totalPlaytimeMinutes} minutes");
            GD.Print($"Day 1 Retained: {Instance.IsDay1Retained}");
            GD.Print($"Day 7 Retained: {Instance.IsDay7Retained}");
            GD.Print($"Day 30 Retained: {Instance.IsDay30Retained}");
        }

        /// <summary>
        /// Get average session length in minutes
        /// </summary>
        /// <returns>Average session length</returns>
        public static float GetAverageSessionLength()
        {
            if (Instance == null || Instance._totalSessions == 0)
                return 0f;

            return (float)Instance._totalPlaytimeMinutes / Instance._totalSessions;
        }

        #endregion

        #region Private Methods

        private void LoadRetentionData()
        {
            // In a real implementation, this would load from save data
            // For now, simulate a new player
            _firstSessionDate = DateTime.Today;
            _lastSessionDate = DateTime.Today;
            _totalSessions = 0;
            _totalPlaytimeMinutes = 0;
            _dayRetention.Clear();

            // TODO: Integrate with save system
        }

        private void SaveRetentionData()
        {
            // In a real implementation, this would save to persistent storage
            // TODO: Integrate with save system
            GD.Print("Retention data saved (placeholder)");
        }

        #endregion
    }

    #region Event Data Structures

    /// <summary>
    /// Data for session started event
    /// </summary>
    public class SessionData
    {
        public int SessionNumber { get; set; }
        public int DaysSinceInstall { get; set; }
    }

    #endregion
}
