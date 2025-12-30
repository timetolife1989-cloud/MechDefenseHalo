using Godot;
using System;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Weapons
{
    public partial class HitscanWeapon : WeaponBase
    {
        protected override void OnFire()
        {
            Vector3 origin = _muzzlePoint?.GlobalPosition ?? GlobalPosition;
            Vector3 direction = GetAimDirection();
            
            // Raycast
            var spaceState = GetWorld3D().DirectSpaceState;
            var query = PhysicsRayQueryParameters3D.Create(origin, origin + direction * Range);
            query.CollideWithAreas = true;
            
            var result = spaceState.IntersectRay(query);
            
            if (result.Count > 0)
            {
                Vector3 hitPoint = (Vector3)result["position"];
                Node hitNode = (Node)result["collider"];
                
                // Apply damage
                var healthComp = hitNode.GetNodeOrNull<HealthComponent>("HealthComponent");
                if (healthComp != null)
                {
                    healthComp.TakeDamage(Damage, this);
                    GD.Print($"{WeaponName} hit {hitNode.Name} for {Damage} damage");
                }
                
                // Spawn impact effect
                SpawnImpactEffect(hitPoint, (Vector3)result["normal"]);
            }
        }
        
        private void SpawnImpactEffect(Vector3 position, Vector3 normal)
        {
            if (ImpactEffect != null)
            {
                var impact = ImpactEffect.Instantiate<Node3D>();
                GetTree().Root.AddChild(impact);
                impact.GlobalPosition = position;
                impact.LookAt(position + normal, Vector3.Up);
            }
        }
    }
}
