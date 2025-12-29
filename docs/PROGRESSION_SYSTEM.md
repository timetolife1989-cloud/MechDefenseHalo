# Player Progression System

## Overview

The Player Progression System provides a comprehensive leveling experience with XP tracking, level rewards, and prestige mechanics. Players earn XP from various activities and level up from 1 to 100, unlocking rewards and features along the way.

## Core Components

### XPCurve.cs
Calculates XP requirements for each level using a balanced quadratic curve:
- **Level 1-10**: Linear progression (100 XP per level)
- **Level 11-30**: Quadratic growth (early exponential phase)
- **Level 31-60**: Steeper quadratic growth (mid-game)
- **Level 61-100**: Very steep growth (endgame challenge)

**Total XP to Level 100**: ~2.4 million XP (~120 hours of gameplay)

### PlayerLevel.cs
Main manager class that tracks player level and XP:
- Tracks current level and XP
- Handles level-up logic
- Manages XP gain from various sources
- Emits events for UI updates

**Key Methods**:
- `AddXP(amount, source)` - Add XP and trigger level-ups
- `SetLevel(level, xp)` - Set level directly (for save/load)
- `GetProgressToNextLevel()` - Get progress as 0.0 to 1.0

### LevelRewards.cs
Manages reward distribution for level-ups:

**Every Level**:
- +100 Credits
- +5 Cores

**Every 5 Levels**:
- +25 Bonus Cores
- +10 Inventory Slots (handled by inventory system)

**Milestones**:
- **Level 10**: Second Weapon Slot
- **Level 20**: Third Drone Slot
- **Level 30**: Crafting Speed +10%
- **Level 50**: Fourth Weapon Slot + 500 Cores
- **Level 100**: Prestige System + Legendary Item

### PrestigeSystem.cs
Endgame progression system for max-level players:
- Available at Level 100
- Resets level to 1 but keeps items/currency
- Grants permanent +5% to all stats per prestige
- Maximum 10 prestiges (+50% total bonus)

### ProgressionUI.cs
UI management for displaying progression:
- XP bar showing progress to next level
- Level display with prestige indicator
- Level-up notifications
- Reward popup

## XP Sources

| Source | XP Gain | Example |
|--------|---------|---------|
| Enemy Kill | 10 × enemy level | Level 5 enemy = 50 XP |
| Wave Complete | 100 × wave number | Wave 10 = 1,000 XP |
| Boss Defeat | 500 × boss tier | Tier 2 boss = 1,000 XP |
| Crafting | 20 per item | Any crafted item = 20 XP |
| Achievement | 50-500 | Based on difficulty |
| Daily Mission | 100-300 | Based on complexity |

## Integration Points

### Enemy System
- Added `Level` property to `EnemyBase.cs`
- XP granted automatically on enemy death
- Formula: `10 × enemy.Level`

### Wave System
- XP granted on wave completion in `WaveSpawner.cs`
- Formula: `100 × waveNumber`

### Boss System
- Added `BossTier` property to `BossBase.cs`
- XP granted on boss defeat
- Formula: `500 × bossTier`

### Crafting System
- XP granted on craft completion in `CraftingManager.cs`
- Formula: `20 XP per item`

## Events

The system emits the following events via EventBus:

```csharp
// XP gain event
EventBus.Emit("xp_gained", new XPGainedData {
    Amount, Source, CurrentXP, CurrentLevel, XPToNextLevel
});

// Level up event
EventBus.Emit("player_leveled_up", new PlayerLevelUpData {
    NewLevel, IsMilestone
});

// Reward granted event
EventBus.Emit("level_reward_granted", new LevelRewardData {
    Level, Credits, Cores, IsMilestone
});

// Feature unlocked event
EventBus.Emit("feature_unlocked", new FeatureUnlockedData {
    FeatureName, Level, Description
});

// Prestige event
EventBus.Emit("player_prestiged", new PrestigeData {
    PrestigeLevel, StatBonus
});
```

## Usage Example

```csharp
// Initialize the progression system (add to scene tree)
var playerLevel = new PlayerLevel();
AddChild(playerLevel);

// Award XP
PlayerLevel.AddXP(100, "Quest completion");

// Check if player can prestige
if (PlayerLevel.CanPrestige())
{
    PrestigeSystem.Prestige();
}

// Get stat multiplier for prestige bonuses
float statMultiplier = PrestigeSystem.GetStatMultiplier(); // 1.0 to 1.5
```

## UI Integration

Add the ProgressionUI scene to your HUD:

```gdscript
# In your HUD scene
var progression_ui = preload("res://UI/ProgressionUI.tscn").instantiate()
add_child(progression_ui)
```

The UI automatically:
- Updates XP bar in real-time
- Displays level and prestige
- Shows level-up notifications
- Displays rewards

## Data Files

Configuration files in `Data/Progression/`:
- **xp_curve.json** - Documentation of XP curve design
- **level_rewards.json** - Reward definitions and XP sources
- **prestige_bonuses.json** - Prestige system configuration

## Testing

Comprehensive test suites are available:
- **XPCurveTests.cs** - Tests XP calculations
- **PlayerLevelTests.cs** - Tests level progression
- **PrestigeSystemTests.cs** - Tests prestige mechanics

Run tests via GdUnit4 in Godot Editor.

## Progression Analysis

Use the progression analysis tool to visualize the XP curve:

```bash
python3 docs/progression_analysis.py
```

This shows:
- XP requirements per level
- Estimated play time
- XP sources and their impact
- Milestone rewards
- Prestige bonuses

## Balance Notes

The progression system is designed to provide:
- **Early game (1-30)**: Fast progression, ~1-2 hours per 10 levels
- **Mid game (31-60)**: Moderate progression, ~3-5 hours per 10 levels
- **Late game (61-100)**: Slower progression, ~10-20 hours per 10 levels
- **Total time to max**: ~120 hours of regular gameplay

This ensures:
- New players feel rewarded quickly
- Mid-game provides steady progression
- Endgame offers significant goals
- Prestige system extends replayability

## Future Enhancements

Potential additions:
- Level-based stat bonuses
- Skill point system
- Talent trees
- Seasonal progression
- Battle pass integration
- Daily/weekly challenges with XP rewards
