# Quest System

Dynamic quest system with objectives, rewards, and progression tracking for MechDefenseHalo.

## Features

- ✅ Quest creation and registration system
- ✅ Multi-objective quest support
- ✅ Progress tracking and state management
- ✅ Reward distribution (Credits, XP, Items)
- ✅ Quest UI for viewing and managing quests
- ✅ Active quest tracker HUD component
- ✅ Event-driven architecture for quest updates
- ⏳ Persistent progress (SaveManager integration pending)

## Architecture

### Core Components

1. **Quest.cs** - Core quest data model
   - Stores quest information (ID, name, description)
   - Contains objectives list and rewards
   - Tracks quest status (NotStarted, Active, Completed, Failed)
   - Provides progress calculation methods

2. **QuestObjective.cs** - Individual objective tracking
   - Tracks objective description and completion requirements
   - Manages current progress vs required count
   - Calculates objective completion percentage

3. **QuestRewards.cs** - Reward structure
   - Defines Credits, Experience, and Item rewards
   - Simple data container for quest completion rewards

4. **QuestManager.cs** - Main quest management system (Singleton)
   - Registers and stores all available quests
   - Handles quest activation, progression, and completion
   - Integrates with CurrencyManager and PlayerLevel for rewards
   - Emits events via EventBus for quest state changes
   - Manages active quests list

5. **QuestTracker.cs** - Active quest HUD component
   - Displays active quests on screen
   - Shows real-time objective progress
   - Automatically updates on quest events
   - Configurable display settings

6. **QuestUI.cs** - Full quest management UI
   - Displays available, active, and completed quests
   - Shows detailed quest information
   - Allows players to start new quests
   - Tracks quest selection and filtering

## Usage

### Setting Up QuestManager

Add QuestManager as an autoload singleton or add it to your scene tree:

```gdscript
# In Godot project settings, add as autoload:
# Name: QuestManager
# Path: res://Scripts/Quests/QuestManager.cs
```

Or in your game initialization:

```csharp
var questManager = new QuestManager();
AddChild(questManager);
```

### Creating Custom Quests

Edit the `RegisterQuests()` method in `QuestManager.cs`:

```csharp
private void RegisterQuests()
{
    AddQuest(new Quest
    {
        Id = "my_custom_quest",
        Name = "My Custom Quest",
        Description = "Complete this epic quest!",
        Objectives = new List<QuestObjective>
        {
            new QuestObjective { 
                Description = "Defeat 50 enemies", 
                RequiredCount = 50 
            },
            new QuestObjective { 
                Description = "Reach wave 5", 
                RequiredCount = 1 
            }
        },
        Rewards = new QuestRewards
        {
            Credits = 1000,
            Experience = 250,
            Items = new List<string> { "epic_weapon", "health_pack" }
        }
    });
}
```

### Starting a Quest

```csharp
// Start a quest by ID
QuestManager.Instance.StartQuest("tutorial_quest");
```

### Updating Quest Progress

```csharp
// Update specific objective (questId, objectiveIndex, progressAmount)
QuestManager.Instance.UpdateObjective("tutorial_quest", 0, 1);

// Example: When enemy is killed
QuestManager.Instance.UpdateObjective("tutorial_quest", 0, 1); // +1 to "Kill 5 enemies"

// Example: When wave is completed
QuestManager.Instance.UpdateObjective("tutorial_quest", 1, 1); // +1 to "Survive 3 waves"
```

### Quest Completion

Quests automatically complete when all objectives are met. You can also manually complete or fail quests:

```csharp
// Manual completion
QuestManager.Instance.CompleteQuest("tutorial_quest");

// Manual failure
QuestManager.Instance.FailQuest("tutorial_quest");
```

### Adding Quest UI to Scene

1. Create a Control node in your scene
2. Attach `QuestUI.cs` script
3. Configure the exported properties or let it auto-generate UI
4. Show/hide as needed:

```csharp
questUI.ShowQuestUI();
questUI.HideQuestUI();
```

### Adding Quest Tracker HUD

1. Create a Control node in your HUD
2. Attach `QuestTracker.cs` script
3. Configure display settings via exports:
   - MaxDisplayedQuests: Maximum number of active quests to show
   - ShowObjectiveProgress: Whether to show objective details

The tracker automatically updates when quests change.

## Event System Integration

The Quest System emits the following events via EventBus:

- `quest_started` - When a quest is activated
- `quest_completed` - When a quest is successfully completed
- `quest_failed` - When a quest fails
- `quest_objective_completed` - When an individual objective is completed

### Listening to Quest Events

```csharp
EventBus.Instance.On("quest_completed", (data) => {
    if (data is QuestEventData questData)
    {
        GD.Print($"Quest completed: {questData.QuestName}");
    }
});
```

## Integration with Other Systems

### Currency System
Quests automatically grant Credits via `CurrencyManager.AddCredits()` when completed.

### Progression System
Quests automatically grant Experience via `PlayerLevel.AddXP()` when completed.

### Save System
Quest progress persistence is prepared for integration with SaveManager (currently logged but not saved).

### Item System
Item rewards are tracked but require inventory system integration to deliver items to the player.

## Examples

### Tutorial Quest
The system includes a pre-registered tutorial quest:
- **ID**: tutorial_quest
- **Objectives**: Kill 5 enemies, Survive 3 waves
- **Rewards**: 500 Credits, 100 XP

### Veteran Quest
A more challenging quest example:
- **ID**: veteran_quest
- **Objectives**: Reach wave 10, Kill 100 enemies
- **Rewards**: 2000 Credits, 500 XP, rare_weapon_token item

## Testing

Unit tests are provided in `Tests/Quests/QuestManagerTests.cs`:

```bash
# Run tests via Godot
godot --headless --run-tests --quit
```

Tests cover:
- Quest registration and retrieval
- Quest state transitions
- Objective progress tracking
- Reward distribution
- Event emission
- Progress calculations

## Future Enhancements

- [ ] Save/Load quest progress via SaveManager
- [ ] Item reward delivery via Inventory system
- [ ] Quest prerequisites and chains
- [ ] Time-limited quests
- [ ] Repeatable daily/weekly quests
- [ ] Quest categories and filtering
- [ ] Quest notifications and popups
- [ ] Quest rewards preview before starting
- [ ] Quest difficulty ratings
- [ ] Dynamic quest generation

## API Reference

### QuestManager

**Public Methods:**
- `StartQuest(string questId)` - Start a quest
- `UpdateObjective(string questId, int objectiveIndex, int progress)` - Update objective progress
- `CompleteQuest(string questId)` - Complete a quest
- `FailQuest(string questId)` - Fail a quest
- `GetQuest(string questId)` - Get quest by ID
- `GetQuestsByStatus(QuestStatus status)` - Get quests by status

**Public Properties:**
- `IReadOnlyList<Quest> ActiveQuests` - List of currently active quests
- `IReadOnlyDictionary<string, Quest> AllQuests` - All registered quests

### Quest

**Properties:**
- `string Id` - Unique quest identifier
- `string Name` - Display name
- `string Description` - Quest description
- `List<QuestObjective> Objectives` - Quest objectives
- `QuestRewards Rewards` - Quest rewards
- `QuestStatus Status` - Current quest status

**Methods:**
- `float GetProgress()` - Get overall quest progress (0.0 to 1.0)
- `bool AreAllObjectivesCompleted()` - Check if all objectives are complete

### QuestObjective

**Properties:**
- `string Description` - Objective description
- `int RequiredCount` - Required count to complete
- `int CurrentCount` - Current progress count
- `bool IsCompleted` - Whether objective is completed

**Methods:**
- `float GetProgress()` - Get objective progress (0.0 to 1.0)

## License

Part of the MechDefenseHalo project.
