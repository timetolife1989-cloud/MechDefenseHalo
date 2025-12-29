using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Hangar
{
    /// <summary>
    /// Simple weapon database for hangar viewer
    /// </summary>
    public static class WeaponDatabase
    {
        private static Dictionary<string, WeaponData> weapons = new Dictionary<string, WeaponData>();
        
        static WeaponDatabase()
        {
            InitializeWeapons();
        }
        
        private static void InitializeWeapons()
        {
            // Add sample weapons
            weapons["assault_rifle"] = new WeaponData
            {
                Id = "assault_rifle",
                Name = "Assault Rifle",
                Description = "Standard military assault rifle",
                Damage = 25,
                FireRate = 10f,
                Accuracy = 0.85f,
                Range = 100f,
                ModelPath = "res://Models/Weapons/AssaultRifle.tscn"
            };
            
            weapons["plasma_cannon"] = new WeaponData
            {
                Id = "plasma_cannon",
                Name = "Plasma Cannon",
                Description = "High-damage plasma weapon",
                Damage = 75,
                FireRate = 2f,
                Accuracy = 0.7f,
                Range = 80f,
                ModelPath = "res://Models/Weapons/PlasmaCannon.tscn"
            };
            
            weapons["railgun"] = new WeaponData
            {
                Id = "railgun",
                Name = "Railgun",
                Description = "Long-range precision weapon",
                Damage = 150,
                FireRate = 1f,
                Accuracy = 0.95f,
                Range = 200f,
                ModelPath = "res://Models/Weapons/Railgun.tscn"
            };
        }
        
        public static WeaponData GetWeapon(string weaponId)
        {
            if (weapons.ContainsKey(weaponId))
            {
                return weapons[weaponId];
            }
            
            // Return default weapon if not found
            return new WeaponData
            {
                Id = weaponId,
                Name = "Unknown Weapon",
                Description = "No data available",
                Damage = 0,
                FireRate = 0f,
                Accuracy = 0f,
                Range = 0f,
                ModelPath = ""
            };
        }
    }
}
