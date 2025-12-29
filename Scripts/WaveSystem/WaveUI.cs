using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.WaveSystem
{
    /// <summary>
    /// UI controller for wave system display
    /// Shows wave number, enemies remaining, progress, and break timer
    /// </summary>
    public partial class WaveUI : Control
    {
        #region Exports

        [Export] public NodePath WaveCounterPath { get; set; }
        [Export] public NodePath EnemiesRemainingPath { get; set; }
        [Export] public NodePath WaveProgressBarPath { get; set; }
        [Export] public NodePath BreakTimerPath { get; set; }
        [Export] public NodePath AnimationPlayerPath { get; set; }

        #endregion

        #region Private Fields

        private Label _waveCounterLabel;
        private Label _enemiesRemainingLabel;
        private ProgressBar _waveProgressBar;
        private Label _breakTimerLabel;
        private AnimationPlayer _animationPlayer;

        private int _currentWave = 0;
        private int _totalEnemies = 0;
        private int _enemiesRemaining = 0;
        private float _breakTimeRemaining = 0f;
        private bool _isBreakActive = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get UI nodes
            _waveCounterLabel = GetNodeOrNull<Label>(WaveCounterPath);
            _enemiesRemainingLabel = GetNodeOrNull<Label>(EnemiesRemainingPath);
            _waveProgressBar = GetNodeOrNull<ProgressBar>(WaveProgressBarPath);
            _breakTimerLabel = GetNodeOrNull<Label>(BreakTimerPath);
            _animationPlayer = GetNodeOrNull<AnimationPlayer>(AnimationPlayerPath);

            // Subscribe to wave events
            EventBus.On(EventBus.WaveStarted, OnWaveStarted);
            EventBus.On(EventBus.WaveCompleted, OnWaveCompleted);

            // Initialize UI
            UpdateWaveCounter();
            UpdateEnemiesRemaining();
            UpdateBreakTimer();

            GD.Print("WaveUI initialized");
        }

        public override void _ExitTree()
        {
            // Unsubscribe from events
            EventBus.Off(EventBus.WaveStarted, OnWaveStarted);
            EventBus.Off(EventBus.WaveCompleted, OnWaveCompleted);
        }

        public override void _Process(double delta)
        {
            if (_isBreakActive && _breakTimeRemaining > 0)
            {
                _breakTimeRemaining -= (float)delta;
                UpdateBreakTimer();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle wave started event
        /// </summary>
        private void OnWaveStarted(object data)
        {
            if (data is WaveStartedEventData waveData)
            {
                _currentWave = waveData.WaveNumber;
                _totalEnemies = waveData.TotalEnemies;
                _enemiesRemaining = _totalEnemies;
                _isBreakActive = false;

                UpdateWaveCounter();
                UpdateEnemiesRemaining();
                UpdateWaveProgress();
                HideBreakTimer();

                // Play wave start animation
                PlayWaveStartAnimation(waveData.IsBossWave);

                GD.Print($"WaveUI: Wave {_currentWave} started with {_totalEnemies} enemies");
            }
        }

        /// <summary>
        /// Handle wave completed event
        /// </summary>
        private void OnWaveCompleted(object data)
        {
            if (data is WaveCompletedEventData waveData)
            {
                _isBreakActive = true;
                _breakTimeRemaining = 30f; // Default break time

                UpdateBreakTimer();
                ShowBreakTimer();

                GD.Print($"WaveUI: Wave {waveData.WaveNumber} completed");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update enemies remaining count
        /// </summary>
        public void UpdateEnemiesCount(int remaining)
        {
            _enemiesRemaining = remaining;
            UpdateEnemiesRemaining();
            UpdateWaveProgress();
        }

        /// <summary>
        /// Set break time remaining
        /// </summary>
        public void SetBreakTime(float seconds)
        {
            _breakTimeRemaining = seconds;
            _isBreakActive = true;
            UpdateBreakTimer();
        }

        #endregion

        #region Private Methods - UI Updates

        /// <summary>
        /// Update wave counter label
        /// </summary>
        private void UpdateWaveCounter()
        {
            if (_waveCounterLabel != null)
            {
                _waveCounterLabel.Text = $"WAVE {_currentWave}";
            }
        }

        /// <summary>
        /// Update enemies remaining label
        /// </summary>
        private void UpdateEnemiesRemaining()
        {
            if (_enemiesRemainingLabel != null)
            {
                _enemiesRemainingLabel.Text = $"Enemies: {_enemiesRemaining}/{_totalEnemies}";
            }
        }

        /// <summary>
        /// Update wave progress bar
        /// </summary>
        private void UpdateWaveProgress()
        {
            if (_waveProgressBar != null && _totalEnemies > 0)
            {
                float progress = 1.0f - ((float)_enemiesRemaining / _totalEnemies);
                _waveProgressBar.Value = progress * 100f;
            }
        }

        /// <summary>
        /// Update break timer label
        /// </summary>
        private void UpdateBreakTimer()
        {
            if (_breakTimerLabel != null)
            {
                int seconds = Mathf.CeilToInt(_breakTimeRemaining);
                _breakTimerLabel.Text = $"Next wave in: {seconds}s";
            }
        }

        /// <summary>
        /// Show break timer
        /// </summary>
        private void ShowBreakTimer()
        {
            if (_breakTimerLabel != null)
            {
                _breakTimerLabel.Visible = true;
            }
        }

        /// <summary>
        /// Hide break timer
        /// </summary>
        private void HideBreakTimer()
        {
            if (_breakTimerLabel != null)
            {
                _breakTimerLabel.Visible = false;
            }
        }

        /// <summary>
        /// Play wave start animation
        /// </summary>
        private void PlayWaveStartAnimation(bool isBossWave)
        {
            if (_animationPlayer != null)
            {
                string animationName = isBossWave ? "boss_wave_start" : "wave_start";
                
                // Check if animation exists
                if (_animationPlayer.HasAnimation(animationName))
                {
                    _animationPlayer.Play(animationName);
                }
                else if (_animationPlayer.HasAnimation("wave_start"))
                {
                    _animationPlayer.Play("wave_start");
                }
            }
        }

        #endregion
    }
}
