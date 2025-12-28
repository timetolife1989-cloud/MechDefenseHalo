using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Enemies.Bosses
{
    /// <summary>
    /// Abstract base class for boss enemies.
    /// Provides phase system, weak points, and special abilities.
    /// </summary>
    public abstract partial class BossBase : EnemyBase
    {
        #region Exported Properties

        [Export] public int PhaseCount { get; set; } = 3;
        [Export] public bool HasWeakPoints { get; set; } = true;

        #endregion

        #region Public Properties

        public int CurrentPhase { get; protected set; } = 1;
        public float HealthPercent => _health != null ? _health.HealthPercent : 1f;
        public bool IsEnraged => CurrentPhase >= PhaseCount;

        #endregion

        #region Protected Fields

        protected List<WeakPointComponent> _weakPoints = new List<WeakPointComponent>();
        protected float[] _phaseThresholds; // HP % thresholds for phase changes

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            base._Ready();

            // Calculate phase thresholds
            _phaseThresholds = new float[PhaseCount];
            for (int i = 0; i < PhaseCount; i++)
            {
                _phaseThresholds[i] = 1f - ((float)(i + 1) / PhaseCount);
            }

            // Find weak points
            FindWeakPoints();

            // Emit boss spawned event
            EventBus.Emit(EventBus.BossSpawned, new BossSpawnedData
            {
                Boss = this,
                BossName = EnemyName
            });

            GD.Print($"Boss {EnemyName} spawned!");
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            if (!IsAlive)
                return;

            // Check for phase transitions
            CheckPhaseTransition();
        }

        #endregion

        #region Protected Methods

        protected override void UpdateBehavior(float delta)
        {
            // Call phase-specific behavior
            switch (CurrentPhase)
            {
                case 1:
                    Phase1Behavior(delta);
                    break;
                case 2:
                    Phase2Behavior(delta);
                    break;
                case 3:
                    Phase3Behavior(delta);
                    break;
                default:
                    RageBehavior(delta);
                    break;
            }
        }

        protected override void OnDeath()
        {
            EventBus.Emit(EventBus.BossDefeated, new BossDefeatedData
            {
                Boss = this,
                BossName = EnemyName
            });

            GD.Print($"Boss {EnemyName} defeated!");
            
            // TODO: Play epic death animation, drop loot
        }

        protected virtual void OnPhaseChange(int newPhase)
        {
            GD.Print($"{EnemyName} entered Phase {newPhase}!");
            
            EventBus.Emit(EventBus.BossPhaseChanged, new BossPhaseChangedData
            {
                Boss = this,
                BossName = EnemyName,
                Phase = newPhase
            });

            // TODO: Play phase transition effects
        }

        /// <summary>
        /// Called when a weak point is destroyed
        /// </summary>
        public virtual void OnWeakPointDestroyed(WeakPointComponent weakPoint)
        {
            GD.Print($"{EnemyName} weak point destroyed: {weakPoint.WeakPointName}");
            
            // Optional: Reduce boss HP or change behavior
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Phase 1 behavior (100% - ~66% HP)
        /// </summary>
        protected abstract void Phase1Behavior(float delta);

        /// <summary>
        /// Phase 2 behavior (~66% - ~33% HP)
        /// </summary>
        protected abstract void Phase2Behavior(float delta);

        /// <summary>
        /// Phase 3 behavior (~33% - 0% HP)
        /// </summary>
        protected abstract void Phase3Behavior(float delta);

        /// <summary>
        /// Rage mode behavior (optional, for extra phases)
        /// </summary>
        protected virtual void RageBehavior(float delta)
        {
            Phase3Behavior(delta); // Default to phase 3 behavior
        }

        #endregion

        #region Private Methods

        private void FindWeakPoints()
        {
            foreach (Node child in GetChildren())
            {
                if (child is WeakPointComponent weakPoint)
                {
                    _weakPoints.Add(weakPoint);
                }
            }

            GD.Print($"Found {_weakPoints.Count} weak points on {EnemyName}");
        }

        private void CheckPhaseTransition()
        {
            if (_health == null)
                return;

            float healthPercent = _health.HealthPercent;

            // Check if we should transition to next phase
            for (int i = CurrentPhase; i < PhaseCount; i++)
            {
                if (healthPercent <= _phaseThresholds[i] && CurrentPhase == i)
                {
                    CurrentPhase = i + 1;
                    OnPhaseChange(CurrentPhase);
                    break;
                }
            }
        }

        #endregion
    }

    #region Event Data Structures

    public class BossSpawnedData
    {
        public BossBase Boss { get; set; }
        public string BossName { get; set; }
    }

    public class BossDefeatedData
    {
        public BossBase Boss { get; set; }
        public string BossName { get; set; }
    }

    public class BossPhaseChangedData
    {
        public BossBase Boss { get; set; }
        public string BossName { get; set; }
        public int Phase { get; set; }
    }

    #endregion
}
