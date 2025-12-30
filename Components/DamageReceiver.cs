using Godot;
using System;
using MechDefenseHalo.Combat;

namespace MechDefenseHalo.Components
{
    /// <summary>
    /// Component that receives damage and forwards it to HealthComponent.
    /// Provides a cleaner interface for damage dealing systems.
    /// </summary>
    public partial class DamageReceiver : Node
    {
        private HealthComponent _healthComponent;
        
        public override void _Ready()
        {
            _healthComponent = GetParent().GetNodeOrNull<HealthComponent>("HealthComponent");
            
            if (_healthComponent == null)
            {
                GD.PrintErr($"DamageReceiver on {GetParent().Name} has no HealthComponent!");
            }
        }
        
        public void ReceiveDamage(float damage, Vector3 hitPosition, DamageType damageType = DamageType.Kinetic, bool isCritical = false)
        {
            if (_healthComponent != null)
            {
                _healthComponent.TakeDamage(damage, hitPosition, damageType, isCritical);
            }
        }
    }
}
