using Godot;
using System;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Weapons
{
    public partial class Projectile : Area3D
    {
        private Vector3 _velocity;
        private float _damage;
        private float _maxDistance;
        private Vector3 _startPosition;
        
        public void Initialize(Vector3 direction, float speed, float damage, float maxDistance)
        {
            _velocity = direction * speed;
            _damage = damage;
            _maxDistance = maxDistance;
            _startPosition = GlobalPosition;
            
            LookAt(GlobalPosition + direction, Vector3.Up);
        }
        
        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            AreaEntered += OnAreaEntered;
        }
        
        public override void _PhysicsProcess(double delta)
        {
            GlobalPosition += _velocity * (float)delta;
            
            // Check max distance
            if (GlobalPosition.DistanceTo(_startPosition) > _maxDistance)
            {
                QueueFree();
            }
        }
        
        private void OnBodyEntered(Node3D body)
        {
            HitTarget(body);
        }
        
        private void OnAreaEntered(Area3D area)
        {
            HitTarget(area);
        }
        
        private void HitTarget(Node target)
        {
            var healthComp = target.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (healthComp != null)
            {
                healthComp.TakeDamage(_damage, this);
            }
            
            // TODO: Spawn impact effect
            QueueFree();
        }
    }
}
