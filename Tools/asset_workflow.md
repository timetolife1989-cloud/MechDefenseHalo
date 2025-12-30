# Asset Workflow Guide

## Directory Structure

```
Assets/
├── Models/
│   ├── Enemies/
│   ├── Weapons/
│   ├── Items/
│   └── Environment/
├── Textures/
│   ├── Enemies/
│   ├── Weapons/
│   └── Environment/
├── Audio/
│   ├── SFX/
│   ├── Music/
│   └── Voice/
└── VFX/
```

## Naming Convention

**Format:** `category_name_variant_detail.ext`

### Examples:
- `enemy_grunt_v01.blend`
- `weapon_rifle_laser_v02.fbx`
- `texture_metal_rusty_diffuse.png`
- `audio_sfx_explosion_large.wav`

## Import Pipeline

1. Place asset in appropriate folder
2. System validates naming and format
3. Import preset applied automatically
4. Version tracked in Git LFS
5. Ready to use in scenes

## Best Practices

- Keep models under 10k polygons (mobile target)
- Textures power-of-2 (512, 1024, 2048)
- Audio files compressed (OGG format)
- Commit with descriptive messages

## Tools

- **Blender:** 3D modeling
- **GIMP/Photoshop:** Texture editing
- **Audacity:** Audio editing
- **Git LFS:** Large file versioning
