using Godot;
using MechDefenseHalo.VFX;

namespace MechDefenseHalo.Abilities
{
    /// <summary>
    /// Dash ability - provides a quick burst of movement in the current direction.
    /// Creates a trail effect and temporarily increases movement speed.
    /// </summary>
    public class DashAbility : AbilityBase
    {
        private const float DASH_SPEED = 20.0f;
        private const float DASH_DURATION = 0.3f;
        
        public DashAbility()
        {
            AbilityId = "dash";
            AbilityName = "Tactical Dash";
            Description = "Rapidly dash in the movement direction with a burst of speed";
            Cooldown = 5.0f;
            EnergyCost = 15f;
            IconPath = "res://Assets/UI/Icons/ability_dash.png";
        }
        
        public override void Execute(Node3D user)
        {
            // Apply dash velocity
            if (user is CharacterBody3D character)
            {
                Vector3 dashDirection = GetDashDirection(character);
                
                if (dashDirection != Vector3.Zero)
                {
                    Vector3 velocity = character.Velocity;
                    velocity.X = dashDirection.X * DASH_SPEED;
                    velocity.Z = dashDirection.Z * DASH_SPEED;
                    character.Velocity = velocity;
                }
                
                // Create dash VFX trail
                CreateDashEffect(character);
                
                GD.Print($"[Dash] Executed! Direction: {dashDirection}");
            }
        }
        
        private Vector3 GetDashDirection(CharacterBody3D character)
        {
            // Get the character's forward direction based on rotation
            Vector3 forward = -character.Transform.Basis.Z;
            
            // If the character has velocity, dash in that direction
            Vector3 velocity = character.Velocity;
            if (velocity.Length() > 0.1f)
            {
                return new Vector3(velocity.X, 0, velocity.Z).Normalized();
            }
            
            // Otherwise dash forward
            return new Vector3(forward.X, 0, forward.Z).Normalized();
        }
        
        private void CreateDashEffect(Node3D user)
        {
            // Create a trail effect at the user's position
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayEffect("dash_trail", user.GlobalPosition, user.GlobalRotation);
            }
            
            // Create a blue energy burst
            var particles = new GpuParticles3D
            {
                Emitting = true,
                OneShot = true,
                Amount = 20,
                Lifetime = 0.5f,
                GlobalPosition = user.GlobalPosition
            };
            
            user.GetParent().AddChild(particles);
            
            // Auto-delete after emission
            var timer = user.GetTree().CreateTimer(1.0);
            timer.Timeout += () => particles.QueueFree();
        }
        
        public override bool CanUse(Node3D user)
        {
            // Can't dash if not on the floor
            if (user is CharacterBody3D character)
            {
                return character.IsOnFloor();
            }
            return true;
        }
    }
}
