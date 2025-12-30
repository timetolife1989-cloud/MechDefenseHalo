using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.Leaderboard
{
    /// <summary>
    /// Central manager for the global leaderboard system
    /// Handles score submission, ranking, and persistence
    /// </summary>
    public partial class LeaderboardManager : Node
    {
        #region Singleton
        
        private static LeaderboardManager _instance;
        
        public static LeaderboardManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("LeaderboardManager accessed before initialization!");
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Exports
        
        [Export] public int MaxLeaderboardEntries { get; set; } = 100;
        [Export] public string SaveFilePath { get; set; } = "user://leaderboard_data.json";
        
        #endregion
        
        #region Private Fields
        
        private List<LeaderboardEntry> _entries = new List<LeaderboardEntry>();
        private FirebaseLeaderboard _firebaseLeaderboard;
        private RankCalculator _rankCalculator;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple LeaderboardManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }
            
            _instance = this;
            
            // Initialize components
            _firebaseLeaderboard = new FirebaseLeaderboard();
            _rankCalculator = new RankCalculator();
            
            LoadLeaderboard();
            
            GD.Print("LeaderboardManager initialized successfully");
        }
        
        public override void _ExitTree()
        {
            SaveLeaderboard();
            
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        #endregion
        
        #region Public Methods - Score Submission
        
        /// <summary>
        /// Submit a score to the leaderboard
        /// </summary>
        public void SubmitScore(string playerName, int score, int wave, int kills)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                GD.PrintErr("Cannot submit score: player name is empty");
                return;
            }
            
            var entry = new LeaderboardEntry
            {
                PlayerName = playerName,
                Score = score,
                Wave = wave,
                Kills = kills,
                Timestamp = DateTime.Now
            };
            
            _entries.Add(entry);
            _entries = _entries.OrderByDescending(e => e.Score).Take(MaxLeaderboardEntries).ToList();
            
            SaveLeaderboard();
            
            // Upload to Firebase
            _firebaseLeaderboard.UploadScore(entry);
            
            // Emit event for UI updates
            EventBus.Emit("leaderboard_updated", null);
            
            GD.Print($"Score submitted: {playerName} - {score} points (Wave {wave}, {kills} kills)");
        }
        
        /// <summary>
        /// Submit current game score
        /// </summary>
        public void SubmitCurrentGameScore()
        {
            var scoreTracker = ScoreTracker.Instance;
            if (scoreTracker != null)
            {
                string playerName = GetPlayerName();
                SubmitScore(playerName, scoreTracker.TotalScore, scoreTracker.CurrentWave, scoreTracker.TotalKills);
            }
        }
        
        #endregion
        
        #region Public Methods - Queries
        
        /// <summary>
        /// Get top scores from the leaderboard
        /// </summary>
        public List<LeaderboardEntry> GetTopScores(int count = 10)
        {
            return _entries.Take(count).ToList();
        }
        
        /// <summary>
        /// Get leaderboard entries for a specific time period
        /// </summary>
        public List<LeaderboardEntry> GetScoresByTimePeriod(TimePeriod period, int count = 10)
        {
            DateTime cutoffDate = GetCutoffDate(period);
            return _entries
                .Where(e => e.Timestamp >= cutoffDate)
                .OrderByDescending(e => e.Score)
                .Take(count)
                .ToList();
        }
        
        /// <summary>
        /// Get player's rank in the leaderboard
        /// </summary>
        public int GetPlayerRank(string playerName)
        {
            return _rankCalculator.GetPlayerRank(_entries, playerName);
        }
        
        /// <summary>
        /// Get player's best score
        /// </summary>
        public LeaderboardEntry GetPlayerBestScore(string playerName)
        {
            return _entries
                .Where(e => e.PlayerName == playerName)
                .OrderByDescending(e => e.Score)
                .FirstOrDefault();
        }
        
        /// <summary>
        /// Get all entries for a specific player
        /// </summary>
        public List<LeaderboardEntry> GetPlayerEntries(string playerName)
        {
            return _entries
                .Where(e => e.PlayerName == playerName)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }
        
        /// <summary>
        /// Get friend leaderboard (stub for future implementation)
        /// </summary>
        public List<LeaderboardEntry> GetFriendLeaderboard(List<string> friendNames, int count = 10)
        {
            return _entries
                .Where(e => friendNames.Contains(e.PlayerName))
                .OrderByDescending(e => e.Score)
                .Take(count)
                .ToList();
        }
        
        #endregion
        
        #region Private Methods - Persistence
        
        /// <summary>
        /// Save leaderboard to local storage
        /// </summary>
        private void SaveLeaderboard()
        {
            try
            {
                var data = new LeaderboardSaveData
                {
                    Entries = _entries,
                    LastSaved = DateTime.Now
                };
                
                string json = System.Text.Json.JsonSerializer.Serialize(data);
                var file = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Write);
                
                if (file != null)
                {
                    file.StoreString(json);
                    file.Close();
                    GD.Print($"Leaderboard saved successfully ({_entries.Count} entries)");
                }
                else
                {
                    GD.PrintErr($"Failed to open leaderboard file for writing: {SaveFilePath}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error saving leaderboard: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load leaderboard from local storage
        /// </summary>
        private void LoadLeaderboard()
        {
            try
            {
                if (!FileAccess.FileExists(SaveFilePath))
                {
                    GD.Print("No leaderboard save file found, starting fresh");
                    _entries = new List<LeaderboardEntry>();
                    
                    // Try to load from Firebase
                    _firebaseLeaderboard.DownloadLeaderboard(OnFirebaseLeaderboardLoaded);
                    return;
                }
                
                var file = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Read);
                if (file != null)
                {
                    string json = file.GetAsText();
                    file.Close();
                    
                    var data = System.Text.Json.JsonSerializer.Deserialize<LeaderboardSaveData>(json);
                    
                    if (data != null && data.Entries != null)
                    {
                        _entries = data.Entries;
                        GD.Print($"Leaderboard loaded successfully ({_entries.Count} entries)");
                        
                        // Merge with Firebase data
                        _firebaseLeaderboard.DownloadLeaderboard(OnFirebaseLeaderboardLoaded);
                    }
                    else
                    {
                        GD.PrintErr("Failed to deserialize leaderboard data");
                        _entries = new List<LeaderboardEntry>();
                    }
                }
                else
                {
                    GD.PrintErr($"Failed to open leaderboard file for reading: {SaveFilePath}");
                    _entries = new List<LeaderboardEntry>();
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error loading leaderboard: {ex.Message}");
                _entries = new List<LeaderboardEntry>();
            }
        }
        
        /// <summary>
        /// Callback when Firebase leaderboard is loaded
        /// </summary>
        private void OnFirebaseLeaderboardLoaded(List<LeaderboardEntry> firebaseEntries)
        {
            if (firebaseEntries != null && firebaseEntries.Count > 0)
            {
                // Merge Firebase entries with local entries
                var allEntries = new List<LeaderboardEntry>(_entries);
                allEntries.AddRange(firebaseEntries);
                
                // Remove duplicates and keep top scores
                _entries = allEntries
                    .GroupBy(e => new { e.PlayerName, e.Timestamp })
                    .Select(g => g.First())
                    .OrderByDescending(e => e.Score)
                    .Take(MaxLeaderboardEntries)
                    .ToList();
                
                SaveLeaderboard();
                EventBus.Emit("leaderboard_updated", null);
                
                GD.Print($"Firebase leaderboard merged ({firebaseEntries.Count} new entries)");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private string GetPlayerName()
        {
            // Get player name from settings
            if (SettingsManager.Instance != null && SettingsManager.Instance.CurrentSettings != null)
            {
                return SettingsManager.Instance.CurrentSettings.Gameplay.PlayerName;
            }
            
            // Fallback to default if settings not available
            return "Player";
        }
        
        private DateTime GetCutoffDate(TimePeriod period)
        {
            DateTime now = DateTime.Now;
            
            switch (period)
            {
                case TimePeriod.Daily:
                    return now.AddDays(-1);
                case TimePeriod.Weekly:
                    return now.AddDays(-7);
                case TimePeriod.Monthly:
                    return now.AddMonths(-1);
                case TimePeriod.AllTime:
                default:
                    return DateTime.MinValue;
            }
        }
        
        #endregion
    }
    
    #region Data Structures
    
    /// <summary>
    /// Leaderboard entry representing a single score
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public int Wave { get; set; }
        public int Kills { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Save data container for leaderboard
    /// </summary>
    [Serializable]
    public class LeaderboardSaveData
    {
        public List<LeaderboardEntry> Entries { get; set; }
        public DateTime LastSaved { get; set; }
    }
    
    /// <summary>
    /// Time period filter for leaderboard queries
    /// </summary>
    public enum TimePeriod
    {
        Daily,
        Weekly,
        Monthly,
        AllTime
    }
    
    #endregion
}
