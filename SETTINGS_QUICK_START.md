# Settings Menu Quick Start Guide

## Overview

This guide will help you quickly integrate the new settings menu into your Godot project.

## What Was Created

A complete settings menu system with:
- **6 C# components** for managing different settings categories
- **Comprehensive validation** to ensure safe values
- **Real-time preview** for audio settings
- **Full key rebinding** system
- **8 accessibility features** including colorblind modes
- **Complete documentation** and test suite

## Directory Structure

```
Scripts/UI/Settings/
├── SettingsMenu.cs              # Main controller
├── GraphicsSettings.cs          # Graphics tab
├── AudioSettings.cs             # Audio tab
├── ControlSettings.cs           # Controls tab
├── AccessibilitySettings.cs     # Accessibility tab
├── SettingsValidator.cs         # Validation utilities
└── README.md                    # Full documentation

Tests/Settings/
└── UISettingsTest.cs            # Test suite
```

## Quick Integration (5 Steps)

### Step 1: Verify Dependencies

Ensure these exist in your project:
- ✅ `SettingsManager` in autoload (should already exist)
- ✅ `EventBus` in autoload (should already exist)
- ✅ Audio buses: Master, Music, SFX, UI

### Step 2: Create the Main Settings Scene

1. Create new scene: `UI/SettingsMenu.tscn`
2. Set root as `Control` node
3. Attach script: `Scripts/UI/Settings/SettingsMenu.cs`
4. Add this structure:

```
Control (SettingsMenu)
├── ColorRect (Background)
│   └── VBoxContainer
│       ├── Label - "SETTINGS"
│       ├── TabContainer (name it "TabContainer")
│       │   ├── Control (Graphics)
│       │   ├── Control (Audio)
│       │   ├── Control (Controls)
│       │   └── Control (Accessibility)
│       └── HBoxContainer
│           ├── Button - "Apply"
│           ├── Button - "Cancel"
│           └── Button - "Reset to Defaults"
```

### Step 3: Setup Each Tab

For each tab (Graphics, Audio, Controls, Accessibility):

1. Select the Control node in TabContainer
2. Attach the corresponding script:
   - Graphics → `Scripts/UI/Settings/GraphicsSettings.cs`
   - Audio → `Scripts/UI/Settings/AudioSettings.cs`
   - Controls → `Scripts/UI/Settings/ControlSettings.cs`
   - Accessibility → `Scripts/UI/Settings/AccessibilitySettings.cs`

3. Add UI elements (see detailed layouts in `README.md`)

### Step 4: Wire Export Variables

In the SettingsMenu root node, wire these exports:
- `tabContainer` → the TabContainer node
- `graphicsSettings` → Graphics Control node
- `audioSettings` → Audio Control node
- `controlSettings` → Controls Control node
- `accessibilitySettings` → Accessibility Control node
- `applyButton` → Apply Button
- `cancelButton` → Cancel Button
- `defaultsButton` → Reset to Defaults Button

### Step 5: Add to Main Menu

In your main menu or pause menu:

```csharp
// Add a button to open settings
settingsButton.Pressed += () => {
    var settingsMenu = GetNode<SettingsMenu>("Path/To/SettingsMenu");
    settingsMenu.ShowSettings();
};
```

## Minimal Example Layouts

### Graphics Tab Minimum
```
VBoxContainer
├── HBoxContainer
│   ├── Label - "Resolution:"
│   └── OptionButton (export as resolutionOption)
├── HBoxContainer
│   ├── Label - "VSync:"
│   └── CheckButton (export as vsyncCheck)
└── HBoxContainer
    ├── Label - "Quality:"
    └── OptionButton (export as qualityPresetOption)
```

### Audio Tab Minimum
```
VBoxContainer
├── HBoxContainer
│   ├── Label - "Master Volume:"
│   ├── HSlider (export as masterVolumeSlider)
│   │   └── min: 0, max: 1, step: 0.01
│   └── Label (export as masterVolumeLabel)
└── HBoxContainer
    ├── Label - "Music Volume:"
    ├── HSlider (export as musicVolumeSlider)
    │   └── min: 0, max: 1, step: 0.01
    └── Label (export as musicVolumeLabel)
```

### Controls Tab Minimum
```
VBoxContainer
├── HBoxContainer
│   ├── Label - "Mouse Sensitivity:"
│   ├── HSlider (export as mouseSensitivitySlider)
│   │   └── min: 0.1, max: 5.0, step: 0.1
│   └── Label (export as mouseSensitivityLabel)
└── VBoxContainer (export as keyBindingContainer)
    └── (Key bindings created automatically)
```

### Accessibility Tab Minimum
```
VBoxContainer
├── HBoxContainer
│   ├── Label - "Colorblind Mode:"
│   └── OptionButton (export as colorblindModeOption)
├── HBoxContainer
│   ├── Label - "Subtitles:"
│   └── CheckButton (export as subtitlesCheck)
└── HBoxContainer
    ├── Label - "UI Scale:"
    ├── HSlider (export as uiScaleSlider)
    │   └── min: 0.5, max: 2.0, step: 0.1
    └── Label (export as uiScaleLabel)
```

## Testing

Run the test to verify everything works:

1. Add `Tests/Settings/UISettingsTest.cs` to a scene
2. Run the scene
3. Check console for test results:
   - ✅ All tests should pass
   - ✅ No error messages

## Troubleshooting

### "SettingsManager not available!"
- Ensure SettingsManager is added to autoload
- Check Project Settings → Autoload

### Audio preview not working
- Verify audio buses exist: Master, Music, SFX, UI
- Check Project Settings → Audio → Buses

### Key bindings not saving
- Ensure ControlSettingsApplier.DefaultKeyBindings is populated
- Check that InputMap has the required actions

### Export variables showing as null
- Verify all nodes are named correctly
- Check that node paths match export variable names
- Ensure scripts are attached to correct nodes

## Features Summary

### Graphics Settings
- Resolution, window mode, VSync
- FPS limiter, quality presets
- Render scale, shadows, effects

### Audio Settings
- 4 volume channels with real-time preview
- Master mute toggle

### Control Settings
- Key rebinding system
- Mouse/controller sensitivity
- Controller deadzone, Y-axis inversion

### Accessibility Settings
- 5 colorblind modes
- Subtitles, UI scaling
- High contrast, reduced motion
- Text size, auto-pause

## API Quick Reference

### Show Settings Menu
```csharp
settingsMenu.ShowSettings();
```

### Hide Settings Menu
```csharp
settingsMenu.HideSettings();
```

### Programmatic Settings Change
```csharp
var settings = SettingsManager.Instance.CurrentSettings;
settings.Graphics.VSync = true;
settings.Audio.MasterVolume = 0.8f;
SettingsManager.Instance.ApplyAllSettings();
SettingsManager.Instance.SaveSettings();
```

### Validate Settings
```csharp
using MechDefenseHalo.UI.Settings;

int validFPS = SettingsValidator.ClampFPS(300); // Returns 240
float validVolume = SettingsValidator.ClampVolume(1.5f); // Returns 1.0
```

## Performance Notes

- Settings save to disk only when Apply is clicked
- Audio preview happens in real-time (no Apply needed)
- Graphics changes may require scene reload for full effect
- Settings are loaded once on game startup

## Next Steps

1. ✅ Read full documentation in `Scripts/UI/Settings/README.md`
2. ✅ Review implementation details in `SETTINGS_MENU_IMPLEMENTATION.md`
3. ✅ Run tests in `Tests/Settings/UISettingsTest.cs`
4. ✅ Create your settings menu scenes
5. ✅ Customize UI styling to match your game

## Support

For detailed information:
- **Full Documentation**: `Scripts/UI/Settings/README.md`
- **Implementation Summary**: `SETTINGS_MENU_IMPLEMENTATION.md`
- **Component Code**: `Scripts/UI/Settings/*.cs`
- **Tests**: `Tests/Settings/UISettingsTest.cs`

## Success Checklist

Before considering integration complete:
- [ ] All export variables are wired in the editor
- [ ] Settings menu opens from main menu
- [ ] All tabs are accessible
- [ ] Changes persist after restarting game
- [ ] Apply button saves settings
- [ ] Cancel button discards changes
- [ ] Reset to Defaults works
- [ ] Audio preview works in real-time
- [ ] Key rebinding functions correctly
- [ ] No console errors when opening settings

---

**Implementation Status**: ✅ COMPLETE

All code is ready for integration. Only scene creation and UI styling remain.
