# Tutorial System Documentation

## Overview

The Interactive Tutorial System teaches players core gameplay mechanics through a 10-step guided experience with rewards. The system is fully integrated with the game's event system, save manager, and UI framework.

## Architecture

### Core Components

1. **TutorialManager.cs** - Main controller that orchestrates the tutorial flow
2. **TutorialStep.cs** - Data model for individual tutorial steps
3. **TutorialProgressTracker.cs** - Tracks objective completion via event listening
4. **TutorialDialog.cs** - UI component for displaying instructions
5. **TutorialHighlight.cs** - UI component for highlighting elements and keys
6. **TutorialSkipHandler.cs** - Handles skip logic and confirmations

### Data Files

- **Data/Tutorial/tutorial_steps.json** - Defines all 10 tutorial steps
- **Data/Tutorial/tutorial_rewards.json** - Defines completion and step rewards

## Integration

### Adding TutorialManager to Your Scene

Add the TutorialManager as a child node in your main game scene or GameManager:

```gdscript
# In your main scene tree
Main/
├── GameManager/
│   ├── TutorialManager  # Add this node
│   ├── SaveManager
│   ├── EventBus
│   └── ...
```

Or instantiate programmatically:

```csharp
var tutorialManager = new TutorialManager();
AddChild(tutorialManager);
```

### Starting the Tutorial

The tutorial automatically starts for first-time players. To manually start:

```csharp
var tutorialManager = GetNode<TutorialManager>("TutorialManager");
tutorialManager.StartTutorial();
```

### Restarting the Tutorial

Allow players to replay the tutorial from settings:

```csharp
tutorialManager.RestartTutorial();
```

## Tutorial Steps

The system includes 10 progressive steps:

1. **Movement** - Learn WASD controls (10 meters)
2. **Combat** - Fire weapon (10 shots)
3. **Enemy Elimination** - Destroy training dummies (3 enemies)
4. **Loot Collection** - Collect dropped items (3 items)
5. **Inventory** - Open inventory UI (Press I)
6. **Equipment** - Equip an item (1 item)
7. **Drone Deployment** - Deploy attack drone (Press F)
8. **Wave Defense** - Complete first wave
9. **Shop Access** - Open shop UI (Press P)
10. **Crafting** - Start a craft job (Press C)

## Objective Types

The system supports various objective types:

- `distance_moved` - Track player movement distance
- `shots_fired` - Count weapon fire events
- `enemy_kills` - Count enemy deaths
- `items_collected` - Track loot collection
- `ui_opened` - Detect UI panel opening
- `item_equipped` - Track equipment changes
- `drone_deployed` - Count drone deployments
- `wave_completed` - Check wave completion
- `craft_started` - Track crafting start

## Event Integration

The tutorial system listens to game events via EventBus:

```csharp
// Emit events in your game code to trigger progress
EventBus.Emit(EventBus.WeaponFired, null);
EventBus.Emit(EventBus.EntityDied, enemyType);
EventBus.Emit(EventBus.LootPickedUp, item);
EventBus.Emit("ui_opened", "inventory");
```

### Required Events

Make sure these events are emitted in your game code:

| Event | When to Emit | Data |
|-------|-------------|------|
| `player_moved` | Player moves | float distance |
| `WeaponFired` | Weapon fires | null |
| `EntityDied` | Enemy dies | string type |
| `LootPickedUp` | Item collected | Item object |
| `ui_opened` | UI panel opens | string panelName |
| `ItemEquipped` | Item equipped | Item object |
| `DroneDeployed` | Drone spawns | Drone object |
| `WaveCompleted` | Wave ends | int waveNumber |
| `CraftStarted` | Crafting begins | Craft object |

## Rewards System

### Completion Rewards

Granted when tutorial is completed:

- 500 Credits
- 50 Cores
- 200 Experience
- Starter weapon (Assault Rifle MK1)
- 5 Health Potions

### Step Rewards

Individual steps grant bonus credits:

- Step 1: 10 credits
- Step 3: 25 credits
- Step 5: 15 credits
- Step 7: 30 credits
- Step 8: 50 credits
- Step 10: 100 credits

## Skip Functionality

### Skippable Steps

Most steps can be skipped except critical ones:

- Step 1 (Movement) - Cannot skip
- Step 3 (Combat) - Cannot skip
- Step 7 (Drones) - Cannot skip
- Step 8 (Wave Defense) - Cannot skip

### Skip Entire Tutorial

Players can skip the entire tutorial at any time via confirmation dialog.

## Save Integration

Tutorial state is persisted via SaveManager:

```csharp
// Check if tutorial completed
bool completed = SaveManager.GetBool("tutorial_completed");

// Check if first launch
bool firstLaunch = SaveManager.GetBool("is_first_launch");

// Set tutorial completed
SaveManager.SetBool("tutorial_completed", true);
```

## UI Customization

### TutorialDialog Appearance

Edit `UI/Tutorial/TutorialDialog.tscn` to customize:

- Panel styling
- Font sizes
- Colors
- Button appearance

### TutorialHighlight Styling

Edit `UI/Tutorial/TutorialHighlight.tscn` to change:

- Overlay darkness
- Highlight frame color
- Key indicator design

### Completion Screen

Customize `UI/Tutorial/TutorialCompleteScreen.tscn` for:

- Celebration effects
- Reward display
- Continue button style

## API Reference

### TutorialManager

```csharp
// Properties
bool IsTutorialActive { get; }
bool IsTutorialComplete { get; }
int CurrentStepIndex { get; }

// Methods
void StartTutorial()
void StopTutorial()
void RestartTutorial()
void SkipCurrentStep()
void SkipTutorial()
```

### TutorialStep

```csharp
// Properties
int StepNumber
string Title
string Description
string ObjectiveType
object ObjectiveValue
int CurrentProgress
bool CanSkip

// Methods
bool IsObjectiveComplete()
float GetProgressPercentage()
string GetObjectiveText()
```

### TutorialProgressTracker

```csharp
// Properties
TutorialStep CurrentStep { get; }

// Methods
void StartTracking(TutorialStep step)
void StopTracking()
void SetProgress(int progress)
```

## Events Emitted

The tutorial system emits these events:

- `tutorial_started` - Tutorial begins
- `tutorial_stopped` - Tutorial stopped
- `tutorial_completed` - Tutorial finished
- `tutorial_step_started` - New step begins
- `tutorial_objective_complete` - Step objective met

### Listening to Tutorial Events

```csharp
EventBus.On("tutorial_started", (data) => {
    GD.Print("Tutorial started!");
});

EventBus.On("tutorial_step_started", (data) => {
    var stepData = (TutorialStepEventData)data;
    GD.Print($"Step {stepData.StepNumber}: {stepData.Title}");
});

EventBus.On("tutorial_completed", (data) => {
    GD.Print("Tutorial completed!");
});
```

## Testing

Run unit tests via GdUnit4:

```bash
# In Godot Editor: GdUnit4 > Run Tests
# Or select specific test: Tests/Tutorial/TutorialTests.cs
```

Tests cover:

- Step objective tracking
- Progress percentage calculation
- Skip functionality
- String vs int objectives
- Edge cases and null handling

## Troubleshooting

### Tutorial doesn't start automatically

Check that:
1. TutorialManager is in the scene tree
2. SaveManager is initialized
3. `IsFirstLaunch` is set correctly

### Objectives not progressing

Verify that:
1. Events are being emitted correctly
2. Event names match exactly
3. EventBus is initialized

### UI not showing

Ensure:
1. TutorialDialog and TutorialHighlight are created
2. Scenes are properly loaded
3. UI layer settings allow visibility

### Rewards not granted

Check that:
1. CurrencyManager is initialized
2. InventoryManager is available
3. Reward data JSON is valid

## Best Practices

1. **Test Events** - Verify all game events are properly emitted
2. **Customize Steps** - Adjust tutorial_steps.json for your game
3. **Balance Rewards** - Tune rewards to match game economy
4. **UI Feedback** - Add visual/audio feedback for milestones
5. **Save Often** - Tutorial progress is saved automatically
6. **Accessibility** - Ensure all steps are achievable by new players

## Future Enhancements

Potential additions:

- Voice-over support
- Animated arrows/pointers
- Mini-map integration
- Achievement unlocks
- Tutorial analytics
- Multiple tutorial tracks
- Advanced player skip (with cheat codes)
