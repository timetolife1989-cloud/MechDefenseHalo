using Godot;
using System;

namespace MechDefenseHalo.VFX
{
    /// <summary>
    /// Controls projectile trail effects that follow bullets and missiles.
    /// Emits continuous particles while projectile is in flight.
    /// </summary>
    public partial class TrailEffect : GpuParticles3D
    {
        [Export]
        public float TrailLength { get; set; } = 1.0f;
        
        [Export]
        public float TrailWidth { get; set; } = 0.1f;
        
        [Export]
        public Color TrailColor { get; set; } = new Color(1.0f, 0.9f, 0.7f, 0.5f);
        
        [Export]
        public float ParticleLifetime { get; set; } = 0.3f;
        
        [Export]
        public bool AutoStart { get; set; } = true;
        
        private Node3D _projectile;
        
        public override void _Ready()
        {
            OneShot = false;
            Emitting = AutoStart;
            Lifetime = ParticleLifetime;
            
            // Setup trail material if needed
            SetupTrailMaterial();
        }
        
        public override void _Process(double delta)
        {
            // Follow projectile if attached
            if (_projectile != null && IsInstanceValid(_projectile))
            {
                GlobalPosition = _projectile.GlobalPosition;
            }
        }
        
        /// <summary>
        /// Attach trail to a projectile node.
        /// </summary>
        /// <param name="projectile">The projectile to follow</param>
        public void AttachToProjectile(Node3D projectile)
        {
            _projectile = projectile;
            if (projectile != null)
            {
                GlobalPosition = projectile.GlobalPosition;
                StartTrail();
            }
        }
        
        /// <summary>
        /// Start emitting trail particles.
        /// </summary>
        public void StartTrail()
        {
            Emitting = true;
        }
        
        /// <summary>
        /// Stop emitting trail particles.
        /// </summary>
        public void StopTrail()
        {
            Emitting = false;
        }
        
        /// <summary>
        /// Set trail color dynamically.
        /// </summary>
        /// <param name="color">New trail color</param>
        public void SetTrailColor(Color color)
        {
            TrailColor = color;
            var material = ProcessMaterial as ParticleProcessMaterial;
            if (material != null)
            {
                material.Color = color;
            }
        }
        
        /// <summary>
        /// Setup trail particle material properties.
        /// </summary>
        private void SetupTrailMaterial()
        {
            var material = ProcessMaterial as ParticleProcessMaterial;
            if (material != null)
            {
                material.Color = TrailColor;
                material.ScaleMin = TrailWidth * 0.5f;
                material.ScaleMax = TrailWidth;
            }
        }
    }
}
