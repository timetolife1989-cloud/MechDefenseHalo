using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Monetization
{
    /// <summary>
    /// Central manager that initializes and coordinates all monetization systems.
    /// Should be added to the game's autoload/singleton scene.
    /// </summary>
    public partial class MonetizationManager : Node
    {
        #region Singleton

        private static MonetizationManager _instance;

        public static MonetizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("MonetizationManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Child Nodes

        private AdFrequencyCap _adFrequencyCap;
        private AdConsentManager _adConsentManager;
        private AdPlacementManager _adPlacementManager;
        private VictoryBonusAd _victoryBonusAd;
        private MilestoneRewardAd _milestoneRewardAd;
        private DailyLoginReward _dailyLoginReward;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple MonetizationManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;

            // Initialize child systems
            InitializeSystems();

            // Check for consent dialog on first launch
            if (AdConsentManager.ShouldShowConsentDialog())
            {
                CallDeferred(nameof(ShowConsentDialogDeferred));
            }

            // Check for daily login reward
            if (_dailyLoginReward != null)
            {
                CallDeferred(nameof(CheckDailyLoginDeferred));
            }

            GD.Print("MonetizationManager initialized successfully");
            GD.Print("===========================================");
            GD.Print("Ethical Victory Monetization System Active");
            GD.Print("- Max 5 ads per day");
            GD.Print("- 5 minute cooldown between ads");
            GD.Print("- All ads are 100% optional");
            GD.Print("- GDPR/COPPA compliant");
            GD.Print("===========================================");
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
            // Create and add child nodes for each system
            
            // Core systems
            _adFrequencyCap = new AdFrequencyCap();
            AddChild(_adFrequencyCap);
            _adFrequencyCap.Name = "AdFrequencyCap";

            _adConsentManager = new AdConsentManager();
            AddChild(_adConsentManager);
            _adConsentManager.Name = "AdConsentManager";

            _adPlacementManager = new AdPlacementManager();
            AddChild(_adPlacementManager);
            _adPlacementManager.Name = "AdPlacementManager";

            // Ad placement systems
            _victoryBonusAd = new VictoryBonusAd();
            AddChild(_victoryBonusAd);
            _victoryBonusAd.Name = "VictoryBonusAd";

            _milestoneRewardAd = new MilestoneRewardAd();
            AddChild(_milestoneRewardAd);
            _milestoneRewardAd.Name = "MilestoneRewardAd";

            _dailyLoginReward = new DailyLoginReward();
            AddChild(_dailyLoginReward);
            _dailyLoginReward.Name = "DailyLoginReward";

            GD.Print("All monetization systems initialized");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually trigger consent dialog
        /// </summary>
        public static void ShowConsentDialog()
        {
            AdConsentManager.ShowConsentDialog();
        }

        /// <summary>
        /// Manually check daily login reward
        /// </summary>
        public static void CheckDailyLogin()
        {
            if (Instance?._dailyLoginReward != null)
            {
                Instance._dailyLoginReward.CheckDailyLogin();
            }
        }

        /// <summary>
        /// Get current ad limits status
        /// </summary>
        /// <returns>Status string</returns>
        public static string GetAdLimitStatus()
        {
            int remaining = AdFrequencyCap.Instance?.RemainingAds ?? 0;
            int shown = AdFrequencyCap.Instance?.AdsShownToday ?? 0;
            
            return $"Ads today: {shown}/5 (Remaining: {remaining})";
        }

        /// <summary>
        /// Print debug info about monetization state
        /// </summary>
        public static void PrintDebugInfo()
        {
            GD.Print("=== MONETIZATION DEBUG INFO ===");
            GD.Print($"Consent Status: {AdConsentManager.Instance?.CurrentConsentStatus}");
            GD.Print($"Can Show Ads: {AdConsentManager.CanShowAds()}");
            GD.Print(GetAdLimitStatus());
            GD.Print("================================");
        }

        #endregion

        #region Private Methods

        private void ShowConsentDialogDeferred()
        {
            AdConsentManager.ShowConsentDialog();
        }

        private void CheckDailyLoginDeferred()
        {
            _dailyLoginReward.CheckDailyLogin();
        }

        #endregion
    }
}
