# Debug Tools Suite

Comprehensive debug tools for rapid testing and development iteration in MechDefenseHalo.

## Features

### 1. Debug Console
**Toggle:** Press `~` (Tilde key)

A full-featured command-line interface for in-game debugging and testing.

#### Available Commands

| Command | Arguments | Description |
|---------|-----------|-------------|
| `help` | - | Show list of all commands |
| `god` | - | Toggle god mode (invulnerability) |
| `levelup` | `[levels]` | Gain levels (default: 1) |
| `spawn` | `<type>` | Spawn enemy (Grunt, Tank, Flyer, Swarm, Shooter) |
| `giveitem` | `<item_id> [amount]` | Give item to inventory |
| `skipwave` | - | Skip to next wave |
| `wave` | `<number>` | Jump to specific wave number |
| `timescale` | `<value>` | Set time scale (0.1-10.0) |
| `noclip` | - | Toggle noclip mode |
| `credits` | `[amount]` | Add credits (default: 1000) |
| `cores` | `[amount]` | Add cores (default: 100) |
| `kill` | - | Kill all enemies |
| `tp` | `<x> <y> <z>` | Teleport player to coordinates |
| `heal` | - | Heal player to full |
| `clear` | - | Clear console output |

#### Usage Examples

```
> levelup 5          # Gain 5 levels
> spawn Tank         # Spawn a Tank enemy
> giveitem scrap 100 # Give 100 scrap
> credits 5000       # Add 5000 credits
> timescale 0.5      # Slow motion (50% speed)
> tp 10 5 20         # Teleport to (10, 5, 20)
```

### 2. Free Camera
**Toggle:** Press `F7`

Detach from player and explore the scene freely.

**Controls:**
- `WASD` - Move horizontally
- `Q/E` - Move up/down
- `Shift` - Speed boost
- `Mouse` - Look around

### 3. Performance Profiler
**Toggle:** Press `F8`

Real-time performance monitoring overlay.

**Displays:**
- FPS (color-coded: green ≥60, yellow ≥30, red <30)
- Frame time (ms)
- Physics time (ms)
- Render time (ms)
- Draw calls
- Vertices rendered
- Memory usage (MB)
- Enemy count

### 4. Debug Hotkeys
Quick access to common debug functions via function keys.

| Hotkey | Function |
|--------|----------|
| `F1` | Toggle God Mode |
| `F2` | Level Up (+1) |
| `F3` | Spawn Random Enemy |
| `F4` | Give Test Resources (5000 credits, 500 cores) |
| `F5` | Skip to Next Wave |
| `F6` | Toggle Slow Motion (0.3x / 1.0x) |
| `F7` | Toggle Free Camera |
| `F8` | Toggle Performance Stats |
| `~` | Toggle Debug Console |

## Implementation Details

### File Structure

```
Scripts/Debug/
├── DebugConsole.cs          # Console UI and input handling
├── DebugCommands.cs         # Command execution system
├── FreeCamera.cs            # Free camera controller
├── PerformanceProfiler.cs   # Performance monitoring
└── DevModeManager.cs        # Hotkey management

UI/Debug/
├── DebugOverlay.tscn        # Console UI scene
└── PerformanceGraph.tscn    # Performance stats UI scene
```

### Auto-Disable in Release Builds

All debug tools are automatically disabled in release builds using conditional compilation:

```csharp
#if !DEBUG && !TOOLS
QueueFree();
return;
#endif
```

This ensures zero overhead and prevents debug tools from appearing in production builds.

### Integration

The debug tools integrate seamlessly with existing game systems:

- **Player System**: Access to player stats, health, position
- **Wave System**: Wave control, enemy spawning
- **Inventory System**: Item management
- **Economy System**: Credits and cores management
- **Level System**: XP and level progression

### Scene Structure Flexibility

The debug tools avoid hard-coded scene paths and use flexible node discovery:

```csharp
// Tries multiple common locations
Node parentNode = GetTree().CurrentScene;
if (parentNode == null)
{
    parentNode = GetTree().Root;
}
```

## Usage in Development

### Testing Combat
```
> spawn Tank
> spawn Flyer
> god           # Enable god mode
> timescale 0.3 # Slow motion for observation
```

### Testing Economy
```
> credits 10000
> cores 1000
> giveitem plasma_core 50
```

### Testing Progression
```
> levelup 10
> skipwave
> wave 15
```

### Performance Testing
Press `F8` to monitor performance while spawning enemies:
```
> spawn Grunt
> spawn Grunt
> spawn Grunt
# Watch FPS, memory, draw calls
```

## Benefits

✅ **Rapid Testing** - Test game scenarios instantly without grinding  
✅ **Bug Investigation** - Teleport, spawn enemies, manipulate state  
✅ **Performance Analysis** - Real-time monitoring of critical metrics  
✅ **Save Development Time** - Hours saved vs. manual testing  
✅ **Zero Production Impact** - Completely disabled in release builds  
✅ **Easy to Extend** - Add new commands in `DebugCommands.cs`

## Adding New Commands

To add a new debug command:

1. Open `Scripts/Debug/DebugCommands.cs`
2. Add command to `RegisterCommands()`:
   ```csharp
   ["mycommand"] = MyCommand,
   ```
3. Implement the method:
   ```csharp
   private void MyCommand(string[] args)
   {
       // Your debug code here
       GD.Print("My command executed");
   }
   ```
4. Update help text in `Help()` method

## Notes

- Debug console captures keyboard input when visible
- Free camera captures mouse when active
- All debug features respect Godot's scene tree structure
- Performance profiler updates every frame when visible
- Commands use flexible node discovery to work with various scene setups

## Security

✅ **Conditional Compilation** - Removed from release builds  
✅ **No Secrets** - No hardcoded credentials or sensitive data  
✅ **Input Validation** - Commands validate arguments before execution  
✅ **Safe Damage Values** - Uses large but safe float values (not float.MaxValue to avoid overflow)
