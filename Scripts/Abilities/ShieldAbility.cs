using Godot;
using MechDefenseHalo.VFX;

namespace MechDefenseHalo.Abilities
{
    /// <summary>
    /// Shield ability - creates a temporary protective shield that absorbs damage.
    /// Provides visual feedback with a shield bubble effect.
    /// </summary>
    public class ShieldAbility : AbilityBase
    {
        private const float SHIELD_DURATION = 5.0f;
        private const float SHIELD_STRENGTH = 200f;
        
        public ShieldAbility()
        {
            AbilityId = "shield";
            AbilityName = "Energy Shield";
            Description = "Deploy an energy shield that absorbs incoming damage";
            Cooldown = 12.0f;
            EnergyCost = 25f;
            IconPath = "res://Assets/UI/Icons/ability_shield.png";
        }
        
        public override void Execute(Node3D user)
        {
            // Create shield effect
            CreateShieldVisual(user);
            
            // Add shield component if it doesn't exist
            var shieldComponent = user.GetNodeOrNull<ShieldComponent>("ShieldComponent");
            if (shieldComponent == null)
            {
                shieldComponent = new ShieldComponent();
                user.AddChild(shieldComponent);
            }
            
            // Activate the shield
            shieldComponent.ActivateShield(SHIELD_STRENGTH, SHIELD_DURATION);
            
            GD.Print($"[Shield] Activated! Strength: {SHIELD_STRENGTH}, Duration: {SHIELD_DURATION}s");
        }
        
        private void CreateShieldVisual(Node3D user)
        {
            // Create shield bubble effect
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayEffect("shield_activate", user.GlobalPosition, user.GlobalRotation);
            }
            
            // Create a glowing sphere mesh for the shield
            var meshInstance = new MeshInstance3D();
            var sphereMesh = new SphereMesh
            {
                Radius = 1.5f,
                Height = 3.0f
            };
            meshInstance.Mesh = sphereMesh;
            
            // Create shield material with transparency and emission
            var material = new StandardMaterial3D
            {
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                AlbedoColor = new Color(0.2f, 0.5f, 1.0f, 0.3f),
                Emission = new Color(0.3f, 0.7f, 1.0f),
                EmissionEnabled = true,
                EmissionEnergy = 2.0f
            };
            meshInstance.MaterialOverride = material;
            
            user.AddChild(meshInstance);
            
            // Animate the shield
            var tween = user.CreateTween();
            tween.TweenProperty(meshInstance, "scale", Vector3.One * 1.2f, 0.2f)
                .SetTrans(Tween.TransitionType.Bounce);
            tween.TweenProperty(material, "albedo_color:a", 0.0f, SHIELD_DURATION - 0.5f)
                .SetDelay(0.5f);
            
            // Remove shield visual after duration
            var timer = user.GetTree().CreateTimer(SHIELD_DURATION);
            timer.Timeout += () => meshInstance.QueueFree();
        }
    }
    
    /// <summary>
    /// Component that manages shield health and damage absorption
    /// </summary>
    public partial class ShieldComponent : Node
    {
        private float _currentShield = 0f;
        private float _maxShield = 0f;
        private bool _isActive = false;
        private SceneTreeTimer _durationTimer;
        
        public float CurrentShield => _currentShield;
        public float MaxShield => _maxShield;
        public bool IsActive => _isActive;
        
        public void ActivateShield(float strength, float duration)
        {
            _maxShield = strength;
            _currentShield = strength;
            _isActive = true;
            
            // Set expiration timer
            _durationTimer = GetTree().CreateTimer(duration);
            _durationTimer.Timeout += DeactivateShield;
            
            GD.Print($"[ShieldComponent] Shield active: {_currentShield}/{_maxShield}");
        }
        
        public float AbsorbDamage(float damage)
        {
            if (!_isActive || _currentShield <= 0)
                return damage;
            
            float absorbed = Mathf.Min(damage, _currentShield);
            _currentShield -= absorbed;
            float remaining = damage - absorbed;
            
            GD.Print($"[ShieldComponent] Absorbed {absorbed} damage, {remaining} passed through");
            
            if (_currentShield <= 0)
            {
                DeactivateShield();
            }
            
            return remaining;
        }
        
        private void DeactivateShield()
        {
            _isActive = false;
            _currentShield = 0f;
            GD.Print("[ShieldComponent] Shield deactivated");
        }
    }
}
