using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Core
{
    /// <summary>
    /// Manages elemental damage system, resistances, and status effects.
    /// </summary>
    public static class ElementalSystem
    {
        #region Damage Multipliers

        /// <summary>
        /// Get damage multiplier based on elemental type and target resistances
        /// </summary>
        /// <param name="elementType">Type of elemental damage</param>
        /// <param name="target">Target node to check for resistances</param>
        /// <returns>Damage multiplier (1.0 = normal, 2.0 = weak, 0.5 = resistant, 0 = immune)</returns>
        public static float GetDamageMultiplier(ElementalType elementType, Node target)
        {
            // Try to find ElementalResistance component
            var resistance = target.GetNodeOrNull<ElementalResistanceComponent>("ElementalResistanceComponent");
            if (resistance == null)
            {
                resistance = target.FindChild("ElementalResistanceComponent") as ElementalResistanceComponent;
            }

            if (resistance != null)
            {
                return resistance.GetResistance(elementType);
            }

            // Default multiplier (normal damage)
            return 1f;
        }

        /// <summary>
        /// Apply status effect based on elemental type
        /// </summary>
        /// <param name="elementType">Type of elemental damage</param>
        /// <param name="target">Target node</param>
        /// <param name="duration">Duration of status effect in seconds</param>
        public static void ApplyStatusEffect(ElementalType elementType, Node target, float duration = 3f)
        {
            var statusEffect = target.GetNodeOrNull<StatusEffectComponent>("StatusEffectComponent");
            if (statusEffect == null)
            {
                statusEffect = target.FindChild("StatusEffectComponent") as StatusEffectComponent;
            }

            if (statusEffect != null)
            {
                switch (elementType)
                {
                    case ElementalType.Fire:
                        statusEffect.ApplyBurning(duration);
                        break;
                    case ElementalType.Ice:
                        statusEffect.ApplyFrozen(duration);
                        break;
                    case ElementalType.Electric:
                        statusEffect.ApplyShocked(duration);
                        break;
                    case ElementalType.Toxic:
                        statusEffect.ApplyPoisoned(duration);
                        break;
                }
            }
        }

        #endregion

        #region Element Information

        /// <summary>
        /// Get color associated with elemental type (for visual effects)
        /// </summary>
        public static Color GetElementColor(ElementalType elementType)
        {
            return elementType switch
            {
                ElementalType.Physical => new Color(0.7f, 0.7f, 0.7f),
                ElementalType.Fire => new Color(1f, 0.3f, 0f),
                ElementalType.Ice => new Color(0.3f, 0.7f, 1f),
                ElementalType.Electric => new Color(0.3f, 0.3f, 1f),
                ElementalType.Toxic => new Color(0.3f, 1f, 0.3f),
                _ => Colors.White
            };
        }

        /// <summary>
        /// Get name of elemental type
        /// </summary>
        public static string GetElementName(ElementalType elementType)
        {
            return elementType.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Component for managing elemental resistances on an entity
    /// </summary>
    public partial class ElementalResistanceComponent : Node
    {
        #region Exported Properties

        [Export] public float PhysicalResistance { get; set; } = 1f; // 1.0 = normal, 0.5 = resistant, 2.0 = weak
        [Export] public float FireResistance { get; set; } = 1f;
        [Export] public float IceResistance { get; set; } = 1f;
        [Export] public float ElectricResistance { get; set; } = 1f;
        [Export] public float ToxicResistance { get; set; } = 1f;

        #endregion

        #region Public Methods

        /// <summary>
        /// Get resistance multiplier for a specific element type
        /// </summary>
        public float GetResistance(ElementalType elementType)
        {
            return elementType switch
            {
                ElementalType.Physical => PhysicalResistance,
                ElementalType.Fire => FireResistance,
                ElementalType.Ice => IceResistance,
                ElementalType.Electric => ElectricResistance,
                ElementalType.Toxic => ToxicResistance,
                _ => 1f
            };
        }

        /// <summary>
        /// Set resistance for a specific element type
        /// </summary>
        public void SetResistance(ElementalType elementType, float value)
        {
            switch (elementType)
            {
                case ElementalType.Physical:
                    PhysicalResistance = value;
                    break;
                case ElementalType.Fire:
                    FireResistance = value;
                    break;
                case ElementalType.Ice:
                    IceResistance = value;
                    break;
                case ElementalType.Electric:
                    ElectricResistance = value;
                    break;
                case ElementalType.Toxic:
                    ToxicResistance = value;
                    break;
            }
        }

        #endregion
    }

    /// <summary>
    /// Component for managing status effects on an entity
    /// </summary>
    public partial class StatusEffectComponent : Node
    {
        #region Private Fields

        private float _burningTimer = 0f;
        private float _frozenTimer = 0f;
        private float _shockedTimer = 0f;
        private float _poisonedTimer = 0f;

        private const float BurningDamagePerSecond = 5f;
        private const float PoisonDamagePerSecond = 3f;

        #endregion

        #region Public Properties

        public bool IsBurning => _burningTimer > 0;
        public bool IsFrozen => _frozenTimer > 0;
        public bool IsShocked => _shockedTimer > 0;
        public bool IsPoisoned => _poisonedTimer > 0;

        #endregion

        #region Godot Lifecycle

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // Update timers
            if (_burningTimer > 0)
            {
                _burningTimer -= dt;
                ApplyBurningDamage(dt);
            }

            if (_frozenTimer > 0)
            {
                _frozenTimer -= dt;
            }

            if (_shockedTimer > 0)
            {
                _shockedTimer -= dt;
            }

            if (_poisonedTimer > 0)
            {
                _poisonedTimer -= dt;
                ApplyPoisonDamage(dt);
            }
        }

        #endregion

        #region Public Methods

        public void ApplyBurning(float duration)
        {
            _burningTimer = Mathf.Max(_burningTimer, duration);
        }

        public void ApplyFrozen(float duration)
        {
            _frozenTimer = Mathf.Max(_frozenTimer, duration);
        }

        public void ApplyShocked(float duration)
        {
            _shockedTimer = Mathf.Max(_shockedTimer, duration);
        }

        public void ApplyPoisoned(float duration)
        {
            _poisonedTimer = Mathf.Max(_poisonedTimer, duration);
        }

        public void ClearAllEffects()
        {
            _burningTimer = 0f;
            _frozenTimer = 0f;
            _shockedTimer = 0f;
            _poisonedTimer = 0f;
        }

        /// <summary>
        /// Get movement speed multiplier based on active effects
        /// </summary>
        public float GetMovementMultiplier()
        {
            float multiplier = 1f;

            if (IsFrozen)
                multiplier *= 0.5f; // 50% slow when frozen

            if (IsShocked)
                multiplier *= 0.7f; // 30% slow when shocked

            return multiplier;
        }

        #endregion

        #region Private Methods

        private void ApplyBurningDamage(float delta)
        {
            var health = GetParent().GetNodeOrNull<HealthComponent>("HealthComponent");
            if (health != null)
            {
                health.TakeDamage(BurningDamagePerSecond * delta);
            }
        }

        private void ApplyPoisonDamage(float delta)
        {
            var health = GetParent().GetNodeOrNull<HealthComponent>("HealthComponent");
            if (health != null)
            {
                health.TakeDamage(PoisonDamagePerSecond * delta);
            }
        }

        #endregion
    }
}
