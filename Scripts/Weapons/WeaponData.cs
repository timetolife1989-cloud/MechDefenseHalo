using Godot;
using System;

namespace MechDefenseHalo.Weapons
{
    /// <summary>
    /// Resource definition for weapon data.
    /// Can be used to create weapon configurations as .tres files.
    /// </summary>
    [GlobalClass]
    public partial class WeaponData : Resource
    {
        [Export] public string WeaponName { get; set; } = "Weapon";
        [Export] public float Damage { get; set; } = 10f;
        [Export] public float FireRate { get; set; } = 0.1f;
        [Export] public int MagazineSize { get; set; } = 30;
        [Export] public float ReloadTime { get; set; } = 2f;
        [Export] public float Range { get; set; } = 100f;
        [Export] public float Accuracy { get; set; } = 0.95f;
        [Export] public bool IsAutomatic { get; set; } = true;
        [Export] public float ProjectileSpeed { get; set; } = 50f;
        
        [Export] public PackedScene MuzzleFlashEffect { get; set; }
        [Export] public PackedScene ImpactEffect { get; set; }
        [Export] public PackedScene ProjectilePrefab { get; set; }
        [Export] public AudioStream FireSound { get; set; }
        [Export] public AudioStream ReloadSound { get; set; }
        
        public WeaponData() { }
    }
}
