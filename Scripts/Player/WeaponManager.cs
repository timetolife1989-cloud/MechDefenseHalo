using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Weapons;

namespace MechDefenseHalo.Player
{
    /// <summary>
    /// Manages player weapons - switching, firing, reloading.
    /// Handles both keyboard and mobile touch controls.
    /// </summary>
    public partial class WeaponManager : Node3D
    {
        #region Exported Properties

        [Export] public int MaxWeapons { get; set; } = 4;
        [Export] public NodePath WeaponMountPath { get; set; }

        #endregion

        #region Public Properties

        public WeaponBase CurrentWeapon => _currentWeaponIndex >= 0 && _currentWeaponIndex < _weapons.Count 
            ? _weapons[_currentWeaponIndex] 
            : null;
        
        public int CurrentWeaponIndex => _currentWeaponIndex;
        public int WeaponCount => _weapons.Count;

        #endregion

        #region Private Fields

        private List<WeaponBase> _weapons = new List<WeaponBase>();
        private int _currentWeaponIndex = -1;
        private Node3D _weaponMount;
        private bool _isFiring = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get weapon mount point
            if (WeaponMountPath != null && !WeaponMountPath.IsEmpty)
            {
                _weaponMount = GetNode<Node3D>(WeaponMountPath);
            }
            else
            {
                _weaponMount = this;
            }

            // Find existing weapons in children
            foreach (Node child in _weaponMount.GetChildren())
            {
                if (child is WeaponBase weapon)
                {
                    AddWeapon(weapon);
                }
            }

            // Select first weapon if available
            if (_weapons.Count > 0)
            {
                SwitchToWeapon(0);
            }
        }

        public override void _Process(double delta)
        {
            HandleInput();
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            // Weapon switching (1-4 keys)
            if (Input.IsActionJustPressed("weapon_1"))
                SwitchToWeapon(0);
            if (Input.IsActionJustPressed("weapon_2"))
                SwitchToWeapon(1);
            if (Input.IsActionJustPressed("weapon_3"))
                SwitchToWeapon(2);
            if (Input.IsActionJustPressed("weapon_4"))
                SwitchToWeapon(3);

            // Reload
            if (Input.IsActionJustPressed("reload"))
            {
                CurrentWeapon?.StartReload();
            }

            // Firing
            if (CurrentWeapon != null)
            {
                if (CurrentWeapon.IsAutomatic)
                {
                    // Hold to fire
                    if (Input.IsActionPressed("fire"))
                    {
                        CurrentWeapon.TryFire();
                    }
                }
                else
                {
                    // Single shot
                    if (Input.IsActionJustPressed("fire"))
                    {
                        CurrentWeapon.TryFire();
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a weapon to the inventory
        /// </summary>
        public bool AddWeapon(WeaponBase weapon)
        {
            if (_weapons.Count >= MaxWeapons)
            {
                GD.PrintErr("Cannot add weapon - inventory full!");
                return false;
            }

            if (!_weapons.Contains(weapon))
            {
                _weapons.Add(weapon);
                weapon.Visible = false; // Hide by default
                
                GD.Print($"Added weapon: {weapon.WeaponName}");

                // If this is the first weapon, equip it
                if (_weapons.Count == 1)
                {
                    SwitchToWeapon(0);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove a weapon from inventory
        /// </summary>
        public bool RemoveWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count)
                return false;

            var weapon = _weapons[index];
            _weapons.RemoveAt(index);

            if (index == _currentWeaponIndex)
            {
                // Switch to next available weapon
                if (_weapons.Count > 0)
                {
                    SwitchToWeapon(Mathf.Min(index, _weapons.Count - 1));
                }
                else
                {
                    _currentWeaponIndex = -1;
                }
            }
            else if (index < _currentWeaponIndex)
            {
                _currentWeaponIndex--;
            }

            weapon.QueueFree();
            return true;
        }

        /// <summary>
        /// Switch to weapon by index
        /// </summary>
        public void SwitchToWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count)
                return;

            if (index == _currentWeaponIndex)
                return;

            // Hide current weapon
            if (CurrentWeapon != null)
            {
                CurrentWeapon.Visible = false;
                CurrentWeapon.CancelReload();
            }

            // Switch to new weapon
            _currentWeaponIndex = index;
            CurrentWeapon.Visible = true;

            GD.Print($"Switched to {CurrentWeapon.WeaponName}");

            EventBus.Emit(EventBus.WeaponSwitched, new WeaponSwitchedData
            {
                WeaponName = CurrentWeapon.WeaponName,
                WeaponIndex = index,
                CurrentAmmo = CurrentWeapon.CurrentAmmo,
                MaxAmmo = CurrentWeapon.MaxAmmo
            });

            EventBus.Emit(EventBus.AmmoChanged, new Components.AmmoChangedData
            {
                CurrentAmmo = CurrentWeapon.CurrentAmmo,
                MaxAmmo = CurrentWeapon.MaxAmmo
            });
        }

        /// <summary>
        /// Switch to next weapon in inventory
        /// </summary>
        public void SwitchToNextWeapon()
        {
            if (_weapons.Count <= 1)
                return;

            int nextIndex = (_currentWeaponIndex + 1) % _weapons.Count;
            SwitchToWeapon(nextIndex);
        }

        /// <summary>
        /// Switch to previous weapon in inventory
        /// </summary>
        public void SwitchToPreviousWeapon()
        {
            if (_weapons.Count <= 1)
                return;

            int prevIndex = (_currentWeaponIndex - 1 + _weapons.Count) % _weapons.Count;
            SwitchToWeapon(prevIndex);
        }

        /// <summary>
        /// Fire current weapon (called from mobile UI)
        /// </summary>
        public void FireCurrentWeapon()
        {
            CurrentWeapon?.TryFire();
        }

        /// <summary>
        /// Reload current weapon (called from mobile UI)
        /// </summary>
        public void ReloadCurrentWeapon()
        {
            CurrentWeapon?.StartReload();
        }

        #endregion
    }

    #region Event Data Structures

    public class WeaponSwitchedData
    {
        public string WeaponName { get; set; }
        public int WeaponIndex { get; set; }
        public int CurrentAmmo { get; set; }
        public int MaxAmmo { get; set; }
    }

    #endregion
}
