# Placeholder Model Specifications

## Scale Reference
- **Player Height**: 2.0m (standard human in armor)
- **Unit System**: Meters (Unity default)

## Weapon Models

### Assault Rifle (MA5 series)
- **Dimensions**: 1.0m × 0.3m × 0.2m
- **Shape**: Rectangular box (elongated)
- **Color**: Dark Gray (#3C4041)
- **Pivot Point**: Rear center (grip position) - local transform origin for player hand attachment

### Sniper Rifle (SRS99)
- **Dimensions**: 1.5m × 0.2m × 0.2m
- **Shape**: Long thin box
- **Color**: Dark Gray (#3C4041)
- **Pivot Point**: Center of mass

### Rocket Launcher (SPNKr)
- **Dimensions**: 0.8m × 0.4m × 0.3m
- **Shape**: Thick rectangular box
- **Color**: Olive Green (#4A5D23)
- **Pivot Point**: Center rear

## Enemy Models

### Grunt (Unggoy)
- **Type**: Sphere
- **Diameter**: 1.2m
- **Color**: Purple (#7B2D8E)
- **Height**: 1.2m (short enemy)

### Elite (Sangheili)
- **Type**: Capsule (cylinder + rounded ends)
- **Dimensions**: 0.6m diameter × 2.5m tall
- **Color**: Blue (#2E4A9B)
- **Height**: 2.5m (tall enemy)

### Hunter (Mgalekgolo)
- **Type**: Cylinder
- **Dimensions**: 1.2m diameter × 3.5m tall
- **Color**: Orange (#FF8C00)
- **Height**: 3.5m (large enemy)

## Turret Models

### Machine Gun Turret
- **Base**: Cylinder 1m diameter × 0.3m tall
- **Gun**: Box 0.8m × 0.2m × 0.2m on top
- **Color**: Gray (#6B6F6D)
- **Total Height**: 1.0m

### Rocket Turret
- **Base**: Cylinder 1m diameter × 0.3m tall
- **Launcher**: Box 0.6m × 0.4m × 0.4m
- **Color**: Dark Gray (#3C4041)
- **Total Height**: 1.2m

## Creation Instructions (Unity)

### Using Unity Primitives:
1. Create GameObject → 3D Object → Cube/Sphere/Cylinder/Capsule
2. Scale to match specifications
3. Create material with specified color
4. Apply material to model
5. Save as prefab in `Assets/Prefabs/Placeholders/`

### Naming Convention:
- `[Type]_Placeholder` (e.g., `AssaultRifle_Placeholder`)
- Prefabs: `[Type]_Placeholder.prefab`
- Materials: `Mat_[ColorName].mat`
