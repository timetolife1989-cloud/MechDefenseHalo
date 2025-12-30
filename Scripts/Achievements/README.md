# Achievement System

## Overview

The Achievement System tracks player accomplishments across 50+ achievements in 5 categories:
- **Combat** (15 achievements) - Combat-related feats
- **Collection** (10 achievements) - Item and collection goals
- **Progression** (10 achievements) - Level and wave milestones
- **Boss** (10 achievements) - Boss battle achievements
- **Secret** (5 achievements) - Hidden achievements

## Architecture

### Core Components

1. **Achievement.cs** - Data model for individual achievements
   - Tracks progress, completion status, and rewards
   - Handles display logic for secret achievements
   - Provides completion percentage calculations

2. **AchievementManager.cs** - Singleton manager (autoloaded)
   - Loads achievement data from JSON files
   - Tracks achievement progress
   - Handles unlocking and reward granting
   - Saves/loads progress via SaveManager
   - Event-driven updates via EventBus

3. **AchievementTracker.cs** - Event listener
   - Subscribes to game events from EventBus
   - Automatically updates achievement progress
   - Tracks special conditions (no damage, time limits, etc.)

4. **AchievementNotification.cs** - Toast notification UI
   - Displays popup when achievements unlock
   - Shows achievement name and rewards
   - Plays sound effects and animations

5. **AchievementUI.cs** - Full achievement screen
   - Lists all achievements by category
   - Shows progress bars for incomplete achievements
   - Hides secret achievements until unlocked
   - Displays completion percentage
   - Toggle with 'A' key

6. **PlatformIntegration.cs** - Platform-specific hooks
   - Auto-detects platform (Steam, Google Play, etc.)
   - Syncs achievements to external platforms
   - Maps internal IDs to platform-specific IDs
   - Supports Steam, Google Play, Game Center, Console platforms

## Setup

### Autoload Configuration

The following nodes are autoloaded in `project.godot`:
```
SaveManager="*res://_Core/SaveManager.cs"
AchievementManager="*res://Scripts/Achievements/AchievementManager.cs"
PlatformIntegration="*res://Scripts/Achievements/PlatformIntegration.cs"  # Optional
```

### Scene Setup

1. **AchievementTracker** - Add to main game scene
2. **AchievementNotification** - Add to HUD/UI layer
3. **AchievementUI** - Add to UI/Menu system

## Usage

### Tracking Events

The AchievementTracker automatically listens to EventBus events:

```csharp
// Kill an enemy - automatically tracked
EventBus.Emit(EventBus.EntityDied, enemyData);

// Wave completed - automatically tracked
EventBus.Emit(EventBus.WaveCompleted, waveNumber);

// Boss defeated - automatically tracked
EventBus.Emit(EventBus.BossDefeated, bossName);
```

### Level and Wave Milestone Tracking

Level-based achievements (Novice, Expert, Master) and wave milestones (Wave Breaker, Endgame) use threshold checking instead of incremental progress:

```csharp
// Check level achievements when player levels up
AchievementManager.Instance.CheckLevelAchievements(playerLevel);

// Check wave achievements when wave is completed
AchievementManager.Instance.CheckWaveAchievements(waveNumber);
```

### Manual Achievement Unlocking

For special conditions not covered by events:

```csharp
// Unlock achievement directly
AchievementManager.Instance.UnlockAchievement("secret_achievement_id");

// Track custom event
AchievementManager.Instance.TrackEvent("custom_event_type", 1);
```

### Checking Achievement Status

```csharp
// Get specific achievement
var achievement = AchievementManager.Instance.GetAchievement("first_blood");

// Get completion stats
int total = AchievementManager.Instance.TotalAchievements;
int completed = AchievementManager.Instance.CompletedAchievements;
float percentage = AchievementManager.Instance.CompletionPercentage;

// Get achievements by category
var combatAchievements = AchievementManager.Instance.GetAchievementsByCategory("Combat");
```

## Achievement Data Format

Achievements are defined in JSON files in `Data/Achievements/`:

```json
{
  "ID": "first_blood",
  "Name": "First Blood",
  "Description": "Kill your first enemy",
  "RequiredProgress": 1,
  "Rewards": {
    "reward_xp": 50,
    "reward_credits": 100
  }
}
```

### Reward Types
- `reward_xp` - Experience points
- `reward_credits` - In-game currency (Credits)
- `reward_cores` - Premium currency (Cores)
- `reward_legendary` - Legendary items

## Event Mapping

| Event Type | Tracked Achievements |
|------------|---------------------|
| `enemy_kill` | First Blood, Soldier, Warrior, Genocide |
| `weak_point_hit` | Headhunter |
| `wave_no_damage` | Untouchable |
| `wave_completed` | Veteran (total count) |
| `boss_defeated` | Colossus Killer, Boss Hunter (incremental) |
| `legendary_obtained` | Legendary Hunter |
| `set_completed` | Full Set |
| `drone_unlocked` | Drone Commander |

### Boss Achievement Requirements

Boss achievements require specific data in the EventBus event payload:

```csharp
// Example boss defeated event with full data
var bossData = new Godot.Collections.Dictionary
{
    ["boss_name"] = "Frost Titan",
    ["boss_id"] = "frost_titan",  // Unique ID for boss_rush tracking
    ["all_weak_points"] = true,   // All weak points destroyed
    ["drones_used"] = false       // No drones used (for solo_boss)
};
EventBus.Emit(EventBus.BossDefeated, bossData);
```

Special boss achievements:
- **Titan Slayer**: Frost Titan specifically
- **Flawless Victory**: No player deaths during boss fight
- **Speed Run**: Boss defeated in < 3 minutes
- **Quick Reflexes**: Boss defeated in < 1 minute
- **Boss No Hit**: No damage taken during boss fight
- **All Weak Points**: All weak points destroyed
- **Solo Boss**: No drones used during boss fight
- **Boss Rush**: 5 different bosses in one session
- **Colossus Killer**: 10 total boss defeats
- **Boss Hunter**: 50 total boss defeats

## Persistence

Achievement progress is automatically saved to `user://save_data.json` via the SaveManager. Progress includes:
- Current progress value
- Completion status
- Unlock timestamp

## UI Features

### Achievement Screen
- Filter by category (All, Combat, Collection, etc.)
- Progress bars for incomplete achievements
- Secret achievements shown as "???" until unlocked
- Completion percentage display
- Visual distinction for completed achievements (gold text, checkmark)

### Toast Notification
- Appears when achievement unlocked
- Shows achievement name and icon
- Lists rewards granted
- Plays unlock sound effect
- Auto-dismisses after 3 seconds

## Platform Integration

### Overview

The achievement system supports integration with external platforms:
- **Steam** (via GodotSteam plugin)
- **Google Play Games** (via Google Play Games plugin)
- **Apple Game Center** (via Game Center plugin)
- **Console Platforms** (Xbox, PlayStation, Nintendo)

### How It Works

1. **Auto-Detection**: Platform is automatically detected on startup
2. **ID Mapping**: Internal achievement IDs are mapped to platform-specific IDs
3. **Auto-Sync**: Achievements sync automatically when unlocked
4. **Progress Updates**: Incremental achievements sync progress to platforms that support it

### Setup for Steam

1. Install GodotSteam plugin for Godot 4.x
2. Configure Steam App ID
3. Update achievement ID mappings in `PlatformIntegration.cs`
4. Add PlatformIntegration to autoload in `project.godot`

### Setup for Google Play

1. Install Google Play Games plugin for Godot
2. Configure Google Play Console achievement IDs
3. Update achievement ID mappings in `PlatformIntegration.cs`
4. Add PlatformIntegration to autoload in `project.godot`

### Manual Sync

To manually sync all achievements (useful after offline play):

```csharp
AchievementManager.Instance.SyncAllToPlatform();
```

### Testing Without Plugins

PlatformIntegration works without plugins installed - it will log "[HOOK]" messages showing what would be synced to the platform, allowing you to test integration logic without actual platform credentials.

## Testing

Unit tests are available in `Tests/Achievements/`:
- `AchievementTests.cs` - Tests for Achievement model
- `AchievementManagerTests.cs` - Tests for AchievementManager

Run tests using GdUnit4 framework in Godot editor.

## Future Enhancements

Potential additions:
- Achievement point system
- Tiered achievements (Bronze/Silver/Gold)
- Daily/Weekly challenges
- Achievement rewards preview
- Social sharing
- Rarity tiers for achievements
- Daily/Weekly challenges
- Achievement rewards preview
- Social sharing
- Rarity tiers for achievements
