using Godot;
using System.Collections.Generic;
using MechDefenseHalo.VFX;

namespace MechDefenseHalo.Abilities
{
    /// <summary>
    /// EMP ability - creates an electromagnetic pulse that stuns enemies and disables electronics.
    /// Area of effect ability with visual lightning effects.
    /// </summary>
    public class EMPAbility : AbilityBase
    {
        private const float EMP_RADIUS = 10.0f;
        private const float EMP_STUN_DURATION = 3.0f;
        private const float EMP_DAMAGE = 50f;
        
        public EMPAbility()
        {
            AbilityId = "emp";
            AbilityName = "EMP Blast";
            Description = "Release an electromagnetic pulse that stuns and damages nearby enemies";
            Cooldown = 15.0f;
            EnergyCost = 30f;
            IconPath = "res://Assets/UI/Icons/ability_emp.png";
        }
        
        public override void Execute(Node3D user)
        {
            Vector3 position = user.GlobalPosition;
            
            // Create EMP visual effect
            CreateEMPEffect(user);
            
            // Find and affect enemies in radius
            var affectedEnemies = FindEnemiesInRadius(user, position, EMP_RADIUS);
            
            foreach (var enemy in affectedEnemies)
            {
                ApplyEMPEffect(enemy);
            }
            
            GD.Print($"[EMP] Executed! Affected {affectedEnemies.Count} enemies in {EMP_RADIUS}m radius");
        }
        
        private void CreateEMPEffect(Node3D user)
        {
            Vector3 position = user.GlobalPosition;
            
            // Play EMP sound effect
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayEffect("emp_blast", position, Vector3.Zero);
            }
            
            // Create expanding blue energy ring
            var meshInstance = new MeshInstance3D();
            var cylinderMesh = new CylinderMesh
            {
                TopRadius = 0.1f,
                BottomRadius = 0.1f,
                Height = 0.5f
            };
            meshInstance.Mesh = cylinderMesh;
            meshInstance.GlobalPosition = position;
            
            // EMP material - blue/white electric
            var material = new StandardMaterial3D
            {
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                AlbedoColor = new Color(0.5f, 0.8f, 1.0f, 0.8f),
                Emission = new Color(0.7f, 0.9f, 1.0f),
                EmissionEnabled = true,
                EmissionEnergy = 5.0f
            };
            meshInstance.MaterialOverride = material;
            
            user.GetParent().AddChild(meshInstance);
            
            // Animate expanding ring
            var tween = user.GetTree().CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(cylinderMesh, "top_radius", EMP_RADIUS, 0.5f)
                .SetTrans(Tween.TransitionType.Expo)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(cylinderMesh, "bottom_radius", EMP_RADIUS, 0.5f)
                .SetTrans(Tween.TransitionType.Expo)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(material, "albedo_color:a", 0.0f, 0.5f);
            
            // Create lightning particles
            var particles = new GpuParticles3D
            {
                Emitting = true,
                OneShot = true,
                Amount = 50,
                Lifetime = 0.5f,
                Explosiveness = 1.0f,
                GlobalPosition = position
            };
            
            user.GetParent().AddChild(particles);
            
            // Clean up after animation
            tween.Finished += () => 
            {
                meshInstance.QueueFree();
                particles.QueueFree();
            };
        }
        
        private List<Node3D> FindEnemiesInRadius(Node3D user, Vector3 center, float radius)
        {
            var enemies = new List<Node3D>();
            var space = user.GetWorld3D().DirectSpaceState;
            
            // Use a sphere query to find all bodies in range
            var query = PhysicsShapeQueryParameters3D.Create(new SphereShape3D { Radius = radius });
            query.Transform = new Transform3D(Basis.Identity, center);
            query.CollideWithAreas = false;
            query.CollideWithBodies = true;
            
            var results = space.IntersectShape(query, 32);
            
            foreach (var result in results)
            {
                if (result.TryGetValue("collider", out var collider))
                {
                    if (collider is Node3D node && node != user)
                    {
                        // Check if it's an enemy (primarily use group membership)
                        if (node.IsInGroup("enemies"))
                        {
                            enemies.Add(node);
                        }
                    }
                }
            }
            
            return enemies;
        }
        
        private void ApplyEMPEffect(Node3D enemy)
        {
            // Apply damage
            if (enemy.HasMethod("TakeDamage"))
            {
                enemy.Call("TakeDamage", EMP_DAMAGE);
            }
            
            // Apply stun effect
            if (enemy.HasMethod("ApplyStun"))
            {
                enemy.Call("ApplyStun", EMP_STUN_DURATION);
            }
            else
            {
                // Fallback: disable the enemy temporarily
                enemy.SetProcess(false);
                enemy.SetPhysicsProcess(false);
                
                var timer = enemy.GetTree().CreateTimer(EMP_STUN_DURATION);
                timer.Timeout += () =>
                {
                    if (GodotObject.IsInstanceValid(enemy))
                    {
                        enemy.SetProcess(true);
                        enemy.SetPhysicsProcess(true);
                    }
                };
            }
            
            // Create lightning effect on enemy
            CreateLightningEffect(enemy);
        }
        
        private void CreateLightningEffect(Node3D target)
        {
            // Create small electric particles on the stunned enemy
            var particles = new GpuParticles3D
            {
                Emitting = true,
                Amount = 10,
                Lifetime = EMP_STUN_DURATION,
                GlobalPosition = target.GlobalPosition + Vector3.Up * 1.0f
            };
            
            target.GetParent().AddChild(particles);
            
            var timer = target.GetTree().CreateTimer(EMP_STUN_DURATION + 0.5f);
            timer.Timeout += () => particles.QueueFree();
        }
    }
}
