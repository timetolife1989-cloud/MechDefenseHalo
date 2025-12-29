using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Main coordinator for procedural enemy generation
    /// Generates unlimited unique enemy variants from base meshes
    /// </summary>
    public partial class ProceduralEnemyGenerator : Node
    {
        private List<PackedScene> baseMeshes = new();
        private EnemyStatMixer statMixer;
        private EnemyVisualMutator visualMutator;
        private EliteAbilitySystem abilitySystem;
        
        public override void _Ready()
        {
            statMixer = GetNode<EnemyStatMixer>("StatMixer");
            visualMutator = GetNode<EnemyVisualMutator>("VisualMutator");
            abilitySystem = GetNode<EliteAbilitySystem>("AbilitySystem");
            
            LoadBaseMeshes();
        }
        
        private void LoadBaseMeshes()
        {
            // 5 base enemy meshes (will be placeholders initially)
            baseMeshes.Add(ResourceLoader.Load<PackedScene>("res://Entities/Enemies/Base/enemy_type_a.tscn"));
            baseMeshes.Add(ResourceLoader.Load<PackedScene>("res://Entities/Enemies/Base/enemy_type_b.tscn"));
            baseMeshes.Add(ResourceLoader.Load<PackedScene>("res://Entities/Enemies/Base/enemy_type_c.tscn"));
            baseMeshes.Add(ResourceLoader.Load<PackedScene>("res://Entities/Enemies/Base/enemy_type_d.tscn"));
            baseMeshes.Add(ResourceLoader.Load<PackedScene>("res://Entities/Enemies/Base/enemy_type_e.tscn"));
        }
        
        public Node3D GenerateEnemy(EnemyRarity rarity = EnemyRarity.Common)
        {
            // Pick random base mesh
            var baseMesh = baseMeshes[GD.RandRange(0, baseMeshes.Count - 1)];
            var enemy = baseMesh.Instantiate<Node3D>();
            
            // Generate stats with trade-offs
            var stats = statMixer.GenerateStats(rarity);
            ApplyStatsToEnemy(enemy, stats);
            
            // Apply visual mutation
            visualMutator.MutateVisuals(enemy, stats, rarity);
            
            // Add elite abilities if rare+
            if (rarity >= EnemyRarity.Rare)
            {
                abilitySystem.AddRandomAbility(enemy);
            }
            
            return enemy;
        }
        
        private void ApplyStatsToEnemy(Node3D enemy, EnemyStats stats)
        {
            // Apply to enemy script
            if (enemy is EnemyBase baseEnemy)
            {
                baseEnemy.MaxHealth = stats.HP;
                baseEnemy.AttackDamage = stats.Damage;
                baseEnemy.MoveSpeed = stats.Speed;
                baseEnemy.AttackRange = stats.Range;
                // Size is handled by scale in visual mutator
            }
        }
    }
}
