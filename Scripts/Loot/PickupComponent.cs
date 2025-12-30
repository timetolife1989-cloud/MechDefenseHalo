using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Loot
{
    /// <summary>
    /// Component that enables an entity to pick up loot drops
    /// Supports auto-pickup and magnetic collection
    /// </summary>
    public partial class PickupComponent : Node
    {
        #region Exported Properties

        [Export] public bool CanPickup { get; set; } = true;
        [Export] public bool AutoPickup { get; set; } = false;
        [Export] public bool MagneticCollection { get; set; } = true;
        [Export] public float MagneticRange { get; set; } = 5.0f;
        [Export] public float MagneticForce { get; set; } = 8.0f;
        [Export] public float PickupRange { get; set; } = 2.0f;

        #endregion

        #region Private Fields

        private Node3D _owner;
        private Area3D _magneticArea;
        private Area3D _pickupArea;
        private List<LootDrop> _nearbyLoot = new();
        private List<LootDrop> _magnetizedLoot = new();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            _owner = GetParent<Node3D>();

            if (_owner == null)
            {
                GD.PrintErr("PickupComponent must be a child of a Node3D");
                return;
            }

            SetupMagneticArea();
            SetupPickupArea();

            GD.Print($"PickupComponent initialized for {_owner.Name}");
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!CanPickup || _owner == null)
            {
                return;
            }

            // Apply magnetic force to nearby loot
            if (MagneticCollection)
            {
                ApplyMagneticForce((float)delta);
            }

            // Auto-pickup loot in range
            if (AutoPickup)
            {
                PickupNearbyLoot();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually pickup a specific loot drop
        /// </summary>
        /// <param name="lootDrop">The loot drop to pick up</param>
        /// <returns>True if pickup was successful</returns>
        public bool PickupLoot(LootDrop lootDrop)
        {
            if (!CanPickup || lootDrop == null || lootDrop.IsPickedUp)
            {
                return false;
            }

            // Check distance
            float distance = _owner.GlobalPosition.DistanceTo(lootDrop.GlobalPosition);
            if (distance > PickupRange)
            {
                return false;
            }

            lootDrop.OnPickedUp(_owner);
            
            // Remove from tracking lists
            _nearbyLoot.Remove(lootDrop);
            _magnetizedLoot.Remove(lootDrop);

            return true;
        }

        /// <summary>
        /// Try to pickup all loot within range
        /// </summary>
        /// <returns>Number of items picked up</returns>
        public int PickupAllNearby()
        {
            int count = 0;
            var lootToPickup = new List<LootDrop>(_nearbyLoot);

            foreach (var loot in lootToPickup)
            {
                if (PickupLoot(loot))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Enable or disable pickup capability
        /// </summary>
        public void SetCanPickup(bool canPickup)
        {
            CanPickup = canPickup;
        }

        /// <summary>
        /// Enable or disable auto-pickup
        /// </summary>
        public void SetAutoPickup(bool autoPickup)
        {
            AutoPickup = autoPickup;
            GD.Print($"Auto-pickup {(autoPickup ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Enable or disable magnetic collection
        /// </summary>
        public void SetMagneticCollection(bool magnetic)
        {
            MagneticCollection = magnetic;
            GD.Print($"Magnetic collection {(magnetic ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Set the magnetic range
        /// </summary>
        public void SetMagneticRange(float range)
        {
            MagneticRange = Mathf.Max(0, range);
            UpdateMagneticAreaSize();
        }

        /// <summary>
        /// Get count of nearby loot items
        /// </summary>
        public int GetNearbyLootCount()
        {
            return _nearbyLoot.Count;
        }

        #endregion

        #region Private Methods - Setup

        private void SetupMagneticArea()
        {
            // Create magnetic detection area
            _magneticArea = new Area3D();
            _magneticArea.Name = "MagneticArea";
            _magneticArea.CollisionLayer = 0;
            _magneticArea.CollisionMask = 1; // Assuming loot is on layer 1
            
            var shape = new SphereShape3D();
            shape.Radius = MagneticRange;
            
            var collisionShape = new CollisionShape3D();
            collisionShape.Shape = shape;
            
            _magneticArea.AddChild(collisionShape);
            _owner.AddChild(_magneticArea);

            _magneticArea.BodyEntered += OnMagneticAreaEntered;
            _magneticArea.BodyExited += OnMagneticAreaExited;
        }

        private void SetupPickupArea()
        {
            // Create pickup detection area
            _pickupArea = new Area3D();
            _pickupArea.Name = "PickupArea";
            _pickupArea.CollisionLayer = 0;
            _pickupArea.CollisionMask = 1;
            
            var shape = new SphereShape3D();
            shape.Radius = PickupRange;
            
            var collisionShape = new CollisionShape3D();
            collisionShape.Shape = shape;
            
            _pickupArea.AddChild(collisionShape);
            _owner.AddChild(_pickupArea);

            _pickupArea.BodyEntered += OnPickupAreaEntered;
            _pickupArea.BodyExited += OnPickupAreaExited;
        }

        private void UpdateMagneticAreaSize()
        {
            if (_magneticArea != null)
            {
                var collisionShape = _magneticArea.GetChild<CollisionShape3D>(0);
                if (collisionShape?.Shape is SphereShape3D sphere)
                {
                    sphere.Radius = MagneticRange;
                }
            }
        }

        #endregion

        #region Private Methods - Pickup Logic

        private void ApplyMagneticForce(float delta)
        {
            foreach (var loot in _magnetizedLoot.ToArray())
            {
                if (!IsInstanceValid(loot) || loot.IsPickedUp)
                {
                    _magnetizedLoot.Remove(loot);
                    continue;
                }

                // Calculate direction to owner
                Vector3 direction = (_owner.GlobalPosition - loot.GlobalPosition).Normalized();
                float distance = _owner.GlobalPosition.DistanceTo(loot.GlobalPosition);

                // Stronger pull when closer
                float pullStrength = MagneticForce * (1.0f - (distance / MagneticRange));
                
                // Move loot towards owner
                Vector3 newPosition = loot.GlobalPosition + direction * pullStrength * delta;
                loot.GlobalPosition = newPosition;
            }
        }

        private void PickupNearbyLoot()
        {
            var lootToPickup = new List<LootDrop>(_nearbyLoot);

            foreach (var loot in lootToPickup)
            {
                PickupLoot(loot);
            }
        }

        #endregion

        #region Signal Handlers

        private void OnMagneticAreaEntered(Node3D body)
        {
            if (body is LootDrop lootDrop && !lootDrop.IsPickedUp)
            {
                if (!_magnetizedLoot.Contains(lootDrop))
                {
                    _magnetizedLoot.Add(lootDrop);
                }
            }
        }

        private void OnMagneticAreaExited(Node3D body)
        {
            if (body is LootDrop lootDrop)
            {
                _magnetizedLoot.Remove(lootDrop);
            }
        }

        private void OnPickupAreaEntered(Node3D body)
        {
            if (body is LootDrop lootDrop && !lootDrop.IsPickedUp)
            {
                if (!_nearbyLoot.Contains(lootDrop))
                {
                    _nearbyLoot.Add(lootDrop);
                }

                // Immediately pickup if auto-pickup is enabled
                if (AutoPickup)
                {
                    PickupLoot(lootDrop);
                }
            }
        }

        private void OnPickupAreaExited(Node3D body)
        {
            if (body is LootDrop lootDrop)
            {
                _nearbyLoot.Remove(lootDrop);
            }
        }

        #endregion

        #region Cleanup

        public override void _ExitTree()
        {
            _nearbyLoot.Clear();
            _magnetizedLoot.Clear();
        }

        #endregion
    }
}
