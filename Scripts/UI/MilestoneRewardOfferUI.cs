using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Monetization;

namespace MechDefenseHalo.UI.Monetization
{
    /// <summary>
    /// UI controller for Milestone Reward offer panel.
    /// Shows wave milestone chest upgrade offer.
    /// </summary>
    public partial class MilestoneRewardOfferUI : Control
    {
        #region Node References

        private Label _titleLabel;
        private Label _baseRewardLabel;
        private Label _bonusRewardLabel;
        private Button _watchAdButton;
        private Button _noThanksButton;

        #endregion

        #region Private Fields

        private MilestoneRewardOfferData _currentOffer;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get node references
            _titleLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/TitleLabel");
            _baseRewardLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/BaseRewardLabel");
            _bonusRewardLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/BonusRewardLabel");
            _watchAdButton = GetNodeOrNull<Button>("Panel/VBoxContainer/HBoxContainer/WatchAdButton");
            _noThanksButton = GetNodeOrNull<Button>("Panel/VBoxContainer/HBoxContainer/NoThanksButton");

            // Connect button signals
            if (_watchAdButton != null)
                _watchAdButton.Pressed += OnWatchAdPressed;
            
            if (_noThanksButton != null)
                _noThanksButton.Pressed += OnNoThanksPressed;

            // Subscribe to event
            EventBus.On(EventBus.ShowMilestoneRewardOffer, OnShowOffer);

            // Hide by default
            Hide();

            GD.Print("MilestoneRewardOfferUI initialized");
        }

        public override void _ExitTree()
        {
            EventBus.Off(EventBus.ShowMilestoneRewardOffer, OnShowOffer);

            if (_watchAdButton != null)
                _watchAdButton.Pressed -= OnWatchAdPressed;
            
            if (_noThanksButton != null)
                _noThanksButton.Pressed -= OnNoThanksPressed;
        }

        #endregion

        #region Event Handlers

        private void OnShowOffer(object data)
        {
            if (data is MilestoneRewardOfferData offerData)
            {
                ShowOffer(offerData);
            }
        }

        private void OnWatchAdPressed()
        {
            GD.Print("Milestone ad - Watch Ad button pressed");
            
            // Start ad playback
            AdPlacementManager.WatchAd("milestone", () =>
            {
                Hide();
            });
        }

        private void OnNoThanksPressed()
        {
            GD.Print("Milestone ad - No Thanks button pressed");
            
            AdPlacementManager.RecordAdSkipped("milestone");
            Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show the milestone reward offer
        /// </summary>
        /// <param name="offerData">Offer data to display</param>
        public void ShowOffer(MilestoneRewardOfferData offerData)
        {
            _currentOffer = offerData;

            // Update UI
            if (_titleLabel != null)
                _titleLabel.Text = $"🎖️ WAVE {offerData.WaveNumber} COMPLETE!";

            if (_baseRewardLabel != null)
            {
                _baseRewardLabel.Text = $"Standard Chest:\n" +
                    $"📦 {offerData.BaseItems} Items\n" +
                    $"💰 {offerData.BaseCredits} Credits";
            }

            if (_bonusRewardLabel != null)
            {
                _bonusRewardLabel.Text = $"🎁 WATCH AD FOR PREMIUM CHEST:\n" +
                    $"📦 {offerData.BonusItems} Items (+{offerData.BonusItems - offerData.BaseItems})\n" +
                    $"💰 {offerData.BonusCredits} Credits (+{offerData.BonusCredits - offerData.BaseCredits})\n" +
                    $"⬆️ Better Item Rarity";
            }

            if (_watchAdButton != null)
                _watchAdButton.Text = "Watch 30s Ad";

            if (_noThanksButton != null)
                _noThanksButton.Text = "Claim Standard";

            // Show the panel
            Show();

            GD.Print($"Milestone reward offer displayed for wave {offerData.WaveNumber}");
        }

        #endregion
    }
}
