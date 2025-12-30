using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Settings;

namespace MechDefenseHalo.Leaderboard
{
    /// <summary>
    /// UI component for displaying the leaderboard
    /// Shows top scores, player rank, and filtering options
    /// </summary>
    public partial class LeaderboardUI : Control
    {
        #region Exports
        
        [Export] public NodePath EntriesContainerPath { get; set; }
        [Export] public NodePath PlayerRankLabelPath { get; set; }
        [Export] public NodePath TimePeriodButtonsPath { get; set; }
        [Export] public NodePath RefreshButtonPath { get; set; }
        [Export] public PackedScene EntryPrefab { get; set; }
        [Export] public int MaxDisplayedEntries { get; set; } = 10;
        
        #endregion
        
        #region Private Fields
        
        private Container _entriesContainer;
        private Label _playerRankLabel;
        private Button _refreshButton;
        private TimePeriod _currentPeriod = TimePeriod.AllTime;
        private Dictionary<TimePeriod, Button> _periodButtons = new Dictionary<TimePeriod, Button>();
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            InitializeNodes();
            ConnectSignals();
            RefreshLeaderboard();
            
            GD.Print("LeaderboardUI initialized");
        }
        
        public override void _ExitTree()
        {
            DisconnectSignals();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeNodes()
        {
            // Get entries container
            if (EntriesContainerPath != null)
            {
                _entriesContainer = GetNodeOrNull<Container>(EntriesContainerPath);
                if (_entriesContainer == null)
                {
                    GD.PrintErr("LeaderboardUI: Entries container not found");
                }
            }
            
            // Get player rank label
            if (PlayerRankLabelPath != null)
            {
                _playerRankLabel = GetNodeOrNull<Label>(PlayerRankLabelPath);
            }
            
            // Get refresh button
            if (RefreshButtonPath != null)
            {
                _refreshButton = GetNodeOrNull<Button>(RefreshButtonPath);
                if (_refreshButton != null)
                {
                    _refreshButton.Pressed += OnRefreshButtonPressed;
                }
            }
            
            // Initialize time period buttons
            if (TimePeriodButtonsPath != null)
            {
                var buttonsContainer = GetNodeOrNull<Container>(TimePeriodButtonsPath);
                if (buttonsContainer != null)
                {
                    InitializePeriodButtons(buttonsContainer);
                }
            }
        }
        
        private void InitializePeriodButtons(Container container)
        {
            foreach (Node child in container.GetChildren())
            {
                if (child is Button button)
                {
                    // Parse period from button name
                    string buttonName = button.Name.ToLower();
                    
                    if (buttonName.Contains("daily"))
                    {
                        _periodButtons[TimePeriod.Daily] = button;
                        button.Pressed += () => OnPeriodButtonPressed(TimePeriod.Daily);
                    }
                    else if (buttonName.Contains("weekly"))
                    {
                        _periodButtons[TimePeriod.Weekly] = button;
                        button.Pressed += () => OnPeriodButtonPressed(TimePeriod.Weekly);
                    }
                    else if (buttonName.Contains("monthly"))
                    {
                        _periodButtons[TimePeriod.Monthly] = button;
                        button.Pressed += () => OnPeriodButtonPressed(TimePeriod.Monthly);
                    }
                    else if (buttonName.Contains("alltime") || buttonName.Contains("all"))
                    {
                        _periodButtons[TimePeriod.AllTime] = button;
                        button.Pressed += () => OnPeriodButtonPressed(TimePeriod.AllTime);
                    }
                }
            }
        }
        
        private void ConnectSignals()
        {
            EventBus.On("leaderboard_updated", OnLeaderboardUpdated);
        }
        
        private void DisconnectSignals()
        {
            EventBus.Off("leaderboard_updated", OnLeaderboardUpdated);
            
            if (_refreshButton != null)
            {
                _refreshButton.Pressed -= OnRefreshButtonPressed;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Refresh the leaderboard display
        /// </summary>
        public void RefreshLeaderboard()
        {
            if (LeaderboardManager.Instance == null)
            {
                GD.PrintErr("LeaderboardUI: LeaderboardManager not found");
                return;
            }
            
            // Get entries based on current filter
            List<LeaderboardEntry> entries;
            
            if (_currentPeriod == TimePeriod.AllTime)
            {
                entries = LeaderboardManager.Instance.GetTopScores(MaxDisplayedEntries);
            }
            else
            {
                entries = LeaderboardManager.Instance.GetScoresByTimePeriod(_currentPeriod, MaxDisplayedEntries);
            }
            
            // Display entries
            DisplayEntries(entries);
            
            // Update player rank
            UpdatePlayerRank();
            
            // Update period button states
            UpdatePeriodButtons();
        }
        
        /// <summary>
        /// Show friend leaderboard
        /// </summary>
        public void ShowFriendLeaderboard(List<string> friendNames)
        {
            if (LeaderboardManager.Instance == null)
                return;
            
            var entries = LeaderboardManager.Instance.GetFriendLeaderboard(friendNames, MaxDisplayedEntries);
            DisplayEntries(entries);
        }
        
        #endregion
        
        #region Private Methods - Display
        
        private void DisplayEntries(List<LeaderboardEntry> entries)
        {
            if (_entriesContainer == null)
                return;
            
            // Clear existing entries
            foreach (Node child in _entriesContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            // Create entry UI for each leaderboard entry
            int rank = 1;
            foreach (var entry in entries)
            {
                CreateEntryUI(entry, rank);
                rank++;
            }
            
            // Show message if no entries
            if (entries.Count == 0)
            {
                CreateEmptyMessage();
            }
        }
        
        private void CreateEntryUI(LeaderboardEntry entry, int rank)
        {
            // If we have a prefab, use it
            if (EntryPrefab != null)
            {
                var entryNode = EntryPrefab.Instantiate();
                _entriesContainer.AddChild(entryNode);
                
                // Try to populate the entry (assumes specific node structure)
                PopulateEntryNode(entryNode, entry, rank);
            }
            else
            {
                // Create a simple entry
                var entryControl = CreateSimpleEntry(entry, rank);
                _entriesContainer.AddChild(entryControl);
            }
        }
        
        private void PopulateEntryNode(Node entryNode, LeaderboardEntry entry, int rank)
        {
            // Try to find and populate standard child nodes
            var rankLabel = entryNode.GetNodeOrNull<Label>("RankLabel");
            if (rankLabel != null)
                rankLabel.Text = $"#{rank}";
            
            var nameLabel = entryNode.GetNodeOrNull<Label>("NameLabel");
            if (nameLabel != null)
                nameLabel.Text = entry.PlayerName;
            
            var scoreLabel = entryNode.GetNodeOrNull<Label>("ScoreLabel");
            if (scoreLabel != null)
                scoreLabel.Text = entry.Score.ToString("N0");
            
            var waveLabel = entryNode.GetNodeOrNull<Label>("WaveLabel");
            if (waveLabel != null)
                waveLabel.Text = $"Wave {entry.Wave}";
            
            var killsLabel = entryNode.GetNodeOrNull<Label>("KillsLabel");
            if (killsLabel != null)
                killsLabel.Text = $"{entry.Kills} kills";
            
            var dateLabel = entryNode.GetNodeOrNull<Label>("DateLabel");
            if (dateLabel != null)
                dateLabel.Text = FormatDate(entry.Timestamp);
        }
        
        private Control CreateSimpleEntry(LeaderboardEntry entry, int rank)
        {
            var container = new HBoxContainer();
            container.AddThemeConstantOverride("separation", 10);
            
            // Rank
            var rankLabel = new Label();
            rankLabel.Text = $"#{rank}";
            rankLabel.CustomMinimumSize = new Vector2(50, 0);
            container.AddChild(rankLabel);
            
            // Name
            var nameLabel = new Label();
            nameLabel.Text = entry.PlayerName;
            nameLabel.CustomMinimumSize = new Vector2(150, 0);
            container.AddChild(nameLabel);
            
            // Score
            var scoreLabel = new Label();
            scoreLabel.Text = entry.Score.ToString("N0");
            scoreLabel.CustomMinimumSize = new Vector2(100, 0);
            container.AddChild(scoreLabel);
            
            // Wave
            var waveLabel = new Label();
            waveLabel.Text = $"Wave {entry.Wave}";
            waveLabel.CustomMinimumSize = new Vector2(80, 0);
            container.AddChild(waveLabel);
            
            // Kills
            var killsLabel = new Label();
            killsLabel.Text = $"{entry.Kills} kills";
            killsLabel.CustomMinimumSize = new Vector2(80, 0);
            container.AddChild(killsLabel);
            
            return container;
        }
        
        private void CreateEmptyMessage()
        {
            var label = new Label();
            label.Text = "No leaderboard entries yet. Be the first!";
            label.HorizontalAlignment = HorizontalAlignment.Center;
            _entriesContainer.AddChild(label);
        }
        
        private void UpdatePlayerRank()
        {
            if (_playerRankLabel == null || LeaderboardManager.Instance == null)
                return;
            
            string playerName = GetPlayerName();
            int rank = LeaderboardManager.Instance.GetPlayerRank(playerName);
            
            if (rank > 0)
            {
                var bestScore = LeaderboardManager.Instance.GetPlayerBestScore(playerName);
                if (bestScore != null)
                {
                    _playerRankLabel.Text = $"Your Rank: #{rank} - {bestScore.Score:N0} points";
                }
                else
                {
                    _playerRankLabel.Text = $"Your Rank: #{rank}";
                }
            }
            else
            {
                _playerRankLabel.Text = "Your Rank: Not ranked yet";
            }
        }
        
        private void UpdatePeriodButtons()
        {
            foreach (var kvp in _periodButtons)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.ButtonPressed = (kvp.Key == _currentPeriod);
                }
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnRefreshButtonPressed()
        {
            RefreshLeaderboard();
        }
        
        private void OnPeriodButtonPressed(TimePeriod period)
        {
            _currentPeriod = period;
            RefreshLeaderboard();
        }
        
        private void OnLeaderboardUpdated(object data)
        {
            RefreshLeaderboard();
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
        
        private string FormatDate(DateTime date)
        {
            TimeSpan diff = DateTime.Now - date;
            
            if (diff.TotalDays < 1)
                return "Today";
            else if (diff.TotalDays < 2)
                return "Yesterday";
            else if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} days ago";
            else
                return date.ToString("MMM d");
        }
        
        #endregion
    }
}
