using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Statistics
{
    /// <summary>
    /// UI display for statistics
    /// </summary>
    public partial class StatisticsUI : Control
    {
        #region Node References

        private TabContainer _tabContainer;
        
        // Combat tab labels
        private Label _totalKillsLabel;
        private Label _bossesDefeatedLabel;
        private Label _accuracyLabel;
        private Label _damageDealtLabel;
        private Label _highestWaveLabel;
        private Label _deathCountLabel;
        private Label _killStreakLabel;
        
        // Economy tab labels
        private Label _creditsEarnedLabel;
        private Label _creditsSpentLabel;
        private Label _itemsLootedLabel;
        private Label _legendariesLabel;
        private Label _itemsCraftedLabel;
        private Label _chestsOpenedLabel;
        
        // Session tab labels
        private Label _totalPlaytimeLabel;
        private Label _sessionsLabel;
        private Label _loginStreakLabel;
        private Label _firstPlayedLabel;
        private Label _currentSessionLabel;
        
        // Records tab labels
        private Label _fastestBossLabel;
        private Label _longestStreakLabel;
        private Label _dronesDeployedLabel;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            InitializeNodes();
            RefreshDisplay();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh all statistics displays
        /// </summary>
        public void RefreshDisplay()
        {
            if (StatisticsManager.Instance == null)
            {
                GD.PrintErr("StatisticsManager not available");
                return;
            }

            UpdateCombatTab();
            UpdateEconomyTab();
            UpdateSessionTab();
            UpdateRecordsTab();
        }

        /// <summary>
        /// Export statistics button handler
        /// </summary>
        public void OnExportButtonPressed()
        {
            if (StatisticsManager.Instance != null)
            {
                bool success = StatisticsManager.Instance.ExportStatistics("json");
                if (success)
                {
                    GD.Print("Statistics exported successfully");
                    // Could show a popup here
                }
            }
        }

        #endregion

        #region Private Methods - Initialization

        private void InitializeNodes()
        {
            _tabContainer = GetNodeOrNull<TabContainer>("TabContainer");
            
            if (_tabContainer == null)
            {
                GD.PrintErr("TabContainer not found in StatisticsUI");
                return;
            }

            // Get or create tabs
            var combatTab = GetOrCreateTab("Combat");
            var economyTab = GetOrCreateTab("Economy");
            var sessionTab = GetOrCreateTab("Session");
            var recordsTab = GetOrCreateTab("Records");

            // Initialize labels
            InitializeCombatLabels(combatTab);
            InitializeEconomyLabels(economyTab);
            InitializeSessionLabels(sessionTab);
            InitializeRecordsLabels(recordsTab);
        }

        private Control GetOrCreateTab(string tabName)
        {
            var tab = _tabContainer.GetNodeOrNull<Control>(tabName);
            if (tab == null)
            {
                tab = new VBoxContainer();
                tab.Name = tabName;
                _tabContainer.AddChild(tab);
            }
            return tab;
        }

        private void InitializeCombatLabels(Control parent)
        {
            _totalKillsLabel = GetOrCreateLabel(parent, "TotalKills");
            _bossesDefeatedLabel = GetOrCreateLabel(parent, "BossesDefeated");
            _accuracyLabel = GetOrCreateLabel(parent, "Accuracy");
            _damageDealtLabel = GetOrCreateLabel(parent, "DamageDealt");
            _highestWaveLabel = GetOrCreateLabel(parent, "HighestWave");
            _deathCountLabel = GetOrCreateLabel(parent, "DeathCount");
            _killStreakLabel = GetOrCreateLabel(parent, "KillStreak");
        }

        private void InitializeEconomyLabels(Control parent)
        {
            _creditsEarnedLabel = GetOrCreateLabel(parent, "CreditsEarned");
            _creditsSpentLabel = GetOrCreateLabel(parent, "CreditsSpent");
            _itemsLootedLabel = GetOrCreateLabel(parent, "ItemsLooted");
            _legendariesLabel = GetOrCreateLabel(parent, "Legendaries");
            _itemsCraftedLabel = GetOrCreateLabel(parent, "ItemsCrafted");
            _chestsOpenedLabel = GetOrCreateLabel(parent, "ChestsOpened");
        }

        private void InitializeSessionLabels(Control parent)
        {
            _totalPlaytimeLabel = GetOrCreateLabel(parent, "TotalPlaytime");
            _sessionsLabel = GetOrCreateLabel(parent, "Sessions");
            _loginStreakLabel = GetOrCreateLabel(parent, "LoginStreak");
            _firstPlayedLabel = GetOrCreateLabel(parent, "FirstPlayed");
            _currentSessionLabel = GetOrCreateLabel(parent, "CurrentSession");
        }

        private void InitializeRecordsLabels(Control parent)
        {
            _fastestBossLabel = GetOrCreateLabel(parent, "FastestBoss");
            _longestStreakLabel = GetOrCreateLabel(parent, "LongestStreak");
            _dronesDeployedLabel = GetOrCreateLabel(parent, "DronesDeployed");
        }

        private Label GetOrCreateLabel(Control parent, string labelName)
        {
            var label = parent.GetNodeOrNull<Label>(labelName);
            if (label == null)
            {
                label = new Label();
                label.Name = labelName;
                parent.AddChild(label);
            }
            return label;
        }

        #endregion

        #region Private Methods - Update Display

        private void UpdateCombatTab()
        {
            var combat = StatisticsManager.Instance.Combat;

            if (_totalKillsLabel != null)
                _totalKillsLabel.Text = $"Total Kills: {FormatNumber(combat.TotalKills)}";
            
            if (_bossesDefeatedLabel != null)
                _bossesDefeatedLabel.Text = $"Bosses Defeated: {combat.BossesDefeated}";
            
            if (_accuracyLabel != null)
                _accuracyLabel.Text = $"Accuracy: {combat.AccuracyPercentage:F1}%";
            
            if (_damageDealtLabel != null)
                _damageDealtLabel.Text = $"Damage Dealt: {FormatNumber(combat.TotalDamageDealt)}";
            
            if (_highestWaveLabel != null)
                _highestWaveLabel.Text = $"Highest Wave: {combat.HighestWaveReached}";
            
            if (_deathCountLabel != null)
                _deathCountLabel.Text = $"Deaths: {combat.DeathCount}";
            
            if (_killStreakLabel != null)
                _killStreakLabel.Text = $"Longest Kill Streak: {combat.LongestKillStreak}";
        }

        private void UpdateEconomyTab()
        {
            var economy = StatisticsManager.Instance.Economy;

            if (_creditsEarnedLabel != null)
                _creditsEarnedLabel.Text = $"Credits Earned: {FormatNumber(economy.TotalCreditsEarned)}";
            
            if (_creditsSpentLabel != null)
                _creditsSpentLabel.Text = $"Credits Spent: {FormatNumber(economy.TotalCreditsSpent)}";
            
            if (_itemsLootedLabel != null)
                _itemsLootedLabel.Text = $"Items Looted: {FormatNumber(economy.ItemsLooted)}";
            
            if (_legendariesLabel != null)
                _legendariesLabel.Text = $"Legendaries: {economy.LegendariesObtained}";
            
            if (_itemsCraftedLabel != null)
                _itemsCraftedLabel.Text = $"Items Crafted: {economy.ItemsCrafted}";
            
            if (_chestsOpenedLabel != null)
                _chestsOpenedLabel.Text = $"Chests Opened: {economy.ChestsOpened}";
        }

        private void UpdateSessionTab()
        {
            var session = StatisticsManager.Instance.Session;

            if (_totalPlaytimeLabel != null)
                _totalPlaytimeLabel.Text = $"Total Playtime: {FormatPlaytime(session.TotalPlaytimeSeconds)}";
            
            if (_sessionsLabel != null)
                _sessionsLabel.Text = $"Sessions: {session.TotalSessions}";
            
            if (_loginStreakLabel != null)
                _loginStreakLabel.Text = $"Login Streak: {session.DailyLoginStreak} days";
            
            if (_firstPlayedLabel != null)
            {
                string firstPlayed = session.FirstPlayedDate == DateTime.MinValue 
                    ? "Never" 
                    : session.FirstPlayedDate.ToString("yyyy-MM-dd");
                _firstPlayedLabel.Text = $"First Played: {firstPlayed}";
            }
            
            if (_currentSessionLabel != null)
                _currentSessionLabel.Text = $"Current Session: {FormatPlaytime(session.CurrentSessionTime)}";
        }

        private void UpdateRecordsTab()
        {
            var combat = StatisticsManager.Instance.Combat;

            if (_fastestBossLabel != null)
            {
                string fastestBoss = combat.FastestBossKill == float.MaxValue 
                    ? "N/A" 
                    : FormatTime(combat.FastestBossKill);
                _fastestBossLabel.Text = $"Fastest Boss Kill: {fastestBoss}";
            }
            
            if (_longestStreakLabel != null)
                _longestStreakLabel.Text = $"Longest Streak: {combat.LongestKillStreak}";
            
            if (_dronesDeployedLabel != null)
                _dronesDeployedLabel.Text = $"Drones Deployed: {combat.DronesDeployed}";
        }

        #endregion

        #region Utility Methods

        private string FormatNumber(long number)
        {
            if (number >= 1000000)
                return $"{number / 1000000.0:F1}M";
            else if (number >= 1000)
                return $"{number / 1000.0:F1}K";
            else
                return number.ToString();
        }

        private string FormatPlaytime(float seconds)
        {
            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);
            
            if (hours > 0)
                return $"{hours}h {minutes}m";
            else
                return $"{minutes}m";
        }

        private string FormatTime(float seconds)
        {
            int minutes = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{minutes}:{secs:D2}";
        }

        #endregion
    }
}
