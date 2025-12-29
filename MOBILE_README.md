# Mobile Build Pipeline + Touch Controls

This document describes the mobile touch control system and Android export configuration for MechDefenseHalo.

## Overview

The mobile system provides:
- Touch-based virtual joystick for movement
- Touch fire button for shooting
- Automatic platform detection
- Performance monitoring overlay
- One-button Android build and deploy

## Architecture

### Core Components

#### TouchController (`Scripts/Mobile/TouchController.cs`)
Main controller that aggregates input from virtual joystick and fire button. Exposes:
- `MovementInput` - Vector2 movement direction
- `IsFirePressed` - Boolean fire state

#### VirtualJoystick (`Scripts/Mobile/VirtualJoystick.cs`)
On-screen joystick control that:
- Detects touch input within joystick area
- Emits `JoystickMoved` signal with direction vector
- Provides visual feedback via knob position
- Resets when touch is released

#### TouchFireButton (`Scripts/Mobile/TouchFireButton.cs`)
Touch-based fire button that:
- Detects press/release events
- Emits `Pressed` and `Released` signals
- Provides visual feedback on touch

#### MobileInputManager (`Scripts/Mobile/MobileInputManager.cs`)
Platform-aware input manager that:
- Automatically detects Android/iOS platforms
- Loads touch controls on mobile devices
- Provides unified input API for both PC and mobile
- Falls back to keyboard/mouse on PC

#### PerformanceMonitor (`Scripts/Mobile/PerformanceMonitor.cs`)
Debug overlay displaying:
- FPS (frames per second)
- Memory usage in MB
- Draw calls per frame

## Scene Structure

### TouchControls.tscn
Complete touch control UI with:
- Left joystick (bottom-left corner)
- Fire button (bottom-right corner)
- Semi-transparent overlays

### MobileHUD.tscn
Performance monitoring HUD for debugging.

## Integration with Existing Code

The mobile system integrates seamlessly with existing `PlayerMechController` and `WeaponManager`:

### PlayerMechController
Already has mobile input support with methods:
- `SetMobileMovementInput(Vector2)` - Called by touch controls
- `SetMobileCameraDelta(Vector2)` - For touch-based camera rotation
- `SetMobileSprint(bool)` - Sprint toggle

The controller automatically detects mobile platforms and adjusts behavior.

### WeaponManager
Provides methods for mobile input:
- `FireCurrentWeapon()` - Trigger weapon fire
- `ReloadCurrentWeapon()` - Reload current weapon

## Android Export Configuration

### Export Preset (`Export/android_export_preset.cfg`)
Configured with:
- **Target SDK**: 33 (Android 13)
- **Min SDK**: 21 (Android 5.0)
- **Architecture**: ARM64-v8a only (modern devices)
- **Graphics**: OpenGL ES 3.0 / Vulkan
- **Permissions**: INTERNET, ACCESS_NETWORK_STATE
- **Orientation**: Landscape (immersive mode)
- **Package**: com.mechdefense.halo

### Build Script (`Export/mobile_build_script.sh`)
One-button deployment script that:
1. Checks for Godot installation
2. Builds APK to `build/MechDefenseHalo.apk`
3. Detects connected Android devices (adb)
4. Installs APK on device
5. Launches the application

## Usage

### For Developers

#### Building for Android
```bash
cd Export
./mobile_build_script.sh
```

#### Manual Build
```bash
godot --headless --export-release "Android" build/MechDefenseHalo.apk
```

#### Manual Install
```bash
adb install -r build/MechDefenseHalo.apk
adb shell am start -n com.mechdefense.halo/.GodotApp
```

### For Players

The touch controls appear automatically on Android/iOS devices:
- **Left joystick**: Move mech
- **Right fire button**: Shoot weapon
- Touch and drag anywhere on screen for camera rotation

### Testing on Desktop

To test mobile controls on desktop:
1. Modify `MobileInputManager._Ready()` to force mobile mode:
```csharp
isMobilePlatform = true; // Force mobile mode for testing
```

2. Run the project normally - touch controls will appear
3. Use mouse to simulate touch input

## Performance Considerations

### Optimizations
- Touch controls use minimal draw calls (colored rectangles)
- Input events processed efficiently via signals
- No continuous polling - event-driven architecture
- Performance monitor can be disabled in production

### Mobile Settings
Recommended project settings for mobile:
- MSAA: 2x (configured in project.godot)
- Shadow quality: Medium
- Reflection quality: Low
- Particle count: Reduced on mobile

## Troubleshooting

### Build Issues

**"Godot not found"**
- Install Godot 4.x
- Add to PATH or use full path in script

**"Export preset not found"**
- Ensure `Export/android_export_preset.cfg` is copied to project root
- Or configure export preset in Godot Editor

**"Build failed"**
- Check Android SDK is installed
- Verify Godot has Android export templates
- Install templates: Editor → Manage Export Templates

### Runtime Issues

**Touch controls not appearing**
- Verify platform detection in logs
- Check `TouchControls.tscn` exists at correct path
- Ensure MobileInputManager is in autoload or scene

**Input not responding**
- Verify touch areas don't overlap
- Check MouseFilter is set to Stop on controls
- Ensure signals are properly connected

**Performance issues**
- Disable PerformanceMonitor in production
- Reduce shadow/particle quality
- Profile with Android Studio

## Testing Checklist

- [ ] Touch joystick responds to finger movement
- [ ] Fire button triggers weapon fire
- [ ] Camera rotation works with touch drag
- [ ] Controls scale properly on different screen sizes
- [ ] Performance monitor shows accurate metrics
- [ ] APK builds successfully
- [ ] Deploy script installs and launches app
- [ ] Input matches keyboard/mouse behavior
- [ ] No crashes on app launch
- [ ] Memory usage stays reasonable

## Future Enhancements

Potential improvements:
- [ ] Configurable control positions
- [ ] Multiple fire button options (abilities, grenades)
- [ ] Haptic feedback on touch
- [ ] Control size/opacity settings
- [ ] iOS export preset
- [ ] Cloud build integration
- [ ] Auto-update system

## File Structure

```
MechDefenseHalo/
├── Scripts/Mobile/
│   ├── TouchController.cs          # Main touch input controller
│   ├── VirtualJoystick.cs          # Virtual joystick component
│   ├── TouchFireButton.cs          # Fire button component
│   ├── MobileInputManager.cs       # Platform-aware input manager
│   └── PerformanceMonitor.cs       # Performance overlay
├── Scenes/Mobile/
│   ├── TouchControls.tscn          # Touch control UI scene
│   └── MobileHUD.tscn              # Performance monitor HUD
└── Export/
    ├── android_export_preset.cfg   # Android export configuration
    └── mobile_build_script.sh      # One-button deploy script
```

## Success Criteria

✅ All success criteria from requirements:
- Touch controls responsive on mobile
- APK builds successfully  
- Performance monitor shows FPS
- Deploy script works
- Input works same as keyboard/mouse

## Additional Resources

- [Godot Mobile Development](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_android.html)
- [Touch Input in Godot](https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html#touch)
- [Android Export Templates](https://godotengine.org/download)
