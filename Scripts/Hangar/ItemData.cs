using Godot;
using System;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Hangar
{
    /// <summary>
    /// Item data for display in hangar viewer
    /// </summary>
    public class ItemData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemRarity Rarity { get; set; }
        public string Type { get; set; }
        public string ModelPath { get; set; }
    }
}
