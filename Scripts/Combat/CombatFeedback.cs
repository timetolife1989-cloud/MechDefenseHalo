using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Combat
{
    public partial class CombatFeedback : Node
    {
        [Export] public PackedScene DamageNumberPrefab { get; set; }
        [Export] public PackedScene HitMarkerPrefab { get; set; }
        
        public override void _Ready()
        {
            // Listen to damage events
            EventBus.On(EventBus.EntityDamaged, OnEntityDamagedWrapper);
        }
        
        public override void _ExitTree()
        {
            EventBus.Off(EventBus.EntityDamaged, OnEntityDamagedWrapper);
        }
        
        private void OnEntityDamagedWrapper(object data)
        {
            if (data is EntityDamagedData damagedData)
            {
                OnEntityDamaged(damagedData.Target, damagedData.Damage, damagedData.Position);
            }
        }
        
        private void OnEntityDamaged(Node target, float damage, Vector3 position)
        {
            SpawnDamageNumber(damage, position, false);
            SpawnHitMarker(position);
        }
        
        public void SpawnDamageNumber(float damage, Vector3 worldPosition, bool isCritical)
        {
            if (DamageNumberPrefab == null)
                return;
            
            var damageNumber = DamageNumberPrefab.Instantiate<Label3D>();
            GetTree().Root.AddChild(damageNumber);
            damageNumber.GlobalPosition = worldPosition + Vector3.Up * 0.5f;
            damageNumber.Text = damage.ToString("F0");
            
            if (isCritical)
            {
                damageNumber.Modulate = Colors.Orange;
                damageNumber.Scale *= 1.5f;
            }
            
            // Animate upwards and fade out
            var tween = CreateTween();
            tween.TweenProperty(damageNumber, "global_position", worldPosition + Vector3.Up * 2, 1.0);
            tween.Parallel().TweenProperty(damageNumber, "modulate:a", 0.0, 1.0);
            tween.TweenCallback(Callable.From(() => damageNumber.QueueFree()));
        }
        
        public void SpawnHitMarker(Vector3 worldPosition)
        {
            if (HitMarkerPrefab == null)
                return;
            
            var hitMarker = HitMarkerPrefab.Instantiate<Node2D>();
            GetTree().Root.AddChild(hitMarker);
            
            // Don't show hit marker if position is behind camera or camera not found
            var camera = GetViewport().GetCamera3D();
            if (camera == null || camera.IsPositionBehind(worldPosition))
            {
                hitMarker.QueueFree();
                return;
            }
            
            // Simple fade out
            var tween = CreateTween();
            tween.TweenProperty(hitMarker, "modulate:a", 0.0, 0.5);
            tween.TweenCallback(Callable.From(() => hitMarker.QueueFree()));
        }
    }
    
    /// <summary>
    /// Data structure for entity damaged events
    /// </summary>
    public class EntityDamagedData
    {
        public Node Target { get; set; }
        public float Damage { get; set; }
        public Vector3 Position { get; set; }
        public DamageType DamageType { get; set; }
        public bool IsCritical { get; set; }
    }
}
