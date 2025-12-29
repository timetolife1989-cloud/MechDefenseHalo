using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Monetization;

namespace MechDefenseHalo.UI.Monetization
{
    /// <summary>
    /// UI controller for Ad Consent dialog.
    /// Handles GDPR/COPPA compliance consent flow.
    /// </summary>
    public partial class AdConsentDialogUI : Control
    {
        #region Node References

        private Label _titleLabel;
        private Label _messageLabel;
        private SpinBox _ageSpinBox;
        private Button _acceptButton;
        private Button _declineButton;
        private CheckBox _privacyPolicyCheckbox;
        private Label _errorLabel;

        #endregion

        #region Private Fields

        private bool _requiresAgeVerification = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get node references
            _titleLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/TitleLabel");
            _messageLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/MessageLabel");
            _ageSpinBox = GetNodeOrNull<SpinBox>("Panel/VBoxContainer/AgeContainer/AgeSpinBox");
            _acceptButton = GetNodeOrNull<Button>("Panel/VBoxContainer/HBoxContainer/AcceptButton");
            _declineButton = GetNodeOrNull<Button>("Panel/VBoxContainer/HBoxContainer/DeclineButton");
            _privacyPolicyCheckbox = GetNodeOrNull<CheckBox>("Panel/VBoxContainer/PrivacyPolicyCheckbox");
            _errorLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/ErrorLabel");

            // Connect button signals
            if (_acceptButton != null)
                _acceptButton.Pressed += OnAcceptPressed;
            
            if (_declineButton != null)
                _declineButton.Pressed += OnDeclinePressed;

            // Subscribe to event
            EventBus.On(EventBus.ShowConsentDialog, OnShowDialog);

            // Hide by default
            Hide();

            GD.Print("AdConsentDialogUI initialized");
        }

        public override void _ExitTree()
        {
            EventBus.Off(EventBus.ShowConsentDialog, OnShowDialog);

            if (_acceptButton != null)
                _acceptButton.Pressed -= OnAcceptPressed;
            
            if (_declineButton != null)
                _declineButton.Pressed -= OnDeclinePressed;
        }

        #endregion

        #region Event Handlers

        private void OnShowDialog(object data)
        {
            if (data is ConsentDialogData dialogData)
            {
                ShowDialog(dialogData);
            }
        }

        private void OnAcceptPressed()
        {
            // Validate age if required
            if (_requiresAgeVerification && _ageSpinBox != null)
            {
                int age = (int)_ageSpinBox.Value;
                
                if (age < AdConsentManager.MinimumAge)
                {
                    ShowError($"You must be at least {AdConsentManager.MinimumAge} years old for personalized ads.");
                    return;
                }

                AdConsentManager.SetUserAge(age);
            }

            // Check privacy policy agreement
            if (_privacyPolicyCheckbox != null && !_privacyPolicyCheckbox.ButtonPressed)
            {
                ShowError("Please read and accept the Privacy Policy to continue.");
                return;
            }

            // Grant consent
            AdConsentManager.SetConsentStatus(AdConsentManager.ConsentStatus.Granted);
            
            GD.Print("User granted ad consent");
            Hide();
        }

        private void OnDeclinePressed()
        {
            // Deny consent
            AdConsentManager.SetConsentStatus(AdConsentManager.ConsentStatus.Denied);
            
            GD.Print("User declined ad consent");
            Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show the consent dialog
        /// </summary>
        /// <param name="dialogData">Dialog configuration data</param>
        public void ShowDialog(ConsentDialogData dialogData)
        {
            _requiresAgeVerification = dialogData.RequiresAgeVerification;

            // Update UI
            if (_titleLabel != null)
                _titleLabel.Text = "Privacy & Ads";

            if (_messageLabel != null)
            {
                string message = "This game shows optional rewarded video ads.\n\n";
                message += "✅ Ads are 100% optional (never required)\n";
                message += "✅ Watch ads to get bonus rewards\n";
                message += "✅ Maximum 5 ads per day\n";
                message += "✅ You can always skip ads\n\n";

                if (dialogData.Region == "EU" || dialogData.Region == "UK")
                {
                    message += "🇪🇺 GDPR: We need your consent to show personalized ads.\n";
                }

                message += "\nBy accepting, you agree to watch optional ads for bonus rewards.";

                _messageLabel.Text = message;
            }

            // Show/hide age verification
            if (_ageSpinBox != null)
            {
                var ageContainer = _ageSpinBox.GetParent();
                if (ageContainer != null)
                {
                    ageContainer.Visible = _requiresAgeVerification;
                }
                
                if (_requiresAgeVerification)
                {
                    _ageSpinBox.MinValue = 0;
                    _ageSpinBox.MaxValue = 120;
                    _ageSpinBox.Value = 18;
                }
            }

            if (_privacyPolicyCheckbox != null)
            {
                _privacyPolicyCheckbox.Text = "I have read and agree to the Privacy Policy";
                _privacyPolicyCheckbox.ButtonPressed = false;
            }

            if (_acceptButton != null)
                _acceptButton.Text = "Accept";

            if (_declineButton != null)
                _declineButton.Text = "Decline (No Ads)";

            // Show the dialog
            Show();

            GD.Print($"Consent dialog displayed (Region: {dialogData.Region})");
        }

        #endregion

        #region Private Methods

        private void ShowError(string message)
        {
            GD.PrintErr(message);
            
            // Show error in UI if label exists
            if (_errorLabel != null)
            {
                _errorLabel.Text = message;
                _errorLabel.Modulate = new Color(1, 0.3f, 0.3f); // Red color
                _errorLabel.Show();
            }
        }

        #endregion
    }
}
