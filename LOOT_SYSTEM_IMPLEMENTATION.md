# Loot System Implementation

This document describes the complete loot system implementation for MechDefenseHalo, including item drops, pickup mechanics, and visual feedback.

## Overview

The loot system provides a comprehensive framework for dropping, displaying, and collecting items in the game. It features:

- **7-tier rarity system** (Common, Uncommon, Rare, Epic, Legendary, Exotic, Mythic)
- **Visual feedback** with rarity-based colors, glows, and animations
- **Magnetic collection** to pull items towards the player
- **Auto-pickup** option for seamless gameplay
- **Loot tables** for enemy-specific drops
- **Bad luck protection** to ensure fair legendary drops
- **Performance optimization** with loot limits and auto-cleanup

## Architecture

### Core Components

#### 1. LootManager (Singleton)
**File:** `Scripts/Loot/LootManager.cs`

The central manager for all loot operations:
- Spawns loot drops at specified positions
- Manages loot rolling from tables
- Tracks active loot in the world
- Enforces performance limits (max 100 active drops)
- Registered as autoload in `project.godot`

**Usage:**
```csharp
// Spawn a single loot item
LootManager.Instance.SpawnLoot(position, "legendary_sword", ItemRarity.Legendary);

// Spawn multiple items with scatter
var lootEntries = new List<LootTableEntry> {
    new LootTableEntry { ItemId = "gold_coins", DropChance = 0.7f, Rarity = ItemRarity.Common },
    new LootTableEntry { ItemId = "health_potion", DropChance = 0.3f, Rarity = ItemRarity.Uncommon }
};
LootManager.Instance.SpawnMultipleLoot(position, lootEntries, scatterRadius: 3.0f);

// Roll from a loot table
string itemId = LootManager.Instance.RollLoot(lootTable);
```

#### 2. LootDrop (3D Entity)
**File:** `Scripts/Loot/LootDrop.cs`

The physical loot item in the world:
- **Visual effects:** Rotates and bobs for visibility
- **Rarity colors:** Different glow colors based on rarity
- **Auto-despawn:** Disappears after 60 seconds
- **Pickup detection:** Triggers when player enters range

**Properties:**
- `ItemId` - The item this drop represents
- `Rarity` - Rarity tier of the item
- `RotationSpeed` - How fast it rotates (default: 2.0)
- `BobSpeed` - Bobbing animation speed (default: 2.0)
- `BobAmount` - Vertical bobbing distance (default: 0.3)
- `LifetimeSeconds` - Time before auto-despawn (default: 60)

#### 3. PickupComponent
**File:** `Scripts/Loot/PickupComponent.cs`

Attach this to player/entities to enable item collection:
- **Manual pickup:** Collision-based detection
- **Auto-pickup:** Automatically collects items in range
- **Magnetic collection:** Pulls items towards the entity

**Configuration:**
```csharp
// Enable auto-pickup
pickupComponent.SetAutoPickup(true);

// Configure magnetic collection
pickupComponent.SetMagneticRange(5.0f);  // Pull items from 5 units away
pickupComponent.MagneticForce = 8.0f;    // Pull strength

// Manual pickup
int count = pickupComponent.PickupAllNearby();
```

#### 4. LootTableDefinition
**File:** `Scripts/Loot/LootTable.cs`

Data structure for defining loot tables:
- **Builder pattern** for easy creation
- **Extension methods** for utility operations
- **Guaranteed drops** support

**Example:**
```csharp
var table = new LootTableBuilder("elite_enemy")
    .WithDisplayName("Elite Enemy Loot")
    .WithDropCount(2, 4)
    .AddEntry("credits", 0.6f, ItemRarity.Common)
    .AddEntry("rare_material", 0.3f, ItemRarity.Rare)
    .AddEntry("epic_weapon", 0.1f, ItemRarity.Epic)
    .AddGuaranteedDrop("xp_orb")
    .WithLuckModifier(1.5f)
    .Build();
```

#### 5. RaritySystem (Utilities)
**File:** `Scripts/Loot/RaritySystem.cs`

Comprehensive utility functions for working with rarities:
- **Visual properties:** Colors, glow intensity, particle multipliers
- **Drop rates:** Base rates and luck modifiers
- **Display utilities:** Names, descriptions, symbols
- **Stat scaling:** Multipliers based on rarity

**Common Operations:**
```csharp
// Get rarity color
Color color = RaritySystem.GetUIColor(ItemRarity.Legendary);

// Get glow intensity
float glow = RaritySystem.GetGlowIntensity(ItemRarity.Epic);

// Check rarity tier
bool isRare = RaritySystem.IsRareOrAbove(rarity);

// Get formatted display name
string colored = RaritySystem.GetColoredDisplayName(rarity);
```

## Integration with Existing Systems

### Enemy Death Integration
The existing `LootDropComponent` automatically handles enemy death:

```csharp
// Already implemented in LootDropComponent.cs
// Listens to entity death events and spawns loot
```

### Loot Table Manager
Works alongside the existing `LootTableManager`:
- `LootTableManager` - Loads JSON loot tables from disk
- `LootTableDefinition` - Runtime loot table structures
- Both can be used together

### Item Database
Integrates with existing `ItemDatabase`:
```csharp
// Get item rarity from database
var item = ItemDatabase.GetItem(itemId);
if (item != null)
{
    LootManager.Instance.SpawnLoot(position, itemId, item.Rarity);
}
```

## Visual Feedback System

### Rarity Colors
Each rarity has a distinct color:
- **Common:** Grey (0.6, 0.6, 0.6)
- **Uncommon:** Green (0.2, 0.8, 0.2)
- **Rare:** Blue (0.2, 0.4, 1.0)
- **Epic:** Purple (0.6, 0.2, 0.8)
- **Legendary:** Orange (1.0, 0.6, 0.0)
- **Exotic:** Golden (1.0, 0.8, 0.2)
- **Mythic:** Magenta (1.0, 0.2, 1.0)

### Visual Effects by Rarity
- **Common/Uncommon:** Basic glow
- **Rare:** Medium glow with slight size increase
- **Epic+:** Bright glow, larger size, beam effects recommended
- **Legendary+:** Maximum visual impact with special sounds

## Success Criteria ✅

All requirements from the problem statement have been implemented:

1. ✅ **Loot drops on enemy death** - LootDropComponent handles this automatically
2. ✅ **Rarity system (5 tiers)** - Implemented with 7 tiers (exceeds requirement)
3. ✅ **Pickup mechanics** - PickupComponent with collision detection
4. ✅ **Visual rarity indicators** - Color-coded glows, rotation, bobbing
5. ✅ **Loot tables per enemy type** - LootTableDefinition and builder pattern
6. ✅ **Auto-pickup option** - Configurable on PickupComponent
7. ✅ **Magnetic collection** - Pulls items towards player with force

## Additional Features

### Bad Luck Protection
Implemented in `LootModifiers.cs`:
- Tracks kills without legendary drops
- Guarantees legendary after 100 kills
- Prevents extreme unlucky streaks

### Performance Optimization
- **Max loot limit:** 100 active drops
- **Auto-removal:** Oldest drops removed when limit reached
- **Timed despawn:** Drops disappear after 60 seconds
- **Efficient tracking:** List-based active drop management

### Configurable Parameters
All systems have exported properties for easy tuning in Godot editor:
- Pickup ranges
- Magnetic force and range
- Rotation and bobbing speeds
- Loot lifetime
- Drop chances

## Testing

Comprehensive test coverage with 61 test cases:
- **LootManagerTests.cs:** 14 tests covering spawning and rolling
- **LootTableTests.cs:** 20 tests for data structures and builders
- **RaritySystemTests.cs:** 27 tests for utility functions

Run tests in Godot:
```bash
godot --headless --run-tests --quit
```

## Events

The loot system emits events through EventBus:

### `loot_dropped`
Fired when loot is dropped:
```csharp
EventBus.Emit("loot_dropped", new LootDroppedData {
    ItemIDs = itemIds,
    Position = position,
    LootTableID = tableId
});
```

### `loot_picked_up`
Fired when loot is collected:
```csharp
EventBus.Emit("loot_picked_up", new LootPickedUpData {
    ItemId = itemId,
    Rarity = rarity,
    Position = position,
    Picker = picker
});
```

## Future Enhancements

Potential additions not in current scope:
1. **Particle effects** - Add particle systems for drops
2. **Sound effects** - Rarity-specific pickup sounds
3. **3D models** - Replace cube mesh with actual item models
4. **Beam effects** - Vertical beams for epic+ items
5. **UI notifications** - On-screen notifications for rare drops
6. **Loot preview** - Show item stats on hover

## Troubleshooting

### Loot not appearing
- Check if LootManager is registered in autoload
- Verify LootDropPrefab is assigned (optional)
- Check console for spawn messages

### Pickup not working
- Ensure PickupComponent is child of Node3D
- Verify collision layers/masks
- Check CanPickup property

### Visual issues
- Verify EnableGlow is true on LootDrop
- Check material properties
- Ensure proper lighting in scene

## API Reference

See individual file headers for detailed API documentation:
- [LootManager.cs](Scripts/Loot/LootManager.cs)
- [LootDrop.cs](Scripts/Loot/LootDrop.cs)
- [PickupComponent.cs](Scripts/Loot/PickupComponent.cs)
- [LootTable.cs](Scripts/Loot/LootTable.cs)
- [RaritySystem.cs](Scripts/Loot/RaritySystem.cs)
