using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.SaveSystem
{
    /// <summary>
    /// Controls automatic saving at intervals and milestones
    /// </summary>
    public partial class AutoSaveController : Node
    {
        #region Exported Properties
        
        [Export] private float _autoSaveInterval = 300f; // 5 minutes in seconds
        [Export] private bool _enableAutoSave = true;
        [Export] private int _waveIntervalForAutoSave = 5; // Auto-save every 5 waves
        
        #endregion
        
        #region Private Fields
        
        private float _timeSinceLastSave = 0f;
        private bool _autoSaveNotificationShown = false;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Subscribe to game events for milestone-based auto-save
            EventBus.On(EventBus.WaveCompleted, OnWaveCompleted);
            EventBus.On(EventBus.BossDefeated, OnBossDefeated);
            EventBus.On("player_level_up", OnPlayerLevelUp);
            
            GD.Print($"AutoSaveController initialized (interval: {_autoSaveInterval}s)");
        }
        
        public override void _ExitTree()
        {
            EventBus.Off(EventBus.WaveCompleted, OnWaveCompleted);
            EventBus.Off(EventBus.BossDefeated, OnBossDefeated);
            EventBus.Off("player_level_up", OnPlayerLevelUp);
        }
        
        public override void _Process(double delta)
        {
            if (!_enableAutoSave)
                return;
            
            _timeSinceLastSave += (float)delta;
            
            if (_timeSinceLastSave >= _autoSaveInterval)
            {
                TriggerAutoSave();
                _timeSinceLastSave = 0f;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Enable or disable auto-save
        /// </summary>
        public void SetAutoSaveEnabled(bool enabled)
        {
            _enableAutoSave = enabled;
            GD.Print($"Auto-save {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Set auto-save interval in seconds
        /// </summary>
        public void SetAutoSaveInterval(float intervalSeconds)
        {
            _autoSaveInterval = Mathf.Max(60f, intervalSeconds); // Minimum 1 minute
            GD.Print($"Auto-save interval set to {_autoSaveInterval}s");
        }
        
        /// <summary>
        /// Manually trigger an auto-save
        /// </summary>
        public void TriggerAutoSave()
        {
            var saveManager = SaveManager.Instance;
            if (saveManager != null)
            {
                saveManager.SaveGame();
                ShowAutoSaveNotification();
            }
            else
            {
                GD.PrintErr("AutoSaveController: SaveManager not found!");
            }
        }
        
        /// <summary>
        /// Reset the auto-save timer
        /// </summary>
        public void ResetTimer()
        {
            _timeSinceLastSave = 0f;
        }
        
        #endregion
        
        #region Private Methods
        
        private void ShowAutoSaveNotification()
        {
            // Emit event for UI to show notification
            EventBus.Emit("auto_save_notification", null);
            GD.Print("Auto-save completed");
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnWaveCompleted(object data)
        {
            if (data is int wave)
            {
                if (wave % _waveIntervalForAutoSave == 0)
                {
                    GD.Print($"Auto-save triggered: Wave {wave} completed");
                    TriggerAutoSave();
                }
            }
        }
        
        private void OnBossDefeated(object data)
        {
            GD.Print("Auto-save triggered: Boss defeated");
            TriggerAutoSave();
        }
        
        private void OnPlayerLevelUp(object data)
        {
            GD.Print("Auto-save triggered: Player leveled up");
            TriggerAutoSave();
        }
        
        #endregion
    }
}
