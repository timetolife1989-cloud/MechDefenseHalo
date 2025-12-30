using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.DLC
{
    /// <summary>
    /// Displays DLC teaser with video, features, and countdown
    /// </summary>
    public partial class DLCTeaserSystem : Control
    {
        #region Exported Fields
        
        [Export] private VideoStreamPlayer videoPlayer;
        [Export] private Label titleLabel;
        [Export] private Label descriptionLabel;
        [Export] private Label countdownLabel;
        [Export] private VBoxContainer featuresContainer;
        [Export] private Button wishlistButton;
        
        #endregion
        
        #region Private Fields
        
        private DLCData currentDLC;
        
        #endregion
        
        #region Public Methods
        
        public void ShowTeaser(string dlcId)
        {
            currentDLC = DLCManager.Instance.GetDLCData(dlcId);
            
            if (currentDLC == null)
            {
                GD.PrintErr($"DLC not found: {dlcId}");
                return;
            }
            
            if (titleLabel != null)
                titleLabel.Text = currentDLC.Name;
            
            if (descriptionLabel != null)
                descriptionLabel.Text = currentDLC.TeaserText;
            
            // Load teaser video
            if (videoPlayer != null && FileAccess.FileExists(currentDLC.TeaserVideoPath))
            {
                var videoStream = ResourceLoader.Load<VideoStream>(currentDLC.TeaserVideoPath);
                videoPlayer.Stream = videoStream;
                videoPlayer.Play();
            }
            
            // Display features
            DisplayFeatures();
            
            // Show countdown
            UpdateCountdown();
            
            Visible = true;
        }
        
        #endregion
        
        #region Private Methods
        
        private void DisplayFeatures()
        {
            if (featuresContainer == null)
                return;
            
            foreach (var child in featuresContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            foreach (string feature in currentDLC.Features)
            {
                var featureLabel = new Label();
                featureLabel.Text = "• " + feature;
                featuresContainer.AddChild(featureLabel);
            }
        }
        
        private void UpdateCountdown()
        {
            if (countdownLabel == null || currentDLC == null)
                return;
            
            TimeSpan timeUntilRelease = currentDLC.ReleaseDate - DateTime.Now;
            
            if (timeUntilRelease.TotalSeconds > 0)
            {
                countdownLabel.Text = $"Releases in: {timeUntilRelease.Days} days";
            }
            else
            {
                countdownLabel.Text = "AVAILABLE NOW!";
            }
        }
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Process(double delta)
        {
            if (currentDLC != null && Visible)
            {
                UpdateCountdown();
            }
        }
        
        #endregion
    }
}
