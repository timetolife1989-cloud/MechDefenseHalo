using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Drones;

namespace MechDefenseHalo.Player
{
    /// <summary>
    /// Manages player drones - deployment, energy, and active drone tracking.
    /// </summary>
    public partial class DroneManager : Node3D
    {
        #region Exported Properties

        [Export] public int MaxActiveDrones { get; set; } = 5;
        [Export] public float MaxEnergy { get; set; } = 100f;
        [Export] public float EnergyRegenRate { get; set; } = 5f; // Per second
        [Export] public NodePath PlayerPath { get; set; }

        #endregion

        #region Public Properties

        public float CurrentEnergy { get; private set; }
        public int ActiveDroneCount => _activeDrones.Count;
        public float EnergyPercent => CurrentEnergy / MaxEnergy;

        #endregion

        #region Private Fields

        private List<DroneBase> _activeDrones = new List<DroneBase>();
        private List<PackedScene> _droneLoadout = new List<PackedScene>();
        private Node3D _player;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            CurrentEnergy = MaxEnergy;

            // Find player
            if (PlayerPath != null && !PlayerPath.IsEmpty)
            {
                _player = GetNode<Node3D>(PlayerPath);
            }
            else
            {
                _player = GetParent<Node3D>();
            }

            // TODO: Load drone loadout from player data
            InitializeDefaultLoadout();
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // Regenerate energy
            if (CurrentEnergy < MaxEnergy)
            {
                CurrentEnergy = Mathf.Min(MaxEnergy, CurrentEnergy + EnergyRegenRate * dt);
                
                EventBus.Emit(EventBus.EnergyChanged, new EnergyChangedData
                {
                    CurrentEnergy = CurrentEnergy,
                    MaxEnergy = MaxEnergy
                });
            }

            // Clean up destroyed drones
            _activeDrones.RemoveAll(drone => !IsInstanceValid(drone) || !drone.IsActive);

            // Handle input for drone deployment
            HandleDroneInput();
        }

        #endregion

        #region Input Handling

        private void HandleDroneInput()
        {
            if (Input.IsActionJustPressed("deploy_drone_1"))
                TryDeployDrone(0);
            if (Input.IsActionJustPressed("deploy_drone_2"))
                TryDeployDrone(1);
            if (Input.IsActionJustPressed("deploy_drone_3"))
                TryDeployDrone(2);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Try to deploy a drone from loadout
        /// </summary>
        public bool TryDeployDrone(int loadoutIndex)
        {
            if (loadoutIndex < 0 || loadoutIndex >= _droneLoadout.Count)
            {
                GD.PrintErr($"Invalid drone loadout index: {loadoutIndex}");
                return false;
            }

            // Check if we can deploy more drones
            if (_activeDrones.Count >= MaxActiveDrones)
            {
                GD.Print("Maximum active drones reached!");
                return false;
            }

            // Instantiate drone
            var droneScene = _droneLoadout[loadoutIndex];
            if (droneScene == null)
            {
                GD.PrintErr($"Drone scene at index {loadoutIndex} is null!");
                return false;
            }

            var drone = droneScene.Instantiate<DroneBase>();
            if (drone == null)
            {
                GD.PrintErr("Failed to instantiate drone!");
                return false;
            }

            // Check energy cost
            if (CurrentEnergy < drone.EnergyCost)
            {
                GD.Print("Not enough energy to deploy drone!");
                drone.QueueFree();
                return false;
            }

            // Deploy drone
            GetTree().Root.AddChild(drone);
            drone.GlobalPosition = _player != null ? _player.GlobalPosition : GlobalPosition;
            drone.Activate(_player);

            // Consume energy
            CurrentEnergy -= drone.EnergyCost;
            
            EventBus.Emit(EventBus.EnergyChanged, new EnergyChangedData
            {
                CurrentEnergy = CurrentEnergy,
                MaxEnergy = MaxEnergy
            });

            // Add to active drones
            _activeDrones.Add(drone);

            GD.Print($"Deployed {drone.DroneName} (Energy: {CurrentEnergy}/{MaxEnergy})");
            return true;
        }

        /// <summary>
        /// Recall all active drones
        /// </summary>
        public void RecallAllDrones()
        {
            foreach (var drone in _activeDrones)
            {
                if (IsInstanceValid(drone))
                {
                    drone.Deactivate();
                }
            }

            _activeDrones.Clear();
            GD.Print("All drones recalled");
        }

        /// <summary>
        /// Set drone loadout from packed scenes
        /// </summary>
        public void SetDroneLoadout(List<PackedScene> loadout)
        {
            _droneLoadout = new List<PackedScene>(loadout);
        }

        /// <summary>
        /// Add a drone scene to loadout
        /// </summary>
        public void AddDroneToLoadout(PackedScene droneScene)
        {
            _droneLoadout.Add(droneScene);
        }

        #endregion

        #region Private Methods

        private void InitializeDefaultLoadout()
        {
            // For now, create default drone types programmatically
            // In production, these would be loaded from packed scenes or resources
            
            // We can't easily create PackedScenes at runtime, so we'll
            // store the types and instantiate them when needed
            // This is a simplified version - in production use proper scene resources
            
            GD.Print("DroneManager initialized with default loadout");
            // TODO: Load from player save data
        }

        #endregion
    }

    #region Event Data Structures

    public class EnergyChangedData
    {
        public float CurrentEnergy { get; set; }
        public float MaxEnergy { get; set; }
    }

    #endregion
}
