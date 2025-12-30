using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Managers
{
    /// <summary>
    /// Centralized WeaponManager singleton to handle weapon firing, switching, ammo tracking, and weapon state management for the player mech.
    /// </summary>
    public partial class WeaponManager : Node
    {
        #region Singleton

        public static WeaponManager Instance { get; private set; }

        #endregion

        #region Exported Properties

        [Export] public int MaxWeaponSlots { get; set; } = 4;
        [Export] public int MaxAmmo { get; set; } = 300;
        [Export] public int CurrentAmmo { get; set; } = 300;
        [Export] public float ReloadTime { get; set; } = 2.0f;

        #endregion

        #region Private Fields

        private List<WeaponData> equippedWeapons = new List<WeaponData>();
        private int currentWeaponIndex = 0;
        private bool isReloading = false;

        #endregion

        #region Signals

        [Signal] public delegate void AmmoChangedEventHandler(int current, int max);
        [Signal] public delegate void WeaponSwitchedEventHandler(int slotIndex);
        [Signal] public delegate void ReloadStartedEventHandler();
        [Signal] public delegate void ReloadCompleteEventHandler();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            Instance = this;

            // Initialize with a default weapon for testing/demo purposes
            InitializeDefaultWeapon();

            GD.Print("WeaponManager initialized");
        }

        public override void _Input(InputEvent @event)
        {
            // Mouse left click / fire button
            if (Input.IsActionPressed("fire") && !isReloading)
            {
                FireCurrentWeapon();
            }

            // Weapon switching (1-4 keys)
            if (Input.IsActionJustPressed("weapon_1")) SwitchWeapon(0);
            if (Input.IsActionJustPressed("weapon_2")) SwitchWeapon(1);
            if (Input.IsActionJustPressed("weapon_3")) SwitchWeapon(2);
            if (Input.IsActionJustPressed("weapon_4")) SwitchWeapon(3);

            // Reload (R key)
            if (Input.IsActionJustPressed("reload")) StartReload();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a weapon to the equipped weapons list
        /// </summary>
        public void AddWeapon(WeaponData weapon)
        {
            if (equippedWeapons.Count >= MaxWeaponSlots)
            {
                GD.PrintErr("Cannot add weapon: all slots are full!");
                return;
            }

            equippedWeapons.Add(weapon);
            GD.Print($"Weapon added: {weapon.Name} (Slot {equippedWeapons.Count - 1})");
        }

        /// <summary>
        /// Get the currently equipped weapon
        /// </summary>
        public WeaponData GetCurrentWeapon()
        {
            if (equippedWeapons.Count == 0 || currentWeaponIndex >= equippedWeapons.Count)
                return null;

            return equippedWeapons[currentWeaponIndex];
        }

        #endregion

        #region Private Methods - Weapon Firing

        private void FireCurrentWeapon()
        {
            if (CurrentAmmo <= 0)
            {
                GD.Print("Out of ammo!");
                return;
            }

            var weapon = GetCurrentWeapon();
            if (weapon == null) return;

            // Raycast from camera for hit detection
            var camera = GetViewport().GetCamera3D();
            if (camera == null)
            {
                GD.PrintErr("No camera found for weapon firing!");
                return;
            }

            var from = camera.GlobalPosition;
            var to = from + camera.GlobalTransform.Basis.Z * -1000f;

            var spaceState = GetWorld3D().DirectSpaceState;
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                var hitPosition = (Vector3)result["position"];
                var hitObject = (Node)result["collider"];

                GD.Print($"Hit: {hitObject.Name} at {hitPosition}");

                // Apply damage if enemy
                if (hitObject.HasMethod("TakeDamage"))
                {
                    hitObject.Call("TakeDamage", weapon.Damage);
                }
            }

            CurrentAmmo--;
            EmitSignal(SignalName.AmmoChanged, CurrentAmmo, MaxAmmo);
        }

        #endregion

        #region Private Methods - Weapon Switching

        private void SwitchWeapon(int slotIndex)
        {
            if (slotIndex >= equippedWeapons.Count) return;

            currentWeaponIndex = slotIndex;
            EmitSignal(SignalName.WeaponSwitched, currentWeaponIndex);
            GD.Print($"Switched to weapon slot {slotIndex}");
        }

        #endregion

        #region Private Methods - Initialization

        private void InitializeDefaultWeapon()
        {
            // Add a default assault rifle for testing/demo purposes
            var defaultWeapon = new WeaponData
            {
                Name = "Assault Rifle",
                Damage = 10f,
                FireRate = 0.1f,
                MagazineSize = 30
            };

            AddWeapon(defaultWeapon);
        }

        #endregion

        #region Private Methods - Reload System

        private async void StartReload()
        {
            if (isReloading || CurrentAmmo == MaxAmmo) return;

            isReloading = true;
            EmitSignal(SignalName.ReloadStarted);

            await ToSignal(GetTree().CreateTimer(ReloadTime), "timeout");

            CurrentAmmo = MaxAmmo;
            isReloading = false;
            EmitSignal(SignalName.ReloadComplete);
            EmitSignal(SignalName.AmmoChanged, CurrentAmmo, MaxAmmo);
        }

        #endregion

        #region Inner Classes

        /// <summary>
        /// WeaponData class containing weapon properties
        /// </summary>
        public class WeaponData
        {
            public string Name { get; set; }
            public float Damage { get; set; } = 10f;
            public float FireRate { get; set; } = 0.1f; // seconds between shots
            public int MagazineSize { get; set; } = 30;
        }

        #endregion
    }
}
