# Settings Menu UI Components

Complete settings menu implementation with graphics, audio, controls, and accessibility options.

## Overview

This directory contains UI components for a comprehensive settings menu system that integrates with the existing `MechDefenseHalo.Settings` infrastructure.

## Architecture

### Components

#### SettingsMenu.cs
Main controller that coordinates all settings tabs and manages the overall menu flow.

**Responsibilities:**
- Coordinate between settings tabs
- Handle Apply/Cancel/Defaults buttons
- Integrate with SettingsManager for persistence
- Show/hide menu functionality

**Export Variables:**
- `TabContainer tabContainer` - Container for settings tabs
- `GraphicsSettings graphicsSettings` - Graphics settings tab reference
- `AudioSettings audioSettings` - Audio settings tab reference
- `ControlSettings controlSettings` - Controls settings tab reference
- `AccessibilitySettings accessibilitySettings` - Accessibility settings tab reference
- `Button applyButton` - Apply button
- `Button cancelButton` - Cancel button
- `Button defaultsButton` - Reset to defaults button

#### GraphicsSettings.cs
Manages graphics and display settings including resolution, quality presets, and visual effects.

**Features:**
- Resolution selection
- Window mode (Fullscreen/Windowed)
- VSync toggle
- FPS limiter
- Quality presets (Low/Medium/High/Ultra)
- Render scale
- Visual effects (Bloom, Motion Blur)
- Shadow quality

#### AudioSettings.cs
Manages audio volume controls with real-time preview.

**Features:**
- Master volume control
- Music volume control
- SFX volume control
- UI volume control
- Master mute toggle
- Real-time audio preview

#### ControlSettings.cs
Manages input settings including key bindings and sensitivity.

**Features:**
- Mouse sensitivity
- Controller sensitivity
- Controller deadzone
- Invert Y-axis toggle
- Key rebinding system
- Visual feedback for key binding

#### AccessibilitySettings.cs
**NEW** - Manages accessibility features for improved user experience.

**Features:**
- Colorblind modes (Protanopia, Deuteranopia, Tritanopia, Monochromacy)
- Subtitles toggle
- UI scaling
- High contrast mode
- Screen reader support
- Reduced motion
- Text size adjustment
- Auto-pause on focus loss

#### SettingsValidator.cs
Static utility class for validating and clamping settings values.

**Validation Features:**
- Resolution validation (1280x720 minimum, 8K maximum)
- FPS clamping (30-240, or 0 for unlimited)
- Volume clamping (0.0-1.0)
- Sensitivity clamping (0.1-5.0)
- Key binding validation
- UI/Text scale validation

## Integration with Existing Systems

### SettingsManager
All settings components integrate with the existing `MechDefenseHalo.Settings.SettingsManager` singleton:

```csharp
var settingsManager = MechDefenseHalo.Settings.SettingsManager.Instance;
var graphics = settingsManager.CurrentSettings.Graphics;
```

### Settings Appliers
Settings are applied through existing applier classes:
- `GraphicsSettingsApplier.Apply(graphics)`
- `AudioSettingsApplier.Apply(audio)`
- `ControlSettingsApplier.Apply(controls)`

### Persistence
Settings are persisted through SettingsManager:
```csharp
settingsManager.SaveSettings(); // Saves to user://settings.cfg
```

## Usage

### Creating the Settings Menu Scene

1. **Create Main Settings Control**
   ```
   Control (SettingsMenu.tscn)
   ├─ Script: Scripts/UI/Settings/SettingsMenu.cs
   └─ ColorRect (Background)
      └─ VBoxContainer (Container)
         ├─ Label (Title) - "SETTINGS"
         ├─ TabContainer (Tabs)
         │  ├─ Control (Graphics)
         │  │  └─ Script: Scripts/UI/Settings/GraphicsSettings.cs
         │  ├─ Control (Audio)
         │  │  └─ Script: Scripts/UI/Settings/AudioSettings.cs
         │  ├─ Control (Controls)
         │  │  └─ Script: Scripts/UI/Settings/ControlSettings.cs
         │  └─ Control (Accessibility)
         │     └─ Script: Scripts/UI/Settings/AccessibilitySettings.cs
         └─ HBoxContainer (Buttons)
            ├─ Button (Apply)
            ├─ Button (Cancel)
            └─ Button (Defaults)
   ```

2. **Wire Export Variables**
   - Assign TabContainer to SettingsMenu
   - Assign each tab Control to SettingsMenu
   - Assign buttons to SettingsMenu

3. **Create UI Elements in Each Tab**
   - See detailed layout below for each tab

### GraphicsSettings Tab Layout

```
VBoxContainer
├─ Label - "Display"
├─ HBoxContainer
│  ├─ Label - "Resolution:"
│  └─ OptionButton (resolutionOption)
├─ HBoxContainer
│  ├─ Label - "Window Mode:"
│  └─ OptionButton (windowModeOption)
├─ HBoxContainer
│  ├─ Label - "VSync:"
│  └─ CheckButton (vsyncCheck)
├─ Separator
├─ Label - "Performance"
├─ HBoxContainer
│  ├─ Label - "FPS Limit:"
│  ├─ HSlider (fpsLimitSlider) - min: 0, max: 240, step: 10
│  └─ Label (fpsLimitLabel)
├─ HBoxContainer
│  ├─ Label - "Quality Preset:"
│  └─ OptionButton (qualityPresetOption)
├─ HBoxContainer
│  ├─ Label - "Render Scale:"
│  ├─ HSlider (renderScaleSlider) - min: 0.5, max: 2.0, step: 0.1
│  └─ Label (renderScaleLabel)
├─ Separator
├─ Label - "Effects"
├─ HBoxContainer
│  ├─ Label - "Shadow Quality:"
│  ├─ HSlider (shadowQualitySlider) - min: 0, max: 3, step: 1
│  └─ Label (shadowQualityLabel)
├─ HBoxContainer
│  ├─ Label - "Bloom:"
│  └─ CheckButton (bloomCheck)
└─ HBoxContainer
   ├─ Label - "Motion Blur:"
   └─ CheckButton (motionBlurCheck)
```

### AudioSettings Tab Layout

```
VBoxContainer
├─ Label - "Volume Levels"
├─ HBoxContainer
│  ├─ Label - "Master:"
│  ├─ HSlider (masterVolumeSlider) - min: 0, max: 1, step: 0.01
│  ├─ Label (masterVolumeLabel)
│  └─ CheckButton (muteMasterCheck)
├─ HBoxContainer
│  ├─ Label - "Music:"
│  ├─ HSlider (musicVolumeSlider) - min: 0, max: 1, step: 0.01
│  └─ Label (musicVolumeLabel)
├─ HBoxContainer
│  ├─ Label - "SFX:"
│  ├─ HSlider (sfxVolumeSlider) - min: 0, max: 1, step: 0.01
│  └─ Label (sfxVolumeLabel)
└─ HBoxContainer
   ├─ Label - "UI:"
   ├─ HSlider (uiVolumeSlider) - min: 0, max: 1, step: 0.01
   └─ Label (uiVolumeLabel)
```

### ControlSettings Tab Layout

```
VBoxContainer
├─ Label - "Mouse"
├─ HBoxContainer
│  ├─ Label - "Mouse Sensitivity:"
│  ├─ HSlider (mouseSensitivitySlider) - min: 0.1, max: 5.0, step: 0.1
│  └─ Label (mouseSensitivityLabel)
├─ HBoxContainer
│  ├─ Label - "Invert Y-Axis:"
│  └─ CheckButton (invertYCheck)
├─ Separator
├─ Label - "Controller"
├─ HBoxContainer
│  ├─ Label - "Controller Sensitivity:"
│  ├─ HSlider (controllerSensitivitySlider) - min: 0.1, max: 5.0, step: 0.1
│  └─ Label (controllerSensitivityLabel)
├─ HBoxContainer
│  ├─ Label - "Deadzone:"
│  ├─ HSlider (controllerDeadzoneSlider) - min: 0, max: 50, step: 1
│  └─ Label (controllerDeadzoneLabel)
├─ Separator
├─ Label - "Key Bindings"
└─ ScrollContainer
   └─ VBoxContainer (keyBindingContainer)
      └─ (Key binding rows created dynamically)
```

### AccessibilitySettings Tab Layout

```
VBoxContainer
├─ Label - "Visual Accessibility"
├─ HBoxContainer
│  ├─ Label - "Colorblind Mode:"
│  └─ OptionButton (colorblindModeOption)
├─ HBoxContainer
│  ├─ Label - "High Contrast:"
│  └─ CheckButton (highContrastCheck)
├─ HBoxContainer
│  ├─ Label - "UI Scale:"
│  ├─ HSlider (uiScaleSlider) - min: 0.5, max: 2.0, step: 0.1
│  └─ Label (uiScaleLabel)
├─ HBoxContainer
│  ├─ Label - "Text Size:"
│  ├─ HSlider (textSizeSlider) - min: 0.5, max: 2.0, step: 0.1
│  └─ Label (textSizeLabel)
├─ Separator
├─ Label - "Audio/Video Accessibility"
├─ HBoxContainer
│  ├─ Label - "Subtitles:"
│  └─ CheckButton (subtitlesCheck)
├─ HBoxContainer
│  ├─ Label - "Reduced Motion:"
│  └─ CheckButton (reducedMotionCheck)
├─ Separator
├─ Label - "Other"
├─ HBoxContainer
│  ├─ Label - "Screen Reader:"
│  └─ CheckButton (screenReaderCheck)
└─ HBoxContainer
   ├─ Label - "Auto Pause on Focus Loss:"
   └─ CheckButton (autoPauseCheck)
```

## Code Examples

### Showing the Settings Menu

```csharp
// From anywhere in your code
var settingsMenu = GetNode<SettingsMenu>("Path/To/SettingsMenu");
settingsMenu.ShowSettings();
```

### Programmatically Changing Settings

```csharp
var settingsManager = SettingsManager.Instance;

// Change graphics settings
settingsManager.CurrentSettings.Graphics.VSync = true;
settingsManager.CurrentSettings.Graphics.QualityLevel = QualityPreset.High;

// Change audio settings
settingsManager.CurrentSettings.Audio.MasterVolume = 0.8f;

// Apply and save
settingsManager.ApplyAllSettings();
settingsManager.SaveSettings();
```

### Validating Settings

```csharp
using MechDefenseHalo.UI.Settings;

// Validate individual values
int validFPS = SettingsValidator.ClampFPS(300); // Returns 240
float validVolume = SettingsValidator.ClampVolume(1.5f); // Returns 1.0

// Validate entire settings objects
var validatedGraphics = SettingsValidator.ValidateGraphicsSettings(graphics);
var validatedAudio = SettingsValidator.ValidateAudioSettings(audio);
```

## Success Criteria Checklist

- ✅ **Graphics Settings**: Resolution, VSync, quality presets, render scale, effects
- ✅ **Audio Settings**: Master, Music, SFX, UI volumes with real-time preview
- ✅ **Control Settings**: Key rebinding, mouse/controller sensitivity, invert Y
- ✅ **Accessibility**: Colorblind modes, subtitles, UI scaling, high contrast, reduced motion
- ✅ **Apply/Cancel/Defaults Buttons**: Full functionality implemented
- ✅ **Settings Validation**: Comprehensive validation for all settings
- ✅ **Persistent Storage**: Integration with SettingsManager for saving to disk

## Features

### Real-time Preview
- Audio sliders provide immediate feedback
- Graphics changes can be previewed before applying
- Key binding shows immediate visual feedback

### Validation
- All numeric values are clamped to safe ranges
- Key bindings prevent system keys
- Resolution validates minimum/maximum sizes

### User-Friendly
- Clear labels and descriptions
- Percentage display for sliders
- Visual feedback for interactions
- Default reset functionality

### Extensible
- Easy to add new settings options
- Modular component design
- Clear separation of concerns

## Integration Notes

### Required Dependencies
- `MechDefenseHalo.Settings.SettingsManager` - Must be in autoload
- `MechDefenseHalo.Settings.*SettingsData` classes
- `MechDefenseHalo.Settings.*SettingsApplier` classes
- `MechDefenseHalo.Core.EventBus` - Optional for events

### Optional Enhancements
- Add confirmation dialog for "Reset to Defaults"
- Add "Apply on change" option for immediate feedback
- Add preset saving/loading for custom configurations
- Add import/export settings functionality
- Add settings profiles for multiple users

## Testing

To test the settings system:

1. Create the settings menu scene with all required nodes
2. Wire all export variables
3. Add SettingsMenu to your main menu or pause menu
4. Run the game and open settings
5. Test each tab:
   - Change values
   - Click Apply to save
   - Click Cancel to revert
   - Click Defaults to reset
6. Restart the game to verify persistence

## Troubleshooting

### Settings not saving
- Ensure SettingsManager is in autoload
- Check console for save errors
- Verify write permissions to user data directory

### Audio preview not working
- Check that audio buses exist in project settings
- Verify bus names match: "Master", "Music", "SFX", "UI"

### Key binding not working
- Ensure InputMap actions exist
- Check ControlSettingsApplier.DefaultKeyBindings
- Verify key codes are valid

### UI elements not connected
- Double-check all export variables are assigned
- Ensure node paths are correct
- Check console for "not available" errors

## Future Enhancements

Potential additions to the settings system:
- Graphics presets based on hardware detection
- Audio device selection
- Controller remapping UI
- Custom colorblind shader implementation
- Text-to-speech for screen reader
- Multiple language support
- Cloud settings sync
- Settings backup/restore
- Advanced graphics options (anti-aliasing, anisotropic filtering)
- Separate subtitle customization (size, color, background)

## License

Part of the MechDefenseHalo project.
