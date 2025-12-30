using Godot;
using System;

namespace MechDefenseHalo.VFX
{
    /// <summary>
    /// Controls impact effect behavior for bullet, plasma, and energy hits.
    /// Spawns appropriate particle effects based on impact type.
    /// </summary>
    public partial class ImpactEffect : GpuParticles3D
    {
        public enum ImpactType
        {
            Bullet,
            Plasma,
            Energy,
            Explosion
        }
        
        [Export]
        public ImpactType Type { get; set; } = ImpactType.Bullet;
        
        [Export]
        public float EffectDuration { get; set; } = 0.4f;
        
        [Export]
        public float EffectScale { get; set; } = 1.0f;
        
        public override void _Ready()
        {
            OneShot = true;
            Emitting = false;
            Lifetime = EffectDuration;
            
            // Set colors based on impact type
            SetImpactColor();
        }
        
        /// <summary>
        /// Play the impact effect at the specified position and normal.
        /// </summary>
        /// <param name="position">World position of impact</param>
        /// <param name="normal">Surface normal at impact</param>
        public void PlayAt(Vector3 position, Vector3 normal)
        {
            GlobalPosition = position;
            
            // Orient effect to surface normal
            if (normal != Vector3.Zero)
            {
                LookAt(position + normal, Vector3.Up);
            }
            
            Scale = Vector3.One * EffectScale;
            Restart();
            Emitting = true;
        }
        
        /// <summary>
        /// Play the impact effect with custom scale.
        /// </summary>
        /// <param name="position">World position of impact</param>
        /// <param name="normal">Surface normal at impact</param>
        /// <param name="scale">Scale multiplier</param>
        public void PlayAtWithScale(Vector3 position, Vector3 normal, float scale)
        {
            EffectScale = scale;
            PlayAt(position, normal);
        }
        
        /// <summary>
        /// Set impact color based on type.
        /// </summary>
        private void SetImpactColor()
        {
            var material = ProcessMaterial as ParticleProcessMaterial;
            if (material != null)
            {
                material.Color = Type switch
                {
                    ImpactType.Bullet => new Color(0.8f, 0.8f, 0.8f),
                    ImpactType.Plasma => new Color(0.2f, 0.5f, 1.0f),
                    ImpactType.Energy => new Color(0.3f, 0.7f, 1.0f),
                    ImpactType.Explosion => new Color(1.0f, 0.5f, 0.1f),
                    _ => new Color(1.0f, 1.0f, 1.0f)
                };
            }
        }
    }
}
