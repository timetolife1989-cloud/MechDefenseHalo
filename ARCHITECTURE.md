# MechDefenseHalo Architecture

## Overview

MechDefenseHalo follows a **thin-layer modular architecture** designed for maintainability, testability, and AI-assisted development. The architecture emphasizes decoupling, composition, and event-driven communication.

## Core Design Principles

### 1. Thin-Layer Modular Architecture

Each system is designed as a thin, focused layer with a single responsibility. This approach:
- Reduces complexity in individual modules
- Makes code easier to understand and modify
- Facilitates parallel development
- Enables incremental testing

### 2. Event Bus Pattern

All major systems communicate through a centralized **Event Bus** rather than direct coupling:

```
Component A --[Event]--> Event Bus --[Event]--> Component B
```

**Benefits:**
- Systems remain decoupled and independent
- Easy to add/remove listeners without changing emitters
- Simplifies testing (can mock the event bus)
- Clear data flow patterns

**Event Bus Location:** `res://_Core/EventBus.cs`

### 3. Composition Over Inheritance

Rather than deep inheritance hierarchies, we use:
- **Component-based architecture:** Small, reusable components attached to nodes
- **Interfaces for contracts:** Define behavior without implementation coupling
- **Dependency injection:** Pass dependencies explicitly

**Example:**
```csharp
// Bad: Deep inheritance
class Mech : Unit : Entity : Node3D { }

// Good: Composition with components
class Mech : Node3D
{
    private HealthComponent health;
    private MovementComponent movement;
    private WeaponComponent weapons;
}
```

## Folder Structure

### res://_Core/
**Purpose:** Singletons, global configuration, and the event bus

Contains:
- `EventBus.cs` - Central event dispatcher
- `GameConfig.cs` - Global game configuration
- `ServiceLocator.cs` - Dependency injection container

**Naming Convention:** Prefix with underscore to appear first in Godot file browser

### res://Assets/
**Purpose:** Raw asset files (models, textures, audio, shaders)

Structure:
```
Assets/
├── Models/         # .fbx, .gltf, .blend mech and environment models
├── Textures/       # .png, .jpg texture maps (albedo, normal, roughness)
├── Audio/          # .wav, .ogg sound effects and music
└── Shaders/        # Custom .gdshader files for visual effects
```

**Guidelines:**
- Keep original asset files separate from Godot resources
- Use subfolders for organization (e.g., `Models/Mechs/`, `Models/Enemies/`)
- Follow UNSC aesthetic: military grey/green, worn materials

### res://Components/
**Purpose:** Reusable logic components that can be attached to any node

Examples:
- `HealthComponent.cs` - Manages HP, damage, death
- `MovementComponent.cs` - Handles movement logic
- `WeaponComponent.cs` - Manages firing, ammo, cooldowns
- `TargetingComponent.cs` - AI targeting logic

**Design Pattern:**
```csharp
public partial class HealthComponent : Node
{
    [Export] public float MaxHealth { get; set; } = 100f;
    public float CurrentHealth { get; private set; }

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float amount)
    {
        CurrentHealth -= amount;
        EventBus.Emit("health_changed", this);
        
        if (CurrentHealth <= 0)
        {
            EventBus.Emit("entity_died", GetParent());
        }
    }
}
```

### res://GamePlay/
**Purpose:** Game-specific mechanics and systems

Structure:
```
GamePlay/
├── Cockpit/        # First-person cockpit view, HUD, controls
├── TowerDefense/   # Top-down strategy, wave spawning, base defense
└── Units/          # Mech and enemy definitions (compositions of components)
```

**Separation of Concerns:**
- `Cockpit/` - Immersive piloting experience
- `TowerDefense/` - Strategic gameplay systems
- `Units/` - Unit definitions (combine components)

### res://Scenes/
**Purpose:** Godot scene files (.tscn)

Contains:
- `Main.tscn` - Entry point scene
- `MainMenu.tscn` - Main menu UI
- Level scenes
- Prefab scenes (reusable unit templates)

**Naming Convention:**
- PascalCase for scene names
- Descriptive names (e.g., `Level01_Desert.tscn`)

### res://Scripts/
**Purpose:** C# scripts not tied to specific components or gameplay

Contains:
- Autoload scripts
- Manager classes
- State machines
- Data structures

### res://UI/
**Purpose:** User interface scenes and scripts

Contains:
- Menu systems
- HUD elements
- Dialogs and popups
- UI components (buttons, panels, etc.)

**Guidelines:**
- Separate UI logic from game logic
- Use signals/events for UI interactions
- Follow UNSC aesthetic (military, tactical design)

### res://Utils/
**Purpose:** Helper scripts and utility functions

Contains:
- Math utilities
- Extension methods
- Debug tools
- Serialization helpers

## System Communication

### Event-Driven Architecture

All inter-system communication uses the Event Bus:

```csharp
// Emitting events
EventBus.Emit("enemy_spawned", enemyNode);
EventBus.Emit("wave_completed", waveNumber);

// Listening to events
EventBus.On("player_damaged", OnPlayerDamaged);

private void OnPlayerDamaged(object data)
{
    // Handle player damage
}
```

### Common Events

| Event Name | Data | Purpose |
|------------|------|---------|
| `game_started` | null | Game begins |
| `game_paused` | bool | Game pause state |
| `wave_started` | int | New wave begins |
| `enemy_spawned` | Node | Enemy created |
| `entity_died` | Node | Entity destroyed |
| `player_damaged` | float | Player takes damage |
| `resource_changed` | (string, int) | Resource updated |

## Code Style Guidelines

### C# Conventions (EditorConfig)
- **Indentation:** 4 spaces (no tabs)
- **Line endings:** LF (Unix-style)
- **Charset:** UTF-8
- **Brace style:** Allman (new line for braces)
- **Naming:**
  - PascalCase for public members
  - camelCase for private fields (prefix with underscore: `_privateField`)
  - PascalCase for methods

### Godot-Specific Patterns
```csharp
// Use partial classes for Godot nodes
public partial class MyNode : Node3D
{
    // Export properties for editor
    [Export] public float Speed { get; set; } = 5.0f;
    
    // Node references
    private Node3D _targetNode;
    
    public override void _Ready()
    {
        // Initialize on scene ready
    }
    
    public override void _Process(double delta)
    {
        // Per-frame logic
    }
}
```

## Testing Strategy

- **Unit tests:** Test components in isolation
- **Integration tests:** Test system interactions via Event Bus
- **Scene tests:** Test complete scene functionality
- **Manual testing:** Playtest gameplay feel and balance

## Future Considerations

### Firebase Integration
- Wrap Firebase calls in service classes
- Use async/await for network operations
- Cache data locally for offline play

### Monetization
- AdMob integration via plugin
- In-app purchase handling
- Non-intrusive ad placement

### Performance
- Object pooling for frequently spawned entities
- LOD (Level of Detail) for distant objects
- Optimize draw calls for mobile

## Summary

This architecture enables:
- ✅ Clean separation of concerns
- ✅ Easy AI-assisted development
- ✅ Testable, maintainable code
- ✅ Scalable for future features
- ✅ Mobile-optimized structure

Follow these patterns consistently for best results.
