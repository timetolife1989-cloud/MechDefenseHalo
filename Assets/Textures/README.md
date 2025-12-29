# UI Texture Placeholders

This directory contains placeholder textures for the MechDefenseHalo UI system.

## Directory Structure

```
Assets/Textures/
├── UI/
│   ├── Rarity/          - Rarity border textures (64x64)
│   ├── Icons/           - Item category icons (64x64)
│   └── Elements/        - UI element textures (various sizes)
└── Items/               - Placeholder item icons (64x64)
```

## Rarity Borders (64x64 PNG)

Located in `Assets/Textures/UI/Rarity/`

All borders match colors from `Scripts/Items/ItemRarity.cs` (RarityConfig class):

- `border_common.png` - Grey (#999999 / RGB 153,153,153)
- `border_uncommon.png` - Green (#33CC33 / RGB 51,204,51)
- `border_rare.png` - Blue (#3366FF / RGB 51,102,255)
- `border_epic.png` - Purple (#9933CC / RGB 153,51,204)
- `border_legendary.png` - Orange (#FF9900 / RGB 255,153,0)
- `border_exotic.png` - Rainbow gradient (dynamic effect)
- `border_mythic.png` - Prismatic shimmer (dynamic effect)

## Item Category Icons (64x64 PNG)

Located in `Assets/Textures/UI/Icons/`

- `icon_weapon.png` - Crossed swords symbol
- `icon_armor.png` - Shield symbol
- `icon_drone.png` - Hexagon symbol
- `icon_consumable.png` - Potion bottle
- `icon_material.png` - Isometric cube
- `icon_cosmetic.png` - Gold star
- `icon_currency_credits.png` - Gold coin
- `icon_currency_cores.png` - Blue gem/crystal

## UI Elements (Various Sizes)

Located in `Assets/Textures/UI/Elements/`

- `button_normal.png` (128x48) - Grey button base
- `button_hover.png` (128x48) - Light grey hover state
- `button_pressed.png` (128x48) - Dark grey pressed state
- `panel_background.png` (256x256) - Semi-transparent dark panel
- `slot_background.png` (70x70) - Dark inventory slot background
- `crosshair.png` (32x32) - White crosshair overlay
- `healthbar_fill.png` (200x20) - Red gradient health bar fill

## Placeholder Item Icons (64x64 PNG)

Located in `Assets/Textures/Items/`

### Basic Placeholders
- `placeholder_weapon.png` - Grey gun shape
- `placeholder_armor.png` - Grey armor piece with shoulder pads
- `placeholder_consumable.png` - Grey with "CONS" text
- `placeholder_drone.png` - Grey with "DRON" text
- `placeholder_cosmetic.png` - Grey with "COSM" text
- `placeholder_weaponmod.png` - Grey with "WEAP" text
- `placeholder_mechpart.png` - Grey with "MECH" text

### Material Placeholders (Colored by Type)
- `placeholder_material_common.png` - Grey stacked blocks
- `placeholder_material_metal.png` - Silver-blue stacked blocks
- `placeholder_material_crystal.png` - Cyan stacked blocks
- `placeholder_material_organic.png` - Green stacked blocks
- `placeholder_material_tech.png` - Orange stacked blocks

## Usage in Scripts

These textures can be referenced in Godot scripts using:

```csharp
// Load a texture
var texture = GD.Load<Texture2D>("res://Assets/Textures/UI/Rarity/border_legendary.png");

// Assign to a UI element
myTextureRect.Texture = texture;
```

Or in Godot scenes by selecting the texture file from the FileSystem dock.

## Import Settings

All textures are configured with the following Godot import settings:
- **Filter**: Enabled (for smooth scaling)
- **Mipmaps**: Disabled (not needed for UI)
- **Compression**: Disabled (for quality)
- **Format**: RGBA8

Each texture has a corresponding `.import` file that Godot uses to configure these settings.

## Replacing Placeholders

These are placeholder assets meant to be replaced with final artwork. When replacing:

1. Keep the same file names and dimensions for compatibility
2. Maintain PNG format with alpha transparency
3. Preserve the color schemes for rarity borders (or update RarityConfig.cs)
4. Test in-game to ensure proper scaling and visibility

## Color Reference

Rarity colors in code (Godot Color format -> RGB):
```csharp
Common:    Color(0.6, 0.6, 0.6)    -> RGB(153, 153, 153)
Uncommon:  Color(0.2, 0.8, 0.2)    -> RGB(51, 204, 51)
Rare:      Color(0.2, 0.4, 1.0)    -> RGB(51, 102, 255)
Epic:      Color(0.6, 0.2, 0.8)    -> RGB(153, 51, 204)
Legendary: Color(1.0, 0.6, 0.0)    -> RGB(255, 153, 0)
Exotic:    Color(1.0, 0.8, 0.2)    -> RGB(255, 204, 51) [Base color]
Mythic:    Color(1.0, 0.2, 1.0)    -> RGB(255, 51, 255) [Base color]
```

**Note**: Exotic and Mythic use gradient/shimmer effects in addition to their base colors.
