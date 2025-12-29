using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Economy;

namespace MechDefenseHalo.Monetization
{
    /// <summary>
    /// Manages daily login rewards with optional ad bonuses.
    /// Non-intrusive rewards given before gameplay starts.
    /// </summary>
    public partial class DailyLoginReward : Node
    {
        #region Constants

        private const string AD_TYPE = "daily_login";
        private const float BONUS_MULTIPLIER = 3.0f;
        private const int DAY_7_BONUS_CORES = 50;

        #endregion

        #region Private Fields

        private int[] _dailyRewards = { 100, 150, 200, 300, 500, 750, 1000 };
        private DateTime _lastLoginDate;
        private int _currentLoginDay = 0;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            LoadLoginData();

            GD.Print($"DailyLoginReward system initialized - Day {_currentLoginDay}");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check and show daily login bonus if applicable
        /// </summary>
        public void CheckDailyLogin()
        {
            DateTime today = DateTime.Today;

            // Check if player already claimed today
            if (_lastLoginDate.Date == today)
            {
                GD.Print("Daily login already claimed today");
                return;
            }

            // Check if streak is broken (more than 1 day gap)
            TimeSpan timeSinceLastLogin = today - _lastLoginDate.Date;
            if (timeSinceLastLogin.Days > 1)
            {
                // Streak broken, reset to day 1
                _currentLoginDay = 1;
                GD.Print("Login streak broken - reset to day 1");
            }
            else
            {
                // Continue streak
                _currentLoginDay++;
                if (_currentLoginDay > 7)
                {
                    _currentLoginDay = 1; // Loop back to day 1
                }
            }

            // Update last login date
            _lastLoginDate = today;
            SaveLoginData();

            // Show daily bonus offer
            ShowDailyBonus();
        }

        /// <summary>
        /// Show daily login bonus offer
        /// </summary>
        public void ShowDailyBonus()
        {
            int baseCredits = _dailyRewards[_currentLoginDay - 1];
            int bonusCredits = (int)(baseCredits * BONUS_MULTIPLIER);
            bool isDay7 = _currentLoginDay == 7;

            // Request ad offer
            bool canShowOffer = AdPlacementManager.RequestAdOffer(
                AD_TYPE,
                () => OnAdWatched(bonusCredits, isDay7),
                () => OnAdSkipped(baseCredits, isDay7)
            );

            if (canShowOffer)
            {
                // Emit event to show UI
                EventBus.Emit("show_daily_login_offer", new DailyLoginOfferData
                {
                    LoginDay = _currentLoginDay,
                    BaseCredits = baseCredits,
                    BonusCredits = bonusCredits,
                    BonusCores = isDay7 ? DAY_7_BONUS_CORES : 0,
                    IsDay7Bonus = isDay7
                });

                GD.Print($"Daily login offer shown for day {_currentLoginDay}");
            }
            else
            {
                // Give base rewards immediately
                GiveBaseRewards(baseCredits, isDay7);
            }
        }

        #endregion

        #region Private Methods

        private void OnAdWatched(int bonusCredits, bool isDay7)
        {
            GD.Print($"Daily login ad watched - giving {bonusCredits} credits (3x bonus)!");
            
            // Give bonus credits
            CurrencyManager.AddCredits(bonusCredits, "Daily Login Bonus");

            // Day 7 always gives cores bonus
            if (isDay7)
            {
                CurrencyManager.AddCores(DAY_7_BONUS_CORES, "Day 7 Login Bonus");
                GD.Print($"Day 7 bonus: +{DAY_7_BONUS_CORES} Cores!");
            }

            // Emit analytics event
            EventBus.Emit("daily_login_claimed", new DailyLoginClaimedData
            {
                LoginDay = _currentLoginDay,
                AdType = AD_TYPE,
                Credits = bonusCredits,
                WatchedAd = true
            });
        }

        private void OnAdSkipped(int baseCredits, bool isDay7)
        {
            GD.Print($"Daily login ad skipped - giving {baseCredits} credits");
            GiveBaseRewards(baseCredits, isDay7);
            AdPlacementManager.RecordAdSkipped(AD_TYPE);
        }

        private void GiveBaseRewards(int baseCredits, bool isDay7)
        {
            // Give base credits
            CurrencyManager.AddCredits(baseCredits, "Daily Login");

            // Day 7 always gives cores bonus (even without ad)
            if (isDay7)
            {
                CurrencyManager.AddCores(DAY_7_BONUS_CORES, "Day 7 Login Bonus");
                GD.Print($"Day 7 bonus: +{DAY_7_BONUS_CORES} Cores!");
            }

            // Emit analytics event
            EventBus.Emit("daily_login_claimed", new DailyLoginClaimedData
            {
                LoginDay = _currentLoginDay,
                AdType = AD_TYPE,
                Credits = baseCredits,
                WatchedAd = false
            });
        }

        private void LoadLoginData()
        {
            // In a real implementation, this would load from save data
            // For now, use default values
            _lastLoginDate = DateTime.Today.AddDays(-1); // Simulate previous day login
            _currentLoginDay = 1;

            // TODO: Integrate with save system
        }

        private void SaveLoginData()
        {
            // In a real implementation, this would save to persistent storage
            // TODO: Integrate with save system
            GD.Print($"Login data saved: Day {_currentLoginDay}, Last login: {_lastLoginDate.ToShortDateString()}");
        }

        #endregion
    }

    #region Event Data Structures

    /// <summary>
    /// Data for daily login offer display
    /// </summary>
    public class DailyLoginOfferData
    {
        public int LoginDay { get; set; }
        public int BaseCredits { get; set; }
        public int BonusCredits { get; set; }
        public int BonusCores { get; set; }
        public bool IsDay7Bonus { get; set; }
    }

    /// <summary>
    /// Data for daily login claimed event
    /// </summary>
    public class DailyLoginClaimedData
    {
        public int LoginDay { get; set; }
        public string AdType { get; set; }
        public int Credits { get; set; }
        public bool WatchedAd { get; set; }
    }

    #endregion
}
