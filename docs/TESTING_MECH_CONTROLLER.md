# Mech Controller Testing Guide

This document provides a comprehensive guide for testing the first-person mech controller implementation.

## Prerequisites

- Godot 4.3 or higher with .NET support
- .NET SDK 6.0 or higher
- Android SDK (for mobile builds)

## Opening the Project

1. Clone the repository
2. Open Godot 4.x
3. Import project by selecting `project.godot`
4. Wait for Godot to generate C# project files (.csproj, .sln)
5. The project should compile automatically

## Testing Checklist

### ✅ Scene Structure

**PlayerMech Scene (res://Scenes/Mech/PlayerMech.tscn):**
- [ ] Scene opens without errors in Godot
- [ ] CharacterBody3D root node exists
- [ ] CapsuleShape3D collision shape is visible
- [ ] Placeholder meshes visible (Body, Head, Arms, Legs)
- [ ] CameraMount node with Camera3D child exists
- [ ] PlayerMechController.cs script is attached
- [ ] Exported properties visible in inspector (WalkSpeed, SprintSpeed, etc.)

**Main Scene (res://Scenes/Main.tscn):**
- [ ] Scene opens without errors
- [ ] Ground plane (StaticBody3D) is visible
- [ ] DirectionalLight3D illuminates the scene
- [ ] PlayerMech instance is present at origin
- [ ] MobileControls UI overlay is present

**MobileControls Scene (res://UI/MobileControls.tscn):**
- [ ] Scene opens without errors
- [ ] Virtual joystick panel visible (left side)
- [ ] Touch area visible (right side, semi-transparent)
- [ ] Action buttons visible (Fire, Shield, Ability, Weapon Switch)
- [ ] HUD elements visible (Health bar, Energy bar, Crosshair)

### ✅ PC Controls (Editor Testing)

**Press F5 to run the main scene, then test:**

**Movement:**
- [ ] W key moves mech forward
- [ ] S key moves mech backward
- [ ] A key moves mech left
- [ ] D key moves mech right
- [ ] Arrow keys also work for movement
- [ ] Diagonal movement works (W+A, W+D, etc.)
- [ ] Shift key increases movement speed (sprint)

**Camera:**
- [ ] Mouse is captured on start
- [ ] Mouse movement rotates camera horizontally (yaw)
- [ ] Mouse movement rotates camera vertically (pitch)
- [ ] Camera pitch is clamped (can't look too far up/down)
- [ ] Camera yaw works 360 degrees
- [ ] ESC releases/recaptures mouse

**Physics:**
- [ ] Mech doesn't fall through the ground
- [ ] Gravity works when airborne (if mech is moved above ground)
- [ ] IsOnFloor() detection works

**Mobile UI (PC):**
- [ ] Mobile controls are hidden on PC
- [ ] HUD is still visible (Health, Energy, Crosshair)
- [ ] Action buttons are hidden

### ✅ Console Output

**Check the Godot output console for:**
- [ ] "PlayerMechController initialized on [OS Name]" message
- [ ] No error messages during initialization
- [ ] No runtime errors during movement

### ✅ Inspector Properties

**Select PlayerMech node in Main scene:**
- [ ] WalkSpeed = 5.0
- [ ] SprintSpeed = 8.0
- [ ] MouseSensitivity = 0.002
- [ ] MaxHealth = 100
- [ ] MaxEnergy = 100
- [ ] Properties can be modified in inspector
- [ ] Modified values affect gameplay

### ✅ Mobile Testing (Requires Android Device)

**Build APK:**
1. Project → Export → Android
2. Configure export settings (keystore, etc.)
3. Export APK
4. Install on Android device

**Mobile Controls:**
- [ ] Virtual joystick appears on left side
- [ ] Touch joystick activates on first touch
- [ ] Joystick thumb follows finger within radius
- [ ] Mech moves based on joystick input
- [ ] Joystick resets when released
- [ ] Movement stops when joystick is released

**Touch Camera:**
- [ ] Touch and drag on right side rotates camera
- [ ] Camera rotation is smooth
- [ ] Camera pitch is clamped
- [ ] Camera yaw works 360 degrees

**Multi-Touch:**
- [ ] Can use joystick and camera simultaneously
- [ ] Both inputs work independently
- [ ] No interference between touches

**Action Buttons:**
- [ ] All four buttons are visible and tappable
- [ ] Console logs appear when buttons pressed:
  - "Fire button pressed!"
  - "Shield button pressed!"
  - "Ability button pressed!"
  - "Weapon switch button pressed!"

**HUD:**
- [ ] Health bar shows 100% (green)
- [ ] Energy bar shows 100% (yellow)
- [ ] Crosshair visible in center

**Platform Detection:**
- [ ] Mobile controls visible on mobile device
- [ ] Console shows "Mobile controls enabled"

### ✅ Code Quality

**PlayerMechController.cs:**
- [ ] Code compiles without errors
- [ ] No warnings in build output
- [ ] Proper namespaces and using statements
- [ ] XML documentation comments present
- [ ] Proper region organization
- [ ] Follows C# naming conventions

**MobileControlsUI.cs:**
- [ ] Code compiles without errors
- [ ] No warnings in build output
- [ ] Touch input handling works
- [ ] Event connections work
- [ ] Platform detection works

## Known Limitations

These are expected and documented:

- Action buttons only log to console (functionality not yet implemented)
- No weapon system yet
- No damage system yet
- No energy consumption yet
- Placeholder geometry only (no final models)
- Basic materials (no textures)

## Troubleshooting

**"PlayerMech not found in scene tree!"**
- Check that PlayerMech instance exists in Main.tscn
- Verify node path is correct: "Main/PlayerMech"

**Mouse not captured on PC:**
- Check Input.MouseMode is set to Captured in _Ready()
- Press ESC to recapture mouse

**Mech falls through ground:**
- Verify Ground has StaticBody3D and CollisionShape3D
- Check collision layers/masks

**No mobile controls on mobile:**
- Check OS.GetName() returns "Android" or "iOS"
- Verify platform detection logic

**Build errors:**
- Ensure .NET SDK is installed
- Let Godot regenerate C# project files
- Check for syntax errors in scripts

## Success Criteria

All items marked ✅ in the testing checklist should pass for a successful implementation.

## Next Steps

After basic testing is complete:
1. Test on actual Android device
2. Fine-tune input sensitivity
3. Add weapon systems
4. Add energy consumption
5. Replace placeholder models
6. Add visual effects
7. Add sound effects
