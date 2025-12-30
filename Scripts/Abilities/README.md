# Ability System

A comprehensive special ability system for the MechDefenseHalo game, featuring cooldowns, energy costs, visual effects, and an upgrade system.

## Overview

The Ability System provides players with 4 unique special abilities that can be activated using hotkeys. Each ability has:
- Cooldown timer to prevent spam
- Energy cost for resource management
- Visual effects for feedback
- Upgrade system for progression
- UI integration for status display

## Architecture

### Core Components

```
Scripts/Abilities/
├── AbilityBase.cs          # Abstract base class for all abilities
├── AbilitySystem.cs        # Main controller managing all abilities
├── CooldownManager.cs      # Tracks cooldowns for abilities
├── DashAbility.cs          # Quick movement burst ability
├── ShieldAbility.cs        # Protective shield ability
├── EMPAbility.cs           # Area-of-effect stun ability
└── TimeSlowAbility.cs      # Time manipulation ability
```

### UI Components

```
Scripts/UI/
└── AbilityBarUI.cs         # Displays ability icons, cooldowns, and status
```

### Test Suite

```
Tests/Abilities/
├── AbilitySystemTests.cs      # Tests for main system
├── CooldownManagerTests.cs    # Tests for cooldown tracking
└── AbilityTests.cs            # Tests for individual abilities
```

## Abilities

### 1. Tactical Dash (Hotkey: 1)
- **Type**: Movement
- **Cooldown**: 5 seconds
- **Energy Cost**: 15
- **Effect**: Rapidly dash in the movement direction with a burst of speed
- **Visual**: Green trail effect

### 2. Energy Shield (Hotkey: 2)
- **Type**: Defensive
- **Cooldown**: 12 seconds
- **Energy Cost**: 25
- **Effect**: Deploy an energy shield that absorbs 200 damage for 5 seconds
- **Visual**: Blue energy bubble

### 3. EMP Blast (Hotkey: 3)
- **Type**: Offensive/Utility
- **Cooldown**: 15 seconds
- **Energy Cost**: 30
- **Effect**: Release an electromagnetic pulse that stuns and damages enemies within 10m
- **Damage**: 50
- **Stun Duration**: 3 seconds
- **Visual**: Expanding blue energy ring with lightning effects

### 4. Temporal Field (Hotkey: 4)
- **Type**: Tactical
- **Cooldown**: 20 seconds
- **Energy Cost**: 40
- **Effect**: Create a field that slows time for enemies within 15m radius
- **Duration**: 8 seconds
- **Time Scale**: 30% normal speed
- **Visual**: Purple distortion field with swirling particles

## Setup

### 1. Add AbilitySystem to Player

In your player scene hierarchy:

```
PlayerMechController (CharacterBody3D)
└── AbilitySystem (Node)
```

The AbilitySystem automatically:
- Registers all 4 abilities on `_Ready()`
- Creates a CooldownManager child node
- Connects to parent's energy system
- Sets up input handling

### 2. Add UI to HUD

Add the AbilityBarUI to your HUD scene:

```gdscript
# In your HUD scene
var ability_bar = AbilityBarUI.new()
ability_bar.AbilitySystemPath = NodePath("../PlayerMechController/AbilitySystem")
add_child(ability_bar)
```

Or configure in the Godot editor:
1. Add `AbilityBarUI` node to HUD
2. Set `Ability System Path` to point to the AbilitySystem node
3. Configure `Show Hotkeys` and `Show Energy Costs` as desired

## Usage

### Using Abilities (Player)

Abilities are mapped to number keys 1-4:
- **1**: Dash
- **2**: Shield
- **3**: EMP
- **4**: Time Slow

Press the corresponding key to activate an ability if:
- Not on cooldown
- Sufficient energy available
- Custom conditions met (e.g., Dash requires being on ground)

### Programmatic Usage

```csharp
// Get the ability system
var abilitySystem = GetNode<AbilitySystem>("AbilitySystem");

// Use ability by index
abilitySystem.UseAbility(0); // Use Dash

// Use ability by ID
abilitySystem.UseAbilityById("shield"); // Use Shield

// Check if ability is ready
bool ready = abilitySystem.GetAbilityReady(2); // Check EMP

// Get cooldown info
float progress = abilitySystem.GetAbilityCooldownProgress(3);
float remaining = abilitySystem.GetAbilityRemainingCooldown(3);

// Upgrade ability
abilitySystem.UpgradeAbility(0); // Upgrade Dash
```

### Signals

The AbilitySystem emits signals you can connect to:

```csharp
// Connect to ability events
abilitySystem.AbilityUsed += OnAbilityUsed;
abilitySystem.AbilityCooldownStarted += OnCooldownStarted;
abilitySystem.AbilityUpgraded += OnAbilityUpgraded;

private void OnAbilityUsed(string abilityId, string abilityName)
{
    GD.Print($"Used {abilityName}!");
}

private void OnCooldownStarted(string abilityId, float duration)
{
    GD.Print($"Cooldown started: {duration}s");
}

private void OnAbilityUpgraded(string abilityId, int newLevel)
{
    GD.Print($"Upgraded to level {newLevel}");
}
```

## Energy Integration

The system integrates with `PlayerMechController`'s energy system:

- Checks `CurrentEnergy` property before using abilities
- Calls `ConsumeEnergy(float cost)` method when ability is used
- Falls back to default values if energy system not found

Ensure your PlayerMechController has:
```csharp
public float CurrentEnergy { get; }
public void ConsumeEnergy(float cost) { }
```

## Upgrade System

Abilities can be upgraded to improve their effectiveness:

```csharp
// Upgrade an ability
abilitySystem.UpgradeAbility(0);

// Each upgrade level (starting at 0):
// - Reduces cooldown by 5% (min 50% of base)
// - Reduces energy cost by 5% (min 50% of base)
```

Example progression:
- **Level 0**: 5s cooldown, 15 energy
- **Level 1**: 4.75s cooldown, 14.25 energy
- **Level 2**: 4.5s cooldown, 13.5 energy
- **Level 10**: 2.5s cooldown, 7.5 energy (minimum)

## Customization

### Creating New Abilities

To add a new ability:

1. Create a class inheriting from `AbilityBase`:

```csharp
using Godot;
using MechDefenseHalo.Abilities;

public class CustomAbility : AbilityBase
{
    public CustomAbility()
    {
        AbilityId = "custom";
        AbilityName = "Custom Ability";
        Description = "Does something cool";
        Cooldown = 10.0f;
        EnergyCost = 20f;
        IconPath = "res://Assets/UI/Icons/ability_custom.png";
    }
    
    public override void Execute(Node3D user)
    {
        // Implement ability effect
        GD.Print("Custom ability activated!");
    }
    
    public override bool CanUse(Node3D user)
    {
        // Add custom conditions
        return true;
    }
}
```

2. Register in `AbilitySystem.RegisterAbilities()`:

```csharp
private void RegisterAbilities()
{
    _abilities.Add(new DashAbility());
    _abilities.Add(new ShieldAbility());
    _abilities.Add(new EMPAbility());
    _abilities.Add(new TimeSlowAbility());
    _abilities.Add(new CustomAbility()); // Add your ability
}
```

### Customizing Visual Effects

Each ability creates its own visual effects. To customize:

1. Modify the `CreateXXXEffect()` methods in each ability class
2. Use `VFXManager.Instance.PlayEffect()` for standardized effects
3. Create custom particle systems and meshes for unique visuals

Example:
```csharp
private void CreateCustomEffect(Node3D user)
{
    // Use VFX manager if available
    if (VFXManager.Instance != null)
    {
        VFXManager.Instance.PlayEffect("custom_effect", user.GlobalPosition);
    }
    
    // Or create custom particles
    var particles = new GpuParticles3D
    {
        Emitting = true,
        OneShot = true,
        Amount = 30,
        Lifetime = 1.0f
    };
    user.GetParent().AddChild(particles);
}
```

## Testing

Run the test suite to verify functionality:

```bash
# In Godot editor: GdUnit4 > Run Tests

# Or via command line
godot --headless --script addons/gdUnit4/bin/GdUnitCmdTool.gd --test=res://Tests/Abilities/
```

### Test Coverage

- **CooldownManagerTests**: Cooldown tracking, progress, reset
- **AbilitySystemTests**: Registration, usage, upgrades, queries
- **AbilityTests**: Individual ability properties and behavior

## Performance Considerations

- Cooldowns update every frame but use efficient dictionary lookups
- Visual effects use object pooling through VFXManager
- Enemy queries use spatial partitioning (PhysicsShapeQueryParameters3D)
- UI updates are minimal, only modifying changed values

## Troubleshooting

### Abilities Not Working

1. **Check AbilitySystem is attached**: Verify it's a child of PlayerMechController
2. **Check energy system**: Ensure PlayerMechController has energy properties
3. **Check input mapping**: Verify weapon_1-4 actions are mapped in project settings
4. **Check console**: Look for error messages about missing references

### UI Not Showing

1. **Check AbilitySystemPath**: Must point to valid AbilitySystem node
2. **Check visibility**: Ensure AbilityBarUI is visible in scene tree
3. **Check parent**: Must be in a CanvasLayer or Control hierarchy

### Visual Effects Missing

1. **Check VFXManager**: Should be available as autoload singleton
2. **Check particle resources**: Ensure GPU particles are supported
3. **Check console**: Look for warnings about missing effects

## Future Enhancements

Potential improvements for the ability system:

- [ ] Save/load ability upgrade levels
- [ ] Ability unlocking system (start with fewer abilities)
- [ ] Custom ability combinations/synergies
- [ ] Ability modifiers (alternative effects)
- [ ] Sound effects integration
- [ ] Animation state machine integration
- [ ] Ability statistics tracking
- [ ] Visual ability trees for upgrades

## API Reference

### AbilityBase

Base class for all abilities.

**Properties**:
- `AbilityId`: Unique identifier
- `AbilityName`: Display name
- `Description`: Ability description
- `Cooldown`: Base cooldown in seconds
- `EnergyCost`: Base energy cost
- `IconPath`: Path to icon texture
- `UpgradeLevel`: Current upgrade level

**Methods**:
- `Execute(Node3D user)`: Execute the ability
- `CanUse(Node3D user)`: Check if ability can be used
- `GetModifiedCooldown()`: Get cooldown with upgrades
- `GetModifiedEnergyCost()`: Get energy cost with upgrades

### AbilitySystem

Main controller for the ability system.

**Methods**:
- `UseAbility(int index)`: Use ability by slot
- `UseAbilityById(string id)`: Use ability by ID
- `GetAbility(int index)`: Get ability reference
- `GetAbilityById(string id)`: Get ability by ID
- `GetAbilityReady(int index)`: Check if ready
- `GetAbilityCooldownProgress(int index)`: Get progress (0-1)
- `GetAbilityRemainingCooldown(int index)`: Get seconds remaining
- `UpgradeAbility(int index)`: Upgrade ability
- `GetAbilityCount()`: Get total ability count

**Signals**:
- `AbilityUsed(string abilityId, string abilityName)`
- `AbilityCooldownStarted(string abilityId, float duration)`
- `AbilityUpgraded(string abilityId, int newLevel)`

### CooldownManager

Manages cooldown timers for abilities.

**Methods**:
- `StartCooldown(string id, float duration)`: Start cooldown
- `IsOnCooldown(string id)`: Check if on cooldown
- `GetRemainingCooldown(string id)`: Get seconds remaining
- `GetCooldownProgress(string id)`: Get progress (0-1)
- `ResetCooldown(string id)`: Reset specific cooldown
- `ResetAllCooldowns()`: Reset all cooldowns

## License

Part of the MechDefenseHalo project.
