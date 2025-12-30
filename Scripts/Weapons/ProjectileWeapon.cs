using Godot;
using System;

namespace MechDefenseHalo.Weapons
{
    public partial class ProjectileWeapon : WeaponBase
    {
        [Export] public PackedScene ProjectilePrefab { get; set; }
        [Export] public float ProjectileSpeed { get; set; } = 50f;
        
        protected override void OnFire()
        {
            if (ProjectilePrefab == null)
            {
                GD.PrintErr($"{WeaponName} has no projectile prefab!");
                return;
            }
            
            Vector3 origin = _muzzlePoint?.GlobalPosition ?? GlobalPosition;
            Vector3 direction = GetAimDirection();
            
            var projectile = ProjectilePrefab.Instantiate<Projectile>();
            GetTree().Root.AddChild(projectile);
            projectile.GlobalPosition = origin;
            projectile.Initialize(direction, ProjectileSpeed, Damage, Range);
        }
    }
}
