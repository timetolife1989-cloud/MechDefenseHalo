using Godot;
using System;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Drones
{
    /// <summary>
    /// EMP drone that slows and stuns enemies in range.
    /// </summary>
    public partial class EMPDrone : DroneBase
    {
        #region Exported Properties

        [Export] public float EMPRange { get; set; } = 10f;
        [Export] public float SlowAmount { get; set; } = 0.5f; // 50% slow
        [Export] public float PulseInterval { get; set; } = 3f; // Seconds between pulses

        #endregion

        #region Private Fields

        private float _pulseTimer = 0f;

        #endregion

        #region Constructor

        public EMPDrone()
        {
            DroneName = "EMP Drone";
            EnergyCost = 20f;
            Lifetime = 30f;
        }

        #endregion

        #region Protected Methods

        protected override Color GetDroneColor()
        {
            return new Color(1f, 1f, 0.3f); // Yellow for EMP
        }

        protected override void OnUpdate(float delta)
        {
            _pulseTimer += delta;

            if (_pulseTimer >= PulseInterval)
            {
                _pulseTimer = 0f;
                EmitEMPPulse();
            }
        }

        #endregion

        #region Private Methods

        private void EmitEMPPulse()
        {
            // Find all enemies in range
            var enemies = GetTree().GetNodesInGroup("enemies");

            int affectedCount = 0;

            foreach (var enemy in enemies)
            {
                if (enemy is Node3D enemy3D)
                {
                    float distance = GlobalPosition.DistanceTo(enemy3D.GlobalPosition);
                    
                    if (distance <= EMPRange)
                    {
                        // Apply shocked status effect
                        var statusEffect = enemy3D.GetNodeOrNull<StatusEffectComponent>("StatusEffectComponent");
                        if (statusEffect != null)
                        {
                            statusEffect.ApplyShocked(PulseInterval);
                            affectedCount++;
                        }
                    }
                }
            }

            if (affectedCount > 0)
            {
                GD.Print($"EMP Drone affected {affectedCount} enemies");
                SpawnEMPPulseEffect();
            }
        }

        private void SpawnEMPPulseEffect()
        {
            // TODO: Create expanding ring pulse effect
        }

        #endregion
    }
}
