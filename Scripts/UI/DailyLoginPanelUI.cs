using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Monetization;

namespace MechDefenseHalo.UI.Monetization
{
    /// <summary>
    /// UI controller for Daily Login reward panel.
    /// Shows daily login bonus with optional ad multiplier.
    /// </summary>
    public partial class DailyLoginPanelUI : Control
    {
        #region Node References

        private Label _titleLabel;
        private Label _dayLabel;
        private Label _baseRewardLabel;
        private Label _bonusRewardLabel;
        private Label _specialBonusLabel;
        private Button _watchAdButton;
        private Button _claimBaseButton;

        #endregion

        #region Private Fields

        private DailyLoginOfferData _currentOffer;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get node references
            _titleLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/TitleLabel");
            _dayLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/DayLabel");
            _baseRewardLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/BaseRewardLabel");
            _bonusRewardLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/BonusRewardLabel");
            _specialBonusLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/SpecialBonusLabel");
            _watchAdButton = GetNodeOrNull<Button>("Panel/VBoxContainer/HBoxContainer/WatchAdButton");
            _claimBaseButton = GetNodeOrNull<Button>("Panel/VBoxContainer/HBoxContainer/ClaimBaseButton");

            // Connect button signals
            if (_watchAdButton != null)
                _watchAdButton.Pressed += OnWatchAdPressed;
            
            if (_claimBaseButton != null)
                _claimBaseButton.Pressed += OnClaimBasePressed;

            // Subscribe to event
            EventBus.On(EventBus.ShowDailyLoginOffer, OnShowOffer);

            // Hide by default
            Hide();

            GD.Print("DailyLoginPanelUI initialized");
        }

        public override void _ExitTree()
        {
            EventBus.Off(EventBus.ShowDailyLoginOffer, OnShowOffer);

            if (_watchAdButton != null)
                _watchAdButton.Pressed -= OnWatchAdPressed;
            
            if (_claimBaseButton != null)
                _claimBaseButton.Pressed -= OnClaimBasePressed;
        }

        #endregion

        #region Event Handlers

        private void OnShowOffer(object data)
        {
            if (data is DailyLoginOfferData offerData)
            {
                ShowOffer(offerData);
            }
        }

        private void OnWatchAdPressed()
        {
            GD.Print("Daily login - Watch Ad button pressed");
            
            // Start ad playback
            AdPlacementManager.WatchAd("daily_login", () =>
            {
                Hide();
            });
        }

        private void OnClaimBasePressed()
        {
            GD.Print("Daily login - Claim Base button pressed");
            
            AdPlacementManager.RecordAdSkipped("daily_login");
            Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show the daily login offer
        /// </summary>
        /// <param name="offerData">Offer data to display</param>
        public void ShowOffer(DailyLoginOfferData offerData)
        {
            _currentOffer = offerData;

            // Update UI
            if (_titleLabel != null)
                _titleLabel.Text = "🌅 DAILY REWARD";

            if (_dayLabel != null)
                _dayLabel.Text = $"Day {offerData.LoginDay}";

            if (_baseRewardLabel != null)
            {
                _baseRewardLabel.Text = $"💰 {offerData.BaseCredits} Credits";
            }

            if (_bonusRewardLabel != null)
            {
                _bonusRewardLabel.Text = $"🎁 WATCH AD FOR 3x BONUS:\n" +
                    $"💰 {offerData.BonusCredits} Credits";
            }

            // Show special day 7 bonus
            if (_specialBonusLabel != null)
            {
                if (offerData.IsDay7Bonus)
                {
                    _specialBonusLabel.Text = $"✨ WEEK COMPLETE! ✨\n" +
                        $"+{offerData.BonusCores} Cores Bonus\n" +
                        $"(Awarded Regardless of Ad)";
                    _specialBonusLabel.Show();
                }
                else
                {
                    _specialBonusLabel.Hide();
                }
            }

            if (_watchAdButton != null)
                _watchAdButton.Text = "Watch 30s Ad (3x)";

            if (_claimBaseButton != null)
                _claimBaseButton.Text = "Claim Base Reward";

            // Show the panel
            Show();

            GD.Print($"Daily login offer displayed for day {offerData.LoginDay}");
        }

        #endregion
    }
}
