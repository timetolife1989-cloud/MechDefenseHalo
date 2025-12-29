using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Notifications
{
    /// <summary>
    /// Manages a queue of toast notifications
    /// </summary>
    public partial class NotificationQueue : Control
    {
        #region Exported Properties

        [Export] private PackedScene toastScene;
        [Export] private float displayDuration = 3.0f;

        #endregion

        #region Private Fields

        private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
        private ToastNotification currentToast;
        private bool isShowingNotification = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Load toast scene if not set
            if (toastScene == null)
            {
                toastScene = GD.Load<PackedScene>("res://UI/Notifications/ToastNotification.tscn");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show a notification
        /// </summary>
        public void ShowNotification(string message, NotificationType type = NotificationType.Info)
        {
            notificationQueue.Enqueue(new NotificationData
            {
                Message = message,
                Type = type,
                Timestamp = DateTime.Now
            });

            if (!isShowingNotification)
            {
                ShowNextNotification();
            }
        }

        #endregion

        #region Private Methods

        private void ShowNextNotification()
        {
            if (notificationQueue.Count == 0)
            {
                isShowingNotification = false;
                return;
            }

            isShowingNotification = true;
            var data = notificationQueue.Dequeue();

            if (toastScene == null)
            {
                GD.PrintErr("ToastNotification scene not loaded!");
                isShowingNotification = false;
                return;
            }

            currentToast = toastScene.Instantiate<ToastNotification>();
            AddChild(currentToast);

            currentToast.Display(data.Message, data.Type, displayDuration);
            currentToast.Dismissed += ShowNextNotification;
        }

        #endregion
    }
}
