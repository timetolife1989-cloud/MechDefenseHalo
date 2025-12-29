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

## Setup

### Autoload Configuration

The following nodes are autoloaded in `project.godot`:
```
SaveManager="*res://_Core/SaveManager.cs"
AchievementManager="*res://Scripts/Achievements/AchievementManager.cs"
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
| `wave_completed` | Wave Breaker, Endgame |
| `boss_defeated` | Boss achievements |
| `legendary_obtained` | Legendary Hunter |
| `set_completed` | Full Set |
| `drone_unlocked` | Drone Commander |

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

## Testing

Unit tests are available in `Tests/Achievements/`:
- `AchievementTests.cs` - Tests for Achievement model
- `AchievementManagerTests.cs` - Tests for AchievementManager

Run tests using GdUnit4 framework in Godot editor.

## Future Enhancements

Potential additions:
- Steam/Console achievement integration
- Achievement point system
- Tiered achievements (Bronze/Silver/Gold)
- Daily/Weekly challenges
- Achievement rewards preview
- Social sharing
- Rarity tiers for achievements
