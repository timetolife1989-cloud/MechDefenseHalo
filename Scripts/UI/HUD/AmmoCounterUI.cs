using Godot;
using System;

namespace MechDefenseHalo.UI.HUD
{
    /// <summary>
    /// Ammo counter UI component with reload indicator
    /// Displays current ammo, max ammo, and reload progress
    /// </summary>
    public partial class AmmoCounterUI : Control
    {
        #region Exported Properties
        
        [Export] public NodePath AmmoLabelPath { get; set; }
        [Export] public NodePath ReloadProgressPath { get; set; }
        [Export] public NodePath ReloadLabelPath { get; set; }
        [Export] public Color NormalColor { get; set; } = Colors.White;
        [Export] public Color LowAmmoColor { get; set; } = Colors.Orange;
        [Export] public Color EmptyColor { get; set; } = Colors.Red;
        [Export] public float LowAmmoThreshold { get; set; } = 0.25f;
        
        #endregion
        
        #region Private Fields
        
        private const int MinMaxAmmo = 1; // Minimum value for max ammo to prevent division by zero
        
        private Label _ammoLabel;
        private ProgressBar _reloadProgress;
        private Label _reloadLabel;
        
        private int _currentAmmo = 0;
        private int _maxAmmo = 30;
        private bool _isReloading = false;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Get UI nodes
            _ammoLabel = GetNodeOrNull<Label>(AmmoLabelPath);
            _reloadProgress = GetNodeOrNull<ProgressBar>(ReloadProgressPath);
            _reloadLabel = GetNodeOrNull<Label>(ReloadLabelPath);
            
            // Configure reload progress bar
            if (_reloadProgress != null)
            {
                _reloadProgress.MinValue = 0;
                _reloadProgress.MaxValue = 100;
                _reloadProgress.Value = 0;
                _reloadProgress.Visible = false;
            }
            
            // Hide reload label initially
            if (_reloadLabel != null)
            {
                _reloadLabel.Visible = false;
            }
            
            // Initialize display
            UpdateDisplay();
            
            GD.Print("AmmoCounterUI initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Update ammo values
        /// </summary>
        public void UpdateAmmo(int current, int max)
        {
            _currentAmmo = Mathf.Max(0, current);
            _maxAmmo = Mathf.Max(MinMaxAmmo, max);
            _isReloading = false;
            
            UpdateDisplay();
        }
        
        /// <summary>
        /// Show reload progress
        /// </summary>
        public void ShowReloadProgress(float progress)
        {
            _isReloading = true;
            
            if (_reloadProgress != null)
            {
                _reloadProgress.Visible = true;
                _reloadProgress.Value = Mathf.Clamp(progress * 100f, 0f, 100f);
            }
            
            if (_reloadLabel != null)
            {
                _reloadLabel.Visible = true;
                _reloadLabel.Text = "RELOADING...";
            }
        }
        
        /// <summary>
        /// Called when reload is complete
        /// </summary>
        public void OnReloadComplete()
        {
            _isReloading = false;
            
            if (_reloadProgress != null)
            {
                _reloadProgress.Visible = false;
            }
            
            if (_reloadLabel != null)
            {
                _reloadLabel.Visible = false;
            }
        }
        
        /// <summary>
        /// Start reloading animation
        /// </summary>
        public void StartReload()
        {
            _isReloading = true;
            ShowReloadProgress(0f);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Update all visual elements
        /// </summary>
        private void UpdateDisplay()
        {
            UpdateAmmoLabel();
            UpdateReloadDisplay();
        }
        
        /// <summary>
        /// Update ammo label text and color
        /// </summary>
        private void UpdateAmmoLabel()
        {
            if (_ammoLabel == null)
                return;
            
            // Update text
            _ammoLabel.Text = $"{_currentAmmo} / {_maxAmmo}";
            
            // Update color based on ammo level
            float ammoPercent = _maxAmmo > 0 ? ((float)_currentAmmo / _maxAmmo) : 0f;
            
            if (_currentAmmo == 0)
            {
                _ammoLabel.AddThemeColorOverride("font_color", EmptyColor);
            }
            else if (ammoPercent <= LowAmmoThreshold)
            {
                _ammoLabel.AddThemeColorOverride("font_color", LowAmmoColor);
            }
            else
            {
                _ammoLabel.AddThemeColorOverride("font_color", NormalColor);
            }
        }
        
        /// <summary>
        /// Update reload display
        /// </summary>
        private void UpdateReloadDisplay()
        {
            if (!_isReloading)
            {
                if (_reloadProgress != null)
                {
                    _reloadProgress.Visible = false;
                }
                
                if (_reloadLabel != null)
                {
                    _reloadLabel.Visible = false;
                }
            }
        }
        
        #endregion
    }
}
