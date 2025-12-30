using Godot;
using System.Collections.Generic;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Weapons
{
    public partial class WeaponSwitcher : Node
    {
        private List<WeaponBase> _weapons = new();
        private int _currentWeaponIndex = 0;
        private WeaponBase _currentWeapon;
        
        public override void _Ready()
        {
            // Collect all weapon children
            foreach (var child in GetChildren())
            {
                if (child is WeaponBase weapon)
                {
                    _weapons.Add(weapon);
                    weapon.Visible = false;
                }
            }
            
            if (_weapons.Count > 0)
            {
                SwitchToWeapon(0);
            }
        }
        
        public void NextWeapon()
        {
            if (_weapons.Count == 0) return;
            
            int nextIndex = (_currentWeaponIndex + 1) % _weapons.Count;
            SwitchToWeapon(nextIndex);
        }
        
        public void PreviousWeapon()
        {
            if (_weapons.Count == 0) return;
            
            int prevIndex = (_currentWeaponIndex - 1 + _weapons.Count) % _weapons.Count;
            SwitchToWeapon(prevIndex);
        }
        
        public void Fire()
        {
            _currentWeapon?.Fire();
        }
        
        public void Reload()
        {
            _currentWeapon?.StartReload();
        }
        
        private void SwitchToWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count) return;
            
            // Hide current
            if (_currentWeapon != null)
            {
                _currentWeapon.Visible = false;
            }
            
            // Show new
            _currentWeaponIndex = index;
            _currentWeapon = _weapons[_currentWeaponIndex];
            _currentWeapon.Visible = true;
            
            EventBus.Emit(EventBus.WeaponSwitched, _currentWeapon.WeaponName);
            GD.Print($"Switched to: {_currentWeapon.WeaponName}");
        }
    }
}
