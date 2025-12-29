using System;

namespace MechDefenseHalo.Notifications
{
    /// <summary>
    /// Data structure for notifications
    /// </summary>
    public struct NotificationData
    {
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Types of notifications
    /// </summary>
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Reward
    }
}
