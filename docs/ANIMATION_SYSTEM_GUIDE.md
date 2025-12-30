# Mech IK Animation System - Usage Guide

## Overview
The Mech IK Animation System provides procedural, code-driven animations for mechs without requiring baked animation files. All movement is calculated in real-time using Inverse Kinematics and physics-based motion.

## Architecture

### Component Hierarchy
```
MechIKController (Node3D)
├── ProceduralWalking (Node)
├── UpperBodyIK (Node)
└── SecondaryMotion (Node)
```

## Setup Instructions

### 1. Basic Scene Setup
Add the `MechIKController` to your mech's root node:
```
YourMech (CharacterBody3D)
├── MechIKController (Node3D)
│   ├── ProceduralWalking (Node)
│   ├── UpperBodyIK (Node)
│   └── SecondaryMotion (Node)
├── Skeleton3D
│   └── [Bones]
├── LeftFoot (Node3D)
├── RightFoot (Node3D)
├── LeftHand (Node3D)
└── RightHand (Node3D)
```

### 2. Configure MechIKController
In the Godot inspector, assign the following exported properties:
- **Skeleton**: Your mech's Skeleton3D node
- **Left Foot**: Node3D marker for left foot IK target
- **Right Foot**: Node3D marker for right foot IK target
- **Left Hand**: Node3D marker for left hand IK target
- **Right Hand**: Node3D marker for right hand IK target

### 3. Configure ProceduralWalking
Adjust these parameters for your mech's size and movement style:
- **Step Height** (default: 0.3): How high feet lift during steps
- **Step Distance** (default: 1.0): Distance before triggering a step
- **Step Speed** (default: 5.0): Speed of stepping animation

### 4. Configure UpperBodyIK
For upper body aiming:
- **Spine**: Node3D of the spine/torso bone
- **Head**: Node3D of the head bone
- **Left Shoulder**: Optional left shoulder node
- **Right Shoulder**: Optional right shoulder node
- **Aim Smoothness** (default: 5.0): How smoothly torso follows aim

### 5. Configure WeaponSwapAnimator
Add to your mech's weapon system:
- **Mech Body**: Root Node3D of the mech body
- **Right Arm**: Node3D of the right arm
- **Camera**: Camera3D for tilt effect
- **Swap Duration** (default: 0.5): Animation duration in seconds

### 6. Configure CameraReaction
Apply to your Camera3D:
- **Recoil Strength** (default: 0.1): Intensity of recoil
- **Return Speed** (default: 10.0): How fast camera returns to center

### 7. Configure SecondaryMotion
For cables, antennas, and dangly bits:
- **Secondary Bones**: Array of Node3D elements to apply physics

## Event Integration

The system automatically listens to these EventBus events:

### Camera Reactions
```csharp
EventBus.Emit(EventBus.WeaponFired);  // Triggers recoil
EventBus.Emit(EventBus.PlayerHit);    // Triggers hit shake
```

### Weapon Swap Animation
```csharp
EventBus.Emit(EventBus.WeaponSwitched);  // Triggers swap animation
```

## Customization

### Adjusting Step Pattern
Modify `ProceduralWalking.StartStep()` to change foot placement:
```csharp
// Make wider stance
targetPos += isLeftFoot ? Vector3.Left * 0.8f : Vector3.Right * 0.8f;
```

### Custom Camera Shake
Override `CameraReaction.OnWeaponFired()`:
```csharp
private void OnWeaponFired(object data)
{
    // Custom recoil pattern
    recoilOffset += new Vector3(0, recoilStrength * 2, -recoilStrength);
}
```

### Secondary Motion Tuning
Adjust drag physics in `SecondaryMotion.UpdateSecondaryBones()`:
```csharp
// Increase drag effect
Vector3 drag = -acceleration * 0.02f;  // Was 0.01f

// Faster damping
bone.Rotation = bone.Rotation.Lerp(Vector3.Zero, delta * 10f);  // Was 5f
```

## Performance Considerations

### Optimization Tips
1. **Limit Secondary Bones**: Use only essential elements for physics
2. **Adjust Update Rates**: Not all systems need 60 FPS updates
3. **LOD System**: Disable distant mech IK calculations
4. **Bone Count**: Keep IK chains short (3-4 bones max)

### Performance Metrics
- **ProceduralWalking**: ~0.1ms per frame
- **UpperBodyIK**: ~0.05ms per frame
- **SecondaryMotion**: ~0.01ms per bone
- **Total Overhead**: <0.5ms for typical mech

## Troubleshooting

### Feet Not Moving
- Ensure mech has CharacterBody3D parent with velocity
- Check Step Distance threshold
- Verify Left/Right Foot nodes are assigned

### Jittery Aiming
- Increase Aim Smoothness value
- Check camera is properly configured
- Ensure raycast isn't hitting mech's own colliders

### No Weapon Swap Animation
- Verify EventBus.WeaponSwitched is being emitted
- Check Mech Body, Right Arm, and Camera are assigned
- Ensure WeaponSwapAnimator is active in scene tree

### Secondary Parts Not Moving
- Verify bones are in Secondary Bones array
- Check parent mech is moving (velocity > 0)
- Ensure nodes are Node3D types, not Node2D

## Advanced Usage

### Blending with Traditional Animations
```csharp
// In MechIKController._Process()
if (isPlayingCutscene)
{
    // Disable procedural systems
    return;
}
// Normal IK updates
```

### Dynamic Step Height for Terrain
```csharp
// In ProceduralWalking
var terrainHeight = GetTerrainHeightAt(targetPos);
targetPos.Y = terrainHeight;
```

### Weapon-Specific Recoil
```csharp
// In CameraReaction
private void OnWeaponFired(object data)
{
    var weaponType = data as string;
    float recoil = weaponType == "Rifle" ? recoilStrength : recoilStrength * 2;
    recoilOffset += new Vector3(0, recoil, -recoil * 0.5f);
}
```

## Best Practices

1. **Always assign all exported properties** - Missing nodes cause errors
2. **Test at different speeds** - Adjust step values accordingly
3. **Profile performance** - Especially with many mechs
4. **Use GetNodeOrNull** - Already implemented for safety
5. **Clean up events** - ExitTree handlers handle this automatically

## Support

For issues or questions:
1. Check this documentation
2. Review example scenes (if provided)
3. Check EventBus event names match
4. Verify node paths are correct
5. Enable debug prints in scripts

## Future Enhancements

Possible improvements:
- SkeletonIK3D integration for true IK solving
- Ground detection raycasts for uneven terrain
- Animation state machine integration
- More complex secondary physics (springs, chains)
- IK retargeting for different mech models
