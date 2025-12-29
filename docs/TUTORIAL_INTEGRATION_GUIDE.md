# Tutorial System Integration Example

This document shows how to integrate the tutorial system into your game.

## Step 1: Add TutorialManager to GameManager Scene

Create or modify `_Core/GameManager.tscn`:

```
[node name="GameManager" type="Node"]

[node name="SaveManager" type="Node" parent="."]
script = ExtResource("SaveManager.cs")

[node name="EventBus" type="Node" parent="."]
script = ExtResource("EventBus.cs")

[node name="CurrencyManager" type="Node" parent="."]
script = ExtResource("CurrencyManager.cs")

[node name="TutorialManager" type="Node" parent="."]
script = ExtResource("TutorialManager.cs")
TutorialStepsPath = "res://Data/Tutorial/tutorial_steps.json"
TutorialRewardsPath = "res://Data/Tutorial/tutorial_rewards.json"
```

## Step 2: Emit Events in Your Game Code

### Player Movement (PlayerMechController.cs)

```csharp
public override void _Process(double delta)
{
    // ... existing movement code ...
    
    // Track distance moved for tutorial
    if (_velocity.Length() > 0)
    {
        float distance = _velocity.Length() * (float)delta;
        EventBus.Emit("player_moved", distance);
    }
}
```

### Weapon Firing (WeaponBase.cs)

```csharp
public void Fire()
{
    // ... existing fire code ...
    
    // Notify tutorial system
    EventBus.Emit(EventBus.WeaponFired, null);
}
```

### Enemy Death (EnemyBase.cs)

```csharp
public void Die()
{
    // ... existing death code ...
    
    // Notify tutorial system
    EventBus.Emit(EventBus.EntityDied, this.GetType().Name);
}
```

### Loot Collection (LootDropComponent.cs)

```csharp
public void OnPickup()
{
    // ... existing pickup code ...
    
    // Notify tutorial system
    EventBus.Emit(EventBus.LootPickedUp, _lootItem);
}
```

### UI Opening (InventoryUI.cs, ShopUI.cs, etc.)

```csharp
public void Open()
{
    Visible = true;
    
    // Notify tutorial system
    EventBus.Emit("ui_opened", "inventory"); // or "shop", "crafting", etc.
}
```

### Item Equipping (EquipmentManager.cs)

```csharp
public bool EquipItem(ItemBase item, EquipSlot slot)
{
    // ... existing equip code ...
    
    // Notify tutorial system
    EventBus.Emit(EventBus.ItemEquipped, item);
    
    return true;
}
```

### Drone Deployment (DroneManager.cs)

```csharp
public void DeployDrone(DroneBase drone)
{
    // ... existing deploy code ...
    
    // Notify tutorial system
    EventBus.Emit(EventBus.DroneDeployed, drone);
}
```

### Wave Completion (WaveSpawner.cs)

```csharp
private void OnWaveComplete()
{
    // ... existing wave complete code ...
    
    // Notify tutorial system
    EventBus.Emit(EventBus.WaveCompleted, CurrentWave);
}
```

### Crafting Start (CraftingManager.cs)

```csharp
public bool StartCrafting(string blueprintId)
{
    // ... existing crafting code ...
    
    // Notify tutorial system
    EventBus.Emit(EventBus.CraftStarted, blueprint);
    
    return true;
}
```

## Step 3: Add Tutorial Button to Main Menu

In your main menu UI:

```csharp
public partial class MainMenuUI : Control
{
    private Button _tutorialButton;
    
    public override void _Ready()
    {
        _tutorialButton = GetNode<Button>("TutorialButton");
        _tutorialButton.Pressed += OnTutorialPressed;
    }
    
    private void OnTutorialPressed()
    {
        var tutorialManager = GetNode<TutorialManager>("/root/GameManager/TutorialManager");
        tutorialManager.RestartTutorial();
        
        // Switch to game scene
        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }
}
```

## Step 4: Test Tutorial Flow

1. **Delete save file** to simulate first launch:
   - Located at: `user://save_data.json`
   - Or use: `SaveManager.Instance.DeleteSave()`

2. **Start game** - Tutorial should auto-start

3. **Test each step**:
   - Move with WASD (Step 1)
   - Fire weapon (Step 2)
   - Kill enemies (Step 3)
   - Collect loot (Step 4)
   - Open inventory (Step 5)
   - Equip item (Step 6)
   - Deploy drone (Step 7)
   - Complete wave (Step 8)
   - Open shop (Step 9)
   - Start crafting (Step 10)

4. **Verify rewards** granted on completion

## Step 5: Configure Tutorial Spawning

If your game needs to spawn enemies for tutorial step 3:

```csharp
// In WaveSpawner.cs or similar
public void SpawnEnemy(string enemyType)
{
    PackedScene enemyScene = enemyType switch
    {
        "Grunt" => GD.Load<PackedScene>("res://Scenes/Enemies/Grunt.tscn"),
        "Shooter" => GD.Load<PackedScene>("res://Scenes/Enemies/Shooter.tscn"),
        _ => null
    };
    
    if (enemyScene != null)
    {
        var enemy = enemyScene.Instantiate<Node3D>();
        AddChild(enemy);
    }
}
```

## Optional: Add Tutorial Control to Settings

```csharp
public partial class SettingsUI : Control
{
    private Button _replayTutorialButton;
    
    public override void _Ready()
    {
        _replayTutorialButton = GetNode<Button>("ReplayTutorialButton");
        _replayTutorialButton.Pressed += OnReplayTutorial;
    }
    
    private void OnReplayTutorial()
    {
        var tutorialManager = GetNode<TutorialManager>("/root/GameManager/TutorialManager");
        tutorialManager.RestartTutorial();
    }
}
```

## Checklist

- [ ] TutorialManager added to scene tree
- [ ] All required events are emitted
- [ ] First launch detection working
- [ ] Tutorial auto-starts for new players
- [ ] Skip functionality tested
- [ ] Completion rewards granted
- [ ] Tutorial state saved correctly
- [ ] UI appears and functions properly
- [ ] All 10 steps complete successfully
- [ ] Replay from settings works

## Troubleshooting

### Issue: Tutorial doesn't start
**Solution**: Check SaveManager.GetBool("is_first_launch") returns true

### Issue: Progress not tracking
**Solution**: Verify events are emitted with correct names

### Issue: Can't skip unskippable steps
**Expected**: Steps 1, 3, 7, 8 cannot be skipped by design

### Issue: Rewards not received
**Solution**: Ensure CurrencyManager is initialized before tutorial completion

### Issue: UI not visible
**Solution**: Check that TutorialDialog is added as child and z-index is high enough
