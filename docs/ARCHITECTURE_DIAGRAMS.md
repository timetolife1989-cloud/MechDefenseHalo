# System Architecture Diagram

## Component Overview

```
┌─────────────────────────────────────────────────────────────┐
│                         Main Scene                           │
│  (res://Scenes/Main.tscn)                                   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────┐  ┌───────────────────────────────────┐ │
│  │ DirectionalLight│  │    Ground (StaticBody3D)         │ │
│  └────────────────┘  │    - 50x50m grey plane            │ │
│                      │    - Collision shape              │ │
│  ┌────────────────┐  └───────────────────────────────────┘ │
│  │ WorldEnvironment│                                        │
│  └────────────────┘  ┌───────────────────────────────────┐ │
│                      │   PlayerMech (Instance)            │ │
│                      │   See below for details            │ │
│                      └───────────────────────────────────┘ │
│                                                              │
│                      ┌───────────────────────────────────┐ │
│                      │   MobileControls (Instance)        │ │
│                      │   See below for details            │ │
│                      └───────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## PlayerMech Scene Detail

```
┌─────────────────────────────────────────────────────────────┐
│              PlayerMech (CharacterBody3D)                    │
│              Script: PlayerMechController.cs                 │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  CollisionShape3D (CapsuleShape3D)                          │
│    - Height: 3.0m                                           │
│    - Radius: 0.8m                                           │
│                                                              │
│  Placeholder Geometry:                                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Body (BoxMesh)         - Grey #B0B0B0               │  │
│  │  Head (SphereMesh)      - Grey + Yellow accent       │  │
│  │  LeftArm (BoxMesh)      - Grey                       │  │
│  │  RightArm (BoxMesh)     - Grey                       │  │
│  │  LeftLeg (BoxMesh)      - Grey                       │  │
│  │  RightLeg (BoxMesh)     - Grey                       │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  CameraMount (Node3D)                                       │
│  └─ Camera3D (First-person view)                           │
│      - FOV: 75°                                             │
│      - Position: At head height (2.5m)                     │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## MobileControls Scene Detail

```
┌─────────────────────────────────────────────────────────────┐
│            MobileControls (CanvasLayer)                      │
│            Script: MobileControlsUI.cs                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Left Side (540px width):                                   │
│  ┌────────────────────────────────┐                         │
│  │  VirtualJoystick (Panel)       │                         │
│  │  ┌──────────────────────────┐  │                         │
│  │  │  JoystickBase            │  │                         │
│  │  │  ┌────────────────────┐  │  │                         │
│  │  │  │ JoystickThumb      │  │  │                         │
│  │  │  │ (Yellow #FFD700)   │  │  │                         │
│  │  │  └────────────────────┘  │  │                         │
│  │  └──────────────────────────┘  │                         │
│  └────────────────────────────────┘                         │
│                                                              │
│  Right Side (540px width):                                  │
│  ┌────────────────────────────────┐                         │
│  │  TouchArea (Panel)              │                         │
│  │  - Semi-transparent             │                         │
│  │  - Full height                  │                         │
│  │  - Camera rotation input        │                         │
│  └────────────────────────────────┘                         │
│                                                              │
│  Action Buttons (Bottom-right):                             │
│  ┌─────────┬─────────┐                                      │
│  │ Fire 🔫 │Shield🛡️│  - Fire: Red                         │
│  │ (Red)   │ (Blue)  │  - Shield: Blue                      │
│  ├─────────┼─────────┤  - Ability: Yellow                   │
│  │Ability⚡│Switch🔄 │  - Switch: Grey                      │
│  │(Yellow) │ (Grey)  │                                      │
│  └─────────┴─────────┘                                      │
│                                                              │
│  HUD (Overlay):                                             │
│  ┌─────────────────────────────────────────┐                │
│  │ [████████████████████████] HEALTH       │ Top           │
│  │ [████████████████████████] ENERGY       │ Top           │
│  │                                         │                │
│  │              +  (Crosshair)             │ Center        │
│  └─────────────────────────────────────────┘                │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                      INPUT SOURCES                            │
└──────────────────────────────────────────────────────────────┘
         │                                  │
         │ PC                              │ Mobile
         ▼                                  ▼
    ┌─────────┐                      ┌────────────────┐
    │ Keyboard│                      │ Touch Events   │
    │  WASD   │                      │ - Screen Touch │
    │  Shift  │                      │ - Screen Drag  │
    └────┬────┘                      └────────┬───────┘
         │                                     │
    ┌────┴────┐                               │
    │  Mouse  │                               │
    │Movement │                               │
    └────┬────┘                               │
         │                                     │
         ▼                                     ▼
┌─────────────────┐                  ┌──────────────────┐
│PlayerMechControl│◄─────API────────│MobileControlsUI  │
│                 │                  │                  │
│ • GetMovement() │  SetMovement()   │ • Joystick Logic │
│ • CameraRotate()│◄─────────────────│ • Touch Logic    │
│ • Physics       │  SetCameraDelta()│ • Platform Check │
└────────┬────────┘                  └──────────────────┘
         │
         ▼
┌─────────────────┐
│ CharacterBody3D │
│ Physics Engine  │
│ • MoveAndSlide()│
│ • Gravity       │
│ • Collision     │
└─────────────────┘
```

## Control Flow: PC vs Mobile

### PC Control Flow
```
1. User presses WASD
   ↓
2. PlayerMechController._PhysicsProcess() detects input
   ↓
3. GetMovementInput() returns keyboard vector
   ↓
4. Calculate movement direction relative to camera
   ↓
5. Apply velocity to CharacterBody3D
   ↓
6. MoveAndSlide() handles physics

Parallel:
1. User moves mouse
   ↓
2. PlayerMechController._UnhandledInput() receives MouseMotion
   ↓
3. HandleCameraRotation() rotates character + camera mount
   ↓
4. Clamp pitch to ±60°
```

### Mobile Control Flow
```
Joystick:
1. User touches left side of screen
   ↓
2. MobileControlsUI detects InputEventScreenTouch
   ↓
3. StartJoystick() activates joystick at touch position
   ↓
4. User drags → UpdateJoystick()
   ↓
5. Calculate input vector from joystick center
   ↓
6. PlayerMech.SetMobileMovementInput(vector)
   ↓
7. PlayerMechController uses vector in _PhysicsProcess()

Camera:
1. User touches right side of screen
   ↓
2. MobileControlsUI detects InputEventScreenTouch
   ↓
3. StartCameraTouch() records start position
   ↓
4. User drags → UpdateCameraTouch()
   ↓
5. Calculate delta from last position
   ↓
6. PlayerMech.SetMobileCameraDelta(delta)
   ↓
7. PlayerMechController rotates in _PhysicsProcess()
```

## Platform Detection Logic

```
┌──────────────────┐
│   _Ready() in    │
│ PlayerMechCtrl & │
│  MobileControlsUI│
└────────┬─────────┘
         │
         ▼
   OS.GetName()
         │
    ┌────┴────┐
    │         │
"Android" │   │ Other OS
   or     │   │ (Windows,
  "iOS"   │   │  Linux, etc)
    │     │   │
    ▼     │   ▼
┌────────┐│ ┌──────────┐
│ Mobile ││ │    PC    │
│  Mode  ││ │   Mode   │
└────────┘│ └──────────┘
    │     │       │
    ▼     │       ▼
Show UI   │   Hide UI
Virtual   │   Capture
Controls  │   Mouse
    │     │       │
    └─────┴───────┘
          │
          ▼
    Game Runs
```

## Export Properties

### PlayerMechController
```
┌───────────────────────────────────┐
│ Exported Properties (Tunable)     │
├───────────────────────────────────┤
│ WalkSpeed         : 5.0 m/s       │
│ SprintSpeed       : 8.0 m/s       │
│ MouseSensitivity  : 0.002         │
│ MaxHealth         : 100.0         │
│ MaxEnergy         : 100.0         │
└───────────────────────────────────┘
```

## Key Algorithms

### Virtual Joystick Vector Calculation
```
Input: Touch position (touchPos)
Output: Normalized vector (-1 to 1)

1. delta = touchPos - joystickCenter
2. distance = delta.Length()
3. if distance > JoystickRadius:
     delta = delta.Normalized() * JoystickRadius
4. inputVector = delta / JoystickRadius
5. if inputVector.Length() < DeadZone:
     inputVector = Vector2.Zero
6. return inputVector
```

### Camera Pitch Clamping
```
Input: Mouse delta Y (or touch delta Y)
Output: Clamped camera pitch

1. cameraPitch -= deltaY * sensitivity
2. cameraPitch = Clamp(cameraPitch, -60°, +60°)
3. cameraMount.Rotation = Vector3(cameraPitch, 0, 0)
```

### Movement Direction Calculation
```
Input: Input vector (X, Y) in local space
Output: World-space direction

1. inputDir = GetMovementInput()  // Returns Vector2
2. direction = Transform.Basis * Vector3(inputDir.X, 0, inputDir.Y)
3. direction = direction.Normalized()
4. velocity.X = direction.X * speed
5. velocity.Z = direction.Z * speed
```

## Multi-Touch Support

```
Touch 1 (Left):              Touch 2 (Right):
┌─────────────┐              ┌─────────────┐
│ Index: 0    │              │ Index: 1    │
│ ↓           │              │ ↓           │
│ Joystick    │              │ Camera      │
│ Tracking    │              │ Tracking    │
└─────────────┘              └─────────────┘
       │                            │
       └──────────┬─────────────────┘
                  │
                  ▼
         Both work simultaneously
         No interference
```

## Success Validation Checklist

✅ Scene hierarchy matches specification
✅ All nodes present (PlayerMech, Ground, MobileControls)
✅ Scripts attached to correct nodes
✅ Input actions configured
✅ Platform detection functional
✅ Multi-touch support implemented
✅ HUD updates in real-time
✅ Physics collision working
✅ Camera clamping functional
✅ Code documented and organized
