using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Monetization
{
    /// <summary>
    /// Manages user consent for ads (GDPR/COPPA compliance)
    /// Handles age verification and consent status
    /// </summary>
    public partial class AdConsentManager : Node
    {
        #region Singleton

        private static AdConsentManager _instance;

        public static AdConsentManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("AdConsentManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Enums

        public enum ConsentStatus
        {
            Unknown,    // Not yet asked
            Granted,    // User gave consent
            Denied      // User denied consent
        }

        #endregion

        #region Constants

        private const int COPPA_AGE_LIMIT = 16;
        private const string CONSENT_SAVE_KEY = "ad_consent_status";
        private const string AGE_VERIFIED_KEY = "age_verified";
        private const string USER_REGION_KEY = "user_region";

        #endregion

        #region Public Properties

        public ConsentStatus CurrentConsentStatus { get; private set; } = ConsentStatus.Unknown;
        public bool IsAgeVerified { get; private set; } = false;
        public bool IsUnderAge { get; private set; } = false;
        public string UserRegion { get; private set; } = "Unknown";
        public bool RequiresConsent => IsEURegion() || !IsAgeVerified;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple AdConsentManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;

            // Load saved consent status
            LoadConsentData();

            GD.Print($"AdConsentManager initialized - Consent: {CurrentConsentStatus}, Region: {UserRegion}");
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
        /// Check if ads can be shown based on consent status
        /// </summary>
        /// <returns>True if ads can be shown</returns>
        public static bool CanShowAds()
        {
            if (Instance == null)
                return false;

            // If user is under COPPA age limit, no personalized ads
            if (Instance.IsUnderAge)
            {
                GD.Print("Cannot show personalized ads - user is under COPPA age limit");
                return false; // Or return true for non-personalized ads only
            }

            // If in EU region, explicit consent required
            if (Instance.IsEURegion() && Instance.CurrentConsentStatus != ConsentStatus.Granted)
            {
                GD.Print("Cannot show ads - EU user without consent");
                return false;
            }

            // If consent was explicitly denied
            if (Instance.CurrentConsentStatus == ConsentStatus.Denied)
            {
                GD.Print("Cannot show ads - user denied consent");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Show the consent dialog to the user
        /// </summary>
        public static void ShowConsentDialog()
        {
            if (Instance == null)
                return;

            // Emit event to show consent UI
            EventBus.Emit("show_consent_dialog", new ConsentDialogData
            {
                RequiresAgeVerification = !Instance.IsAgeVerified,
                Region = Instance.UserRegion
            });

            GD.Print("Consent dialog requested");
        }

        /// <summary>
        /// Set user age verification result
        /// </summary>
        /// <param name="age">User's reported age</param>
        public static void SetUserAge(int age)
        {
            if (Instance == null)
                return;

            Instance.IsAgeVerified = true;
            Instance.IsUnderAge = age < COPPA_AGE_LIMIT;

            SaveConsentData();

            GD.Print($"User age set: {age} (Under age: {Instance.IsUnderAge})");

            // If under age, automatically set consent to denied for personalized ads
            if (Instance.IsUnderAge)
            {
                SetConsentStatus(ConsentStatus.Denied);
            }
        }

        /// <summary>
        /// Set user's consent status
        /// </summary>
        /// <param name="status">Consent status to set</param>
        public static void SetConsentStatus(ConsentStatus status)
        {
            if (Instance == null)
                return;

            Instance.CurrentConsentStatus = status;
            SaveConsentData();

            GD.Print($"Consent status set to: {status}");

            // Emit event
            EventBus.Emit("consent_status_changed", new ConsentStatusData
            {
                Status = status,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// Set user's region
        /// </summary>
        /// <param name="region">ISO region code (e.g., "EU", "US", "UK")</param>
        public static void SetUserRegion(string region)
        {
            if (Instance == null)
                return;

            Instance.UserRegion = region;
            SaveConsentData();

            GD.Print($"User region set to: {region}");
        }

        /// <summary>
        /// Check if consent dialog should be shown on first launch
        /// </summary>
        /// <returns>True if dialog should be shown</returns>
        public static bool ShouldShowConsentDialog()
        {
            if (Instance == null)
                return false;

            // Show if consent is unknown or age is not verified
            return Instance.CurrentConsentStatus == ConsentStatus.Unknown || !Instance.IsAgeVerified;
        }

        #endregion

        #region Private Methods

        private bool IsEURegion()
        {
            // Check if user is in EU or UK (GDPR regions)
            return UserRegion == "EU" || UserRegion == "UK" || UserRegion == "EEA";
        }

        private void LoadConsentData()
        {
            // In a real implementation, this would load from save data
            // For now, we'll use default values
            // TODO: Integrate with save system when available
            
            CurrentConsentStatus = ConsentStatus.Unknown;
            IsAgeVerified = false;
            IsUnderAge = false;
            UserRegion = DetectUserRegion();
        }

        private static void SaveConsentData()
        {
            // In a real implementation, this would save to persistent storage
            // TODO: Integrate with save system when available
            
            if (Instance == null)
                return;

            GD.Print("Consent data saved (placeholder - integrate with save system)");
        }

        private string DetectUserRegion()
        {
            // In a real implementation, this would detect user's region
            // via IP geolocation or system settings
            // For now, return Unknown
            return "Unknown";
        }

        #endregion
    }

    #region Event Data Structures

    /// <summary>
    /// Data for consent dialog display
    /// </summary>
    public class ConsentDialogData
    {
        public bool RequiresAgeVerification { get; set; }
        public string Region { get; set; }
    }

    /// <summary>
    /// Data for consent status changed event
    /// </summary>
    public class ConsentStatusData
    {
        public AdConsentManager.ConsentStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
