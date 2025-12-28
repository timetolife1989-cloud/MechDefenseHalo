using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Components
{
    /// <summary>
    /// Component for dealing damage to entities with HealthComponent.
    /// Supports elemental damage types.
    /// </summary>
    public partial class DamageComponent : Node
    {
        #region Exported Properties

        [Export] public float BaseDamage { get; set; } = 10f;
        [Export] public ElementalType ElementType { get; set; } = ElementalType.Physical;
        [Export] public bool CanCrit { get; set; } = false;
        [Export] public float CritChance { get; set; } = 0.15f; // 15% crit chance
        [Export] public float CritMultiplier { get; set; } = 2.0f; // 2x damage on crit

        #endregion

        #region Public Methods

        /// <summary>
        /// Deal damage to a target node
        /// </summary>
        /// <param name="target">Target node (must have HealthComponent)</param>
        /// <param name="damageMultiplier">Optional damage multiplier</param>
        /// <returns>True if damage was dealt successfully</returns>
        public bool DealDamage(Node target, float damageMultiplier = 1f)
        {
            if (target == null)
                return false;

            // Find HealthComponent on target
            var healthComponent = target.GetNodeOrNull<HealthComponent>("HealthComponent");
            if (healthComponent == null)
            {
                // Try to find it as a child
                healthComponent = target.FindChild("HealthComponent") as HealthComponent;
            }

            if (healthComponent == null || healthComponent.IsDead)
                return false;

            // Calculate final damage
            float damage = BaseDamage * damageMultiplier;

            // Apply elemental multiplier
            damage *= ElementalSystem.GetDamageMultiplier(ElementType, target);

            // Check for critical hit
            bool isCrit = false;
            if (CanCrit && GD.Randf() < CritChance)
            {
                damage *= CritMultiplier;
                isCrit = true;
            }

            // Apply damage
            healthComponent.TakeDamage(damage, GetParent());

            // Emit damage dealt event
            EventBus.Emit(EventBus.DamageDealt, new DamageDealtData
            {
                Source = GetParent(),
                Target = target,
                Damage = damage,
                ElementType = ElementType,
                IsCritical = isCrit
            });

            return true;
        }

        /// <summary>
        /// Deal damage in an area of effect
        /// </summary>
        /// <param name="position">Center position of AOE</param>
        /// <param name="radius">Radius of AOE</param>
        /// <param name="damageMultiplier">Optional damage multiplier</param>
        /// <param name="excludeNode">Optional node to exclude from damage</param>
        public void DealAOEDamage(Vector3 position, float radius, float damageMultiplier = 1f, Node excludeNode = null)
        {
            var spaceState = GetParent<Node3D>().GetWorld3D().DirectSpaceState;
            
            // Create a sphere shape for overlap query
            var query = PhysicsShapeQueryParameters3D.Create(new SphereShape3D { Radius = radius });
            query.Transform = new Transform3D(Basis.Identity, position);
            query.CollideWithAreas = true;
            query.CollideWithBodies = true;

            var results = spaceState.IntersectShape(query);

            foreach (var result in results)
            {
                if (result.ContainsKey("collider"))
                {
                    var collider = result["collider"].As<Node3D>();
                    if (collider != null && collider != excludeNode)
                    {
                        // Calculate distance falloff
                        float distance = position.DistanceTo(collider.GlobalPosition);
                        float falloff = 1f - (distance / radius);
                        
                        DealDamage(collider, damageMultiplier * falloff);
                    }
                }
            }
        }

        #endregion
    }

    #region Enums

    /// <summary>
    /// Elemental damage types
    /// </summary>
    public enum ElementalType
    {
        Physical,
        Fire,
        Ice,
        Electric,
        Toxic
    }

    #endregion

    #region Event Data Structures

    /// <summary>
    /// Data structure for damage dealt events
    /// </summary>
    public class DamageDealtData
    {
        public Node Source { get; set; }
        public Node Target { get; set; }
        public float Damage { get; set; }
        public ElementalType ElementType { get; set; }
        public bool IsCritical { get; set; }
    }

    #endregion
}
