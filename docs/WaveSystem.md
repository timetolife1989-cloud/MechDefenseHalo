# Wave Spawner System

## Overview

The Wave Spawner System is a comprehensive wave-based enemy spawning system with difficulty scaling, boss waves, and dynamic spawn patterns for MechDefenseHalo.

## Features

- **50 Unique Waves**: Progressive difficulty from tutorial to endgame
- **Boss Waves**: Every 10th wave (10, 20, 30, 40, 50) features a boss with support enemies
- **Difficulty Scaling**: Dynamic HP, damage, and enemy count scaling
- **Spawn Patterns**: Circle, Line, Surround, and Random spawn formations
- **Wave Breaks**: 30-second breaks between waves
- **Rewards System**: Credits and XP rewards for completing waves
- **Elite Waves**: Waves 31-50 feature elite enemies with increased stats
- **UI Integration**: Real-time wave progress, enemy count, and timer display

## Architecture

### Core Components

#### WaveManager (`Scripts/WaveSystem/WaveManager.cs`)
- Main controller for wave system
- Manages wave progression and completion
- Integrates with EventBus for game events
- Handles enemy spawning and tracking
- Applies difficulty scaling to enemies

#### DifficultyScaler (`Scripts/WaveSystem/DifficultyScaler.cs`)
- Static utility class for scaling calculations
- **HP Scaling**: Linear (waves 1-10), then exponential (+15% per wave)
- **Damage Scaling**: No scaling until wave 11, then +10% per wave
- **Count Scaling**: +10% enemies per 5 waves
- **Elite Waves**: 2x HP, 1.5x damage for waves 31-50

#### SpawnPoint (`Scripts/WaveSystem/SpawnPoint.cs`)
- Marker for enemy spawn locations
- Supports multiple spawn patterns:
  - **Circle**: Enemies in circular formation
  - **Line**: Linear formation
  - **Surround**: Encircle player
  - **Random**: Random positions within radius

#### BossWaveController (`Scripts/WaveSystem/BossWaveController.cs`)
- Handles boss wave spawning
- Manages support enemies during boss fights
- Triggers boss music
- Emits boss-specific events

#### WaveDefinition (`Scripts/WaveSystem/WaveDefinition.cs`)
- Data structures for wave configuration
- Supports spawn groups with delays
- Boss wave definitions with support enemies

#### WaveUI (`Scripts/WaveSystem/WaveUI.cs`)
- UI controller for wave information
- Displays wave number, enemy count, progress bar
- Shows break timer between waves
- Wave start animations

## Data Files

### wave_definitions.json
Location: `Data/Waves/wave_definitions.json`

Contains definitions for all 50 waves:
- **Waves 1-10**: Tutorial waves with basic enemies
- **Waves 11-30**: Progression with mixed compositions
- **Waves 31-50**: Endgame with elite variants

Example:
```json
{
  "wave_5": {
    "wave_number": 5,
    "is_boss_wave": false,
    "spawn_groups": [
      {
        "enemy_type": "Grunt",
        "count": 15,
        "delay": 1.2,
        "spawn_pattern": "Circle"
      },
      {
        "enemy_type": "Shooter",
        "count": 5,
        "delay": 2.0,
        "spawn_pattern": "Line"
      }
    ]
  }
}
```

### boss_waves.json
Location: `Data/Waves/boss_waves.json`

Defines boss waves (10, 20, 30, 40, 50):
- Boss type and stats
- Support enemy composition
- Music tracks
- Rewards

### spawn_patterns.json
Location: `Data/Waves/spawn_patterns.json`

Documentation for spawn pattern types and their properties.

## Usage

### Setup in Scene

1. Add `WaveManager` node to your game scene
2. Create spawn points:
   ```
   WaveManager
   └── SpawnPoints
       ├── SpawnPoint1
       ├── SpawnPoint2
       └── SpawnPoint3
   ```
3. Configure WaveManager properties:
   - `SpawnPointsPath`: Path to SpawnPoints node
   - `WaveBreakTimer`: Seconds between waves (default: 30)
   - `AutoStartFirstWave`: Auto-start on ready (default: false)
   - `WaveDefinitionsPath`: Path to JSON file

4. Add WaveUI to your HUD:
   ```
   HUD
   └── WaveUI (instance of WaveUI.tscn)
   ```

### Manual Wave Control

```csharp
// Get WaveManager reference
var waveManager = GetNode<WaveManager>("WaveManager");

// Start next wave manually
waveManager.StartNextWave();

// Check wave status
int currentWave = waveManager.CurrentWave;
bool isActive = waveManager.IsWaveActive;
int enemiesLeft = waveManager.EnemiesRemaining;
```

### Event Integration

Listen to wave events:

```csharp
// Wave started
EventBus.On(EventBus.WaveStarted, (data) => {
    var waveData = data as WaveStartedEventData;
    GD.Print($"Wave {waveData.WaveNumber} started!");
});

// Wave completed
EventBus.On(EventBus.WaveCompleted, (data) => {
    var waveData = data as WaveCompletedEventData;
    GD.Print($"Wave completed! Credits: {waveData.CreditsReward}");
});

// Boss spawned
EventBus.On(EventBus.BossSpawned, (data) => {
    var bossData = data as BossSpawnedEventData;
    GD.Print($"Boss {bossData.BossName} spawned!");
});
```

## Difficulty Progression

### Waves 1-10 (Tutorial)
- Linear HP scaling: +50 HP per wave
- No damage scaling
- Basic enemy types (Grunt, Shooter, Swarm)
- Wave 10: First boss (Frost Titan)

### Waves 11-30 (Progression)
- Exponential HP scaling: +15% per wave after wave 10
- Damage scaling: +10% per wave after wave 10
- All enemy types introduced
- Wave 20: Second boss (Inferno Colossus)
- Wave 30: Third boss (Void Wraith)

### Waves 31-50 (Endgame)
- Elite enemy variants: 2x HP, 1.5x damage
- Continued exponential scaling
- Maximum enemy variety and count
- Wave 40: Fourth boss (Storm Lord)
- Wave 50: Final boss (Chaos Bringer)

## Rewards

### Credits
- Formula: `waveNumber * 50`
- Wave 1: 50 credits
- Wave 10: 500 credits
- Wave 50: 2,500 credits

### Experience Points
- Formula: `waveNumber * 100`
- Wave 1: 100 XP
- Wave 10: 1,000 XP
- Wave 50: 5,000 XP

### Boss Wave Bonuses
Boss waves provide additional rewards defined in `boss_waves.json`:
- Wave 10: 1,000 credits, 2,000 XP
- Wave 20: 2,000 credits, 4,000 XP
- Wave 30: 3,000 credits, 6,000 XP
- Wave 40: 4,000 credits, 8,000 XP
- Wave 50: 5,000 credits, 10,000 XP

## Testing

Unit tests are available in `Tests/WaveSystem/`:
- `DifficultyScalerTests.cs`: Tests for scaling calculations
- `SpawnPointTests.cs`: Tests for spawn pattern logic

Run tests using GdUnit4 in Godot editor or command line:
```bash
godot --headless --run-tests --quit
```

## Boss Types

### Wave 10: Frost Titan
- Ice-based attacks
- Freezing aura
- Weak to fire

### Wave 20: Inferno Colossus (Placeholder)
- Fire-based attacks
- High damage output
- Weak to ice

### Wave 30: Void Wraith (Placeholder)
- Dimensional phasing
- Dark magic attacks
- Weak to light

### Wave 40: Storm Lord (Placeholder)
- Lightning attacks
- High mobility
- Weak to earth

### Wave 50: Chaos Bringer (Placeholder)
- Ultimate challenge
- All elemental attacks
- Multiple phases

## Customization

### Adding New Waves

Edit `Data/Waves/wave_definitions.json`:
```json
{
  "wave_51": {
    "wave_number": 51,
    "is_boss_wave": false,
    "spawn_groups": [
      {
        "enemy_type": "YourNewEnemy",
        "count": 10,
        "delay": 1.0,
        "spawn_pattern": "Circle"
      }
    ]
  }
}
```

### Adjusting Difficulty

Modify scaling constants in `DifficultyScaler.cs`:
```csharp
// Change HP scaling rate
float multiplier = 1 + (waveNumber - 10) * 0.20f; // Increased from 0.15

// Change damage scaling rate
float multiplier = 1 + (waveNumber - 10) * 0.15f; // Increased from 0.10
```

### Custom Spawn Patterns

Add new pattern to `SpawnPoint.cs`:
```csharp
public enum SpawnPattern
{
    Circle,
    Line,
    Surround,
    Random,
    YourNewPattern // Add here
}

// Implement in GetSpawnPosition method
case SpawnPattern.YourNewPattern:
    return YourCustomPatternMethod(center, index, total);
```

## Performance Considerations

- Wave definitions are loaded once at startup
- Enemies are spawned with delays to prevent frame spikes
- Dead enemies are cleaned up automatically
- JSON parsing only happens during initialization

## Future Enhancements

- [ ] Complete boss implementations for waves 20, 30, 40, 50
- [ ] Add wave modifiers (speed boost, health boost, etc.)
- [ ] Implement special events (meteor showers, environmental hazards)
- [ ] Add endless mode after wave 50
- [ ] Player XP/level system integration
- [ ] Achievement system for wave milestones
- [ ] Leaderboards for fastest wave clears
- [ ] Daily/weekly wave challenges

## Troubleshooting

### Enemies Not Spawning
- Check spawn points are properly set up
- Verify `SpawnPointsPath` is correct
- Ensure wave definitions JSON is loading properly
- Check console for error messages

### Wave Not Progressing
- Verify all enemies are being destroyed
- Check `EnemiesRemaining` count in debugger
- Ensure enemy death events are being handled

### Boss Not Appearing on Wave 10
- Check boss wave definition in JSON
- Verify `BossWaveController` is attached
- Check console for boss spawning errors

## Support

For issues or questions, refer to:
- Project documentation in `docs/`
- Test files in `Tests/WaveSystem/`
- Example usage in game scenes
