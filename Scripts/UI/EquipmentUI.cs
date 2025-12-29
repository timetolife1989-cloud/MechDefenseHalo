using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Inventory;
using MechDefenseHalo.Items;
using MechDefenseHalo.Items.Sets;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Equipment UI with 15 slots, stats display, set bonuses, and loadout management.
    /// 
    /// REQUIRED SCENE STRUCTURE (create manually in Godot):
    /// 
    /// Control (EquipmentUI) - Script: EquipmentUI.cs
    /// ├─ Panel (Background)
    /// │  ├─ Label (Title) - text: "EQUIPMENT"
    /// │  ├─ HBoxContainer (MainContainer)
    /// │  │  ├─ VBoxContainer (LeftPanel) - Paper Doll
    /// │  │  │  ├─ Label (ArmorLabel) - text: "Armor"
    /// │  │  │  ├─ GridContainer (ArmorSlots) - columns: 2
    /// │  │  │  │  ├─ ItemSlotUI (HeadSlot)
    /// │  │  │  │  ├─ ItemSlotUI (TorsoSlot)
    /// │  │  │  │  ├─ ItemSlotUI (ArmsSlot)
    /// │  │  │  │  └─ ItemSlotUI (LegsSlot)
    /// │  │  │  ├─ Label (WeaponsLabel) - text: "Weapons"
    /// │  │  │  ├─ GridContainer (WeaponSlots) - columns: 2
    /// │  │  │  │  ├─ ItemSlotUI (Weapon1Slot)
    /// │  │  │  │  ├─ ItemSlotUI (Weapon2Slot)
    /// │  │  │  │  ├─ ItemSlotUI (Weapon3Slot)
    /// │  │  │  │  └─ ItemSlotUI (Weapon4Slot)
    /// │  │  │  ├─ Label (DronesLabel) - text: "Drones"
    /// │  │  │  └─ GridContainer (DroneSlots) - columns: 3
    /// │  │  │     ├─ ItemSlotUI (Drone1Slot) through (Drone5Slot)
    /// │  │  ├─ VBoxContainer (RightPanel)
    /// │  │  │  ├─ Panel (StatsPanel)
    /// │  │  │  │  └─ VBoxContainer
    /// │  │  │  │     ├─ Label - text: "STATS"
    /// │  │  │  │     ├─ Label (TotalHPLabel)
    /// │  │  │  │     ├─ Label (TotalShieldLabel)
    /// │  │  │  │     ├─ Label (SpeedLabel)
    /// │  │  │  │     └─ Label (CritChanceLabel)
    /// │  │  │  └─ SetBonusPanelUI (SetBonusPanel)
    /// │  ├─ HBoxContainer (BottomBar)
    /// │  │  ├─ Label - text: "Loadout:"
    /// │  │  ├─ OptionButton (LoadoutSelector)
    /// │  │  ├─ Button (SaveLoadoutButton) - text: "Save"
    /// │  │  ├─ Button (LoadLoadoutButton) - text: "Load"
    /// │  │  └─ Button (CloseButton) - text: "Close"
    /// </summary>
    public partial class EquipmentUI : Control
    {
        #region Export Variables (Wire these in Godot Editor)

        // Equipment Slot Nodes
        [Export] public ItemSlotUI HeadSlot { get; set; }
        [Export] public ItemSlotUI TorsoSlot { get; set; }
        [Export] public ItemSlotUI ArmsSlot { get; set; }
        [Export] public ItemSlotUI LegsSlot { get; set; }
        [Export] public ItemSlotUI[] WeaponSlots { get; set; } = new ItemSlotUI[4];
        [Export] public ItemSlotUI[] DroneSlots { get; set; } = new ItemSlotUI[5];
        [Export] public ItemSlotUI[] AccessorySlots { get; set; } = new ItemSlotUI[2];

        // Stat Display Nodes
        [Export] public Label TotalHPLabel { get; set; }
        [Export] public Label TotalShieldLabel { get; set; }
        [Export] public Label SpeedLabel { get; set; }
        [Export] public Label CritChanceLabel { get; set; }
        [Export] public SetBonusPanelUI SetBonusPanel { get; set; }

        // Loadout System
        [Export] public OptionButton LoadoutSelector { get; set; }
        [Export] public Button SaveLoadoutButton { get; set; }
        [Export] public Button LoadLoadoutButton { get; set; }
        [Export] public Button CloseButton { get; set; }

        #endregion

        #region Private Fields

        private EquipmentManager _equipmentManager;
        private SetManager _setManager;
        private InventoryManager _inventoryManager;
        private Dictionary<EquipmentSlot, ItemSlotUI> _slotUIMap = new();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get manager references
            _equipmentManager = GetNode<EquipmentManager>("/root/EquipmentManager");
            _setManager = GetNode<SetManager>("/root/SetManager");
            _inventoryManager = GetNode<InventoryManager>("/root/InventoryManager");

            // Setup slot mappings
            InitializeSlotMappings();

            // Connect button signals
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

            // Initialize set bonus panel
            if (SetBonusPanel != null && _setManager != null)
            {
                SetBonusPanel.Initialize(_setManager);
            }

            // Listen for equipment changes
            EventBus.On(EventBus.ItemEquipped, OnItemEquippedEvent);
            EventBus.On(EventBus.ItemUnequipped, OnItemUnequippedEvent);

            // Refresh display
            RefreshDisplay();

            // Hide by default
            Hide();

            GD.Print("EquipmentUI initialized");
        }

        public override void _Input(InputEvent @event)
        {
            // Toggle with 'E' key
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.E)
            {
                ToggleVisibility();
                GetViewport().SetInputAsHandled();
            }
        }

        #endregion

        #region Public Methods

        public void ToggleVisibility()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshDisplay();
            }
        }

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
            // Map equipment slots to UI slots
            if (HeadSlot != null)
            {
                _slotUIMap[EquipmentSlot.Head] = HeadSlot;
                HeadSlot.SlotID = "Head";
                HeadSlot.SlotClicked += (slot, button) => OnSlotClicked(EquipmentSlot.Head, button);
            }
            if (TorsoSlot != null)
            {
                _slotUIMap[EquipmentSlot.Torso] = TorsoSlot;
                TorsoSlot.SlotID = "Torso";
                TorsoSlot.SlotClicked += (slot, button) => OnSlotClicked(EquipmentSlot.Torso, button);
            }
            if (ArmsSlot != null)
            {
                _slotUIMap[EquipmentSlot.Arms] = ArmsSlot;
                ArmsSlot.SlotID = "Arms";
                ArmsSlot.SlotClicked += (slot, button) => OnSlotClicked(EquipmentSlot.Arms, button);
            }
            if (LegsSlot != null)
            {
                _slotUIMap[EquipmentSlot.Legs] = LegsSlot;
                LegsSlot.SlotID = "Legs";
                LegsSlot.SlotClicked += (slot, button) => OnSlotClicked(EquipmentSlot.Legs, button);
            }

            // Map weapon slots
            for (int i = 0; i < WeaponSlots.Length && i < 4; i++)
            {
                if (WeaponSlots[i] != null)
                {
                    var equipSlot = (EquipmentSlot)((int)EquipmentSlot.Weapon1 + i);
                    _slotUIMap[equipSlot] = WeaponSlots[i];
                    WeaponSlots[i].SlotID = $"Weapon{i + 1}";
                    WeaponSlots[i].SlotClicked += (_, button) => OnSlotClicked(equipSlot, button);
                }
            }

            // Map drone slots
            for (int i = 0; i < DroneSlots.Length && i < 5; i++)
            {
                if (DroneSlots[i] != null)
                {
                    var equipSlot = (EquipmentSlot)((int)EquipmentSlot.Drone1 + i);
                    _slotUIMap[equipSlot] = DroneSlots[i];
                    DroneSlots[i].SlotID = $"Drone{i + 1}";
                    DroneSlots[i].SlotClicked += (_, button) => OnSlotClicked(equipSlot, button);
                }
            }

            // Map accessory slots
            for (int i = 0; i < AccessorySlots.Length && i < 2; i++)
            {
                if (AccessorySlots[i] != null)
                {
                    var equipSlot = (EquipmentSlot)((int)EquipmentSlot.Accessory1 + i);
                    _slotUIMap[equipSlot] = AccessorySlots[i];
                    AccessorySlots[i].SlotID = $"Accessory{i + 1}";
                    AccessorySlots[i].SlotClicked += (_, button) => OnSlotClicked(equipSlot, button);
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

            foreach (var kvp in _slotUIMap)
            {
                var equipSlot = kvp.Key;
                var slotUI = kvp.Value;

                if (slotUI == null) continue;

                var item = _equipmentManager.GetEquippedItem(equipSlot);
                slotUI.SetItem(item, 1);
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
            if (SetBonusPanel != null && _setManager != null)
            {
                // Update set manager with current equipment
                var equippedItems = new List<ItemBase>();
                foreach (var item in _equipmentManager.GetAllEquippedItems().Values)
                {
                    if (item != null)
                        equippedItems.Add(item);
                }

                _setManager.UpdateEquippedItems(equippedItems);
                SetBonusPanel.RefreshDisplay();
            }
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
            if (_equipmentManager == null || _inventoryManager == null || LoadoutSelector == null) return;

            int loadoutID = LoadoutSelector.Selected;
            _equipmentManager.LoadLoadout(loadoutID, _inventoryManager);

            GD.Print($"Loaded loadout {loadoutID + 1}");
            RefreshDisplay();
        }

        private void OnClosePressed()
        {
            Hide();
        }

        private void OnSlotClicked(EquipmentSlot slot, MouseButton button)
        {
            if (button == MouseButton.Right)
            {
                // Right click - unequip
                if (_equipmentManager != null)
                {
                    var item = _equipmentManager.UnequipItem(slot);
                    if (item != null && _inventoryManager != null)
                    {
                        _inventoryManager.AddItem(item, 1);
                    }
                }
            }
        }

        private void OnItemEquippedEvent(object data)
        {
            RefreshDisplay();
        }

        private void OnItemUnequippedEvent(object data)
        {
            RefreshDisplay();
        }

        #endregion
    }
}
