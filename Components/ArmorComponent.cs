using Godot;
using System;

namespace MechDefenseHalo.Components
{
    public partial class ArmorComponent : Node
    {
        [Export] public float BaseArmor { get; set; } = 10f;
        [Export] public string ArmorType { get; set; } = "Light"; // Light, Heavy, Shield
        
        private float _bonusArmor = 0f;
        
        public float GetArmor() => BaseArmor + _bonusArmor;
        public string GetArmorType() => ArmorType;
        
        public void AddArmorBonus(float amount)
        {
            _bonusArmor += amount;
        }
        
        public void RemoveArmorBonus(float amount)
        {
            _bonusArmor = Mathf.Max(0, _bonusArmor - amount);
        }
    }
}
