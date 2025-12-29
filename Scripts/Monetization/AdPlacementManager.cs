using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Monetization
{
    /// <summary>
    /// Central manager for coordinating ad placements and offers.
    /// Ensures ads are shown at appropriate times with proper consent.
    /// </summary>
    public partial class AdPlacementManager : Node
    {
        #region Singleton

        private static AdPlacementManager _instance;

        public static AdPlacementManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("AdPlacementManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Public Properties

        public bool IsAdCurrentlyShowing { get; private set; } = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple AdPlacementManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;

            GD.Print("AdPlacementManager initialized");
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
        /// Request to show an ad with validation checks
        /// </summary>
        /// <param name="adType">Type of ad to show</param>
        /// <param name="onSuccess">Callback when ad is watched successfully</param>
        /// <param name="onSkipped">Callback when user declines ad</param>
        /// <returns>True if ad offer can be shown</returns>
        public static bool RequestAdOffer(string adType, Action onSuccess, Action onSkipped)
        {
            if (Instance == null)
            {
                onSkipped?.Invoke();
                return false;
            }

            // Check if ad is already showing
            if (Instance.IsAdCurrentlyShowing)
            {
                GD.Print($"Cannot show ad '{adType}' - another ad is currently showing");
                onSkipped?.Invoke();
                return false;
            }

            // Check consent
            if (!AdConsentManager.CanShowAds())
            {
                GD.Print($"Cannot show ad '{adType}' - no consent");
                onSkipped?.Invoke();
                return false;
            }

            // Check frequency cap
            if (!AdFrequencyCap.CanShowAd(adType))
            {
                onSkipped?.Invoke();
                return false;
            }

            // Emit offer event
            EventBus.Emit("ad_offered", new AdOfferedData
            {
                AdType = adType,
                Timestamp = DateTime.Now
            });

            return true;
        }

        /// <summary>
        /// Simulate watching an ad (in real implementation, this would trigger actual ad SDK)
        /// </summary>
        /// <param name="adType">Type of ad being watched</param>
        /// <param name="onComplete">Callback when ad completes</param>
        public static void WatchAd(string adType, Action onComplete)
        {
            if (Instance == null)
            {
                onComplete?.Invoke();
                return;
            }

            Instance.IsAdCurrentlyShowing = true;

            GD.Print($"[AD SIMULATION] Showing 30-second {adType} ad...");

            // In real implementation, this would call ad network SDK
            // For now, simulate a 30-second delay
            Instance.SimulateAdPlayback(adType, onComplete);
        }

        /// <summary>
        /// Record that user skipped/declined an ad offer
        /// </summary>
        /// <param name="adType">Type of ad that was skipped</param>
        public static void RecordAdSkipped(string adType)
        {
            EventBus.Emit("ad_skipped", new AdSkippedData
            {
                AdType = adType,
                Timestamp = DateTime.Now
            });

            GD.Print($"User skipped ad offer: {adType}");
        }

        #endregion

        #region Private Methods

        private async void SimulateAdPlayback(string adType, Action onComplete)
        {
            // TESTING: Reduced from 30 seconds to 1 second for faster iteration
            // PRODUCTION: Change to 30.0 when integrating real ad network
            // Real ad networks will handle their own timing
            await ToSignal(GetTree().CreateTimer(1.0), "timeout");

            IsAdCurrentlyShowing = false;

            // Record ad completion
            AdFrequencyCap.RecordAdShown(adType);

            GD.Print($"[AD SIMULATION] Ad completed: {adType}");

            onComplete?.Invoke();
        }

        #endregion
    }

    #region Event Data Structures

    /// <summary>
    /// Data for ad offered event
    /// </summary>
    public class AdOfferedData
    {
        public string AdType { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Data for ad skipped event
    /// </summary>
    public class AdSkippedData
    {
        public string AdType { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion

    #region Helper Structures

    /// <summary>
    /// Base class for ad offers
    /// </summary>
    public class AdOffer
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string BaseReward { get; set; }
        public string BonusReward { get; set; }
        public int AdDuration { get; set; } = 30;
        public string OptOutText { get; set; } = "No Thanks";
    }

    /// <summary>
    /// Loot-based ad offer
    /// </summary>
    public class LootAdOffer : AdOffer
    {
        public ItemRarity BaseRarity { get; set; }
        public ItemRarity BonusRarity { get; set; }
        public int BaseCredits { get; set; }
        public int BonusCredits { get; set; }
        public int BaseCores { get; set; }
        public int BonusCores { get; set; }
    }

    #endregion
}
