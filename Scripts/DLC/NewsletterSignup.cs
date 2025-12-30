using Godot;
using System;
using System.Threading.Tasks;

namespace MechDefenseHalo.DLC
{
    /// <summary>
    /// Handles newsletter signup for DLC updates
    /// </summary>
    public partial class NewsletterSignup : Control
    {
        #region Exported Fields
        
        [Export] private LineEdit emailInput;
        [Export] private Button signupButton;
        [Export] private Label statusLabel;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            if (signupButton != null)
            {
                signupButton.Pressed += OnSignupPressed;
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private async void OnSignupPressed()
        {
            if (emailInput == null || statusLabel == null)
                return;
            
            string email = emailInput.Text;
            
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                statusLabel.Text = "Invalid email";
                return;
            }
            
            statusLabel.Text = "Subscribing...";
            
            // Send to backend
            bool success = await SubmitEmail(email);
            
            if (success)
            {
                statusLabel.Text = "Subscribed! You'll get DLC news.";
                emailInput.Clear();
            }
            else
            {
                statusLabel.Text = "Error. Try again later.";
            }
        }
        
        private async Task<bool> SubmitEmail(string email)
        {
            // TODO: API call to backend
            await Task.Delay(1000); // Simulate
            GD.Print($"Newsletter signup: {email}");
            return true;
        }
        
        #endregion
    }
}
