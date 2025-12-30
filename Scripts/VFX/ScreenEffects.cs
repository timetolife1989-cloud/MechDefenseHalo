using Godot;
using System;

namespace MechDefenseHalo.VFX
{
    /// <summary>
    /// Controls screen effects like camera shake, chromatic aberration, and vignette.
    /// Provides visual feedback for impacts, explosions, and damage events.
    /// </summary>
    public partial class ScreenEffects : Node
    {
        private static ScreenEffects _instance;
        public static ScreenEffects Instance => _instance;
        
        [Export]
        public Camera3D Camera { get; set; }
        
        [Export]
        public float ShakeDecay { get; set; } = 5.0f;
        
        private float _shakeStrength = 0.0f;
        private Vector3 _originalCameraPosition;
        private RandomNumberGenerator _random = new RandomNumberGenerator();
        
        public override void _Ready()
        {
            if (_instance != null)
            {
                GD.PrintErr("Multiple ScreenEffects instances detected!");
                QueueFree();
                return;
            }
            
            _instance = this;
            _random.Randomize();
            
            // Find camera if not set
            if (Camera == null)
            {
                Camera = GetViewport().GetCamera3D();
            }
            
            if (Camera != null)
            {
                _originalCameraPosition = Camera.Position;
            }
        }
        
        public override void _Process(double delta)
        {
            // Decay shake strength
            if (_shakeStrength > 0)
            {
                _shakeStrength = Mathf.Max(0, _shakeStrength - ShakeDecay * (float)delta);
                
                if (Camera != null)
                {
                    // Apply shake offset
                    Vector3 offset = new Vector3(
                        _random.RandfRange(-_shakeStrength, _shakeStrength),
                        _random.RandfRange(-_shakeStrength, _shakeStrength),
                        _random.RandfRange(-_shakeStrength, _shakeStrength)
                    );
                    Camera.Position = _originalCameraPosition + offset;
                }
            }
            else if (Camera != null)
            {
                // Reset camera position when shake ends
                Camera.Position = _originalCameraPosition;
            }
        }
        
        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        /// <summary>
        /// Trigger screen shake effect.
        /// </summary>
        /// <param name="strength">Strength of the shake (position offset in units)</param>
        /// <param name="duration">Duration of the shake in seconds</param>
        public void Shake(float strength, float duration = 0.5f)
        {
            _shakeStrength = Mathf.Max(_shakeStrength, strength);
            
            // Override decay to match duration
            if (duration > 0)
            {
                ShakeDecay = strength / duration;
            }
        }
        
        /// <summary>
        /// Trigger screen shake based on distance from explosion.
        /// </summary>
        /// <param name="explosionPosition">Position of explosion</param>
        /// <param name="maxRadius">Maximum radius for shake effect</param>
        /// <param name="maxStrength">Maximum shake strength at epicenter</param>
        public void ShakeFromExplosion(Vector3 explosionPosition, float maxRadius, float maxStrength = 1.0f)
        {
            if (Camera == null) return;
            
            float distance = Camera.GlobalPosition.DistanceTo(explosionPosition);
            if (distance < maxRadius)
            {
                // Calculate falloff
                float falloff = 1.0f - (distance / maxRadius);
                float strength = maxStrength * falloff;
                Shake(strength);
            }
        }
        
        /// <summary>
        /// Trigger shake effect for weapon fire.
        /// </summary>
        /// <param name="weaponStrength">Weapon kick strength</param>
        public void ShakeFromWeaponFire(float weaponStrength = 0.05f)
        {
            Shake(weaponStrength, 0.1f);
        }
        
        /// <summary>
        /// Trigger shake effect for taking damage.
        /// </summary>
        /// <param name="damageAmount">Amount of damage taken</param>
        public void ShakeFromDamage(float damageAmount)
        {
            float strength = Mathf.Min(damageAmount * 0.01f, 0.5f);
            Shake(strength, 0.3f);
        }
        
        /// <summary>
        /// Reset all screen effects immediately.
        /// </summary>
        public void Reset()
        {
            _shakeStrength = 0.0f;
            if (Camera != null)
            {
                Camera.Position = _originalCameraPosition;
            }
        }
        
        /// <summary>
        /// Set the camera to apply effects to.
        /// </summary>
        /// <param name="camera">The camera node</param>
        public void SetCamera(Camera3D camera)
        {
            Camera = camera;
            if (camera != null)
            {
                _originalCameraPosition = camera.Position;
            }
        }
    }
}
