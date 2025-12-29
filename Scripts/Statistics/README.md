# Statistics Tracker System

## Overview

The Statistics Tracker System provides comprehensive tracking of combat, economy, and session metrics with persistent storage. All statistics are automatically tracked through the EventBus system and saved periodically.

## Components

### Core Classes

#### `CombatStats.cs`
Tracks all combat-related statistics:
- **Kills**: Total kills, kills by enemy type, kills by weapon
- **Damage**: Total damage dealt/taken, weak point hits, accuracy
- **Weapons**: Shots fired, shots hit, accuracy percentage
- **Drones**: Drones deployed, drone kills
- **Records**: Highest wave reached, fastest boss kill, longest kill streak

#### `EconomyStats.cs`
Tracks all economy-related statistics:
- **Currency**: Credits/cores earned and spent
- **Items**: Items looted, crafted, salvaged, organized by rarity
- **Shop**: Purchase count, total spent
- **Loot**: Chests opened, legendaries obtained

#### `SessionStats.cs`
Tracks session and playtime statistics:
- **Time**: Total playtime, session count, longest session
- **Daily**: Login streak tracking
- **Current Session**: Real-time session statistics

#### `StatisticsManager.cs`
Central manager that:
- Listens to EventBus events
- Updates statistics in real-time
- Auto-saves every 60 seconds
- Provides stat summaries and exports

#### `StatisticsSaveHandler.cs`
Handles persistence:
- JSON serialization/deserialization
- Export to JSON and CSV formats
- Version management

#### `StatisticsUI.cs`
UI component that:
- Displays statistics in organized tabs
- Updates in real-time
- Supports manual refresh and export

## Usage

### Accessing Statistics

The StatisticsManager is available as an autoload singleton:

```csharp
// Get current statistics
var combat = StatisticsManager.Instance.Combat;
var economy = StatisticsManager.Instance.Economy;
var session = StatisticsManager.Instance.Session;

// Example: Get total kills
int totalKills = combat.TotalKills;

// Example: Get playtime in hours
float hours = session.TotalPlaytimeSeconds / 3600f;
```

### Getting Stat Summary

```csharp
var summary = StatisticsManager.Instance.GetStatSummary();
// Returns dictionary with key stats
foreach (var kvp in summary)
{
    GD.Print($"{kvp.Key}: {kvp.Value}");
}
```

### Manual Save

```csharp
// Force save (normally auto-saves every 60 seconds)
StatisticsManager.Instance.SaveStatistics();
```

### Exporting Statistics

```csharp
// Export as JSON
StatisticsManager.Instance.ExportStatistics("json");

// Export as CSV
StatisticsManager.Instance.ExportStatistics("csv");
```

## Events Tracked

The system automatically tracks these events:

### Combat Events
- `EventBus.EntityDied` - Enemy kills
- `EventBus.PlayerDied` - Player deaths
- `EventBus.HealthChanged` - Damage dealt/taken
- `EventBus.WeaponFired` - Shots fired
- `EventBus.BossSpawned` - Boss spawned
- `EventBus.BossDefeated` - Boss defeated
- `EventBus.WaveCompleted` - Wave completed
- `EventBus.DroneDeployed` - Drone deployed

### Economy Events
- `EventBus.CurrencyChanged` - Currency earned/spent
- `EventBus.ItemPurchased` - Shop purchases
- `EventBus.LootPickedUp` - Items looted
- `EventBus.CraftCompleted` - Items crafted
- `"item_salvaged"` - Items salvaged
- `"chest_opened"` - Chests opened

## UI Display

The Statistics UI is available at `res://UI/Statistics.tscn` and includes:

### Combat Tab
- Total Kills
- Bosses Defeated
- Accuracy Percentage
- Damage Dealt
- Highest Wave
- Death Count
- Longest Kill Streak

### Economy Tab
- Credits Earned/Spent
- Items Looted
- Legendaries Obtained
- Items Crafted
- Chests Opened

### Session Tab
- Total Playtime
- Total Sessions
- Login Streak
- First Played Date
- Current Session Time

### Records Tab
- Fastest Boss Kill
- Longest Kill Streak
- Drones Deployed

## Data Storage

### File Locations

Statistics are stored in the Godot user data directory:

- **Lifetime Stats**: `user://lifetime_stats.json`
- **Session Stats**: `user://session_stats.json`
- **Exports**: `user://statistics_export_[timestamp].[format]`

### Data Format

Example JSON structure:

```json
{
  "version": 1,
  "last_updated": "2025-12-29T13:00:00Z",
  "combat": {
    "TotalKills": 1234,
    "KillsByEnemyType": {
      "Grunt": 543,
      "Shooter": 234
    },
    "BossesDefeated": 12
  },
  "economy": {
    "TotalCreditsEarned": 45600,
    "ItemsLooted": 567
  },
  "session": {
    "TotalPlaytimeSeconds": 85470.0,
    "TotalSessions": 56
  }
}
```

## Auto-Save System

The system automatically saves:
- Every 60 seconds
- On significant events (boss defeat)
- When exiting the game

## Performance

The statistics system is designed for minimal performance impact:
- Event-driven updates (no polling)
- Efficient dictionary lookups
- Background saving
- Lazy loading

## Extending the System

### Adding New Statistics

1. Add property to relevant stats class:
```csharp
public class CombatStats
{
    public int MyNewStat { get; set; } = 0;
}
```

2. Add event handler in StatisticsManager:
```csharp
private void OnMyEvent(object data)
{
    Combat.MyNewStat++;
}
```

3. Connect event in `ConnectEventHandlers()`:
```csharp
EventBus.On("my_event", OnMyEvent);
```

4. Add UI label in Statistics.tscn

5. Update display in StatisticsUI.cs:
```csharp
private void UpdateCombatTab()
{
    // ... existing code ...
    if (_myNewStatLabel != null)
        _myNewStatLabel.Text = $"My New Stat: {combat.MyNewStat}";
}
```

## Testing

Run tests with:
```bash
godot --headless --run-tests --test-suite="StatisticsManagerTests" --quit
```

Test coverage includes:
- CombatStats methods
- EconomyStats methods
- SessionStats methods
- Save/Load functionality
- Export functionality

## Troubleshooting

### Statistics not updating
- Verify StatisticsManager is in autoload
- Check EventBus is emitting events
- Enable debug logs in StatisticsManager

### Save file corrupted
- Delete `user://lifetime_stats.json`
- System will create new file with defaults

### UI not displaying
- Verify Statistics.tscn path is correct
- Check StatisticsUI.cs is attached to root node
- Call RefreshDisplay() after showing UI

## Future Enhancements

Potential improvements:
- [ ] Achievement system integration
- [ ] Leaderboard support
- [ ] Detailed weapon statistics per weapon type
- [ ] Enemy type breakdown charts
- [ ] Session comparison over time
- [ ] Statistics reset functionality
- [ ] Cloud backup integration

## See Also

- [EventBus Documentation](../../_Core/EventBus.cs)
- [Testing Guide](../../TESTING.md)
- [Save System](../../_Core/SaveManager.cs)
