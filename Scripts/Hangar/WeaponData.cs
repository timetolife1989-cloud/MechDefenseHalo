using Godot;
using System;

namespace MechDefenseHalo.Hangar
{
    /// <summary>
    /// Weapon data for display in hangar viewer
    /// </summary>
    public class WeaponData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Damage { get; set; }
        public float FireRate { get; set; }
        public float Accuracy { get; set; }
        public float Range { get; set; }
        public string ModelPath { get; set; }
    }
}
