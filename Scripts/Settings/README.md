# Settings System

Comprehensive game settings system for graphics, audio, controls, and gameplay with persistent storage.

## Overview

The settings system provides:
- **Graphics Settings**: Resolution, VSync, quality presets, shadows, particles, bloom, motion blur
- **Audio Settings**: Master, Music, SFX, and UI volume controls with real-time preview
- **Control Settings**: Key rebinding, mouse/controller sensitivity, Y-axis inversion
- **Gameplay Settings**: Auto-pickup, damage numbers, screen shake, FPS counter, language

## Architecture

### Core Components

1. **SettingsManager** (Singleton, Autoload)
   - Manages all settings persistence
   - Loads/saves from `user://settings.cfg`
   - Coordinates between data and appliers

2. **Data Structures**
   - `GameSettings`: Main container
   - `GraphicsSettingsData`: Graphics options
   - `AudioSettingsData`: Audio options
   - `ControlSettingsData`: Control options with key bindings
   - `GameplaySettingsData`: Gameplay options

3. **Appliers** (Static classes)
   - `GraphicsSettingsApplier`: Applies graphics to engine
   - `AudioSettingsApplier`: Applies audio to AudioServer
   - `ControlSettingsApplier`: Applies controls to InputMap
   - `GameplaySettingsApplier`: Stores gameplay preferences

4. **UI Scripts**
   - `SettingsMenuUI`: Main settings window
   - `GraphicsTabUI`: Graphics tab
   - `AudioTabUI`: Audio tab with real-time preview
   - `ControlsTabUI`: Controls tab with key rebinding
   - `GameplayTabUI`: Gameplay tab

## Usage

### Accessing Settings Manager

```csharp
using MechDefenseHalo.Settings;

// Get current settings
var settings = SettingsManager.Instance.CurrentSettings;

// Access specific settings
bool autoPickup = settings.Gameplay.AutoPickupItems;
float mouseSens = settings.Controls.MouseSensitivity;
```

### Modifying Settings Programmatically

```csharp
// Modify settings
SettingsManager.Instance.CurrentSettings.Graphics.Fullscreen = false;
SettingsManager.Instance.CurrentSettings.Audio.MasterVolume = 0.5f;

// Apply changes
SettingsManager.Instance.ApplyAllSettings();

// Save to disk
SettingsManager.Instance.SaveSettings();
```

### Reset to Defaults

```csharp
SettingsManager.Instance.ResetToDefaults();
```

### Accessing Runtime Values

Some settings are stored as static properties for quick access:

```csharp
// Controls
float sensitivity = ControlSettingsApplier.MouseSensitivity;
bool invertY = ControlSettingsApplier.InvertY;

// Gameplay
bool showDamage = GameplaySettingsApplier.ShowDamageNumbers;
bool screenShake = GameplaySettingsApplier.ScreenShake;
float shakeIntensity = GameplaySettingsApplier.ScreenShakeIntensity;
```

## Events

The settings system emits events through EventBus:

- `settings_saved`: When settings are saved
- `settings_applied`: When settings are applied
- `settings_reset`: When settings are reset to defaults

Listen for events:

```csharp
EventBus.On("settings_saved", (data) => {
    GD.Print("Settings have been saved!");
});
```

## Creating UI Scenes

The UI scripts are ready, but scenes need to be created in the Godot editor. See the documentation comments in each UI script for the required scene structure.

### Main Settings Menu Structure

```
Control (SettingsMenu)
├─ Panel (Background)
│  ├─ Label (Title) - text: "SETTINGS"
│  ├─ TabContainer (SettingsTabs)
│  │  ├─ GraphicsTab (Graphics) - Instance GraphicsTab.tscn
│  │  ├─ AudioTab (Audio) - Instance AudioTab.tscn
│  │  ├─ ControlsTab (Controls) - Instance ControlsTab.tscn
│  │  └─ GameplayTab (Gameplay) - Instance GameplayTab.tscn
│  └─ HBoxContainer (ButtonContainer)
│     ├─ Button (ApplyButton)
│     ├─ Button (ResetButton)
│     └─ Button (CloseButton)
```

## Configuration Files

### default_settings.json
Default settings values loaded on first run.

### quality_presets.json
Predefined quality levels (Low, Medium, High, Ultra) with associated graphics settings.

## File Locations

### Scripts
- `Scripts/Settings/*.cs` - Core settings classes
- `Scripts/UI/*TabUI.cs` - UI tab scripts
- `Scripts/UI/SettingsMenuUI.cs` - Main menu script

### Data
- `Data/Settings/default_settings.json` - Default settings
- `Data/Settings/quality_presets.json` - Quality presets

### User Data
- `user://settings.cfg` - User's saved settings (automatically created)

## Quality Presets

The system includes 4 quality presets:

1. **Low**: Best performance, lowest visual quality
2. **Medium**: Balanced performance and visuals
3. **High**: High visual quality, good performance
4. **Ultra**: Maximum visual quality

## Key Binding System

The control settings include a flexible key binding system that supports:
- Keyboard keys
- Mouse buttons
- Default bindings restoration
- Real-time rebinding in UI

Default bindings:
- WASD: Movement
- Space: Jump
- Mouse1: Fire
- R: Reload
- E: Ability
- Q: Shield
- Tab: Weapon Switch
- 1,2,3: Weapon Selection

## Integration Examples

### In Player Controller

```csharp
// Apply mouse sensitivity
float mouseDelta = Input.GetLastMouseVelocity().X * ControlSettingsApplier.MouseSensitivity;

// Check Y-axis inversion
float lookY = ControlSettingsApplier.InvertY ? -mouseDelta.Y : mouseDelta.Y;
```

### In Damage System

```csharp
if (GameplaySettingsApplier.ShowDamageNumbers)
{
    ShowFloatingDamageNumber(damage);
}
```

### In Camera Shake

```csharp
if (GameplaySettingsApplier.ScreenShake)
{
    float intensity = GameplaySettingsApplier.ScreenShakeIntensity;
    ApplyCameraShake(baseShake * intensity);
}
```

## Testing

To test the settings system:

1. Add SettingsManager to autoload (already done in project.godot)
2. Run the game
3. Settings will be auto-initialized with defaults
4. Check `user://settings.cfg` for saved settings
5. Modify settings programmatically or through UI
6. Verify persistence by restarting the game

## Notes

- Settings are automatically loaded on startup
- Changes can be applied in real-time for preview
- Audio sliders provide immediate feedback
- Graphics changes may require scene reload for full effect
- Settings persist across game sessions
- Invalid settings are validated and corrected on load
