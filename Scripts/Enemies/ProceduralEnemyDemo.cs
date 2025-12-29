using Godot;
using System;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Demo/Test script to showcase procedural enemy generation
    /// Usage: Add to a Node in a test scene and run
    /// </summary>
    public partial class ProceduralEnemyDemo : Node
    {
        [Export] private ProceduralEnemyGenerator enemyGenerator;
        [Export] private Vector3 spawnPosition = Vector3.Zero;
        [Export] private float spawnRadius = 10f;
        [Export] private int enemiesToSpawn = 10;
        
        public override void _Ready()
        {
            if (enemyGenerator == null)
            {
                GD.PrintErr("ProceduralEnemyDemo: enemyGenerator is not assigned!");
                return;
            }
            
            GD.Print("=== Procedural Enemy Generation Demo ===");
            
            // Generate enemies of different rarities
            GenerateDemoEnemies();
        }
        
        private void GenerateDemoEnemies()
        {
            var rarities = new[] 
            { 
                EnemyRarity.Common, 
                EnemyRarity.Common,
                EnemyRarity.Common,
                EnemyRarity.Uncommon,
                EnemyRarity.Uncommon,
                EnemyRarity.Rare,
                EnemyRarity.Elite,
                EnemyRarity.Legendary
            };
            
            for (int i = 0; i < enemiesToSpawn && i < rarities.Length; i++)
            {
                var rarity = rarities[i];
                var enemy = enemyGenerator.GenerateEnemy(rarity);
                
                if (enemy != null)
                {
                    // Position enemy in a circle around spawn point
                    float angle = (float)i / enemiesToSpawn * Mathf.Tau;
                    Vector3 offset = new Vector3(
                        Mathf.Cos(angle) * spawnRadius,
                        0,
                        Mathf.Sin(angle) * spawnRadius
                    );
                    
                    enemy.GlobalPosition = spawnPosition + offset;
                    GetTree().Root.AddChild(enemy);
                    
                    GD.Print($"Generated {rarity} enemy at position {enemy.GlobalPosition}");
                    
                    // Print stats if it's an EnemyBase
                    if (enemy is EnemyBase baseEnemy)
                    {
                        GD.Print($"  Stats - HP: {baseEnemy.MaxHealth}, Damage: {baseEnemy.AttackDamage}, Speed: {baseEnemy.MoveSpeed}");
                    }
                }
            }
            
            GD.Print($"=== Generated {enemiesToSpawn} enemies ===");
        }
        
        public override void _Input(InputEvent @event)
        {
            // Press 'G' to generate a random enemy
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.G)
            {
                GenerateRandomEnemy();
            }
        }
        
        private void GenerateRandomEnemy()
        {
            var rarityValues = Enum.GetValues(typeof(EnemyRarity));
            var randomRarity = (EnemyRarity)rarityValues.GetValue(GD.RandRange(0, rarityValues.Length - 1));
            
            var enemy = enemyGenerator.GenerateEnemy(randomRarity);
            if (enemy != null)
            {
                enemy.GlobalPosition = spawnPosition + new Vector3(
                    GD.RandfRange(-spawnRadius, spawnRadius),
                    0,
                    GD.RandfRange(-spawnRadius, spawnRadius)
                );
                GetTree().Root.AddChild(enemy);
                
                GD.Print($"Generated random {randomRarity} enemy (Press G to generate more)");
            }
        }
    }
}
