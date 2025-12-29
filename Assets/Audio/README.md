# Audio Assets - Placeholder Files

## Overview
This directory contains **placeholder audio files** for the MechDefenseHalo game. All audio files are currently 1-second silent OGG Vorbis files to prevent loading errors in the audio system.

## Purpose
These placeholder files serve several important functions:
- **Prevent Runtime Errors**: Ensure `SoundLibrary.cs` can successfully load all registered sound paths
- **Enable Development**: Allow the game to run without actual audio assets
- **Define Structure**: Establish the complete audio asset organization for future implementation
- **Testing**: Enable audio system testing without requiring final audio assets

## Directory Structure

### Sound Effects (SFX)
```
SFX/
├── Weapons/       - Weapon firing, reloading, and swing sounds (7 files)
├── Impacts/       - Impact sounds for different materials (3 files)
├── Explosions/    - Explosion effects of varying sizes (3 files)
├── UI/            - User interface feedback sounds (6 files)
├── Enemies/       - Enemy attack and death sounds (3 files)
└── Drones/        - Drone deployment, attack, and ability sounds (4 files)
```

### Music
```
Music/
├── main_menu.ogg     - Main menu background music (looping)
├── combat.ogg        - Standard combat music (looping)
├── boss_fight.ogg    - Boss battle music (looping)
└── victory.ogg       - Victory celebration music (looping)
```

## File Format
- **Format**: OGG Vorbis
- **Sample Rate**: 44.1 kHz
- **Channels**: Stereo (2 channels)
- **Bit Depth**: 16-bit
- **Duration**: 1 second (silent)
- **Quality**: 3 (low quality for minimal file size)

## Replacing Placeholders

When replacing these placeholder files with actual audio:

1. **Maintain File Names**: Keep the exact same filenames as listed in `SoundLibrary.cs`
2. **Use OGG Format**: Keep using OGG Vorbis format for compatibility
3. **Preserve Structure**: Don't change the directory structure
4. **Update .import Files**: Godot will automatically regenerate `.import` files when you replace files in the editor
5. **Consider Loop Settings**: Music tracks should have `loop=true` in their `.import` files

## Sound Registry
All audio paths are registered in `/Scripts/Audio/SoundLibrary.cs`. This file contains the complete mapping of `SoundID` enums to file paths.

## Notes
- Music tracks are configured to loop by default
- Sound effects (SFX) are configured to play once
- All paths use the `res://` prefix for Godot resource system compatibility
- The `.import` files are required for Godot 4.3 to recognize and load OGG files

## Total Assets
- **30 total audio files**
  - 26 sound effects
  - 4 music tracks

---
*This is a placeholder asset collection. Replace with final audio assets during production.*
