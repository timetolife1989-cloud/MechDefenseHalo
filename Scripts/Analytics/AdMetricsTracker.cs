using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Analytics
{
    /// <summary>
    /// Tracks ad metrics for optimization and analysis.
    /// Monitors conversion rates, revenue, and user behavior.
    /// </summary>
    public partial class AdMetricsTracker : Node
    {
        #region Singleton

        private static AdMetricsTracker _instance;

        public static AdMetricsTracker Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("AdMetricsTracker accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Private Fields

        private Dictionary<string, AdTypeMetrics> _metricsPerAdType = new Dictionary<string, AdTypeMetrics>();
        private int _totalAdsOffered = 0;
        private int _totalAdsWatched = 0;
        private int _totalAdsSkipped = 0;

        #endregion

        #region Public Properties

        public float OverallConversionRate => _totalAdsOffered > 0 
            ? (float)_totalAdsWatched / _totalAdsOffered 
            : 0f;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple AdMetricsTracker instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;

            // Subscribe to ad events
            EventBus.On("ad_offered", OnAdOffered);
            EventBus.On("ad_watched", OnAdWatched);
            EventBus.On("ad_skipped", OnAdSkipped);

            GD.Print("AdMetricsTracker initialized");
        }

        public override void _ExitTree()
        {
            EventBus.Off("ad_offered", OnAdOffered);
            EventBus.Off("ad_watched", OnAdWatched);
            EventBus.Off("ad_skipped", OnAdSkipped);

            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Event Handlers

        private void OnAdOffered(object data)
        {
            if (data is Monetization.AdOfferedData offerData)
            {
                _totalAdsOffered++;
                
                var metrics = GetOrCreateMetrics(offerData.AdType);
                metrics.Offered++;

                GD.Print($"[METRICS] Ad offered: {offerData.AdType} (Total: {_totalAdsOffered})");
            }
        }

        private void OnAdWatched(object data)
        {
            if (data is Monetization.AdWatchedData watchData)
            {
                _totalAdsWatched++;
                
                var metrics = GetOrCreateMetrics(watchData.AdType);
                metrics.Watched++;

                GD.Print($"[METRICS] Ad watched: {watchData.AdType} (Total: {_totalAdsWatched}, CR: {OverallConversionRate:P1})");
            }
        }

        private void OnAdSkipped(object data)
        {
            if (data is Monetization.AdSkippedData skipData)
            {
                _totalAdsSkipped++;
                
                var metrics = GetOrCreateMetrics(skipData.AdType);
                metrics.Skipped++;

                GD.Print($"[METRICS] Ad skipped: {skipData.AdType} (Total: {_totalAdsSkipped})");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get metrics for a specific ad type
        /// </summary>
        /// <param name="adType">Ad type to query</param>
        /// <returns>Metrics for the ad type</returns>
        public static AdTypeMetrics GetMetrics(string adType)
        {
            if (Instance == null)
                return new AdTypeMetrics();

            return Instance._metricsPerAdType.ContainsKey(adType) 
                ? Instance._metricsPerAdType[adType] 
                : new AdTypeMetrics();
        }

        /// <summary>
        /// Print summary of all ad metrics
        /// </summary>
        public static void PrintMetricsSummary()
        {
            if (Instance == null)
                return;

            GD.Print("=== AD METRICS SUMMARY ===");
            GD.Print($"Total Ads Offered: {Instance._totalAdsOffered}");
            GD.Print($"Total Ads Watched: {Instance._totalAdsWatched}");
            GD.Print($"Total Ads Skipped: {Instance._totalAdsSkipped}");
            GD.Print($"Overall Conversion Rate: {Instance.OverallConversionRate:P1}");
            GD.Print("");

            foreach (var kvp in Instance._metricsPerAdType)
            {
                GD.Print($"--- {kvp.Key.ToUpper()} ---");
                GD.Print($"  Offered: {kvp.Value.Offered}");
                GD.Print($"  Watched: {kvp.Value.Watched}");
                GD.Print($"  Skipped: {kvp.Value.Skipped}");
                GD.Print($"  Conversion Rate: {kvp.Value.ConversionRate:P1}");
                GD.Print("");
            }
        }

        /// <summary>
        /// Reset all metrics (for testing)
        /// </summary>
        public static void ResetMetrics()
        {
            if (Instance == null)
                return;

            Instance._metricsPerAdType.Clear();
            Instance._totalAdsOffered = 0;
            Instance._totalAdsWatched = 0;
            Instance._totalAdsSkipped = 0;

            GD.Print("All ad metrics reset");
        }

        #endregion

        #region Private Methods

        private AdTypeMetrics GetOrCreateMetrics(string adType)
        {
            if (!_metricsPerAdType.ContainsKey(adType))
            {
                _metricsPerAdType[adType] = new AdTypeMetrics();
            }

            return _metricsPerAdType[adType];
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Metrics for a specific ad type
    /// </summary>
    public class AdTypeMetrics
    {
        public int Offered { get; set; } = 0;
        public int Watched { get; set; } = 0;
        public int Skipped { get; set; } = 0;

        public float ConversionRate => Offered > 0 
            ? (float)Watched / Offered 
            : 0f;

        public float SkipRate => Offered > 0 
            ? (float)Skipped / Offered 
            : 0f;
    }

    #endregion
}
