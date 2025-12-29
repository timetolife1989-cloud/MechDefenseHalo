# Settings UI Scene Creation Guide

This guide explains how to create the UI scenes for the settings system in Godot.

## Overview

The settings system requires 5 scene files:
1. **SettingsMenu.tscn** - Main settings window
2. **GraphicsTab.tscn** - Graphics settings tab
3. **AudioTab.tscn** - Audio settings tab
4. **ControlsTab.tscn** - Controls settings tab
5. **GameplayTab.tscn** - Gameplay settings tab

## Location

All scene files should be created in: `UI/Settings/`

## 1. SettingsMenu.tscn

### Node Structure
```
Control (SettingsMenu) - Script: Scripts/UI/SettingsMenuUI.cs
├─ Panel (Background)
│  ├─ Label (Title)
│  ├─ TabContainer (SettingsTabs)
│  │  ├─ Node (Graphics) - Instance: GraphicsTab.tscn
│  │  ├─ Node (Audio) - Instance: AudioTab.tscn
│  │  ├─ Node (Controls) - Instance: ControlsTab.tscn
│  │  └─ Node (Gameplay) - Instance: GameplayTab.tscn
│  └─ HBoxContainer (ButtonContainer)
│     ├─ Button (ApplyButton)
│     ├─ Button (ResetButton)
│     └─ Button (CloseButton)
└─ ConfirmationDialog (ResetConfirmDialog)
```

### Export Variable Connections
- Connect `SettingsTabs` → TabContainer node
- Connect `ApplyButton` → Apply button
- Connect `ResetButton` → Reset button
- Connect `CloseButton` → Close button
- Connect `ResetConfirmDialog` → ConfirmationDialog node

### Properties
- **Root Control**: anchor_right=1.0, anchor_bottom=1.0
- **Panel**: Custom minimum size (800x600)
- **Title Label**: text="SETTINGS", align=center, large font
- **ApplyButton**: text="Apply"
- **ResetButton**: text="Reset to Default"
- **CloseButton**: text="Close"

## 2. GraphicsTab.tscn

### Node Structure
```
Control (GraphicsTab) - Script: Scripts/UI/GraphicsTabUI.cs
├─ ScrollContainer
│  └─ VBoxContainer
│     ├─ HBoxContainer (ResolutionRow)
│     │  ├─ Label - text: "Resolution"
│     │  └─ OptionButton (ResolutionDropdown)
│     ├─ HBoxContainer (FullscreenRow)
│     │  ├─ Label - text: "Fullscreen"
│     │  └─ CheckBox (FullscreenCheckbox)
│     ├─ HBoxContainer (VSyncRow)
│     │  ├─ Label - text: "VSync"
│     │  └─ CheckBox (VSyncCheckbox)
│     ├─ HBoxContainer (QualityRow)
│     │  ├─ Label - text: "Quality Preset"
│     │  └─ OptionButton (QualityPresetDropdown)
│     ├─ HBoxContainer (RenderScaleRow)
│     │  ├─ Label - text: "Render Scale"
│     │  ├─ HSlider (RenderScaleSlider)
│     │  └─ Label (RenderScaleLabel) - text: "1.00x"
│     ├─ HBoxContainer (FPSRow)
│     │  ├─ Label - text: "Target FPS"
│     │  └─ SpinBox (TargetFPSSpinBox)
│     ├─ HBoxContainer (ShadowRow)
│     │  ├─ Label - text: "Shadow Quality"
│     │  ├─ HSlider (ShadowQualitySlider)
│     │  └─ Label (ShadowQualityLabel) - text: "Medium"
│     ├─ HBoxContainer (BloomRow)
│     │  ├─ Label - text: "Bloom"
│     │  └─ CheckBox (BloomCheckbox)
│     └─ HBoxContainer (MotionBlurRow)
│        ├─ Label - text: "Motion Blur"
│        └─ CheckBox (MotionBlurCheckbox)
```

### Export Variable Connections
- Connect all sliders, checkboxes, dropdowns to their export variables

### Properties
- **RenderScaleSlider**: min=0.5, max=1.5, step=0.05, value=1.0
- **TargetFPSSpinBox**: min=30, max=240, step=1, value=60
- **ShadowQualitySlider**: min=0, max=3, step=1, value=2

## 3. AudioTab.tscn

### Node Structure
```
Control (AudioTab) - Script: Scripts/UI/AudioTabUI.cs
├─ ScrollContainer
│  └─ VBoxContainer
│     ├─ HBoxContainer (MasterRow)
│     │  ├─ Label - text: "Master Volume"
│     │  ├─ HSlider (MasterVolumeSlider)
│     │  └─ Label (MasterVolumeLabel) - text: "100%"
│     ├─ HBoxContainer (MuteMasterRow)
│     │  ├─ Label - text: "Mute Master"
│     │  └─ CheckBox (MuteMasterCheckbox)
│     ├─ HBoxContainer (MusicRow)
│     │  ├─ Label - text: "Music Volume"
│     │  ├─ HSlider (MusicVolumeSlider)
│     │  └─ Label (MusicVolumeLabel) - text: "80%"
│     ├─ HBoxContainer (SFXRow)
│     │  ├─ Label - text: "SFX Volume"
│     │  ├─ HSlider (SFXVolumeSlider)
│     │  └─ Label (SFXVolumeLabel) - text: "100%"
│     └─ HBoxContainer (UIRow)
│        ├─ Label - text: "UI Volume"
│        ├─ HSlider (UIVolumeSlider)
│        └─ Label (UIVolumeLabel) - text: "90%"
```

### Export Variable Connections
- Connect all sliders and labels to their export variables

### Properties
- **All Volume Sliders**: min=0.0, max=1.0, step=0.01, value varies

## 4. ControlsTab.tscn

### Node Structure
```
Control (ControlsTab) - Script: Scripts/UI/ControlsTabUI.cs
├─ ScrollContainer
│  └─ VBoxContainer
│     ├─ HBoxContainer (MouseSensRow)
│     │  ├─ Label - text: "Mouse Sensitivity"
│     │  ├─ HSlider (MouseSensitivitySlider)
│     │  └─ Label (MouseSensitivityLabel) - text: "1.00"
│     ├─ HBoxContainer (InvertYRow)
│     │  ├─ Label - text: "Invert Y-Axis"
│     │  └─ CheckBox (InvertYCheckbox)
│     ├─ HSeparator
│     ├─ Label - text: "Key Bindings"
│     ├─ ScrollContainer (KeyBindingScrollContainer)
│     │  └─ VBoxContainer (KeyBindingList)
│     │     // Keys are added dynamically by script
│     ├─ HSeparator
│     ├─ HBoxContainer (ControllerSensRow)
│     │  ├─ Label - text: "Controller Sensitivity"
│     │  ├─ HSlider (ControllerSensitivitySlider)
│     │  └─ Label (ControllerSensitivityLabel) - text: "1.00"
│     └─ HBoxContainer (DeadzoneRow)
│        ├─ Label - text: "Controller Deadzone"
│        ├─ HSlider (ControllerDeadzoneSlider)
│        └─ Label (ControllerDeadzoneLabel) - text: "15%"
```

### Export Variable Connections
- Connect all sliders, labels, and containers to their export variables

### Properties
- **MouseSensitivitySlider**: min=0.1, max=5.0, step=0.1, value=1.0
- **ControllerSensitivitySlider**: min=0.1, max=5.0, step=0.1, value=1.0
- **ControllerDeadzoneSlider**: min=0, max=100, step=5, value=15

## 5. GameplayTab.tscn

### Node Structure
```
Control (GameplayTab) - Script: Scripts/UI/GameplayTabUI.cs
├─ ScrollContainer
│  └─ VBoxContainer
│     ├─ HBoxContainer (AutoPickupRow)
│     │  ├─ Label - text: "Auto Pickup Items"
│     │  └─ CheckBox (AutoPickupCheckbox)
│     ├─ HBoxContainer (DamageNumbersRow)
│     │  ├─ Label - text: "Show Damage Numbers"
│     │  └─ CheckBox (DamageNumbersCheckbox)
│     ├─ HBoxContainer (ScreenShakeRow)
│     │  ├─ Label - text: "Screen Shake"
│     │  └─ CheckBox (ScreenShakeCheckbox)
│     ├─ HBoxContainer (ShakeIntensityRow)
│     │  ├─ Label - text: "Shake Intensity"
│     │  ├─ HSlider (ShakeIntensitySlider)
│     │  └─ Label (ShakeIntensityLabel) - text: "100%"
│     ├─ HBoxContainer (ShowFPSRow)
│     │  ├─ Label - text: "Show FPS Counter"
│     │  └─ CheckBox (ShowFPSCheckbox)
│     ├─ HBoxContainer (ShowPingRow)
│     │  ├─ Label - text: "Show Ping"
│     │  └─ CheckBox (ShowPingCheckbox)
│     └─ HBoxContainer (LanguageRow)
│        ├─ Label - text: "Language"
│        └─ OptionButton (LanguageDropdown)
```

### Export Variable Connections
- Connect all controls to their export variables

### Properties
- **ShakeIntensitySlider**: min=0.0, max=2.0, step=0.1, value=1.0

## Usage Instructions

1. **Open Godot Editor**
2. **Create New Scene** for each tab first
3. **Create Main Settings Scene** last (references tab scenes)
4. **Attach Scripts** to root nodes
5. **Connect Export Variables** in Inspector
6. **Save Scenes** in `UI/Settings/` directory

## Opening the Settings Menu

From code:
```csharp
// Get the settings menu node
var settingsMenu = GetNode<SettingsMenuUI>("/path/to/SettingsMenu");
settingsMenu.ShowSettings();
```

Or add to a pause menu or main menu button.

## Notes

- The scripts handle all the logic automatically
- Tabs are populated with data on show
- Real-time preview works for audio settings
- Apply button saves all changes
- Reset button restores defaults with confirmation
- All settings persist automatically to `user://settings.cfg`

## Testing

After creating the scenes:
1. Add SettingsMenu to your main scene or UI layer
2. Create a button to show it: `settingsMenu.ShowSettings()`
3. Test all tabs
4. Verify settings persist after restarting the game
5. Test reset to defaults
6. Test key rebinding
7. Test real-time audio preview
