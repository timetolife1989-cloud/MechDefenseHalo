using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Monetization;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.UI.Monetization
{
    /// <summary>
    /// UI controller for Victory Bonus offer panel.
    /// Shows post-boss victory loot upgrade offer.
    /// </summary>
    public partial class VictoryBonusOfferUI : Control
    {
        #region Node References

        private Label _titleLabel;
        private Label _baseRewardLabel;
        private Label _bonusRewardLabel;
        private Button _watchAdButton;
        private Button _noThanksButton;
        private Panel _backgroundPanel;

        #endregion

        #region Private Fields

        private VictoryBonusOfferData _currentOffer;
        private Action _onAdWatched;
        private Action _onSkipped;

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
            _backgroundPanel = GetNodeOrNull<Panel>("Panel");

            // Connect button signals
            if (_watchAdButton != null)
                _watchAdButton.Pressed += OnWatchAdPressed;
            
            if (_noThanksButton != null)
                _noThanksButton.Pressed += OnNoThanksPressed;

            // Subscribe to event
            EventBus.On(EventBus.ShowVictoryBonusOffer, OnShowOffer);

            // Hide by default
            Hide();

            GD.Print("VictoryBonusOfferUI initialized");
        }

        public override void _ExitTree()
        {
            EventBus.Off(EventBus.ShowVictoryBonusOffer, OnShowOffer);

            if (_watchAdButton != null)
                _watchAdButton.Pressed -= OnWatchAdPressed;
            
            if (_noThanksButton != null)
                _noThanksButton.Pressed -= OnNoThanksPressed;
        }

        #endregion

        #region Event Handlers

        private void OnShowOffer(object data)
        {
            if (data is VictoryBonusOfferData offerData)
            {
                ShowOffer(offerData);
            }
        }

        private void OnWatchAdPressed()
        {
            GD.Print("Watch Ad button pressed");
            
            // Start ad playback
            AdPlacementManager.WatchAd("victory", () =>
            {
                _onAdWatched?.Invoke();
                Hide();
            });
        }

        private void OnNoThanksPressed()
        {
            GD.Print("No Thanks button pressed");
            
            _onSkipped?.Invoke();
            AdPlacementManager.RecordAdSkipped("victory");
            Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show the victory bonus offer
        /// </summary>
        /// <param name="offerData">Offer data to display</param>
        public void ShowOffer(VictoryBonusOfferData offerData)
        {
            _currentOffer = offerData;

            // Update UI
            if (_titleLabel != null)
                _titleLabel.Text = $"🎉 {offerData.BossName} DEFEATED!";

            if (_baseRewardLabel != null)
            {
                _baseRewardLabel.Text = $"Base Loot:\n" +
                    $"💰 {offerData.BaseCredits} Credits\n" +
                    $"🔷 {offerData.BaseCores} Cores\n" +
                    $"⚔️ {RarityConfig.GetDisplayName(offerData.BaseRarity)} Item";
            }

            if (_bonusRewardLabel != null)
            {
                _bonusRewardLabel.Text = $"🎁 WATCH AD TO UPGRADE:\n" +
                    $"💰 {offerData.BonusCredits} Credits (2x)\n" +
                    $"🔷 {offerData.BonusCores} Cores (2x)\n" +
                    $"⚔️ {RarityConfig.GetDisplayName(offerData.BonusRarity)} Item ⬆️";
            }

            if (_watchAdButton != null)
                _watchAdButton.Text = "Watch 30s Ad";

            if (_noThanksButton != null)
                _noThanksButton.Text = "No Thanks";

            // Show the panel
            Show();

            GD.Print($"Victory bonus offer displayed for {offerData.BossName}");
        }

        #endregion
    }
}
