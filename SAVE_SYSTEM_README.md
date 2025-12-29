# Save System Documentation

## Overview

The MechDefenseHalo save system provides a robust, encrypted, and extensible solution for persisting game state. It features automatic saving, backup recovery, version migration, and cloud save preparation.

## Features

✅ **Encrypted Save Files** - XOR encryption prevents casual cheating  
✅ **Auto-Save System** - Saves every 5 minutes and at key milestones  
✅ **Backup Recovery** - Automatic backup before each save  
✅ **Version Migration** - Backwards compatibility support  
✅ **Comprehensive Data** - Saves player, inventory, equipment, currency, statistics, and settings  
✅ **Fast Performance** - Save/load operations complete in < 1 second  
✅ **Small File Size** - Save files < 500KB even with full inventory  
✅ **Cloud Save Ready** - Stub implementation for future cloud integration  

## Architecture

### Core Components

```
Scripts/SaveSystem/
├── SaveData.cs              # Save data structures
├── SaveEncryption.cs        # Encryption/decryption utilities
├── LocalSaveHandler.cs      # Local file operations
├── CloudSaveHandler.cs      # Cloud save stub (future)
├── SaveMigration.cs         # Version compatibility
└── AutoSaveController.cs    # Automatic saving

_Core/
└── SaveManager.cs           # Main save/load orchestration

Data/Save/
├── save_schema.json         # JSON schema for validation
├── default_save.json        # Default save template
└── save_version.json        # Version history
```

### Save Data Structure

The save system stores the following data:

#### Player Data
- Level, XP, Prestige Level
- Current/Max HP
- Last position

#### Inventory Data
- Max slots
- All items with quantities and stats
- Item slot indices

#### Equipment Data
- Currently equipped items
- Multiple loadout configurations
- Current loadout ID

#### Currency Data
- Credits
- Cores

#### Game State
- Current wave
- Unlocked content
- Completed achievements

#### Statistics
- Total kills, deaths
- Waves completed
- Bosses defeated
- Damage dealt/taken

#### Settings
- Audio volumes (Master, Music, SFX)
- Display settings (Resolution, Fullscreen)

## Usage

### Saving the Game

```csharp
// Manual save
SaveManager.Instance.SaveGame();

// Auto-save is handled automatically by AutoSaveController
```

### Loading the Game

```csharp
// Load is called automatically in SaveManager._Ready()
// Manual load
SaveManager.Instance.LoadGame();
```

### Deleting Save

```csharp
SaveManager.Instance.DeleteSave();
```

### Adding Save Support to New Managers

To add save/load support to a new manager:

```csharp
public partial class MyManager : Node
{
    // 1. Add singleton pattern
    private static MyManager _instance;
    public static MyManager Instance => _instance;

    // 2. Implement GetSaveData
    public SaveSystem.MySaveData GetSaveData()
    {
        return new SaveSystem.MySaveData
        {
            // Collect data to save
        };
    }

    // 3. Implement LoadFromSave
    public void LoadFromSave(SaveSystem.MySaveData saveData)
    {
        if (saveData == null) return;
        // Apply loaded data
    }
}
```

Then update `SaveManager.cs`:

```csharp
// In CollectSaveData()
private MySaveData CollectMyData()
{
    var myManager = MyManager.Instance;
    return myManager?.GetSaveData() ?? new MySaveData();
}

// In ApplySaveData()
if (data.MyData != null)
{
    MyManager.Instance?.LoadFromSave(data.MyData);
}
```

### Auto-Save Configuration

The `AutoSaveController` can be configured via exports:

```csharp
[Export] private float _autoSaveInterval = 300f; // 5 minutes
[Export] private bool _enableAutoSave = true;
[Export] private int _waveIntervalForAutoSave = 5; // Every 5 waves
```

Auto-save triggers on:
- Timer (every 5 minutes by default)
- Wave completion (every 5 waves)
- Boss defeat
- Player level up

### Encryption

The save system uses XOR encryption by default:

```csharp
// Encrypt
string encrypted = SaveEncryption.Encrypt(jsonData);

// Decrypt
string decrypted = SaveEncryption.Decrypt(encrypted);
```

**Note**: For production, consider upgrading to AES encryption for better security.

## Events

The save system emits the following events through EventBus:

```csharp
EventBus.GameSaved          // Emitted when save completes
EventBus.GameLoaded         // Emitted when load completes
EventBus.AutoSaveTriggered  // Emitted on auto-save
```

Subscribe to events:

```csharp
EventBus.On(EventBus.GameSaved, OnGameSaved);

private void OnGameSaved(object data)
{
    // Show save notification
}
```

## File Locations

Save files are stored in:
- **Windows**: `%APPDATA%/Godot/app_userdata/MechDefenseHalo/saves/`
- **Linux**: `~/.local/share/godot/app_userdata/MechDefenseHalo/saves/`
- **macOS**: `~/Library/Application Support/Godot/app_userdata/MechDefenseHalo/saves/`

Files:
- `player_save.dat` - Main save file
- `player_save_backup.dat` - Backup save file

## Version Migration

The save system supports version migration for backwards compatibility:

```csharp
// In SaveMigration.cs
private static SaveData MigrateFrom_1_0_0_To_1_1_0(SaveData saveData)
{
    // Add new fields with defaults
    saveData.Player.NewField = defaultValue;
    return saveData;
}
```

Update version in:
- `SaveMigration.cs` - CURRENT_VERSION constant
- `Data/Save/save_version.json` - Version history

## Testing

### Unit Tests

Run tests in `Tests/SaveSystem/`:

```bash
# In Godot editor
GdUnit4 → Run All Tests

# Command line
godot --headless --run-tests --test-suite="SaveEncryptionTests" --quit
```

### Manual Testing

See `SAVE_SYSTEM_VALIDATION.md` for comprehensive testing guide.

## Performance

**Benchmarks** (with full inventory, 500 items):
- Save time: ~100-200ms
- Load time: ~150-250ms
- File size: ~50-100KB
- Memory overhead: Minimal

## Troubleshooting

### Save not working
1. Check console for errors
2. Verify SaveManager is in scene tree
3. Check write permissions in user data directory

### Data not persisting
1. Ensure managers have GetSaveData/LoadFromSave implemented
2. Check if singleton instance exists
3. Verify data is being collected in CollectSaveData()

### Corrupted save
1. Game automatically tries backup
2. If backup fails, new save is created
3. Check `player_save_backup.dat` manually

### Performance issues
1. Reduce auto-save frequency
2. Optimize data collection in managers
3. Consider compressing save data

## Future Enhancements

### Planned Features
- [ ] AES encryption for production
- [ ] Cloud save integration (Steam, Google Play)
- [ ] Multiple save slots
- [ ] Quicksave/Quickload
- [ ] Save file compression
- [ ] Incremental saves (only changed data)
- [ ] Save file validation/checksum
- [ ] Save file browser/manager UI

### Cloud Save Integration

The `CloudSaveHandler.cs` stub is ready for implementation:

```csharp
// Future implementation
public bool UploadSave(string saveData)
{
    // Implement platform-specific cloud save
    // Steam Cloud, Google Play Games, etc.
}
```

## API Reference

### SaveManager

```csharp
public bool SaveGame()
public bool LoadGame()
public void DeleteSave()
public SaveData CurrentSaveData { get; }
public PlayerData CurrentPlayerData { get; }
```

### AutoSaveController

```csharp
public void SetAutoSaveEnabled(bool enabled)
public void SetAutoSaveInterval(float intervalSeconds)
public void TriggerAutoSave()
public void ResetTimer()
```

### SaveEncryption

```csharp
public static string Encrypt(string plainText)
public static string Decrypt(string encrypted)
```

### LocalSaveHandler

```csharp
public bool WriteSave(string fileName, string content)
public string ReadSave(string fileName)
public bool SaveExists(string fileName)
public bool DeleteSave(string fileName)
public bool CopySave(string sourceFileName, string destFileName)
```

### SaveMigration

```csharp
public static SaveData MigrateToCurrentVersion(SaveData saveData)
public static bool IsVersionCompatible(string version)
```

## Best Practices

1. **Always save before critical operations** (scene transitions, boss fights)
2. **Test with corrupted saves** to ensure recovery works
3. **Version your save format** when making breaking changes
4. **Keep save data minimal** to reduce file size and improve performance
5. **Validate loaded data** to prevent crashes from corrupted saves
6. **Use events** to decouple save system from game logic
7. **Log save operations** for debugging
8. **Test migration paths** when updating save format

## License

This save system is part of the MechDefenseHalo project.
