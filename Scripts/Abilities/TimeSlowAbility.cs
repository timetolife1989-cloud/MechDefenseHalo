using Godot;
using MechDefenseHalo.VFX;

namespace MechDefenseHalo.Abilities
{
    /// <summary>
    /// Time Slow ability - creates a field that slows down time for enemies.
    /// Creates a visual distortion effect and affects all enemies in range.
    /// </summary>
    public class TimeSlowAbility : AbilityBase
    {
        private const float SLOW_RADIUS = 15.0f;
        private const float SLOW_DURATION = 8.0f;
        private const float TIME_SCALE = 0.3f; // 30% normal speed
        
        public TimeSlowAbility()
        {
            AbilityId = "time_slow";
            AbilityName = "Temporal Field";
            Description = "Create a field that slows time for enemies, giving you a tactical advantage";
            Cooldown = 20.0f;
            EnergyCost = 40f;
            IconPath = "res://Assets/UI/Icons/ability_time.png";
        }
        
        public override void Execute(Node3D user)
        {
            Vector3 position = user.GlobalPosition;
            
            // Create time slow visual effect
            CreateTimeSlowEffect(user);
            
            // Create a time slow field component
            var timeField = new TimeSlowField
            {
                Name = "TimeSlowField",
                Position = position,
                Radius = SLOW_RADIUS,
                Duration = SLOW_DURATION,
                TimeScale = TIME_SCALE
            };
            
            user.GetParent().AddChild(timeField);
            
            GD.Print($"[Time Slow] Activated! Radius: {SLOW_RADIUS}m, Duration: {SLOW_DURATION}s");
        }
        
        private void CreateTimeSlowEffect(Node3D user)
        {
            Vector3 position = user.GlobalPosition;
            
            // Play time distortion effect
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayEffect("time_distortion", position, Vector3.Zero);
            }
            
            // Create purple/pink time distortion sphere
            var meshInstance = new MeshInstance3D();
            var sphereMesh = new SphereMesh
            {
                Radius = SLOW_RADIUS,
                Height = SLOW_RADIUS * 2
            };
            meshInstance.Mesh = sphereMesh;
            meshInstance.GlobalPosition = position;
            
            // Time distortion material - purple with transparency
            var material = new StandardMaterial3D
            {
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                AlbedoColor = new Color(0.8f, 0.3f, 0.9f, 0.15f),
                Emission = new Color(0.9f, 0.4f, 1.0f),
                EmissionEnabled = true,
                EmissionEnergy = 1.5f,
                Rim = 1.0f,
                RimTint = 0.8f
            };
            meshInstance.MaterialOverride = material;
            
            user.GetParent().AddChild(meshInstance);
            
            // Pulse animation
            var tween = user.GetTree().CreateTween();
            tween.SetLoops();
            tween.TweenProperty(meshInstance, "scale", Vector3.One * 1.1f, 1.0f)
                .SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(meshInstance, "scale", Vector3.One * 0.9f, 1.0f)
                .SetTrans(Tween.TransitionType.Sine);
            
            // Create swirling particles
            var particles = new GpuParticles3D
            {
                Emitting = true,
                Amount = 100,
                Lifetime = SLOW_DURATION,
                GlobalPosition = position
            };
            
            user.GetParent().AddChild(particles);
            
            // Remove effect after duration
            var timer = user.GetTree().CreateTimer(SLOW_DURATION);
            timer.Timeout += () =>
            {
                tween.Kill();
                meshInstance.QueueFree();
                particles.QueueFree();
            };
        }
    }
    
    /// <summary>
    /// Component that manages the time slow field effect.
    /// Continuously applies slow effect to enemies in range.
    /// </summary>
    public partial class TimeSlowField : Node3D
    {
        public float Radius { get; set; } = 15.0f;
        public float Duration { get; set; } = 8.0f;
        public float TimeScale { get; set; } = 0.3f;
        
        private float _elapsed = 0f;
        private const float UPDATE_INTERVAL = 0.1f; // Check every 0.1 seconds
        private float _timeSinceLastUpdate = 0f;
        
        public override void _Ready()
        {
            // Set up automatic cleanup
            var timer = GetTree().CreateTimer(Duration);
            timer.Timeout += () => QueueFree();
        }
        
        public override void _Process(double delta)
        {
            _elapsed += (float)delta;
            _timeSinceLastUpdate += (float)delta;
            
            if (_elapsed >= Duration)
            {
                QueueFree();
                return;
            }
            
            // Only update affected enemies periodically for performance
            if (_timeSinceLastUpdate >= UPDATE_INTERVAL)
            {
                _timeSinceLastUpdate = 0f;
                UpdateAffectedEnemies();
            }
        }
        
        private void UpdateAffectedEnemies()
        {
            var space = GetWorld3D().DirectSpaceState;
            
            // Query for enemies in radius
            var query = PhysicsShapeQueryParameters3D.Create(new SphereShape3D { Radius = Radius });
            query.Transform = new Transform3D(Basis.Identity, GlobalPosition);
            query.CollideWithAreas = false;
            query.CollideWithBodies = true;
            
            var results = space.IntersectShape(query, 64);
            
            foreach (var result in results)
            {
                if (result.TryGetValue("collider", out var collider))
                {
                    if (collider is Node3D node)
                    {
                        // Check if it's an enemy
                        if (node.IsInGroup("enemies") || node.GetType().Name.Contains("Enemy"))
                        {
                            ApplySlowEffect(node);
                        }
                    }
                }
            }
        }
        
        private void ApplySlowEffect(Node3D enemy)
        {
            // Apply time scale to enemy if it has the method
            if (enemy.HasMethod("SetTimeScale"))
            {
                enemy.Call("SetTimeScale", TimeScale);
            }
            else if (enemy.HasMethod("SetSpeedMultiplier"))
            {
                enemy.Call("SetSpeedMultiplier", TimeScale);
            }
            else
            {
                // Fallback: modify the process delta manually
                // Note: This is a simplified approach
                // In a real implementation, enemies should track their own time scale
                enemy.Set("time_scale", TimeScale);
            }
            
            // Add visual indicator if not already present
            if (enemy.GetNodeOrNull("TimeSlowIndicator") == null)
            {
                CreateSlowIndicator(enemy);
            }
        }
        
        private void CreateSlowIndicator(Node3D enemy)
        {
            // Create a small purple glow on slowed enemies
            var light = new OmniLight3D
            {
                Name = "TimeSlowIndicator",
                LightColor = new Color(0.8f, 0.3f, 0.9f),
                LightEnergy = 0.5f,
                OmniRange = 2.0f
            };
            
            enemy.AddChild(light);
            
            // Remove indicator when field expires
            var timer = GetTree().CreateTimer(Duration - _elapsed + 0.5f);
            timer.Timeout += () =>
            {
                if (GodotObject.IsInstanceValid(light))
                {
                    light.QueueFree();
                }
                
                // Reset enemy speed
                if (GodotObject.IsInstanceValid(enemy))
                {
                    if (enemy.HasMethod("SetTimeScale"))
                    {
                        enemy.Call("SetTimeScale", 1.0f);
                    }
                    else if (enemy.HasMethod("SetSpeedMultiplier"))
                    {
                        enemy.Call("SetSpeedMultiplier", 1.0f);
                    }
                }
            };
        }
    }
}
