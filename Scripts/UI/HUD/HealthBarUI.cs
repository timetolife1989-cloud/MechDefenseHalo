using Godot;
using System;

namespace MechDefenseHalo.UI.HUD
{
    /// <summary>
    /// Health bar UI component with shield display
    /// Shows current health, max health, and optional shield
    /// </summary>
    public partial class HealthBarUI : Control
    {
        #region Exported Properties
        
        [Export] public NodePath HealthBarPath { get; set; }
        [Export] public NodePath ShieldBarPath { get; set; }
        [Export] public NodePath HealthLabelPath { get; set; }
        [Export] public bool ShowShield { get; set; } = true;
        [Export] public bool ShowNumericValue { get; set; } = true;
        [Export] public Color HealthColor { get; set; } = Colors.Green;
        [Export] public Color ShieldColor { get; set; } = Colors.Cyan;
        [Export] public Color LowHealthColor { get; set; } = Colors.Red;
        [Export] public float LowHealthThreshold { get; set; } = 0.3f;
        
        #endregion
        
        #region Private Fields
        
        private ProgressBar _healthBar;
        private ProgressBar _shieldBar;
        private Label _healthLabel;
        
        private float _currentHealth = 100f;
        private float _maxHealth = 100f;
        private float _currentShield = 0f;
        private float _maxShield = 0f;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Get UI nodes
            _healthBar = GetNodeOrNull<ProgressBar>(HealthBarPath);
            _shieldBar = GetNodeOrNull<ProgressBar>(ShieldBarPath);
            _healthLabel = GetNodeOrNull<Label>(HealthLabelPath);
            
            // Configure health bar
            if (_healthBar != null)
            {
                _healthBar.MinValue = 0;
                _healthBar.MaxValue = 100;
                _healthBar.Value = 100;
                
                // Set initial color
                var styleBox = _healthBar.Get("theme_override_styles/fill") as StyleBox;
                if (styleBox is StyleBoxFlat flatStyle)
                {
                    flatStyle.BgColor = HealthColor;
                }
            }
            
            // Configure shield bar
            if (_shieldBar != null)
            {
                _shieldBar.MinValue = 0;
                _shieldBar.MaxValue = 100;
                _shieldBar.Value = 0;
                _shieldBar.Visible = ShowShield;
                
                var styleBox = _shieldBar.Get("theme_override_styles/fill") as StyleBox;
                if (styleBox is StyleBoxFlat flatStyle)
                {
                    flatStyle.BgColor = ShieldColor;
                }
            }
            
            // Initialize display
            UpdateDisplay();
            
            GD.Print("HealthBarUI initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Update health values
        /// </summary>
        public void UpdateHealth(float current, float max)
        {
            _currentHealth = Mathf.Max(0, current);
            _maxHealth = Mathf.Max(1, max);
            UpdateDisplay();
        }
        
        /// <summary>
        /// Update shield values
        /// </summary>
        public void UpdateShield(float current, float max)
        {
            _currentShield = Mathf.Max(0, current);
            _maxShield = Mathf.Max(0, max);
            UpdateDisplay();
        }
        
        /// <summary>
        /// Set whether shield is visible
        /// </summary>
        public void SetShieldVisible(bool visible)
        {
            ShowShield = visible;
            if (_shieldBar != null)
            {
                _shieldBar.Visible = visible && _maxShield > 0;
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Update all visual elements
        /// </summary>
        private void UpdateDisplay()
        {
            UpdateHealthBar();
            UpdateShieldBar();
            UpdateHealthLabel();
        }
        
        /// <summary>
        /// Update health bar visual
        /// </summary>
        private void UpdateHealthBar()
        {
            if (_healthBar == null)
                return;
            
            // Calculate health percentage
            float healthPercent = _maxHealth > 0 ? (_currentHealth / _maxHealth) : 0f;
            _healthBar.Value = healthPercent * 100f;
            
            // Update color based on health level
            Color targetColor = healthPercent <= LowHealthThreshold ? LowHealthColor : HealthColor;
            
            var styleBox = _healthBar.Get("theme_override_styles/fill") as StyleBox;
            if (styleBox is StyleBoxFlat flatStyle)
            {
                flatStyle.BgColor = targetColor;
            }
        }
        
        /// <summary>
        /// Update shield bar visual
        /// </summary>
        private void UpdateShieldBar()
        {
            if (_shieldBar == null || !ShowShield)
                return;
            
            // Show/hide based on max shield
            _shieldBar.Visible = _maxShield > 0;
            
            if (_maxShield > 0)
            {
                float shieldPercent = _currentShield / _maxShield;
                _shieldBar.Value = shieldPercent * 100f;
            }
        }
        
        /// <summary>
        /// Update health text label
        /// </summary>
        private void UpdateHealthLabel()
        {
            if (_healthLabel == null || !ShowNumericValue)
                return;
            
            if (ShowShield && _maxShield > 0)
            {
                _healthLabel.Text = $"{_currentShield:F0} / {_currentHealth:F0}";
            }
            else
            {
                _healthLabel.Text = $"{_currentHealth:F0} / {_maxHealth:F0}";
            }
        }
        
        #endregion
    }
}
