# Placeholder Asset Guide

This document describes the temporary placeholder assets used during development.

## 3D Models (Assets/Art/Models/Placeholders/)

### UNSC Weapons
- **AssaultRifle_Placeholder**: Gray cube (1x0.3x0.2m) - MA5 series placeholder
- **SniperRifle_Placeholder**: Dark gray elongated box (1.5x0.2x0.2m) - SRS99 placeholder
- **RocketLauncher_Placeholder**: Green-gray box (0.8x0.4x0.3m) - SPNKr placeholder

### Enemies
- **CovenantGrunt_Placeholder**: Purple sphere (1.2m diameter) - Unggoy placeholder
- **CovenantElite_Placeholder**: Blue-purple capsule (2.5m tall) - Sangheili placeholder
- **CovenantHunter_Placeholder**: Orange-gray cylinder (3.5m tall) - Mgalekgolo placeholder

### Turrets
- **MachineGunTurret_Placeholder**: Gray cylinder + box (1m height) - UNSC turret base
- **RocketTurret_Placeholder**: Dark gray box + cylinder (1.2m) - Missile pod placeholder

**Material**: Standard Unity material with flat colors (no textures initially)

## Textures (Assets/Art/Textures/Placeholders/)

### UNSC Color Palette (256x256 solid color textures)
- **UNSC_OliveGreen.png**: #4A5D23 - Primary UNSC color
- **UNSC_Gray.png**: #6B6F6D - Secondary armor color
- **UNSC_DarkGray.png**: #3C4041 - Weapons/tech color
- **UNSC_Orange.png**: #FF6A00 - Warning/highlight color

### Covenant Color Palette
- **Covenant_Purple.png**: #7B2D8E - Grunt armor
- **Covenant_Blue.png**: #2E4A9B - Elite shields
- **Covenant_Orange.png**: #FF8C00 - Hunter armor
- **Covenant_Cyan.png**: #00CED1 - Plasma glow

**Format**: PNG, 256x256, solid colors

## VFX (Assets/Art/VFX/Placeholders/)

### Particle Systems
- **MuzzleFlash_Placeholder**: Yellow-orange sprite burst (0.2s duration)
- **BulletImpact_Placeholder**: Gray dust particle spray
- **PlasmaShot_Placeholder**: Cyan glowing sphere trail
- **Explosion_Placeholder**: Orange-red expanding sphere with fade

**Note**: Use Unity's built-in particle system with colored sprites

## Audio (Assets/Audio/)

### SFX Placeholders
- **Gunshot_Placeholder.wav**: Short sharp noise burst
- **Explosion_Placeholder.wav**: Deep rumble with decay
- **PlasmaFire_Placeholder.wav**: High-pitched electric zap
- **EnemyDeath_Placeholder.wav**: Descending tone

### Music Placeholders
- **CombatMusic_Placeholder.wav**: Loopable 120 BPM percussion track
- **AmbientMusic_Placeholder.wav**: Atmospheric drone

**Format**: WAV, 44.1kHz, mono (SFX) / stereo (music)

**Source**: Use royalty-free sound packs from:
- [Freesound.org](https://freesound.org)
- [OpenGameArt.org](https://opengameart.org)

## Replacement Strategy

When replacing placeholders with final assets:

1. Keep same file names (update content only)
2. Or use Unity's asset reference system to swap
3. Document replacements in `ASSET_CHANGELOG.md`

## Testing

- All placeholders should be clearly distinguishable by color
- Models should have correct scale relative to player (2m tall)
- Textures should tile without seams (if used for tiling)
- Audio should have proper volume normalization (-6dB peak)

## Next Steps

1. Create basic Unity prefabs from placeholder models
2. Apply placeholder textures to models
3. Set up VFX particle systems
4. Import audio files to AudioManager
5. Test in-game visibility and performance
