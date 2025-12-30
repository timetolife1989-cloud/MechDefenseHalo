using Godot;
using MechDefenseHalo.Core;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.UI.HUD
{
    /// <summary>
    /// Main HUD manager that coordinates all HUD elements
    /// Connects to EventBus for player state updates
    /// </summary>
    public partial class HUDManager : Control
    {
        #region Exported Properties
        
        [Export] public NodePath HealthBarPath { get; set; }
        [Export] public NodePath AmmoCounterPath { get; set; }
        [Export] public NodePath MinimapPath { get; set; }
        [Export] public NodePath ObjectiveTrackerPath { get; set; }
        [Export] public NodePath CrosshairPath { get; set; }
        [Export] public NodePath WaveCounterPath { get; set; }
        [Export] public NodePath ScoreLabelPath { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private HealthBarUI _healthBar;
        private AmmoCounterUI _ammoCounter;
        private MinimapUI _minimap;
        private ObjectiveTrackerUI _objectiveTracker;
        private CrosshairUI _crosshair;
        private Label _waveCounter;
        private Label _scoreLabel;
        
        private int _currentScore = 0;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Get HUD component references
            _healthBar = GetNodeOrNull<HealthBarUI>(HealthBarPath);
            _ammoCounter = GetNodeOrNull<AmmoCounterUI>(AmmoCounterPath);
            _minimap = GetNodeOrNull<MinimapUI>(MinimapPath);
            _objectiveTracker = GetNodeOrNull<ObjectiveTrackerUI>(ObjectiveTrackerPath);
            _crosshair = GetNodeOrNull<CrosshairUI>(CrosshairPath);
            _waveCounter = GetNodeOrNull<Label>(WaveCounterPath);
            _scoreLabel = GetNodeOrNull<Label>(ScoreLabelPath);
            
            // Subscribe to events
            EventBus.On(EventBus.HealthChanged, OnHealthChanged);
            EventBus.On(EventBus.AmmoChanged, OnAmmoChanged);
            EventBus.On(EventBus.WeaponReloaded, OnWeaponReloaded);
            EventBus.On(EventBus.WaveStarted, OnWaveStarted);
            EventBus.On(EventBus.EntityDied, OnEntityDied);
            EventBus.On(EventBus.DamageDealt, OnDamageDealt);
            EventBus.On(EventBus.WeaponSwitched, OnWeaponSwitched);
            
            // Initialize score display
            UpdateScoreDisplay();
            
            GD.Print("HUDManager initialized");
        }
        
        public override void _ExitTree()
        {
            // Unsubscribe from events
            EventBus.Off(EventBus.HealthChanged, OnHealthChanged);
            EventBus.Off(EventBus.AmmoChanged, OnAmmoChanged);
            EventBus.Off(EventBus.WeaponReloaded, OnWeaponReloaded);
            EventBus.Off(EventBus.WaveStarted, OnWaveStarted);
            EventBus.Off(EventBus.EntityDied, OnEntityDied);
            EventBus.Off(EventBus.DamageDealt, OnDamageDealt);
            EventBus.Off(EventBus.WeaponSwitched, OnWeaponSwitched);
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle health changed event
        /// </summary>
        private void OnHealthChanged(object data)
        {
            if (data is HealthChangedData healthData)
            {
                // Check if this is the player (you may need to adjust this check based on your player setup)
                if (healthData.Entity?.Name?.Contains("Player") ?? false)
                {
                    _healthBar?.UpdateHealth(healthData.CurrentHealth, healthData.MaxHealth);
                }
            }
        }
        
        /// <summary>
        /// Handle ammo changed event
        /// </summary>
        private void OnAmmoChanged(object data)
        {
            if (data is AmmoChangedData ammoData)
            {
                _ammoCounter?.UpdateAmmo(ammoData.CurrentAmmo, ammoData.MaxAmmo);
            }
        }
        
        /// <summary>
        /// Handle weapon reloaded event
        /// </summary>
        private void OnWeaponReloaded(object data)
        {
            _ammoCounter?.OnReloadComplete();
        }
        
        /// <summary>
        /// Handle wave started event
        /// </summary>
        private void OnWaveStarted(object data)
        {
            if (_waveCounter != null && data is WaveStartedEventData waveData)
            {
                _waveCounter.Text = $"Wave {waveData.WaveNumber}";
            }
        }
        
        /// <summary>
        /// Handle entity died event for score updates
        /// </summary>
        private void OnEntityDied(object data)
        {
            if (data is EntityDiedData diedData)
            {
                // Check if it's an enemy (not the player)
                if (diedData.Entity?.IsInGroup("enemies") ?? false)
                {
                    AddScore(100); // Base score for enemy kill
                }
            }
        }
        
        /// <summary>
        /// Handle damage dealt event for crosshair feedback
        /// </summary>
        private void OnDamageDealt(object data)
        {
            _crosshair?.ShowHitMarker();
        }
        
        /// <summary>
        /// Handle weapon switched event
        /// </summary>
        private void OnWeaponSwitched(object data)
        {
            if (data is Player.WeaponSwitchedData weaponData)
            {
                _ammoCounter?.UpdateAmmo(weaponData.CurrentAmmo, weaponData.MaxAmmo);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Add score points
        /// </summary>
        public void AddScore(int points)
        {
            _currentScore += points;
            UpdateScoreDisplay();
        }
        
        /// <summary>
        /// Reset score to zero
        /// </summary>
        public void ResetScore()
        {
            _currentScore = 0;
            UpdateScoreDisplay();
        }
        
        /// <summary>
        /// Show or hide the HUD
        /// </summary>
        public void SetHUDVisible(bool visible)
        {
            Visible = visible;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Update score display label
        /// </summary>
        private void UpdateScoreDisplay()
        {
            if (_scoreLabel != null)
            {
                _scoreLabel.Text = $"Score: {_currentScore:N0}";
            }
        }
        
        #endregion
    }
    
    #region Event Data Structures
    
    /// <summary>
    /// Wave started event data
    /// </summary>
    public class WaveStartedEventData
    {
        public int WaveNumber { get; set; }
        public int TotalEnemies { get; set; }
        public bool IsBossWave { get; set; }
    }
    
    /// <summary>
    /// Wave completed event data
    /// </summary>
    public class WaveCompletedEventData
    {
        public int WaveNumber { get; set; }
        public float CompletionTime { get; set; }
        public int EnemiesKilled { get; set; }
    }
    
    #endregion
}
