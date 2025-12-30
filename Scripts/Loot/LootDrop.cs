using Godot;
using System;
using MechDefenseHalo.Items;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Loot
{
    /// <summary>
    /// Represents a physical loot drop in the world that can be picked up by the player
    /// Handles visual feedback, animations, and pickup mechanics
    /// </summary>
    public partial class LootDrop : Node3D
    {
        #region Exported Properties

        [Export] public float RotationSpeed { get; set; } = 2.0f;
        [Export] public float BobSpeed { get; set; } = 2.0f;
        [Export] public float BobAmount { get; set; } = 0.3f;
        [Export] public float LifetimeSeconds { get; set; } = 60.0f; // Auto-despawn after 1 minute
        [Export] public bool EnableGlow { get; set; } = true;

        #endregion

        #region Public Properties

        public string ItemId { get; private set; }
        public ItemRarity Rarity { get; private set; }
        public bool IsPickedUp { get; private set; }

        #endregion

        #region Private Fields

        private float _spawnTime;
        private float _bobTimer;
        private Vector3 _basePosition;
        private MeshInstance3D _visualMesh;
        private OmniLight3D _glowLight;
        private Area3D _pickupArea;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            _spawnTime = Time.GetTicksMsec() / 1000.0f;
            _basePosition = GlobalPosition;

            SetupVisuals();
            SetupPickupArea();
        }

        public override void _Process(double delta)
        {
            if (IsPickedUp)
            {
                return;
            }

            // Rotate loot drop
            RotateY(RotationSpeed * (float)delta);

            // Bob up and down
            _bobTimer += (float)delta * BobSpeed;
            float bobOffset = Mathf.Sin(_bobTimer) * BobAmount;
            GlobalPosition = _basePosition + new Vector3(0, bobOffset, 0);

            // Check lifetime
            float currentTime = Time.GetTicksMsec() / 1000.0f;
            if (currentTime - _spawnTime >= LifetimeSeconds)
            {
                Despawn();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the loot drop with item data
        /// </summary>
        /// <param name="itemId">Item ID to represent</param>
        /// <param name="rarity">Rarity of the item</param>
        public void Initialize(string itemId, ItemRarity rarity)
        {
            ItemId = itemId;
            Rarity = rarity;

            // Update visual appearance based on rarity
            UpdateRarityVisuals();

            GD.Print($"Initialized LootDrop: {itemId} ({rarity})");
        }

        /// <summary>
        /// Called when the loot is picked up
        /// </summary>
        /// <param name="picker">The node that picked up the loot</param>
        public void OnPickedUp(Node picker)
        {
            if (IsPickedUp)
            {
                return;
            }

            IsPickedUp = true;

            GD.Print($"Loot picked up: {ItemId} by {picker.Name}");

            // Play pickup animation/effects
            PlayPickupEffects();

            // Emit pickup event
            EventBus.Emit("loot_picked_up", new LootPickedUpData
            {
                ItemId = ItemId,
                Rarity = Rarity,
                Position = GlobalPosition,
                Picker = picker
            });

            // Unregister from manager
            if (LootManager.Instance != null)
            {
                LootManager.Instance.UnregisterLootDrop(this);
            }

            // Remove from scene after a short delay for effects
            GetTree().CreateTimer(0.5f).Timeout += QueueFree;
        }

        #endregion

        #region Private Methods - Setup

        private void SetupVisuals()
        {
            // Create a simple cube mesh if no custom mesh is present
            _visualMesh = GetNodeOrNull<MeshInstance3D>("VisualMesh");
            
            if (_visualMesh == null)
            {
                _visualMesh = new MeshInstance3D();
                _visualMesh.Name = "VisualMesh";
                
                // Create a small cube as default visual
                var boxMesh = new BoxMesh();
                boxMesh.Size = new Vector3(0.3f, 0.3f, 0.3f);
                _visualMesh.Mesh = boxMesh;
                
                // Create material
                var material = new StandardMaterial3D();
                material.AlbedoColor = new Color(1, 1, 1);
                material.EmissionEnabled = true;
                _visualMesh.SetSurfaceOverrideMaterial(0, material);
                
                AddChild(_visualMesh);
            }

            // Setup glow light
            if (EnableGlow)
            {
                _glowLight = GetNodeOrNull<OmniLight3D>("GlowLight");
                
                if (_glowLight == null)
                {
                    _glowLight = new OmniLight3D();
                    _glowLight.Name = "GlowLight";
                    _glowLight.LightEnergy = 1.0f;
                    _glowLight.OmniRange = 2.0f;
                    AddChild(_glowLight);
                }
            }
        }

        private void SetupPickupArea()
        {
            _pickupArea = GetNodeOrNull<Area3D>("PickupArea");
            
            if (_pickupArea == null)
            {
                _pickupArea = new Area3D();
                _pickupArea.Name = "PickupArea";
                
                var collisionShape = new CollisionShape3D();
                var shape = new SphereShape3D();
                shape.Radius = 1.0f;
                collisionShape.Shape = shape;
                
                _pickupArea.AddChild(collisionShape);
                AddChild(_pickupArea);
            }

            // Connect pickup signal
            _pickupArea.BodyEntered += OnBodyEntered;
        }

        private void UpdateRarityVisuals()
        {
            Color rarityColor = RarityConfig.GetColor(Rarity);

            // Update mesh material
            if (_visualMesh != null)
            {
                var material = _visualMesh.GetActiveMaterial(0) as StandardMaterial3D;
                if (material != null)
                {
                    material.AlbedoColor = rarityColor;
                    material.Emission = rarityColor * 0.5f;
                }
            }

            // Update glow light
            if (_glowLight != null)
            {
                _glowLight.LightColor = rarityColor;
                
                // Higher rarity = brighter glow
                float energyMultiplier = (int)Rarity switch
                {
                    <= 1 => 0.5f,  // Common, Uncommon
                    2 => 0.8f,     // Rare
                    3 => 1.2f,     // Epic
                    >= 4 => 1.5f   // Legendary+
                };
                
                _glowLight.LightEnergy = energyMultiplier;
            }
        }

        private void OnBodyEntered(Node3D body)
        {
            // Check if the body has a PickupComponent
            var pickupComponent = body.GetNodeOrNull<PickupComponent>("PickupComponent");
            if (pickupComponent != null && pickupComponent.CanPickup)
            {
                OnPickedUp(body);
            }
        }

        private void PlayPickupEffects()
        {
            // TODO: Add particle effects, sound effects, etc.
            // For now, just animate scaling down
            
            var tween = CreateTween();
            tween.TweenProperty(this, "scale", Vector3.Zero, 0.3f);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }

        private void Despawn()
        {
            GD.Print($"LootDrop despawned due to timeout: {ItemId}");
            
            // Unregister from manager
            if (LootManager.Instance != null)
            {
                LootManager.Instance.UnregisterLootDrop(this);
            }

            QueueFree();
        }

        #endregion
    }

    /// <summary>
    /// Event data for when loot is picked up
    /// </summary>
    public class LootPickedUpData
    {
        public string ItemId { get; set; }
        public ItemRarity Rarity { get; set; }
        public Vector3 Position { get; set; }
        public Node Picker { get; set; }
    }
}
