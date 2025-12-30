using Godot;
using System;

namespace MechDefenseHalo.VFX
{
    /// <summary>
    /// Controls explosion effect behavior with varying sizes and intensities.
    /// Handles particle emission, light effects, and auto-cleanup.
    /// </summary>
    public partial class ExplosionEffect : GpuParticles3D
    {
        public enum ExplosionSize
        {
            Small,
            Medium,
            Large
        }
        
        [Export]
        public ExplosionSize Size { get; set; } = ExplosionSize.Medium;
        
        [Export]
        public float ExplosionDuration { get; set; } = 1.0f;
        
        [Export]
        public float ExplosionRadius { get; set; } = 1.0f;
        
        [Export]
        public Color ExplosionColor { get; set; } = new Color(1.0f, 0.5f, 0.1f);
        
        [Export]
        public bool CreateLight { get; set; } = true;
        
        private OmniLight3D _explosionLight;
        
        public override void _Ready()
        {
            OneShot = true;
            Emitting = false;
            Lifetime = ExplosionDuration;
            
            // Setup based on size
            SetupExplosionSize();
            
            // Create light if enabled
            if (CreateLight)
            {
                SetupExplosionLight();
            }
        }
        
        /// <summary>
        /// Play the explosion effect at the specified position.
        /// </summary>
        /// <param name="position">World position of explosion</param>
        public void PlayAt(Vector3 position)
        {
            GlobalPosition = position;
            Scale = Vector3.One * ExplosionRadius;
            Restart();
            Emitting = true;
            
            // Flash light
            if (_explosionLight != null)
            {
                _explosionLight.LightEnergy = 2.0f;
                AnimateLight();
            }
        }
        
        /// <summary>
        /// Play the explosion effect with custom radius.
        /// </summary>
        /// <param name="position">World position of explosion</param>
        /// <param name="radius">Explosion radius multiplier</param>
        public void PlayAtWithRadius(Vector3 position, float radius)
        {
            ExplosionRadius = radius;
            PlayAt(position);
        }
        
        /// <summary>
        /// Setup explosion parameters based on size.
        /// </summary>
        private void SetupExplosionSize()
        {
            switch (Size)
            {
                case ExplosionSize.Small:
                    ExplosionDuration = 0.8f;
                    ExplosionRadius = 0.5f;
                    Amount = 24;
                    break;
                case ExplosionSize.Medium:
                    ExplosionDuration = 1.0f;
                    ExplosionRadius = 1.0f;
                    Amount = 32;
                    break;
                case ExplosionSize.Large:
                    ExplosionDuration = 1.5f;
                    ExplosionRadius = 2.0f;
                    Amount = 48;
                    break;
            }
            
            Lifetime = ExplosionDuration;
        }
        
        /// <summary>
        /// Create and setup explosion light effect.
        /// </summary>
        private void SetupExplosionLight()
        {
            _explosionLight = new OmniLight3D
            {
                LightColor = ExplosionColor,
                LightEnergy = 0.0f,
                OmniRange = ExplosionRadius * 5.0f,
                OmniAttenuation = 2.0f
            };
            AddChild(_explosionLight);
        }
        
        /// <summary>
        /// Animate the explosion light to fade out.
        /// </summary>
        private void AnimateLight()
        {
            if (_explosionLight == null) return;
            
            var tween = CreateTween();
            tween.TweenProperty(_explosionLight, "light_energy", 0.0f, ExplosionDuration);
        }
    }
}
