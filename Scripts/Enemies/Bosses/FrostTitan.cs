using Godot;
using System;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Enemies.Bosses
{
    /// <summary>
    /// First boss: Frost Titan
    /// Ice-based boss with freezing attacks and weak points.
    /// </summary>
    public partial class FrostTitan : BossBase
    {
        #region Exported Properties

        [Export] public float IceTornadoRadius { get; set; } = 10f;
        [Export] public float FreezingAuraRadius { get; set; } = 8f;
        [Export] public float FreezingAuraDamage { get; set; } = 3f; // Per second

        #endregion

        #region Private Fields

        private float _specialAttackCooldown = 0f;
        private const float SpecialAttackInterval = 8f;
        private bool _isPerformingSpecial = false;

        #endregion

        #region Constructor

        public FrostTitan()
        {
            EnemyName = "Frost Titan";
            MaxHealth = 50000f;
            MoveSpeed = 2f;
            AttackDamage = 35f;
            AttackRange = 4f;
            AttackCooldown = 2.5f;
            DetectionRange = 50f;
            PhaseCount = 3;
        }

        #endregion

        #region Protected Methods

        protected override void OnReady()
        {
            base.OnReady();

            // Set elemental resistances
            if (_resistance == null)
            {
                _resistance = new ElementalResistanceComponent();
                _resistance.Name = "ElementalResistanceComponent";
                AddChild(_resistance);
            }

            _resistance.IceResistance = 0f;    // Immune to ice
            _resistance.FireResistance = 2f;    // Weak to fire (2x damage)
            _resistance.PhysicalResistance = 0.8f; // 20% less physical damage
            _resistance.ElectricResistance = 1f;
            _resistance.ToxicResistance = 0.9f;

            // Add status effect component if not present
            if (_statusEffect == null)
            {
                _statusEffect = new StatusEffectComponent();
                _statusEffect.Name = "StatusEffectComponent";
                AddChild(_statusEffect);
            }

            // Weak points are defined in the scene structure and will be
            // automatically detected by BossBase.FindWeakPoints()
        }

        protected override void Phase1Behavior(float delta)
        {
            // Phase 1 (100-50%): Slow heavy attacks, basic melee
            
            if (Target == null || !IsInstanceValid(Target))
                return;

            if (IsInAttackRange())
            {
                if (_movement != null)
                {
                    _movement.Stop();
                }
                TryAttack();
            }
            else
            {
                MoveTowardsTarget(delta);
            }

            // Occasional ice nova
            UpdateSpecialAttacks(delta);
        }

        protected override void Phase2Behavior(float delta)
        {
            // Phase 2 (50-25%): Ice tornado AOE attacks
            
            if (!_isPerformingSpecial)
            {
                Phase1Behavior(delta); // Use basic behavior
            }

            UpdateSpecialAttacks(delta);

            // More frequent special attacks
            if (_specialAttackCooldown <= 0)
            {
                PerformIceTornado();
            }
        }

        protected override void Phase3Behavior(float delta)
        {
            // Phase 3 (25-0%): Rage mode - fast attacks, freezing aura
            
            // Increase movement speed
            if (_movement != null)
            {
                _movement.MaxSpeed = MoveSpeed * 1.5f;
            }

            // Apply freezing aura
            ApplyFreezingAura();

            // Aggressive behavior
            if (Target == null || !IsInstanceValid(Target))
                return;

            if (IsInAttackRange())
            {
                if (_movement != null)
                {
                    _movement.Stop();
                }
                TryAttack();
            }
            else
            {
                MoveTowardsTarget(delta);
            }

            // Continuous special attacks
            UpdateSpecialAttacks(delta);
        }

        protected override void OnAttackPerformed()
        {
            // Apply frost effect on hit
            if (Target != null)
            {
                var targetStatus = Target.GetNodeOrNull<StatusEffectComponent>("StatusEffectComponent");
                if (targetStatus != null)
                {
                    targetStatus.ApplyFrozen(3f);
                }
            }

            GD.Print($"{EnemyName} performed frost attack!");
        }

        public override void OnWeakPointDestroyed(WeakPointComponent weakPoint)
        {
            base.OnWeakPointDestroyed(weakPoint);

            if (weakPoint.WeakPointName.Contains("Knee"))
            {
                // Slow boss when knee destroyed
                if (_movement != null)
                {
                    _movement.MaxSpeed *= 0.7f;
                }
                GD.Print($"{EnemyName} slowed - knee destroyed!");
            }
            else if (weakPoint.WeakPointName.Contains("Core"))
            {
                // Deal massive damage when core destroyed
                if (_health != null)
                {
                    _health.TakeDamage(10000f);
                }
                GD.Print($"{EnemyName} critically damaged - core destroyed!");
            }
        }

        #endregion

        #region Private Methods

        private void UpdateSpecialAttacks(float delta)
        {
            if (_specialAttackCooldown > 0)
            {
                _specialAttackCooldown -= delta;
            }
            else
            {
                _specialAttackCooldown = SpecialAttackInterval / CurrentPhase; // Faster in later phases
                PerformIceNova();
            }
        }

        private void PerformIceNova()
        {
            GD.Print($"{EnemyName} performs Ice Nova!");

            // Deal AOE ice damage
            var enemies = GetTree().GetNodesInGroup("player");
            
            foreach (var enemy in enemies)
            {
                if (enemy is Node3D enemy3D)
                {
                    float distance = GlobalPosition.DistanceTo(enemy3D.GlobalPosition);
                    
                    if (distance <= IceTornadoRadius)
                    {
                        float falloff = 1f - (distance / IceTornadoRadius);
                        float damage = 50f * falloff;

                        var health = enemy3D.GetNodeOrNull<HealthComponent>("HealthComponent");
                        if (health != null)
                        {
                            health.TakeDamage(damage, this);
                        }

                        // Apply frozen status
                        var status = enemy3D.GetNodeOrNull<StatusEffectComponent>("StatusEffectComponent");
                        if (status != null)
                        {
                            status.ApplyFrozen(4f);
                        }
                    }
                }
            }

            // TODO: Spawn ice nova visual effect
        }

        private void PerformIceTornado()
        {
            _isPerformingSpecial = true;
            GD.Print($"{EnemyName} summons Ice Tornado!");

            // TODO: Create tornado visual and damage over time
            
            // Reset after duration
            GetTree().CreateTimer(3f).Timeout += () => _isPerformingSpecial = false;
        }

        private void ApplyFreezingAura()
        {
            // Continuously damage and slow nearby players
            var enemies = GetTree().GetNodesInGroup("player");
            
            foreach (var enemy in enemies)
            {
                if (enemy is Node3D enemy3D)
                {
                    float distance = GlobalPosition.DistanceTo(enemy3D.GlobalPosition);
                    
                    if (distance <= FreezingAuraRadius)
                    {
                        var health = enemy3D.GetNodeOrNull<HealthComponent>("HealthComponent");
                        if (health != null)
                        {
                            health.TakeDamage(FreezingAuraDamage * (float)GetPhysicsProcessDeltaTime(), this);
                        }

                        var status = enemy3D.GetNodeOrNull<StatusEffectComponent>("StatusEffectComponent");
                        if (status != null)
                        {
                            status.ApplyFrozen(0.5f);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
