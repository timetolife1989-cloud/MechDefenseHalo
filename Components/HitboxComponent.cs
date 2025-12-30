using Godot;
using System;

namespace MechDefenseHalo.Components
{
    public partial class HitboxComponent : Area3D
    {
        [Export] public bool IsWeakpoint { get; set; } = false;
        [Export] public float DamageMultiplier { get; set; } = 1.0f;
        [Export] public string HitboxName { get; set; } = "Body";
        
        private HealthComponent _healthComponent;
        
        public override void _Ready()
        {
            _healthComponent = GetParent().GetNodeOrNull<HealthComponent>("HealthComponent");
            
            if (_healthComponent == null)
            {
                GD.PrintErr($"HitboxComponent on {GetParent().Name} has no HealthComponent!");
            }
        }
        
        public void OnHit(float damage, Vector3 hitPosition, Combat.DamageType damageType)
        {
            if (_healthComponent == null)
                return;
            
            float finalDamage = damage * DamageMultiplier;
            bool isCritical = IsWeakpoint;
            
            _healthComponent.TakeDamage(finalDamage, hitPosition, damageType, isCritical);
        }
    }
}
