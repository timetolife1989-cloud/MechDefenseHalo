using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Weapons
{
    /// <summary>
    /// Manages ammunition reserves across multiple weapons.
    /// Tracks ammo pools and provides ammo management.
    /// </summary>
    public partial class AmmoManager : Node
    {
        #region Exported Properties
        
        [Export] public int AssaultRifleAmmo { get; set; } = 300;
        [Export] public int PlasmaCells { get; set; } = 100;
        [Export] public int RailgunSlugs { get; set; } = 50;
        [Export] public int CryoCharges { get; set; } = 80;
        [Export] public int TeslaCharges { get; set; } = 120;
        
        #endregion
        
        private Dictionary<string, int> _ammoReserves = new();
        
        public override void _Ready()
        {
            InitializeAmmoReserves();
        }
        
        private void InitializeAmmoReserves()
        {
            _ammoReserves["AssaultRifle"] = AssaultRifleAmmo;
            _ammoReserves["PlasmaCannon"] = PlasmaCells;
            _ammoReserves["Railgun"] = RailgunSlugs;
            _ammoReserves["CryoBlaster"] = CryoCharges;
            _ammoReserves["TeslaGun"] = TeslaCharges;
        }
        
        public int GetAmmoReserve(string weaponType)
        {
            return _ammoReserves.ContainsKey(weaponType) ? _ammoReserves[weaponType] : 0;
        }
        
        public bool ConsumeAmmo(string weaponType, int amount)
        {
            if (!_ammoReserves.ContainsKey(weaponType))
                return false;
            
            if (_ammoReserves[weaponType] < amount)
                return false;
            
            _ammoReserves[weaponType] -= amount;
            EventBus.Emit(EventBus.AmmoChanged, weaponType);
            return true;
        }
        
        public void AddAmmo(string weaponType, int amount)
        {
            if (!_ammoReserves.ContainsKey(weaponType))
                _ammoReserves[weaponType] = 0;
            
            _ammoReserves[weaponType] += amount;
            EventBus.Emit(EventBus.AmmoChanged, weaponType);
        }
    }
}
