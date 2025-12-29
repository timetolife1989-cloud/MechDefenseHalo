using System;

namespace MechDefenseHalo.Statistics
{
    /// <summary>
    /// Tracks session and playtime statistics
    /// </summary>
    public class SessionStats
    {
        // Time
        public float TotalPlaytimeSeconds { get; set; } = 0f;
        public int TotalSessions { get; set; } = 0;
        public DateTime FirstPlayedDate { get; set; } = DateTime.MinValue;
        public DateTime LastPlayedDate { get; set; } = DateTime.MinValue;
        public float LongestSessionSeconds { get; set; } = 0f;
        
        // Daily
        public int DailyLoginStreak { get; set; } = 0;
        public DateTime LastLoginDate { get; set; } = DateTime.MinValue;
        
        // Current Session
        public float CurrentSessionTime { get; set; } = 0f;
        public int CurrentSessionKills { get; set; } = 0;
        public int CurrentSessionWaves { get; set; } = 0;

        // Helper methods
        public void StartNewSession()
        {
            TotalSessions++;
            CurrentSessionTime = 0f;
            CurrentSessionKills = 0;
            CurrentSessionWaves = 0;
            
            DateTime now = DateTime.Now;
            LastPlayedDate = now;

            if (FirstPlayedDate == DateTime.MinValue)
            {
                FirstPlayedDate = now;
            }

            // Update daily login streak
            if (LastLoginDate.Date < now.Date)
            {
                if ((now.Date - LastLoginDate.Date).Days == 1)
                {
                    DailyLoginStreak++;
                }
                else if ((now.Date - LastLoginDate.Date).Days > 1)
                {
                    DailyLoginStreak = 1;
                }
                LastLoginDate = now;
            }
        }

        public void UpdateSession(float deltaTime)
        {
            CurrentSessionTime += deltaTime;
            TotalPlaytimeSeconds += deltaTime;

            if (CurrentSessionTime > LongestSessionSeconds)
            {
                LongestSessionSeconds = CurrentSessionTime;
            }
        }
    }
}
