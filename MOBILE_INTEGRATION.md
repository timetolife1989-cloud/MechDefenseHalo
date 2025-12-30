# Mobile Integration Guide

This guide explains how to integrate the mobile touch controls into your game scenes.

## Quick Start

There are two ways to integrate mobile controls:

### Option 1: Automatic Integration (Recommended)

Add the `MobileTouchBridge` script to your main game scene or player node:

1. Open your main game scene (e.g., `Scenes/Main.tscn`)
2. Add a new Node as a child
3. Attach the `MobileTouchBridge.cs` script to it
4. Set the exported properties:
   - `PlayerControllerPath`: Path to your PlayerMechController node
   - `WeaponManagerPath`: Path to your WeaponManager node

Example scene structure:
```
Main (Node)
├── Player (PlayerMechController)
│   └── WeaponManager
└── MobileTouchBridge
    ├── PlayerControllerPath = "../Player"
    └── WeaponManagerPath = "../Player/WeaponManager"
```

The bridge will:
- Automatically detect mobile platforms
- Load touch controls on mobile devices
- Wire up input to player and weapon systems
- Handle nothing on PC (keyboard/mouse works as normal)

### Option 2: Using MobileInputManager as Autoload

Add `MobileInputManager` to your project's autoload:

1. Open Project Settings → Autoload
2. Add `res://Scripts/Mobile/MobileInputManager.cs` as autoload
3. Name it "MobileInput"
4. Use it in your scripts:

```csharp
// In PlayerMechController
Vector2 movement = GetNode<MobileInputManager>("/root/MobileInput").GetMovementInput();

// In WeaponManager
if (GetNode<MobileInputManager>("/root/MobileInput").IsFirePressed())
{
    FireCurrentWeapon();
}
```

## Manual Integration

If you want more control, you can manually wire up the touch controls:

### Step 1: Load Touch Controls Scene

```csharp
public override void _Ready()
{
    string osName = OS.GetName();
    if (osName == "Android" || osName == "iOS")
    {
        var touchScene = ResourceLoader.Load<PackedScene>("res://Scenes/Mobile/TouchControls.tscn");
        var touchController = touchScene.Instantiate<TouchController>();
        GetTree().Root.AddChild(touchController);
        
        // Store reference for later use
        _touchController = touchController;
    }
}
```

### Step 2: Read Touch Input

```csharp
public override void _Process(double delta)
{
    if (_touchController != null)
    {
        // Get movement from joystick
        Vector2 movement = _touchController.MovementInput;
        
        // Check fire button state
        bool isFiring = _touchController.IsFirePressed;
    }
}
```

## Adding Performance Monitor

To add the performance monitor overlay:

```csharp
var hudScene = ResourceLoader.Load<PackedScene>("res://Scenes/Mobile/MobileHUD.tscn");
var hud = hudScene.Instantiate<Control>();
GetTree().Root.AddChild(hud);
```

Or add it directly in your scene:
1. Instance `Scenes/Mobile/MobileHUD.tscn` into your UI
2. Position it where you want (default is top-left)
3. Toggle visibility with the `ShowMonitor` property

## Testing Mobile Controls on PC

To test touch controls on desktop during development:

### Method 1: Force Mobile Mode in MobileTouchBridge

```csharp
// In MobileTouchBridge._Ready()
isMobilePlatform = true; // Force mobile mode for testing
```

### Method 2: Use Touch Emulation

In Godot Editor:
1. Project → Project Settings → Input Devices → Pointing
2. Enable "Emulate Touch From Mouse"
3. Run the project - mouse clicks will be treated as touch events

### Method 3: Remote Debug on Device

1. Enable USB debugging on Android device
2. Connect via USB
3. In Godot: Debug → Deploy with Remote Debug
4. Test directly on device while monitoring logs

## Custom Control Positions

To adjust touch control positions, edit `Scenes/Mobile/TouchControls.tscn`:

### Move Joystick
Select `LeftJoystick` node and adjust:
- `offset_left`, `offset_top` for top-left corner position
- `offset_right`, `offset_bottom` for size

### Move Fire Button
Select `FireButton` node and adjust anchors/offsets

### Example: Move Fire Button to Top-Right
```gdscript
# In TouchControls.tscn
[node name="FireButton" type="Control" parent="."]
anchors_preset = 1  # Top-right
anchor_left = 1.0
anchor_top = 0.0
anchor_right = 1.0
anchor_bottom = 0.0
offset_left = -250.0
offset_top = 50.0
offset_right = -50.0
offset_bottom = 250.0
```

## Adding Additional Buttons

To add more buttons (abilities, grenades, etc.):

1. Duplicate the `FireButton` node in `TouchControls.tscn`
2. Rename it (e.g., "AbilityButton")
3. Position it where you want
4. In `TouchController.cs`, add:

```csharp
private TouchFireButton abilityButton;
public bool IsAbilityPressed { get; private set; }

public override void _Ready()
{
    // ... existing code ...
    abilityButton = GetNode<TouchFireButton>("AbilityButton");
    abilityButton.Pressed += () => IsAbilityPressed = true;
    abilityButton.Released += () => IsAbilityPressed = false;
}
```

5. Read the state in your game code:
```csharp
if (touchController.IsAbilityPressed)
{
    ActivateAbility();
}
```

## Screen Orientation

The project is configured for landscape orientation in `project.godot`:

```ini
[display]
window/handheld/orientation=1  # Landscape
```

To change to portrait:
```ini
window/handheld/orientation=0  # Portrait
```

To support both:
```ini
window/handheld/orientation=6  # Sensor (auto-rotate)
```

## Troubleshooting

### Controls Don't Appear on Mobile

Check the logs for:
```
MobileTouchBridge initialized on mobile platform
Touch controls loaded and added to scene
```

If missing:
1. Verify `TouchControls.tscn` path is correct
2. Check that MobileTouchBridge is active
3. Ensure platform detection is working

### Input Not Working

1. Verify touch areas don't overlap with other UI
2. Check MouseFilter is set to Stop on controls
3. Ensure signals are connected in _Ready()
4. Test with performance monitor to confirm app is running

### Joystick Sensitivity Issues

Adjust `MaxDistance` in VirtualJoystick:
```csharp
[Export] public float MaxDistance { get; set; } = 75f; // Increase for less sensitive
```

Or edit in `TouchControls.tscn`:
```
[node name="LeftJoystick" ...]
script = ExtResource("2_joystick")
MaxDistance = 100.0  # Adjust this value
```

## Platform-Specific Code

To execute code only on mobile:

```csharp
public override void _Ready()
{
    string osName = OS.GetName();
    bool isMobile = osName == "Android" || osName == "iOS";
    
    if (isMobile)
    {
        // Mobile-specific code
        AdjustUIForMobile();
        EnableTouchControls();
    }
    else
    {
        // PC-specific code
        EnableMouseCapture();
    }
}
```

## Best Practices

1. **Always Test on Device**: Touch input feels different than mouse simulation
2. **Consider Screen Sizes**: Test on different device sizes (phone, tablet)
3. **Provide Feedback**: Visual and haptic feedback for button presses
4. **Avoid Overlaps**: Keep touch controls away from gameplay UI
5. **Performance Monitor**: Use it during development, disable for release
6. **Accessibility**: Make controls large enough for various hand sizes
7. **Save Settings**: Allow players to customize control positions/size

## Next Steps

1. Test on real Android device
2. Adjust control sizes and positions based on feedback
3. Add haptic feedback (vibration) on button presses
4. Implement control customization UI
5. Create iOS export preset
6. Optimize for different screen resolutions
