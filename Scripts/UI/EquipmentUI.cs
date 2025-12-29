using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Inventory;
using MechDefenseHalo.Items;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Equipment UI system with paper doll, stats, and loadout management
    /// </summary>
    public partial class EquipmentUI : Control
    {
        #region Equipment Slot Nodes
        
        [Export] public Panel HeadSlot { get; set; }
        [Export] public Panel TorsoSlot { get; set; }
        [Export] public Panel ArmsSlot { get; set; }
        [Export] public Panel LegsSlot { get; set; }
        [Export] public Panel[] WeaponSlots { get; set; } = new Panel[4];
        [Export] public Panel[] DroneSlots { get; set; } = new Panel[5];
        [Export] public Panel[] AccessorySlots { get; set; } = new Panel[2];
        
        #endregion
        
        #region Stat Display Nodes
        
        [Export] public Label TotalHPLabel { get; set; }
        [Export] public Label TotalShieldLabel { get; set; }
        [Export] public Label SpeedLabel { get; set; }
        [Export] public Label CritChanceLabel { get; set; }
        [Export] public VBoxContainer SetBonusContainer { get; set; }
        
        #endregion
        
        #region Loadout System
        
        [Export] public OptionButton LoadoutSelector { get; set; }
        [Export] public Button SaveLoadoutButton { get; set; }
        [Export] public Button LoadLoadoutButton { get; set; }
        [Export] public Button CloseButton { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private EquipmentManager _equipmentManager;
        private Dictionary<EquipmentSlot, Panel> _slotPanels = new();
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Initially hidden
            Visible = false;
            
            // Setup slot mappings
            InitializeSlotMappings();
            
            // Connect signals
            if (LoadoutSelector != null)
            {
                PopulateLoadoutDropdown();
            }
            
            if (SaveLoadoutButton != null)
            {
                SaveLoadoutButton.Pressed += OnSaveLoadoutPressed;
            }
            
            if (LoadLoadoutButton != null)
            {
                LoadLoadoutButton.Pressed += OnLoadLoadoutPressed;
            }
            
            if (CloseButton != null)
            {
                CloseButton.Pressed += OnClosePressed;
            }
            
            // Listen for equipment changes
            EventBus.On("item_equipped", OnItemEquipped);
            EventBus.On("item_unequipped", OnItemUnequipped);
            
            GD.Print("EquipmentUI initialized");
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.E)
                {
                    ToggleVisibility();
                }
                else if (keyEvent.Keycode == Key.Escape && Visible)
                {
                    Hide();
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize with equipment manager reference
        /// </summary>
        public void Initialize(EquipmentManager equipmentManager)
        {
            _equipmentManager = equipmentManager;
            RefreshDisplay();
        }
        
        /// <summary>
        /// Toggle equipment UI visibility
        /// </summary>
        public void ToggleVisibility()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshDisplay();
            }
        }
        
        /// <summary>
        /// Refresh the entire equipment display
        /// </summary>
        public void RefreshDisplay()
        {
            if (_equipmentManager == null) return;
            
            UpdateEquipmentSlots();
            UpdateStatsDisplay();
            UpdateSetBonuses();
        }
        
        #endregion
        
        #region Private Methods - UI Setup
        
        private void InitializeSlotMappings()
        {
            _slotPanels[EquipmentSlot.Head] = HeadSlot;
            _slotPanels[EquipmentSlot.Torso] = TorsoSlot;
            _slotPanels[EquipmentSlot.Arms] = ArmsSlot;
            _slotPanels[EquipmentSlot.Legs] = LegsSlot;
            
            for (int i = 0; i < WeaponSlots.Length; i++)
            {
                _slotPanels[(EquipmentSlot)((int)EquipmentSlot.Weapon1 + i)] = WeaponSlots[i];
            }
            
            for (int i = 0; i < DroneSlots.Length; i++)
            {
                _slotPanels[(EquipmentSlot)((int)EquipmentSlot.Drone1 + i)] = DroneSlots[i];
            }
            
            for (int i = 0; i < AccessorySlots.Length; i++)
            {
                _slotPanels[(EquipmentSlot)((int)EquipmentSlot.Accessory1 + i)] = AccessorySlots[i];
            }
            
            // Add click handlers to all slots
            foreach (var kvp in _slotPanels)
            {
                var slot = kvp.Key;
                var panel = kvp.Value;
                
                if (panel != null)
                {
                    panel.GuiInput += (inputEvent) => OnSlotClicked(inputEvent, slot);
                }
            }
        }
        
        private void PopulateLoadoutDropdown()
        {
            LoadoutSelector.Clear();
            for (int i = 1; i <= 5; i++)
            {
                LoadoutSelector.AddItem($"Loadout {i}");
            }
            LoadoutSelector.Selected = 0;
        }
        
        #endregion
        
        #region Private Methods - Display Update
        
        private void UpdateEquipmentSlots()
        {
            if (_equipmentManager == null) return;
            
            foreach (var kvp in _slotPanels)
            {
                var slot = kvp.Key;
                var panel = kvp.Value;
                
                if (panel == null) continue;
                
                var item = _equipmentManager.GetEquippedItem(slot);
                
                if (item != null)
                {
                    UpdateSlotWithItem(panel, item);
                }
                else
                {
                    ClearSlot(panel);
                }
            }
        }
        
        private void UpdateStatsDisplay()
        {
            if (_equipmentManager == null) return;
            
            var totalStats = _equipmentManager.GetTotalStats();
            
            if (TotalHPLabel != null)
            {
                TotalHPLabel.Text = $"HP: {totalStats.GetValueOrDefault(StatType.HP, 0):F0}";
            }
            
            if (TotalShieldLabel != null)
            {
                TotalShieldLabel.Text = $"Shield: {totalStats.GetValueOrDefault(StatType.Shield, 0):F0}";
            }
            
            if (SpeedLabel != null)
            {
                SpeedLabel.Text = $"Speed: {totalStats.GetValueOrDefault(StatType.Speed, 0):F1}";
            }
            
            if (CritChanceLabel != null)
            {
                float critChance = totalStats.GetValueOrDefault(StatType.CritChance, 0) * 100;
                float critDamage = totalStats.GetValueOrDefault(StatType.CritDamage, 0) * 100;
                CritChanceLabel.Text = $"Crit: {critChance:F1}% / {critDamage:F0}%";
            }
        }
        
        private void UpdateSetBonuses()
        {
            if (SetBonusContainer == null) return;
            
            // Clear existing set bonus displays
            foreach (var child in SetBonusContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            // TODO: Get active set bonuses from SetManager
            // For now, just display placeholder
            var label = new Label();
            label.Text = "No active set bonuses";
            SetBonusContainer.AddChild(label);
        }
        
        private void UpdateSlotWithItem(Panel slot, ItemBase item)
        {
            if (item == null) return;
            
            // Set background color based on rarity
            slot.Modulate = GetRarityColor(item.Rarity);
            
            // TODO: Add icon texture
        }
        
        private void ClearSlot(Panel slot)
        {
            slot.Modulate = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        }
        
        private Color GetRarityColor(ItemRarity rarity)
        {
            return RarityConfig.GetColor(rarity);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnSaveLoadoutPressed()
        {
            if (_equipmentManager == null || LoadoutSelector == null) return;
            
            int loadoutID = LoadoutSelector.Selected;
            _equipmentManager.SaveLoadout(loadoutID);
            
            GD.Print($"Saved loadout {loadoutID + 1}");
        }
        
        private void OnLoadLoadoutPressed()
        {
            if (_equipmentManager == null || LoadoutSelector == null) return;
            
            int loadoutID = LoadoutSelector.Selected;
            _equipmentManager.LoadLoadout(loadoutID);
            
            GD.Print($"Loaded loadout {loadoutID + 1}");
            RefreshDisplay();
        }
        
        private void OnClosePressed()
        {
            Hide();
        }
        
        private void OnSlotClicked(InputEvent inputEvent, EquipmentSlot slot)
        {
            if (inputEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Right)
                {
                    // Right click - unequip
                    if (_equipmentManager != null)
                    {
                        _equipmentManager.UnequipItem(slot);
                    }
                }
            }
        }
        
        private void OnItemEquipped(object data)
        {
            RefreshDisplay();
        }
        
        private void OnItemUnequipped(object data)
        {
            RefreshDisplay();
        }
        
        #endregion
    }
}
