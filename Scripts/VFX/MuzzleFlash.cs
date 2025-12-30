using Godot;
using System;

namespace MechDefenseHalo.VFX
{
    /// <summary>
    /// Controls muzzle flash effect behavior.
    /// Attaches to weapon muzzle point and plays flash on fire.
    /// </summary>
    public partial class MuzzleFlash : GpuParticles3D
    {
        [Export]
        public float FlashDuration { get; set; } = 0.2f;
        
        [Export]
        public float FlashIntensity { get; set; } = 1.0f;
        
        [Export]
        public Color FlashColor { get; set; } = new Color(1.0f, 0.7f, 0.2f);
        
        public override void _Ready()
        {
            OneShot = true;
            Emitting = false;
            Lifetime = FlashDuration;
        }
        
        /// <summary>
        /// Play the muzzle flash effect.
        /// </summary>
        public void Play()
        {
            Restart();
            Emitting = true;
        }
        
        /// <summary>
        /// Play the muzzle flash effect with custom intensity.
        /// </summary>
        /// <param name="intensity">Scale multiplier for the flash</param>
        public void PlayWithIntensity(float intensity)
        {
            Scale = Vector3.One * intensity;
            Play();
        }
        
        /// <summary>
        /// Stop the muzzle flash effect immediately.
        /// </summary>
        public void Stop()
        {
            Emitting = false;
        }
    }
}
