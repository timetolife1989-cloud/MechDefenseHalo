# UI Scene Wiring Guide

This document explains how to properly wire the Export variables in the Godot Editor for the newly created UI scenes.

## Created Scene Files

### Main UI Scenes
1. **UI/Inventory.tscn** - InventoryUI.cs
2. **UI/Equipment.tscn** - EquipmentUI.cs  
3. **UI/Crafting.tscn** - CraftingUI.cs
4. **UI/Shop.tscn** - ShopUI.cs

### Component Scenes
5. **UI/ItemSlot.tscn** - ItemSlotUI.cs (70x70px minimum)
6. **UI/SetBonusPanel.tscn** - SetBonusPanelUI.cs
7. **UI/ShopItemCard.tscn** - ShopItemCardUI.cs (160x220px)
8. **UI/CraftJobPanel.tscn** - CraftJobPanelUI.cs (280x100px)

## Wiring Export Variables in Godot Editor

### General Steps
1. Open the scene file in Godot Editor
2. Select the root node (e.g., InventoryUI)
3. In the Inspector panel, scroll to the "Script Variables" section
4. For each Export variable, drag the corresponding node from the Scene tree or use the node picker

### UI/Inventory.tscn Export Wiring

Select the `InventoryUI` root node and wire:
- **ItemGrid** → Background/ScrollContainer/ItemGrid
- **SearchBar** → Background/TopBar/SearchBar
- **SortDropdown** → Background/TopBar/SortDropdown
- **SlotCountLabel** → Background/TopBar/SlotCountLabel
- **SalvageAllButton** → Background/BottomBar/SalvageAllButton
- **CloseButton** → Background/BottomBar/CloseButton
- **ItemTooltip** → Background/ItemTooltip
- **TooltipItemName** → Background/ItemTooltip/VBoxContainer/TooltipItemName
- **TooltipItemStats** → Background/ItemTooltip/VBoxContainer/TooltipItemStats
- **TooltipItemDescription** → Background/ItemTooltip/VBoxContainer/TooltipItemDescription
- **ItemSlotPrefab** → Drag UI/ItemSlot.tscn from FileSystem

### UI/Equipment.tscn Export Wiring

Select the `EquipmentUI` root node and wire:

**Armor Slots:**
- **HeadSlot** → Background/MainContainer/LeftPanel/ArmorSlots/HeadSlot
- **TorsoSlot** → Background/MainContainer/LeftPanel/ArmorSlots/TorsoSlot
- **ArmsSlot** → Background/MainContainer/LeftPanel/ArmorSlots/ArmsSlot
- **LegsSlot** → Background/MainContainer/LeftPanel/ArmorSlots/LegsSlot

**Weapon Slots Array (size 4):**
- **WeaponSlots[0]** → Background/MainContainer/LeftPanel/WeaponSlots/Weapon1Slot
- **WeaponSlots[1]** → Background/MainContainer/LeftPanel/WeaponSlots/Weapon2Slot
- **WeaponSlots[2]** → Background/MainContainer/LeftPanel/WeaponSlots/Weapon3Slot
- **WeaponSlots[3]** → Background/MainContainer/LeftPanel/WeaponSlots/Weapon4Slot

**Drone Slots Array (size 5):**
- **DroneSlots[0-4]** → Background/MainContainer/LeftPanel/DroneSlots/Drone1Slot through Drone5Slot

**Accessory Slots Array (size 2):**
- **AccessorySlots[0-1]** → Background/MainContainer/LeftPanel/AccessorySlots/Accessory1Slot and Accessory2Slot

**Stats Display:**
- **TotalHPLabel** → Background/MainContainer/RightPanel/StatsPanel/VBoxContainer/TotalHPLabel
- **TotalShieldLabel** → Background/MainContainer/RightPanel/StatsPanel/VBoxContainer/TotalShieldLabel
- **SpeedLabel** → Background/MainContainer/RightPanel/StatsPanel/VBoxContainer/SpeedLabel
- **CritChanceLabel** → Background/MainContainer/RightPanel/StatsPanel/VBoxContainer/CritChanceLabel
- **SetBonusPanel** → Background/MainContainer/RightPanel/SetBonusPanel

**Loadout Controls:**
- **LoadoutSelector** → Background/BottomBar/LoadoutSelector
- **SaveLoadoutButton** → Background/BottomBar/SaveLoadoutButton
- **LoadLoadoutButton** → Background/BottomBar/LoadLoadoutButton
- **CloseButton** → Background/BottomBar/CloseButton

### UI/Crafting.tscn Export Wiring

Select the `CraftingUI` root node and wire:
- **BlueprintList** → Background/MainContainer/LeftPanel/BlueprintList
- **BlueprintName** → Background/MainContainer/MiddlePanel/BlueprintName
- **MaterialRequirements** → Background/MainContainer/MiddlePanel/MaterialRequirements
- **CreditCost** → Background/MainContainer/MiddlePanel/CreditCost
- **CraftTime** → Background/MainContainer/MiddlePanel/CraftTime
- **CraftButton** → Background/MainContainer/MiddlePanel/CraftButton
- **CraftQueueContainer** → Background/MainContainer/RightPanel/CraftQueueContainer
- **QueueCountLabel** → Background/MainContainer/RightPanel/QueueCountLabel
- **FilterDropdown** → Background/MainContainer/LeftPanel/FilterBar/FilterDropdown
- **CloseButton** → Background/CloseButton
- **CraftJobPanelPrefab** → Drag UI/CraftJobPanel.tscn from FileSystem

Note: BlueprintDetails is not directly wired but is part of MiddlePanel structure.

### UI/Shop.tscn Export Wiring

Select the `ShopUI` root node and wire:
- **CategoryTabs** → Background/CategoryTabs
- **CosmeticsGrid** → Background/CategoryTabs/CosmeticsTab/CosmeticsGrid
- **ConvenienceGrid** → Background/CategoryTabs/ConvenienceTab/ConvenienceGrid
- **MaterialsGrid** → Background/CategoryTabs/MaterialsTab/MaterialsGrid
- **FeaturedContainer** → Background/FeaturedPanel/FeaturedContainer
- **CreditsLabel** → Background/CurrencyBar/CreditsLabel
- **CoresLabel** → Background/CurrencyBar/CoresLabel
- **CloseButton** → Background/CloseButton
- **PurchaseConfirm** → Background/PurchaseConfirm
- **ShopItemCardPrefab** → Drag UI/ShopItemCard.tscn from FileSystem

## Keyboard Shortcuts

The following keyboard shortcuts are implemented in the UI scripts:

- **I** - Toggle Inventory UI (InventoryUI._Input)
- **E** - Toggle Equipment UI (EquipmentUI._Input)
- **C** - Toggle Crafting UI (CraftingUI._Input)
- **S** - Toggle Shop UI (ShopUI._Input)

## Mobile-Friendly Design

All interactive elements meet the 70px minimum touch target requirement:
- Buttons: 70px+ height
- Item slots: 70-100px
- Touch areas properly sized for finger interaction

## Testing Checklist

After wiring all Export variables in the Godot Editor:

1. ✅ Open each scene and verify no null reference warnings
2. ✅ Run the game and press I/E/C/S to test keyboard shortcuts
3. ✅ Verify all panels open and close correctly
4. ✅ Check that UI scales properly on different screen sizes
5. ✅ Test touch targets on mobile or with touch simulation
6. ✅ Verify all buttons are clickable and respond correctly
7. ✅ Check that all labels display placeholder text

## Notes

- The scenes are designed for portrait mobile layout (1080x2400)
- All paths use `res://` notation for Godot resource paths
- Anchors are set for responsive layout (will expand/contract with window size)
- All component scenes (ItemSlot, SetBonusPanel, etc.) are properly referenced as PackedScene exports
- The Equipment scene uses ItemSlotUI instances directly in the scene tree (not dynamically instantiated)
- Inventory creates 500 ItemSlot instances dynamically at runtime
