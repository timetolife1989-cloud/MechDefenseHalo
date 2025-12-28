using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Loot
{
    /// <summary>
    /// Component that handles dropping loot when an entity dies
    /// Attach to any enemy or lootable object
    /// </summary>
    public partial class LootDropComponent : Node
    {
        #region Exported Properties

        [Export] public string LootTableID { get; set; } = "";
        [Export] public float LuckModifier { get; set; } = 1.0f;
        [Export] public bool AutoPickup { get; set; } = false;
        [Export] public float DropRadius { get; set; } = 2.0f; // Scatter radius for multiple drops

        #endregion

        #region Private Fields

        private PackedScene _lootPickupScene;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Subscribe to entity death
            if (GetParent() is Node parent)
            {
                EventBus.On(EventBus.EntityDied, OnEntityDied);
            }

            // TODO: Load loot pickup prefab when it exists
            // _lootPickupScene = GD.Load<PackedScene>("res://Scenes/Loot/LootPickup.tscn");
        }

        public override void _ExitTree()
        {
            EventBus.Off(EventBus.EntityDied, OnEntityDied);
        }

        #endregion

        #region Event Handlers

        private void OnEntityDied(object data)
        {
            if (data is Components.EntityDiedData diedData)
            {
                // Check if this is our parent entity
                if (diedData.Entity == GetParent())
                {
                    DropLoot(diedData.Position);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Drop loot at the specified position
        /// </summary>
        /// <param name="position">World position to spawn loot</param>
        public void DropLoot(Vector3 position)
        {
            if (string.IsNullOrEmpty(LootTableID))
            {
                GD.Print("No loot table assigned to LootDropComponent");
                return;
            }

            // Apply global loot modifiers
            float totalLuckModifier = LuckModifier * LootModifiers.GetGlobalLuckModifier();

            // Roll loot from table
            var itemIDs = LootTableManager.RollLoot(LootTableID, totalLuckModifier);

            if (itemIDs.Count == 0)
            {
                GD.Print($"No loot dropped from table: {LootTableID}");
                return;
            }

            GD.Print($"Dropping {itemIDs.Count} items from {LootTableID}");

            // Spawn loot pickups
            for (int i = 0; i < itemIDs.Count; i++)
            {
                SpawnLootPickup(itemIDs[i], position + GetRandomOffset(), i);
            }

            // Emit loot dropped event
            EventBus.Emit("loot_dropped", new LootDroppedData
            {
                ItemIDs = itemIDs,
                Position = position,
                LootTableID = LootTableID
            });
        }

        #endregion

        #region Private Methods

        private void SpawnLootPickup(string itemID, Vector3 position, int index)
        {
            // For now, just log the drop
            // In a complete implementation, this would spawn a 3D pickup object
            GD.Print($"  [{index}] Spawned loot: {itemID} at {position}");

            // TODO: Spawn actual 3D loot pickup when prefab exists
            /*
            if (_lootPickupScene != null)
            {
                var pickup = _lootPickupScene.Instantiate<Node3D>();
                pickup.GlobalPosition = position;
                GetTree().Root.AddChild(pickup);
                
                // Configure pickup with item data
                if (pickup.HasNode("LootPickup"))
                {
                    var pickupScript = pickup.GetNode<LootPickup>("LootPickup");
                    pickupScript.SetItemID(itemID);
                }
            }
            */
        }

        private Vector3 GetRandomOffset()
        {
            // Random position within drop radius
            float angle = GD.Randf() * Mathf.Tau;
            float distance = GD.Randf() * DropRadius;
            
            return new Vector3(
                Mathf.Cos(angle) * distance,
                0.5f, // Slight height offset
                Mathf.Sin(angle) * distance
            );
        }

        #endregion
    }

    #region Event Data Structures

    /// <summary>
    /// Data for loot dropped event
    /// </summary>
    public class LootDroppedData
    {
        public List<string> ItemIDs { get; set; }
        public Vector3 Position { get; set; }
        public string LootTableID { get; set; }
    }

    #endregion
}
