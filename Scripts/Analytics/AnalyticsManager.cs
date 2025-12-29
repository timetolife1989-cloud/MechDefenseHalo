using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Analytics
{
    /// <summary>
    /// Central manager for analytics systems.
    /// Initializes and coordinates analytics tracking.
    /// </summary>
    public partial class AnalyticsManager : Node
    {
        #region Singleton

        private static AnalyticsManager _instance;

        public static AnalyticsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("AnalyticsManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Child Nodes

        private AdMetricsTracker _adMetricsTracker;
        private RetentionMetrics _retentionMetrics;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple AnalyticsManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;

            // Initialize child systems
            InitializeSystems();

            GD.Print("AnalyticsManager initialized successfully");
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Initialization

        private void InitializeSystems()
        {
            // Create and add child nodes for each analytics system
            
            _adMetricsTracker = new AdMetricsTracker();
            AddChild(_adMetricsTracker);
            _adMetricsTracker.Name = "AdMetricsTracker";

            _retentionMetrics = new RetentionMetrics();
            AddChild(_retentionMetrics);
            _retentionMetrics.Name = "RetentionMetrics";

            GD.Print("All analytics systems initialized");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Print all analytics summaries
        /// </summary>
        public static void PrintAllSummaries()
        {
            GD.Print("\n");
            AdMetricsTracker.PrintMetricsSummary();
            GD.Print("\n");
            RetentionMetrics.PrintRetentionSummary();
            GD.Print("\n");
        }

        /// <summary>
        /// Reset all analytics (for testing)
        /// </summary>
        public static void ResetAllAnalytics()
        {
            AdMetricsTracker.ResetMetrics();
            GD.Print("All analytics reset");
        }

        #endregion
    }
}
