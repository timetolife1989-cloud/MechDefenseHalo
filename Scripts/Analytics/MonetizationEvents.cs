using Godot;
using System;

namespace MechDefenseHalo.Analytics
{
    /// <summary>
    /// Centralized event constants for monetization analytics.
    /// Provides type-safe event names and helper methods.
    /// </summary>
    public static class MonetizationEvents
    {
        #region Event Names

        // Ad Events
        public const string AdOffered = "ad_offered";
        public const string AdWatched = "ad_watched";
        public const string AdSkipped = "ad_skipped";
        public const string AdFailed = "ad_failed";

        // Victory Bonus Events
        public const string VictoryBonusOffered = "victory_bonus_offered";
        public const string VictoryBonusClaimed = "victory_bonus_claimed";
        public const string VictoryBonusSkipped = "victory_bonus_skipped";

        // Milestone Events
        public const string MilestoneRewardOffered = "milestone_reward_offered";
        public const string MilestoneRewardClaimed = "milestone_reward_claimed";
        public const string MilestoneRewardSkipped = "milestone_reward_skipped";

        // Daily Login Events
        public const string DailyLoginOffered = "daily_login_offered";
        public const string DailyLoginClaimed = "daily_login_claimed";
        public const string DailyLoginSkipped = "daily_login_skipped";

        // Consent Events
        public const string ConsentDialogShown = "consent_dialog_shown";
        public const string ConsentGranted = "consent_granted";
        public const string ConsentDenied = "consent_denied";

        // Session Events
        public const string SessionStarted = "session_started";
        public const string SessionEnded = "session_ended";

        // Revenue Events
        public const string RevenueGenerated = "revenue_generated";

        #endregion

        #region Helper Methods

        /// <summary>
        /// Log an event to the console and analytics system
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="parameters">Event parameters</param>
        public static void LogEvent(string eventName, params (string key, object value)[] parameters)
        {
            string paramString = "";
            foreach (var param in parameters)
            {
                paramString += $"{param.key}={param.value}, ";
            }

            GD.Print($"[ANALYTICS] {eventName}: {paramString.TrimEnd(',', ' ')}");

            // In a real implementation, this would send to analytics service
            // TODO: Integrate with analytics SDK (Firebase, Unity Analytics, etc.)
        }

        /// <summary>
        /// Log an ad offered event
        /// </summary>
        public static void LogAdOffered(string adType, string context, int waveNumber = 0)
        {
            LogEvent(AdOffered, 
                ("ad_type", adType),
                ("context", context),
                ("wave", waveNumber),
                ("timestamp", DateTime.Now.ToString("o"))
            );
        }

        /// <summary>
        /// Log an ad watched event
        /// </summary>
        public static void LogAdWatched(string adType, float conversionRate)
        {
            LogEvent(AdWatched,
                ("ad_type", adType),
                ("conversion_rate", conversionRate),
                ("timestamp", DateTime.Now.ToString("o"))
            );
        }

        /// <summary>
        /// Log an ad skipped event
        /// </summary>
        public static void LogAdSkipped(string adType, string reason = "user_declined")
        {
            LogEvent(AdSkipped,
                ("ad_type", adType),
                ("reason", reason),
                ("timestamp", DateTime.Now.ToString("o"))
            );
        }

        /// <summary>
        /// Log revenue generated from an ad
        /// </summary>
        public static void LogRevenue(string adType, float revenueUSD, float eCPM)
        {
            LogEvent(RevenueGenerated,
                ("ad_type", adType),
                ("revenue_usd", revenueUSD),
                ("ecpm", eCPM),
                ("timestamp", DateTime.Now.ToString("o"))
            );
        }

        /// <summary>
        /// Log session start
        /// </summary>
        public static void LogSessionStart(int daysSinceInstall, int sessionNumber)
        {
            LogEvent(SessionStarted,
                ("days_since_install", daysSinceInstall),
                ("session_number", sessionNumber),
                ("timestamp", DateTime.Now.ToString("o"))
            );
        }

        /// <summary>
        /// Log retention milestone
        /// </summary>
        public static void LogRetentionMilestone(int day, bool retained)
        {
            LogEvent("retention_milestone",
                ("day", day),
                ("retained", retained),
                ("timestamp", DateTime.Now.ToString("o"))
            );
        }

        #endregion

        #region Analytics Integration Notes

        /*
         * INTEGRATION GUIDE:
         * 
         * To integrate with real analytics services:
         * 
         * 1. Firebase Analytics (Recommended for mobile):
         *    - Add Firebase Godot plugin
         *    - Call Firebase.Analytics.LogEvent(eventName, parameters)
         * 
         * 2. Unity Analytics:
         *    - Add Unity Analytics plugin
         *    - Call Analytics.CustomEvent(eventName, parameters)
         * 
         * 3. Custom Backend:
         *    - Send HTTP POST to your analytics endpoint
         *    - Include user ID, session ID, and event data
         * 
         * 4. Important Metrics to Track:
         *    - Daily Active Users (DAU)
         *    - Retention (D1, D7, D30)
         *    - Ad conversion rates by type
         *    - Average Revenue Per User (ARPU)
         *    - Average Revenue Per Daily Active User (ARPDAU)
         *    - Session length and frequency
         * 
         * 5. Privacy Considerations:
         *    - Always respect GDPR/COPPA consent
         *    - Anonymize user data where possible
         *    - Provide opt-out mechanisms
         *    - Store data securely
         */

        #endregion
    }
}
