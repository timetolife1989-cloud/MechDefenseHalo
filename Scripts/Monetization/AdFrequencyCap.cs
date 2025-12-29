using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Monetization
{
    /// <summary>
    /// Manages ad frequency caps to prevent ad fatigue and spam.
    /// Enforces daily limits and cooldowns between ads.
    /// </summary>
    public partial class AdFrequencyCap : Node
    {
        #region Singleton

        private static AdFrequencyCap _instance;

        public static AdFrequencyCap Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("AdFrequencyCap accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Constants

        private const int MAX_ADS_PER_DAY = 5;
        private const float MIN_AD_COOLDOWN = 300f; // 5 minutes in seconds

        #endregion

        #region Private Fields

        private Dictionary<string, DateTime> _lastAdShown = new Dictionary<string, DateTime>();
        private int _adsShownToday = 0;
        private DateTime _lastResetDate;

        #endregion

        #region Public Properties

        public int AdsShownToday => _adsShownToday;
        public int RemainingAds => Mathf.Max(0, MAX_ADS_PER_DAY - _adsShownToday);
        public bool HasReachedDailyLimit => _adsShownToday >= MAX_ADS_PER_DAY;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple AdFrequencyCap instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            _lastResetDate = DateTime.Today;

            GD.Print($"AdFrequencyCap initialized - Max ads per day: {MAX_ADS_PER_DAY}");
        }

        public override void _Process(double delta)
        {
            // Check if we need to reset daily counter
            if (DateTime.Today > _lastResetDate)
            {
                ResetDailyCounter();
            }
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if an ad can be shown for a specific type
        /// </summary>
        /// <param name="adType">Type of ad (e.g., "victory", "milestone", "daily")</param>
        /// <returns>True if ad can be shown</returns>
        public static bool CanShowAd(string adType)
        {
            if (Instance == null)
                return false;

            // Check daily cap
            if (Instance._adsShownToday >= MAX_ADS_PER_DAY)
            {
                GD.Print($"Cannot show ad '{adType}' - daily limit reached ({MAX_ADS_PER_DAY} ads)");
                return false;
            }

            // Check cooldown between same ad type
            if (Instance._lastAdShown.ContainsKey(adType))
            {
                float timeSince = (float)(DateTime.Now - Instance._lastAdShown[adType]).TotalSeconds;
                if (timeSince < MIN_AD_COOLDOWN)
                {
                    float remainingCooldown = MIN_AD_COOLDOWN - timeSince;
                    GD.Print($"Cannot show ad '{adType}' - cooldown active ({remainingCooldown:F0}s remaining)");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Record that an ad was shown
        /// </summary>
        /// <param name="adType">Type of ad that was shown</param>
        public static void RecordAdShown(string adType)
        {
            if (Instance == null)
                return;

            Instance._adsShownToday++;
            Instance._lastAdShown[adType] = DateTime.Now;

            GD.Print($"Ad shown: {adType} ({Instance._adsShownToday}/{MAX_ADS_PER_DAY} today)");

            // Emit analytics event
            EventBus.Emit("ad_watched", new AdWatchedData
            {
                AdType = adType,
                AdsShownToday = Instance._adsShownToday,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// Get time remaining until next ad can be shown for a specific type
        /// </summary>
        /// <param name="adType">Type of ad to check</param>
        /// <returns>Seconds until cooldown expires, or 0 if ready</returns>
        public static float GetCooldownRemaining(string adType)
        {
            if (Instance == null)
                return 0f;

            if (!Instance._lastAdShown.ContainsKey(adType))
                return 0f;

            float timeSince = (float)(DateTime.Now - Instance._lastAdShown[adType]).TotalSeconds;
            float remaining = MIN_AD_COOLDOWN - timeSince;

            return Mathf.Max(0f, remaining);
        }

        /// <summary>
        /// Reset the daily ad counter (for testing or daily reset)
        /// </summary>
        public static void ResetDailyCounter()
        {
            if (Instance == null)
                return;

            Instance._adsShownToday = 0;
            Instance._lastResetDate = DateTime.Today;

            GD.Print("Daily ad counter reset");
        }

        /// <summary>
        /// Force reset all cooldowns (for testing only)
        /// </summary>
        public static void ResetAllCooldowns()
        {
            if (Instance == null)
                return;

            Instance._lastAdShown.Clear();
            GD.Print("All ad cooldowns cleared (testing only)");
        }

        #endregion
    }

    #region Event Data Structures

    /// <summary>
    /// Data for ad watched event
    /// </summary>
    public class AdWatchedData
    {
        public string AdType { get; set; }
        public int AdsShownToday { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
