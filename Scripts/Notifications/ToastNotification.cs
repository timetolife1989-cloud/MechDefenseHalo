using Godot;
using System;

namespace MechDefenseHalo.Notifications
{
    /// <summary>
    /// Toast notification UI element
    /// </summary>
    public partial class ToastNotification : Control
    {
        #region Signals

        [Signal]
        public delegate void DismissedEventHandler();

        #endregion

        #region Exported Properties

        [Export] private float fadeInDuration = 0.3f;
        [Export] private float fadeOutDuration = 0.3f;

        #endregion

        #region Private Fields

        private Label messageLabel;
        private Panel backgroundPanel;
        private Timer displayTimer;
        private bool isDismissing = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get UI elements (will be created in scene)
            messageLabel = GetNodeOrNull<Label>("Panel/MessageLabel");
            backgroundPanel = GetNodeOrNull<Panel>("Panel");
            
            if (messageLabel == null || backgroundPanel == null)
            {
                GD.PrintErr("ToastNotification: Required UI elements not found!");
                return;
            }

            // Setup timer
            displayTimer = new Timer();
            AddChild(displayTimer);
            displayTimer.OneShot = true;
            displayTimer.Timeout += OnDisplayTimerTimeout;

            // Start hidden
            Modulate = new Color(1, 1, 1, 0);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Display the notification
        /// </summary>
        public void Display(string message, NotificationType type, float duration)
        {
            if (messageLabel == null) return;

            messageLabel.Text = message;
            SetNotificationStyle(type);

            // Fade in
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), fadeInDuration);

            // Start display timer
            displayTimer.WaitTime = duration;
            displayTimer.Start();
        }

        #endregion

        #region Private Methods

        private void SetNotificationStyle(NotificationType type)
        {
            if (backgroundPanel == null) return;

            // Set color based on notification type
            Color bgColor = type switch
            {
                NotificationType.Success => new Color(0.2f, 0.8f, 0.2f, 0.9f),
                NotificationType.Error => new Color(0.8f, 0.2f, 0.2f, 0.9f),
                NotificationType.Warning => new Color(0.8f, 0.6f, 0.2f, 0.9f),
                NotificationType.Reward => new Color(1.0f, 0.8f, 0.2f, 0.9f),
                _ => new Color(0.3f, 0.3f, 0.3f, 0.9f), // Info
            };

            // Apply to panel
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = bgColor;
            styleBox.CornerRadiusTopLeft = 10;
            styleBox.CornerRadiusTopRight = 10;
            styleBox.CornerRadiusBottomLeft = 10;
            styleBox.CornerRadiusBottomRight = 10;
            styleBox.ContentMarginLeft = 20;
            styleBox.ContentMarginRight = 20;
            styleBox.ContentMarginTop = 15;
            styleBox.ContentMarginBottom = 15;

            backgroundPanel.AddThemeStyleboxOverride("panel", styleBox);
        }

        private void OnDisplayTimerTimeout()
        {
            if (isDismissing) return;
            
            isDismissing = true;

            // Fade out
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), fadeOutDuration);
            tween.TweenCallback(Callable.From(() => 
            {
                EmitSignal(SignalName.Dismissed);
                QueueFree();
            }));
        }

        #endregion
    }
}
