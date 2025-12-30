using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Statistics;

namespace MechDefenseHalo.Leaderboard
{
    /// <summary>
    /// Tracks player score during gameplay
    /// Calculates score based on kills, waves, and performance
    /// </summary>
    public partial class ScoreTracker : Node
    {
        #region Singleton
        
        private static ScoreTracker _instance;
        
        public static ScoreTracker Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("ScoreTracker accessed before initialization!");
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Exports
        
        [Export] public int PointsPerKill { get; set; } = 100;
        [Export] public int PointsPerWave { get; set; } = 500;
        [Export] public int PointsPerBoss { get; set; } = 2000;
        [Export] public float ComboMultiplier { get; set; } = 1.5f;
        [Export] public float ComboDecayTime { get; set; } = 3.0f;
        
        #endregion
        
        #region Public Properties
        
        public int TotalScore { get; private set; } = 0;
        public int CurrentWave { get; private set; } = 0;
        public int TotalKills { get; private set; } = 0;
        public int CurrentCombo { get; private set; } = 0;
        public float ComboTimer { get; private set; } = 0f;
        
        #endregion
        
        #region Private Fields
        
        private float _comboTimeRemaining = 0f;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple ScoreTracker instances detected! Removing duplicate.");
                QueueFree();
                return;
            }
            
            _instance = this;
            
            ConnectEventHandlers();
            ResetScore();
            
            GD.Print("ScoreTracker initialized successfully");
        }
        
        public override void _ExitTree()
        {
            DisconnectEventHandlers();
            
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        public override void _Process(double delta)
        {
            // Handle combo decay
            if (_comboTimeRemaining > 0)
            {
                _comboTimeRemaining -= (float)delta;
                ComboTimer = _comboTimeRemaining;
                
                if (_comboTimeRemaining <= 0)
                {
                    ResetCombo();
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Reset all score tracking
        /// </summary>
        public void ResetScore()
        {
            TotalScore = 0;
            CurrentWave = 0;
            TotalKills = 0;
            ResetCombo();
            
            EventBus.Emit("score_updated", new ScoreUpdateData 
            { 
                Score = TotalScore, 
                Combo = CurrentCombo 
            });
        }
        
        /// <summary>
        /// Add points directly
        /// </summary>
        public void AddPoints(int points)
        {
            if (points <= 0) return;
            
            int actualPoints = CalculateComboPoints(points);
            TotalScore += actualPoints;
            
            EventBus.Emit("score_updated", new ScoreUpdateData 
            { 
                Score = TotalScore, 
                Combo = CurrentCombo,
                PointsGained = actualPoints
            });
            
            GD.Print($"Points added: +{actualPoints} (Total: {TotalScore})");
        }
        
        /// <summary>
        /// Get score summary for leaderboard submission
        /// </summary>
        public ScoreSummary GetScoreSummary()
        {
            return new ScoreSummary
            {
                TotalScore = TotalScore,
                Wave = CurrentWave,
                Kills = TotalKills,
                MaxCombo = GetMaxCombo()
            };
        }
        
        #endregion
        
        #region Private Methods - Event Handlers
        
        private void ConnectEventHandlers()
        {
            EventBus.On(EventBus.EntityDied, OnEntityDied);
            EventBus.On(EventBus.WaveCompleted, OnWaveCompleted);
            EventBus.On(EventBus.BossDefeated, OnBossDefeated);
            EventBus.On(EventBus.GameOver, OnGameOver);
        }
        
        private void DisconnectEventHandlers()
        {
            EventBus.Off(EventBus.EntityDied, OnEntityDied);
            EventBus.Off(EventBus.WaveCompleted, OnWaveCompleted);
            EventBus.Off(EventBus.BossDefeated, OnBossDefeated);
            EventBus.Off(EventBus.GameOver, OnGameOver);
        }
        
        private void OnEntityDied(object data)
        {
            if (data is EntityDiedData diedData)
            {
                // Check if enemy died (not player)
                if (diedData.Entity != null && diedData.Entity.IsInGroup("enemies"))
                {
                    TotalKills++;
                    CurrentCombo++;
                    _comboTimeRemaining = ComboDecayTime;
                    
                    int points = CalculateKillPoints();
                    TotalScore += points;
                    
                    EventBus.Emit("score_updated", new ScoreUpdateData 
                    { 
                        Score = TotalScore, 
                        Combo = CurrentCombo,
                        PointsGained = points
                    });
                }
            }
        }
        
        private void OnWaveCompleted(object data)
        {
            CurrentWave++;
            
            int points = CalculateWavePoints();
            TotalScore += points;
            
            EventBus.Emit("score_updated", new ScoreUpdateData 
            { 
                Score = TotalScore, 
                Combo = CurrentCombo,
                PointsGained = points
            });
            
            GD.Print($"Wave {CurrentWave} completed: +{points} points (Total: {TotalScore})");
        }
        
        private void OnBossDefeated(object data)
        {
            int points = CalculateBossPoints();
            TotalScore += points;
            
            EventBus.Emit("score_updated", new ScoreUpdateData 
            { 
                Score = TotalScore, 
                Combo = CurrentCombo,
                PointsGained = points
            });
            
            GD.Print($"Boss defeated: +{points} points (Total: {TotalScore})");
        }
        
        private void OnGameOver(object data)
        {
            // Submit score to leaderboard on game over
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.SubmitCurrentGameScore();
            }
        }
        
        #endregion
        
        #region Private Methods - Score Calculation
        
        private int CalculateKillPoints()
        {
            return CalculateComboPoints(PointsPerKill);
        }
        
        private int CalculateWavePoints()
        {
            // Wave points increase with wave number
            return PointsPerWave + (CurrentWave * 50);
        }
        
        private int CalculateBossPoints()
        {
            return CalculateComboPoints(PointsPerBoss);
        }
        
        private int CalculateComboPoints(int basePoints)
        {
            if (CurrentCombo <= 1)
                return basePoints;
            
            // Apply combo multiplier
            float multiplier = 1f + ((CurrentCombo - 1) * 0.1f);
            multiplier = Mathf.Min(multiplier, ComboMultiplier);
            
            return (int)(basePoints * multiplier);
        }
        
        private void ResetCombo()
        {
            if (CurrentCombo > 0)
            {
                EventBus.Emit("combo_broken", new ComboData { Combo = CurrentCombo });
            }
            
            CurrentCombo = 0;
            _comboTimeRemaining = 0f;
            ComboTimer = 0f;
        }
        
        private int GetMaxCombo()
        {
            // This would ideally track the max combo throughout the session
            // For now, return current combo
            return CurrentCombo;
        }
        
        #endregion
    }
    
    #region Data Structures
    
    /// <summary>
    /// Score update event data
    /// </summary>
    public class ScoreUpdateData
    {
        public int Score { get; set; }
        public int Combo { get; set; }
        public int PointsGained { get; set; }
    }
    
    /// <summary>
    /// Combo event data
    /// </summary>
    public class ComboData
    {
        public int Combo { get; set; }
    }
    
    /// <summary>
    /// Score summary for leaderboard submission
    /// </summary>
    public class ScoreSummary
    {
        public int TotalScore { get; set; }
        public int Wave { get; set; }
        public int Kills { get; set; }
        public int MaxCombo { get; set; }
    }
    
    #endregion
}
