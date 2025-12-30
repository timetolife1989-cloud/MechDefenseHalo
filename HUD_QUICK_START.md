# HUD System Quick Start Guide

## Overview
This guide provides quick instructions for using the HUD system in MechDefenseHalo.

## Components

### HUDManager
Main controller that coordinates all HUD elements.

```csharp
// Get reference
var hud = GetNode<HUDManager>("/root/HUD");

// Add score
hud.AddScore(100);

// Reset score
hud.ResetScore();

// Show/hide HUD
hud.SetHUDVisible(true);
```

### HealthBarUI
Displays player health and shield.

```csharp
var healthBar = GetNode<HealthBarUI>("HealthBar");

// Update health
healthBar.UpdateHealth(50f, 100f); // current, max

// Update shield
healthBar.UpdateShield(25f, 50f);

// Show/hide shield
healthBar.SetShieldVisible(true);
```

### AmmoCounterUI
Displays ammo and reload status.

```csharp
var ammoCounter = GetNode<AmmoCounterUI>("AmmoCounter");

// Update ammo
ammoCounter.UpdateAmmo(15, 30); // current, max

// Show reload progress
ammoCounter.ShowReloadProgress(0.5f); // 0-1

// Complete reload
ammoCounter.OnReloadComplete();
```

### MinimapUI
Shows player, enemies, and objectives on a map.

```csharp
var minimap = GetNode<MinimapUI>("Minimap");

// Set player
minimap.SetPlayer(playerNode);

// Add/remove enemy markers
minimap.AddEnemyMarker(enemyNode);
minimap.RemoveEnemyMarker(enemyNode);

// Add/remove objective markers
minimap.AddObjectiveMarker(objectiveNode);
minimap.RemoveObjectiveMarker(objectiveNode);

// Clear all markers
minimap.ClearAllMarkers();
```

### ObjectiveTrackerUI
Tracks mission objectives.

```csharp
var tracker = GetNode<ObjectiveTrackerUI>("ObjectiveTracker");

// Add objective
tracker.AddObjective("obj_1", "Defeat 10 enemies");
tracker.AddObjective("obj_2", "Find secret", isOptional: true);

// Update progress
tracker.UpdateObjective("obj_1", 5, 10); // current, total

// Complete objective
tracker.CompleteObjective("obj_1");

// Fail objective
tracker.FailObjective("obj_2");

// Remove objective
tracker.RemoveObjective("obj_1");

// Clear all
tracker.ClearAllObjectives();
```

### CrosshairUI
Dynamic crosshair with hit feedback.

```csharp
var crosshair = GetNode<CrosshairUI>("Crosshair");

// Show hit marker
crosshair.ShowHitMarker();

// Set spread
crosshair.SetSpread(20f);
crosshair.IncreaseSpread(5f);
crosshair.ResetSpread();

// Change color
crosshair.SetColor(Colors.Red);

// Show/hide
crosshair.SetCrosshairVisible(true);
```

## EventBus Integration

The HUD automatically responds to these events:

```csharp
// Health events (from HealthComponent)
EventBus.Emit(EventBus.HealthChanged, new HealthChangedData {
    Entity = player,
    CurrentHealth = 50f,
    MaxHealth = 100f
});

// Ammo events (from WeaponComponent)
EventBus.Emit(EventBus.AmmoChanged, new AmmoChangedData {
    CurrentAmmo = 15,
    MaxAmmo = 30
});

// Wave events
EventBus.Emit(EventBus.WaveStarted, new WaveStartedEventData {
    WaveNumber = 5,
    TotalEnemies = 50
});

// Damage events (for hit marker)
EventBus.Emit(EventBus.DamageDealt, damageData);
```

## Scene Setup

### Basic HUD Scene Structure
```
HUD (Control) [HUDManager.cs]
├── HealthBar (Control) [HealthBarUI.cs]
│   ├── HealthProgress (ProgressBar)
│   ├── ShieldProgress (ProgressBar)
│   └── HealthLabel (Label)
├── AmmoCounter (Control) [AmmoCounterUI.cs]
│   ├── AmmoLabel (Label)
│   ├── ReloadProgress (ProgressBar)
│   └── ReloadLabel (Label)
├── Minimap (Control) [MinimapUI.cs]
│   ├── MinimapViewport (Control)
│   └── PlayerMarker (Control)
├── ObjectiveTracker (Control) [ObjectiveTrackerUI.cs]
│   └── ObjectiveList (VBoxContainer)
├── Crosshair (Control) [CrosshairUI.cs]
│   ├── CrosshairCenter (Control)
│   └── HitMarker (Control)
├── WaveCounter (Label)
└── ScoreLabel (Label)
```

### NodePath Configuration
In HUDManager Inspector, set:
- HealthBarPath: `"HealthBar"`
- AmmoCounterPath: `"AmmoCounter"`
- MinimapPath: `"Minimap"`
- ObjectiveTrackerPath: `"ObjectiveTracker"`
- CrosshairPath: `"Crosshair"`
- WaveCounterPath: `"WaveCounter"`
- ScoreLabelPath: `"ScoreLabel"`

## Common Use Cases

### Update Player Health
```csharp
// This happens automatically via EventBus
// When HealthComponent.TakeDamage() or .Heal() is called
```

### Show Reload Progress
```csharp
// In WeaponComponent._Process()
if (IsReloading)
{
    float progress = _reloadTimer / ReloadTime;
    // HUD automatically shows via EventBus
}
```

### Track Wave Progress
```csharp
// In WaveManager
EventBus.Emit(EventBus.WaveStarted, new WaveStartedEventData {
    WaveNumber = currentWave,
    TotalEnemies = enemyCount
});
```

### Add Dynamic Objective
```csharp
// Add objective when wave starts
var tracker = GetNode<ObjectiveTrackerUI>("HUD/ObjectiveTracker");
tracker.AddObjective($"wave_{waveNum}", $"Survive Wave {waveNum}");

// Update as enemies die
tracker.UpdateObjective($"wave_{waveNum}", enemiesKilled, totalEnemies);

// Complete when wave ends
tracker.CompleteObjective($"wave_{waveNum}");
```

## Customization

### Colors
```csharp
// In Inspector or code
healthBar.HealthColor = Colors.Green;
healthBar.LowHealthColor = Colors.Red;
healthBar.ShieldColor = Colors.Cyan;

ammoCounter.NormalColor = Colors.White;
ammoCounter.LowAmmoColor = Colors.Orange;
ammoCounter.EmptyColor = Colors.Red;

minimap.EnemyColor = Colors.Red;
minimap.ObjectiveColor = Colors.Yellow;
```

### Thresholds
```csharp
healthBar.LowHealthThreshold = 0.3f; // 30%
ammoCounter.LowAmmoThreshold = 0.25f; // 25%
```

### Minimap Settings
```csharp
minimap.MapScale = 10f; // Zoom level
minimap.MapRadius = 100f; // View distance
minimap.ShowEnemies = true;
minimap.ShowObjectives = true;
```

## Troubleshooting

### HUD not showing
- Check HUD visibility: `hud.Visible`
- Verify scene is added to the scene tree
- Check NodePath configuration

### Health bar not updating
- Ensure player entity name contains "Player"
- Verify EventBus is initialized
- Check HealthComponent is emitting events

### Minimap markers not appearing
- Verify MinimapViewport node exists
- Check marker colors (might be invisible against background)
- Ensure enemy nodes are valid

### Objectives not displaying
- Check ObjectiveList VBoxContainer exists
- Verify NodePath is correct
- Check if MaxVisibleObjectives is set too low

## Performance Tips

1. **Minimap**: Limit number of tracked enemies
2. **Objectives**: Keep to 5 or fewer visible
3. **Events**: Unsubscribe when not needed
4. **Updates**: Avoid updating every frame if possible

## Testing

Run unit tests with GdUnit4:
```bash
# In Godot editor
GdUnit4 → Run All Tests

# Or specific test suite
GdUnit4 → Run Tests → Select HUDManagerTests
```

## Support

For issues or questions:
1. Check `HUD_IMPLEMENTATION_SUMMARY.md`
2. Review test files in `Tests/UI/`
3. Examine existing UI implementations
4. Create an issue on GitHub

---

**Quick Reference Table**

| Component | Primary Function | Key Methods |
|-----------|-----------------|-------------|
| HUDManager | Coordination | AddScore, ResetScore, SetHUDVisible |
| HealthBarUI | Health display | UpdateHealth, UpdateShield |
| AmmoCounterUI | Ammo display | UpdateAmmo, ShowReloadProgress |
| MinimapUI | Map display | AddEnemyMarker, AddObjectiveMarker |
| ObjectiveTrackerUI | Objectives | AddObjective, CompleteObjective |
| CrosshairUI | Crosshair | ShowHitMarker, SetSpread |
