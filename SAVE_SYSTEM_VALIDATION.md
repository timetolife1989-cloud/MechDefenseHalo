# Save System Validation Guide

This document provides a comprehensive guide to validate the save system implementation.

## Quick Validation Checklist

### 1. Save/Load Cycle Testing ✓
**Test Steps:**
1. Launch the game in Godot editor
2. Add some currency (credits/cores) using debug commands
3. Add items to inventory
4. Call `SaveManager.Instance.SaveGame()` from debug console
5. Restart the game
6. Verify currency, inventory, and equipment are restored

**Expected Result:**
- Save file created in `user://saves/player_save.dat`
- All data persists across game sessions
- No data loss occurs

### 2. Backup Creation Testing ✓
**Test Steps:**
1. Create a save file
2. Call `SaveGame()` again
3. Check `user://saves/` directory for backup file

**Expected Result:**
- Backup file `player_save_backup.dat` exists
- Backup contains previous save data
- Backup is updated on each save

### 3. Encryption Testing ✓
**Test Steps:**
1. Save the game
2. Open `player_save.dat` in text editor
3. Verify content is encrypted (not readable)
4. Verify the game can decrypt and load the save

**Expected Result:**
- Save file content is Base64 encoded
- Content is not human-readable
- Game successfully decrypts and loads data

### 4. Corrupted Save Recovery ✓
**Test Steps:**
1. Create a valid save
2. Manually corrupt the save file (add invalid characters)
3. Try to load the game

**Expected Result:**
- Game detects corrupted save
- Automatically attempts to load backup
- If backup fails, creates new save
- No crash or data loss

### 5. Auto-Save Testing ✓
**Test Steps:**
1. Add AutoSaveController to the game scene
2. Play the game for 5+ minutes
3. Complete a wave (milestone)
4. Level up (milestone)
5. Defeat a boss (milestone)

**Expected Result:**
- Auto-save triggers every 5 minutes
- Auto-save triggers on wave 5, 10, 15, etc.
- Auto-save triggers on level up
- Auto-save triggers on boss defeat
- Notification appears on auto-save

### 6. Save File Size Verification ✓
**Test Steps:**
1. Fill inventory with ~500 items
2. Equip multiple items
3. Complete several waves
4. Save the game
5. Check file size

**Expected Result:**
- Save file size < 500KB
- Performance is acceptable

### 7. Save/Load Performance ✓
**Test Steps:**
1. Time the save operation
2. Time the load operation
3. Test with full inventory

**Expected Result:**
- Save time < 1 second
- Load time < 1 second
- No noticeable lag

### 8. Version Migration Testing ✓
**Test Steps:**
1. Create a save with version "0.9.0"
2. Update SaveMigration to handle this version
3. Load the save

**Expected Result:**
- Save is migrated to current version
- All data is preserved
- No errors occur

## Unit Test Validation

Run all unit tests in `Tests/SaveSystem/`:

```bash
# In Godot editor
GdUnit4 → Run All Tests

# Or from command line
godot --headless --run-tests --test-suite="SaveEncryptionTests" --quit
godot --headless --run-tests --test-suite="LocalSaveHandlerTests" --quit
godot --headless --run-tests --test-suite="SaveMigrationTests" --quit
godot --headless --run-tests --test-suite="SaveManagerTests" --quit
```

**Expected Result:**
- All tests pass
- No errors in console
- Coverage > 80%

## Integration Testing

### Complete Game Session Test
1. Start new game
2. Play for 10 minutes
3. Earn currency, collect items, complete waves
4. Save manually
5. Close game
6. Reopen game
7. Verify all progress restored

### Multiple Save/Load Cycles
1. Save game 10 times in a row
2. Load game after each save
3. Verify data consistency
4. Check for memory leaks

### Stress Testing
1. Save with 500 items in inventory
2. Save with all equipment slots filled
3. Save with max currency values
4. Verify performance remains acceptable

## Manual Debug Commands

Add these debug commands for testing:

```csharp
// In game console or debug panel
SaveManager.Instance.SaveGame()
SaveManager.Instance.LoadGame()
SaveManager.Instance.DeleteSave()
CurrencyManager.SetCredits(10000)
CurrencyManager.SetCores(500)
```

## Success Criteria Summary

- ✅ All unit tests pass
- ✅ Save/load cycle works correctly
- ✅ Backup system functions
- ✅ Encryption prevents casual editing
- ✅ Corrupted save recovery works
- ✅ Auto-save triggers correctly
- ✅ Save file size < 500KB
- ✅ Save/load time < 1 second
- ✅ Version migration works

## Known Limitations

1. **Item Restoration**: Current implementation logs items to restore but doesn't fully restore them. This requires an item database/registry system to be implemented.

2. **Player Position**: Player position is saved but not yet applied on load. This requires integration with the player controller.

3. **Statistics**: Statistics tracking requires a StatisticsManager to be implemented.

4. **Achievements**: Achievement tracking requires an AchievementManager to be implemented.

5. **Wave State**: Wave state requires integration with WaveManager.

## Next Steps

To fully complete the save system:

1. Implement item database/registry for proper item restoration
2. Implement StatisticsManager with GetSaveData/LoadFromSave
3. Implement AchievementManager with tracking
4. Implement UnlockManager for content unlocks
5. Add player position restoration in player controller
6. Add settings persistence (graphics, audio, controls)
7. Consider upgrading XOR encryption to AES for production
8. Add cloud save integration (Steam, Google Play, etc.)

## Troubleshooting

### Save file not created
- Check `OS.GetUserDataDir() + "/saves/"` directory exists
- Verify write permissions
- Check console for error messages

### Save file corrupted
- Check for disk space
- Verify file isn't opened by another process
- Check backup file exists

### Items not restoring
- This is expected - requires item database implementation
- Check console for "Should restore item" messages

### Auto-save not working
- Verify AutoSaveController is added to scene tree
- Check if auto-save is enabled
- Verify event subscriptions are working
