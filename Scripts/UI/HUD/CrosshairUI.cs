using Godot;
using System;

namespace MechDefenseHalo.UI.HUD
{
    /// <summary>
    /// Crosshair UI component with hit feedback
    /// Displays dynamic crosshair that responds to hits and weapon state
    /// </summary>
    public partial class CrosshairUI : Control
    {
        #region Exported Properties
        
        [Export] public NodePath CrosshairCenterPath { get; set; }
        [Export] public NodePath HitMarkerPath { get; set; }
        [Export] public Color DefaultColor { get; set; } = Colors.White;
        [Export] public Color HitColor { get; set; } = Colors.Red;
        [Export] public float HitMarkerDuration { get; set; } = 0.2f;
        [Export] public float CrosshairSpread { get; set; } = 10f;
        [Export] public bool DynamicSpread { get; set; } = true;
        
        #endregion
        
        #region Private Fields
        
        private Control _crosshairCenter;
        private Control _hitMarker;
        
        private float _currentSpread = 0f;
        private float _targetSpread = 0f;
        private float _hitMarkerTimer = 0f;
        private bool _isShowingHitMarker = false;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Get UI nodes
            _crosshairCenter = GetNodeOrNull<Control>(CrosshairCenterPath);
            _hitMarker = GetNodeOrNull<Control>(HitMarkerPath);
            
            // Hide hit marker initially
            if (_hitMarker != null)
            {
                _hitMarker.Visible = false;
            }
            
            // Set initial color
            if (_crosshairCenter != null)
            {
                _crosshairCenter.Modulate = DefaultColor;
            }
            
            // Center crosshair
            CenterCrosshair();
            
            GD.Print("CrosshairUI initialized");
        }
        
        public override void _Process(double delta)
        {
            UpdateSpread((float)delta);
            UpdateHitMarker((float)delta);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Show hit marker when damage is dealt
        /// </summary>
        public void ShowHitMarker()
        {
            if (_hitMarker != null)
            {
                _hitMarker.Visible = true;
                _hitMarker.Modulate = HitColor;
                _isShowingHitMarker = true;
                _hitMarkerTimer = HitMarkerDuration;
            }
        }
        
        /// <summary>
        /// Set crosshair spread (for weapon firing feedback)
        /// </summary>
        public void SetSpread(float spread)
        {
            _targetSpread = Mathf.Clamp(spread, 0f, 50f);
        }
        
        /// <summary>
        /// Increase spread (when firing)
        /// </summary>
        public void IncreaseSpread(float amount = 5f)
        {
            _targetSpread = Mathf.Min(_targetSpread + amount, 50f);
        }
        
        /// <summary>
        /// Reset spread to minimum
        /// </summary>
        public void ResetSpread()
        {
            _targetSpread = 0f;
        }
        
        /// <summary>
        /// Set crosshair color
        /// </summary>
        public void SetColor(Color color)
        {
            if (_crosshairCenter != null)
            {
                _crosshairCenter.Modulate = color;
            }
        }
        
        /// <summary>
        /// Set crosshair visibility
        /// </summary>
        public void SetCrosshairVisible(bool visible)
        {
            Visible = visible;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Center crosshair on screen
        /// </summary>
        private void CenterCrosshair()
        {
            var viewportSize = GetViewportRect().Size;
            Position = viewportSize / 2;
            
            if (_crosshairCenter != null)
            {
                _crosshairCenter.Position = -_crosshairCenter.Size / 2;
            }
        }
        
        /// <summary>
        /// Update crosshair spread animation
        /// </summary>
        private void UpdateSpread(float delta)
        {
            if (!DynamicSpread || _crosshairCenter == null)
                return;
            
            // Smoothly interpolate current spread to target
            _currentSpread = Mathf.Lerp(_currentSpread, _targetSpread, delta * 10f);
            
            // Apply spread to crosshair (this could be expanded to move crosshair parts)
            float scale = 1f + (_currentSpread / 100f);
            _crosshairCenter.Scale = new Vector2(scale, scale);
            
            // Gradually reduce spread when not firing
            _targetSpread = Mathf.Max(0f, _targetSpread - delta * 20f);
        }
        
        /// <summary>
        /// Update hit marker display
        /// </summary>
        private void UpdateHitMarker(float delta)
        {
            if (!_isShowingHitMarker)
                return;
            
            _hitMarkerTimer -= delta;
            
            if (_hitMarkerTimer <= 0)
            {
                if (_hitMarker != null)
                {
                    _hitMarker.Visible = false;
                }
                _isShowingHitMarker = false;
            }
            else if (_hitMarker != null)
            {
                // Fade out hit marker
                float alpha = _hitMarkerTimer / HitMarkerDuration;
                _hitMarker.Modulate = new Color(HitColor, alpha);
            }
        }
        
        #endregion
    }
}
