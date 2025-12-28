using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Drones
{
    /// <summary>
    /// Abstract base class for all drones.
    /// Provides common functionality for orbiting, energy cost, and lifetime.
    /// </summary>
    public abstract partial class DroneBase : Node3D
    {
        #region Exported Properties

        [Export] public string DroneName { get; set; } = "Drone";
        [Export] public float EnergyCost { get; set; } = 20f;
        [Export] public float Lifetime { get; set; } = 30f; // 0 = infinite
        [Export] public float OrbitRadius { get; set; } = 3f;
        [Export] public float OrbitSpeed { get; set; } = 2f;

        #endregion

        #region Public Properties

        public bool IsActive { get; private set; } = false;
        public float RemainingLifetime { get; private set; }
        public Node3D OrbitTarget { get; set; }

        #endregion

        #region Protected Fields

        protected DroneControllerComponent _controller;
        protected MeshInstance3D _mesh;
        protected float _lifeTimer = 0f;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            RemainingLifetime = Lifetime;

            // Get or create controller
            _controller = GetNodeOrNull<DroneControllerComponent>("DroneControllerComponent");
            if (_controller == null)
            {
                _controller = new DroneControllerComponent();
                _controller.Name = "DroneControllerComponent";
                _controller.OrbitRadius = OrbitRadius;
                _controller.OrbitSpeed = OrbitSpeed;
                AddChild(_controller);
            }

            // Get mesh
            _mesh = GetNodeOrNull<MeshInstance3D>("Mesh");
            if (_mesh == null)
            {
                CreateDefaultMesh();
            }

            OnReady();
        }

        public override void _Process(double delta)
        {
            if (!IsActive)
                return;

            float dt = (float)delta;

            // Update lifetime
            if (Lifetime > 0)
            {
                _lifeTimer += dt;
                RemainingLifetime = Mathf.Max(0, Lifetime - _lifeTimer);

                if (RemainingLifetime <= 0)
                {
                    Deactivate();
                    return;
                }
            }

            OnUpdate(dt);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Activate the drone
        /// </summary>
        public virtual void Activate(Node3D orbitTarget)
        {
            IsActive = true;
            OrbitTarget = orbitTarget;
            _lifeTimer = 0f;
            RemainingLifetime = Lifetime;

            if (_controller != null)
            {
                _controller.SetOrbitTarget(orbitTarget);
            }

            EventBus.Emit(EventBus.DroneDeployed, new DroneDeployedData
            {
                Drone = this,
                DroneName = DroneName
            });

            GD.Print($"{DroneName} activated");
        }

        /// <summary>
        /// Deactivate and remove the drone
        /// </summary>
        public virtual void Deactivate()
        {
            IsActive = false;

            EventBus.Emit(EventBus.DroneDestroyed, new DroneDestroyedData
            {
                Drone = this,
                DroneName = DroneName
            });

            GD.Print($"{DroneName} deactivated");
            QueueFree();
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Called once during _Ready - override for custom initialization
        /// </summary>
        protected virtual void OnReady() { }

        /// <summary>
        /// Called each frame - override for custom behavior
        /// </summary>
        protected virtual void OnUpdate(float delta) { }

        #endregion

        #region Protected Methods

        protected void CreateDefaultMesh()
        {
            _mesh = new MeshInstance3D();
            _mesh.Name = "Mesh";
            
            var boxMesh = new BoxMesh();
            boxMesh.Size = new Vector3(0.5f, 0.3f, 0.5f);
            _mesh.Mesh = boxMesh;

            var material = new StandardMaterial3D();
            material.AlbedoColor = GetDroneColor();
            material.EmissionEnabled = true;
            material.Emission = GetDroneColor();
            material.EmissionEnergy = 1.5f;
            _mesh.MaterialOverride = material;

            AddChild(_mesh);
        }

        /// <summary>
        /// Override to customize drone color
        /// </summary>
        protected virtual Color GetDroneColor()
        {
            return new Color(0.3f, 0.7f, 1f); // Cyan by default
        }

        #endregion
    }

    #region Event Data Structures

    public class DroneDeployedData
    {
        public DroneBase Drone { get; set; }
        public string DroneName { get; set; }
    }

    public class DroneDestroyedData
    {
        public DroneBase Drone { get; set; }
        public string DroneName { get; set; }
    }

    #endregion
}
