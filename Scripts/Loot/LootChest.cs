using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Loot
{
    /// <summary>
    /// Loot chest that can be opened to receive items
    /// </summary>
    public partial class LootChest : Node3D
    {
        #region Exported Properties

        [Export] public ItemRarity ChestRarity { get; set; } = ItemRarity.Common;
        [Export] public int MinItems { get; set; } = 1;
        [Export] public int MaxItems { get; set; } = 3;
        [Export] public bool IsOpened { get; set; } = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Guaranteed item IDs that will always drop
        /// </summary>
        public List<string> GuaranteedItemIDs { get; set; } = new();

        #endregion

        #region Private Fields

        private bool _isAnimating = false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Open the chest and roll contents
        /// </summary>
        /// <param name="player">Player who opened the chest</param>
        public void Open(Node3D player)
        {
            if (IsOpened)
            {
                GD.Print("Chest already opened");
                return;
            }

            if (_isAnimating)
            {
                GD.Print("Chest is currently opening");
                return;
            }

            IsOpened = true;
            _isAnimating = true;

            GD.Print($"Opening {ChestRarity} chest...");

            // Play opening animation
            PlayOpenAnimation();

            // Roll contents
            var items = RollChestContents();

            // Emit event
            EventBus.Emit("chest_opened", new ChestOpenedData
            {
                ChestRarity = ChestRarity,
                Items = items,
                Position = GlobalPosition
            });

            // Display loot to player
            foreach (var itemID in items)
            {
                GD.Print($"  Received: {itemID}");
            }

            _isAnimating = false;
        }

        #endregion

        #region Private Methods

        private void PlayOpenAnimation()
        {
            // TODO: Implement visual opening animation when 3D models exist
            // This would include:
            // - Lid opening animation
            // - Particle effects based on rarity
            // - Sound effects
            // - Item reveal with 3D rotation

            GD.Print($"[Animation] {RarityConfig.GetDisplayName(ChestRarity)} chest opening with {RarityConfig.GetColor(ChestRarity)} glow");
        }

        private List<string> RollChestContents()
        {
            var items = new List<string>();

            // Add guaranteed items
            items.AddRange(GuaranteedItemIDs);

            // Roll random items based on chest rarity
            int itemCount = GD.RandRange(MinItems, MaxItems);

            for (int i = 0; i < itemCount; i++)
            {
                // Roll rarity with higher chances for better items in higher rarity chests
                ItemRarity rolledRarity = RollItemRarity();
                
                // Get random item of that rarity from database
                var availableItems = ItemDatabase.GetItemsByRarity(rolledRarity);
                
                if (availableItems.Count > 0)
                {
                    var randomItem = availableItems[GD.RandRange(0, availableItems.Count - 1)];
                    items.Add(randomItem.ItemID);
                }
            }

            return items;
        }

        private ItemRarity RollItemRarity()
        {
            // Better chests have higher chances of rare items
            float rarityBoost = ChestRarity switch
            {
                ItemRarity.Common => 1.0f,
                ItemRarity.Uncommon => 1.5f,
                ItemRarity.Rare => 2.0f,
                ItemRarity.Epic => 3.0f,
                ItemRarity.Legendary => 5.0f,
                ItemRarity.Exotic => 10.0f,
                _ => 1.0f
            };

            return RarityConfig.RollRarity(rarityBoost);
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Data for chest opened event
    /// </summary>
    public class ChestOpenedData
    {
        public ItemRarity ChestRarity { get; set; }
        public List<string> Items { get; set; }
        public Vector3 Position { get; set; }
    }

    #endregion
}
