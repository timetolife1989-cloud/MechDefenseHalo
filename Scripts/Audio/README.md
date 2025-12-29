# Audio System Integration

This directory contains the audio system integration components for MechDefenseHalo.

## 📁 Files

- **SoundLibrary.cs** - Centralized sound ID registry
- **MusicPlayer.cs** - Background music with crossfade
- **SFXPlayer.cs** - 3D positional sound effects
- **AudioSettings.cs** - Volume control & persistence

## 🎵 Usage Examples

### Playing Weapon Sounds

```csharp
using MechDefenseHalo.Audio;

// In your weapon's OnFire() method:
string soundPath = SoundLibrary.GetSound(SoundID.WeaponAssaultRifleFire);
var stream = GD.Load<AudioStream>(soundPath);
AudioManager.Instance.PlaySFX(stream);

// Or for 3D positional audio:
SFXPlayer.Instance.Play3D(SoundID.WeaponAssaultRifleFire, GlobalPosition);
```

### Playing UI Sounds

```csharp
using MechDefenseHalo.Audio;

// In button click handlers:
SFXPlayer.Instance.Play2D(SoundID.UIButtonClick);

// In inventory system:
SFXPlayer.Instance.Play2D(SoundID.UIItemPickup);
```

### Playing Background Music

```csharp
using MechDefenseHalo.Audio;

// Play combat music with 2-second crossfade:
MusicPlayer.Instance.PlayMusic(SoundID.MusicCombat, crossfadeDuration: 2.0f);

// Stop music with fade out:
MusicPlayer.Instance.StopMusic(fadeOutDuration: 1.0f);
```

### Volume Settings

```csharp
using MechDefenseHalo.Audio;

// Set volume (0.0 to 1.0):
AudioSettings.Instance.SetMasterVolume(0.8f);
AudioSettings.Instance.SetMusicVolume(0.6f);
AudioSettings.Instance.SetSFXVolume(0.9f);

// Save settings to disk:
AudioSettings.Instance.SaveSettings();

// Load settings from disk:
AudioSettings.Instance.LoadSettings();
```

## 🔧 Setup Instructions

### 1. Scene Setup

Add the following nodes to your AudioManager scene:

```
AudioManager (Node)
├─ MusicPlayer (Node)
│  ├─ MusicPlayer1 (AudioStreamPlayer)
│  └─ MusicPlayer2 (AudioStreamPlayer)
├─ SFXPlayer (Node)
└─ AudioSettings (Node)
```

### 2. Configure MusicPlayer

1. Select the MusicPlayer node
2. In the Inspector, assign:
   - `Player1` → MusicPlayer1
   - `Player2` → MusicPlayer2

### 3. Configure Audio Buses

Ensure your Godot project has these audio buses configured:
- Master (root bus)
- Music (child of Master)
- SFX (child of Master)

### 4. Import Audio Assets

Place your audio files in:
```
Assets/Audio/
├─ Music/
│  ├─ main_menu.ogg
│  ├─ combat.ogg
│  ├─ boss_fight.ogg
│  └─ victory.ogg
└─ SFX/
   ├─ Weapons/
   ├─ Impacts/
   ├─ Explosions/
   ├─ UI/
   ├─ Enemies/
   └─ Drones/
```

## 🎯 Sound IDs

All sounds are accessed via the `SoundID` enum:

### Weapons
- `WeaponAssaultRifleFire`
- `WeaponAssaultRifleReload`
- `WeaponPlasmaFire`
- `WeaponCryoFire`
- `WeaponTeslaFire`
- `WeaponSwordSwing`
- `WeaponHammerSwing`

### Impacts
- `ImpactMetal`
- `ImpactFlesh`
- `ImpactEnergy`

### Explosions
- `ExplosionSmall`
- `ExplosionMedium`
- `ExplosionLarge`

### UI
- `UIButtonClick`
- `UIButtonHover`
- `UIItemPickup`
- `UIItemEquip`
- `UICraftComplete`
- `UIPurchase`

### Enemies
- `EnemyGruntAttack`
- `EnemyShooterFire`
- `EnemyDeath`

### Drones
- `DroneDeploy`
- `DroneAttackFire`
- `DroneShieldActivate`
- `DroneRepairLoop`

### Music
- `MusicMainMenu`
- `MusicCombat`
- `MusicBoss`
- `MusicVictory`

## 🔊 Features

### SoundLibrary
- Type-safe sound ID enum
- Centralized path management
- Easy to extend with new sounds

### MusicPlayer
- Smooth crossfading between tracks
- Dual player system for seamless transitions
- Volume control
- Fade in/out support

### SFXPlayer
- 3D positional audio with distance attenuation
- 2D UI sounds
- Pitch variation support
- Auto-cleanup of finished sounds
- Max simultaneous sounds limiting

### AudioSettings
- Persistent volume settings
- Separate Master, Music, and SFX controls
- Audio bus integration
- Settings saved to `user://audio_settings.cfg`

## 📝 Notes

- Audio files must be in .ogg format for music (streaming)
- SFX can be .ogg or .wav
- All audio components use singleton pattern for easy access
- Settings are automatically loaded on startup
- Volume ranges from 0.0 (muted) to 1.0 (full volume)
