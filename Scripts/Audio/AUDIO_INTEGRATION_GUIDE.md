# Audio System Integration Guide

This document provides comprehensive information about the new audio system implementation for MechDefenseHalo.

## Overview

The audio system provides:
- **Dynamic Music System**: Crossfading music based on game state (menu, combat, boss fights)
- **Sound Effect Pooling**: Efficient object pooling for performance
- **Volume Control**: Separate control for Master, Music, SFX, and UI audio buses
- **3D Positional Audio**: Spatial audio for combat sounds
- **Event-Driven Architecture**: Integration with EventBus for decoupled audio triggers

## Architecture

### Components

1. **AudioManager** - Central hub for audio management
2. **MusicController** - Handles background music transitions
3. **SoundEffectPool** - Object pooling for audio players
4. **AudioBusController** - Manages audio bus configuration
5. **AudioSettingsApplier** - Applies volume settings to buses
6. **AudioEventConnections** - Example event wiring

## Setup Instructions

### 1. Scene Setup

Create an AudioManager scene with the following hierarchy:

```
AudioManager (Node)
├─ AudioBusController (Node)
├─ MusicController (Node)
├─ SFXPool (Node) [SoundEffectPool script]
└─ AudioSettingsApplier (Node)
```

### 2. Autoload Configuration

Add AudioManager to your project autoloads in `project.godot`:

```ini
[autoload]
AudioManager="*res://Scenes/AudioManager.tscn"
```

### 3. Audio Bus Configuration

The system automatically creates these audio buses:
- **Master** (root)
- **Music** → Master
- **SFX** → Master
- **UI** → Master

No manual configuration needed!

### 4. Audio Assets

Place your audio files in:

```
Assets/Audio/
├─ Music/
│  ├─ menu_theme.ogg
│  ├─ combat_theme.ogg
│  ├─ boss_theme.ogg
│  ├─ victory_theme.ogg
│  └─ game_over.ogg
└─ SFX/
   ├─ weapon_fire.ogg
   ├─ enemy_hit.ogg
   ├─ enemy_death.ogg
   ├─ loot_pickup.ogg
   ├─ ui_click.ogg
   ├─ level_up.ogg
   ├─ boss_roar.ogg
   └─ achievement.ogg
```

## Usage Examples

### Playing Sound Effects

```csharp
using MechDefenseHalo.Audio;

// Play 2D sound effect
AudioManager.Instance.PlaySound("weapon_fire");

// Play 3D positional sound
AudioManager.Instance.PlaySound("enemy_hit", enemyPosition);

// Play UI sound
AudioManager.Instance.PlayUISound("ui_click");

// Play with custom pitch
AudioManager.Instance.PlaySound("explosion", position, pitch: 0.9f);
```

### Music Transitions

Music automatically transitions based on game state events:

```csharp
// Emit game state change events
EventBus.Emit(EventBus.GameStateChanged, "InGame");  // Combat music
EventBus.Emit(EventBus.BossWaveStarted, bossData);   // Boss music
EventBus.Emit(EventBus.GameStateChanged, "Victory"); // Victory music
```

Manual control:

```csharp
// Access MusicController directly if needed
var musicController = AudioManager.Instance.GetNode<MusicController>("MusicController");
musicController.TransitionTo("boss_fight");
```

### Volume Control

```csharp
using MechDefenseHalo.Audio;

// Get AudioSettingsApplier
var settingsApplier = AudioManager.Instance.GetNode<AudioSettingsApplier>("AudioSettingsApplier");

// Set volumes (0.0 to 1.0 range)
settingsApplier.SetMasterVolume(0.8f);
settingsApplier.SetMusicVolume(0.7f);
settingsApplier.SetSFXVolume(0.9f);
settingsApplier.SetUIVolume(1.0f);
```

Or trigger via events:

```csharp
// Create settings data object
var audioSettings = new 
{
    MasterVolume = 0.8f,
    MusicVolume = 0.7f,
    SFXVolume = 0.9f,
    UIVolume = 1.0f
};

// Emit settings change event
EventBus.Emit(EventBus.AudioSettingsChanged, audioSettings);
```

### Audio Bus Control

```csharp
using MechDefenseHalo.Audio;

var busController = AudioManager.Instance.GetNode<AudioBusController>("AudioBusController");

// Set bus volume directly (in dB)
busController.SetBusVolume("Music", -10f);

// Mute/unmute a bus
busController.SetBusMute("SFX", true);

// Get bus volume
float volume = busController.GetBusVolume("Master");
```

## Event Integration

The audio system listens for these events:

### Game State Events
- `GameStateChanged` - Triggers music transitions
- `BossWaveStarted` - Switches to boss music
- `WaveCompleted` - Returns to combat music after boss

### Combat Events
- `WeaponFired` - Weapon sound
- `EntityDied` - Death sound
- Custom: `EnemyHit` - Impact sound

### UI Events
- `ButtonClicked` - UI click sound
- `LootPickedUp` - Pickup sound
- `PlayerLeveledUp` - Level up sound
- `AchievementUnlocked` - Achievement sound

### To trigger audio events:

```csharp
using MechDefenseHalo.Core;

// Weapon fired
EventBus.Emit(EventBus.WeaponFired, null);

// Enemy hit with position
EventBus.Emit("EnemyHit", new { Position = enemyPosition });

// Button clicked
EventBus.Emit(EventBus.ButtonClicked, null);
```

## Adding New Sounds

### 1. Add the audio file to Assets/Audio/SFX/

### 2. Register it in AudioManager.LoadSoundLibrary():

```csharp
soundLibrary["new_sound"] = LoadSound("res://Assets/Audio/SFX/new_sound.ogg");
```

### 3. Play it anywhere:

```csharp
AudioManager.Instance.PlaySound("new_sound");
```

## Performance Considerations

### Sound Effect Pooling

The system uses object pooling to prevent performance issues:

- **Initial 2D Pool Size**: 20 players
- **Initial 3D Pool Size**: 10 players
- Pool grows dynamically when exhausted
- Players are automatically returned to pool when finished

### Best Practices

1. **Use 3D audio for world sounds**: Weapon impacts, explosions, footsteps
2. **Use 2D audio for UI**: Button clicks, notifications
3. **Keep sound files optimized**: Use .ogg format, mono for 3D, stereo for music
4. **Limit simultaneous sounds**: The pool prevents audio spam automatically
5. **Use pitch variation**: Add variety without creating duplicate assets

## Music System Features

### Crossfading

- Smooth transitions between tracks
- Configurable fade duration (default: 2 seconds)
- Dual-player system prevents interruptions

### Looping

- Combat and menu music loop automatically
- Victory and game over tracks play once

### Volume Management

- Music plays on dedicated "Music" bus
- Independent volume control
- Respects master volume settings

## Troubleshooting

### No sound playing

1. Check AudioManager is in autoload
2. Verify audio files exist at specified paths
3. Check bus volumes aren't muted
4. Look for errors in console

### Audio crackling

1. Reduce simultaneous sound count
2. Use lower sample rate for SFX (22050 Hz is often sufficient)
3. Check system audio buffer settings

### Music not transitioning

1. Verify EventBus events are being emitted
2. Check music files exist
3. Ensure MusicController is child of AudioManager
4. Look for errors in console

## Testing

Run the audio system tests:

```bash
# In Godot editor
# Go to: Project → Tools → GdUnit4 → Run Tests
# Filter: AudioSystemTests
```

Tests cover:
- Bus creation and configuration
- Volume control
- Sound effect pooling
- Null safety
- Volume clamping

## Success Criteria

✅ Audio buses (Master/Music/SFX/UI) configured  
✅ Dynamic music transitions work smoothly  
✅ Sound effect pooling prevents performance issues  
✅ 3D positional audio for combat sounds  
✅ UI sounds play on separate bus  
✅ Music loops correctly  
✅ Volume settings apply immediately  
✅ No audio crackling or clipping  

## API Reference

### AudioManager

```csharp
public class AudioManager : Node
{
    static AudioManager Instance { get; }
    void PlaySound(string soundName, Vector3 position = default, float pitch = 1.0f)
    void PlayUISound(string soundName)
}
```

### MusicController

```csharp
public class MusicController : Node
{
    void TransitionTo(string trackName, bool loop = true)
}
```

### SoundEffectPool

```csharp
public class SoundEffectPool : Node
{
    void Play2D(AudioStream stream, float pitch = 1.0f)
    void Play3D(AudioStream stream, Vector3 position, float pitch = 1.0f)
    void PlayOnBus(AudioStream stream, string busName)
}
```

### AudioBusController

```csharp
public class AudioBusController : Node
{
    int GetBusIndex(string busName)
    void SetBusVolume(string busName, float volumeDb)
    float GetBusVolume(string busName)
    void SetBusMute(string busName, bool mute)
}
```

### AudioSettingsApplier

```csharp
public class AudioSettingsApplier : Node
{
    void SetMasterVolume(float volume)  // 0.0 to 1.0
    void SetMusicVolume(float volume)
    void SetSFXVolume(float volume)
    void SetUIVolume(float volume)
}
```

## Example: Complete Integration

Here's a complete example of integrating audio into a weapon:

```csharp
using Godot;
using MechDefenseHalo.Audio;
using MechDefenseHalo.Core;

public partial class AssaultRifle : Node3D
{
    public void Fire()
    {
        // Existing fire logic...
        
        // Play weapon sound at weapon position
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("weapon_fire", GlobalPosition);
        }
        
        // Or emit event for centralized handling
        EventBus.Emit(EventBus.WeaponFired, new { 
            WeaponType = "AssaultRifle",
            Position = GlobalPosition 
        });
    }
}
```

## Future Enhancements

Potential improvements:
- Dynamic music intensity based on combat
- Reverb zones for environmental effects
- Audio occlusion for 3D sounds
- Sound priority system
- Audio settings persistence
- Music playlist system
- Dynamic mixing based on player distance

## Support

For issues or questions:
1. Check console for error messages
2. Review this documentation
3. Examine test cases for usage examples
4. Check EventBus event names match your code
