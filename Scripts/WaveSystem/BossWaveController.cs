using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Enemies;
using MechDefenseHalo.Enemies.Bosses;
using MechDefenseHalo.Audio;

namespace MechDefenseHalo.WaveSystem
{
    /// <summary>
    /// Handles boss wave spawning and special boss wave logic
    /// </summary>
    public partial class BossWaveController : Node
    {
        [Export] public NodePath SpawnPointsPath { get; set; }

        private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
        private Node3D _spawnedBoss;
        private List<Node3D> _supportEnemies = new List<Node3D>();

        public override void _Ready()
        {
            // Get spawn points
            if (SpawnPointsPath != null)
            {
                var spawnPointsNode = GetNodeOrNull(SpawnPointsPath);
                if (spawnPointsNode != null)
                {
                    foreach (Node child in spawnPointsNode.GetChildren())
                    {
                        if (child is SpawnPoint spawnPoint)
                        {
                            _spawnPoints.Add(spawnPoint);
                        }
                    }
                }
            }

            GD.Print($"BossWaveController initialized with {_spawnPoints.Count} spawn points");
        }

        /// <summary>
        /// Spawn a boss wave with support enemies
        /// Returns list of all spawned enemies for tracking
        /// </summary>
        public List<Node3D> SpawnBossWave(int waveNumber, WaveDefinition waveDefinition)
        {
            var spawnedEnemies = new List<Node3D>();
            
            if (waveDefinition == null || !waveDefinition.IsBossWave)
            {
                GD.PrintErr("Invalid boss wave definition!");
                return spawnedEnemies;
            }

            string bossType = waveDefinition.BossType ?? GetBossForWave(waveNumber);

            // Spawn boss
            _spawnedBoss = SpawnBoss(bossType);

            if (_spawnedBoss == null)
            {
                GD.PrintErr($"Failed to spawn boss: {bossType}");
                return spawnedEnemies;
            }

            spawnedEnemies.Add(_spawnedBoss);

            // Spawn support enemies
            if (waveDefinition.SupportEnemies != null)
            {
                foreach (var supportEnemy in waveDefinition.SupportEnemies)
                {
                    var supportEnemyList = SpawnSupportEnemies(supportEnemy.EnemyType, supportEnemy.Count);
                    spawnedEnemies.AddRange(supportEnemyList);
                }
            }

            // TODO: Start boss music when SoundID enum is extended with boss music tracks
            // MusicPlayer.Instance.PlayMusic(SoundID.MusicBoss, 2.0f);

            // Emit boss wave started event
            EventBus.Emit(EventBus.BossSpawned, new BossSpawnedEventData
            {
                BossName = bossType,
                WaveNumber = waveNumber
            });

            GD.Print($"Boss wave {waveNumber} started: {bossType} with {spawnedEnemies.Count} total enemies");
            
            return spawnedEnemies;
        }

        /// <summary>
        /// Get the boss type for a specific wave number
        /// </summary>
        private string GetBossForWave(int wave)
        {
            return wave switch
            {
                10 => "FrostTitan",
                20 => "InfernoColossus",
                30 => "VoidWraith",
                40 => "StormLord",
                50 => "ChaosBringer",
                _ => "FrostTitan" // Default fallback
            };
        }

        /// <summary>
        /// Spawn a boss enemy
        /// </summary>
        private Node3D SpawnBoss(string bossType)
        {
            Node3D boss = null;

            // Create boss based on type
            switch (bossType)
            {
                case "FrostTitan":
                    boss = new FrostTitan();
                    break;
                // Other bosses can be added here when implemented
                // case "InfernoColossus":
                //     boss = new InfernoColossus();
                //     break;
                default:
                    GD.PrintErr($"Unknown boss type: {bossType}, using FrostTitan");
                    boss = new FrostTitan();
                    break;
            }

            if (boss == null)
                return null;

            // Get spawn position (center spawn point or first available)
            Vector3 spawnPos = Vector3.Zero;
            if (_spawnPoints.Count > 0)
            {
                spawnPos = _spawnPoints[0].GlobalPosition;
            }

            // Add to scene
            GetTree().Root.AddChild(boss);
            boss.GlobalPosition = spawnPos;

            return boss;
        }

        /// <summary>
        /// Spawn support enemies during boss wave
        /// Returns list of spawned enemies
        /// </summary>
        private List<Node3D> SpawnSupportEnemies(string enemyType, int count)
        {
            var spawnedEnemies = new List<Node3D>();
            
            for (int i = 0; i < count; i++)
            {
                Node3D enemy = CreateEnemy(enemyType);
                if (enemy == null)
                    continue;

                // Get spawn position from random spawn point
                Vector3 spawnPos = Vector3.Zero;
                if (_spawnPoints.Count > 0)
                {
                    var spawnPoint = _spawnPoints[GD.Randi() % _spawnPoints.Count];
                    spawnPos = spawnPoint.GetSpawnPosition(
                        SpawnPoint.SpawnPattern.Circle,
                        i,
                        count
                    );
                }

                // Add to scene
                GetTree().Root.AddChild(enemy);
                enemy.GlobalPosition = spawnPos;

                _supportEnemies.Add(enemy);
                spawnedEnemies.Add(enemy);
            }

            GD.Print($"Spawned {spawnedEnemies.Count} {enemyType} as support enemies");
            
            return spawnedEnemies;
        }

        /// <summary>
        /// Create an enemy instance by type name with validation
        /// </summary>
        private Node3D CreateEnemy(string enemyType)
        {
            Node3D enemy = enemyType switch
            {
                "Grunt" => new Grunt(),
                "Shooter" => new Shooter(),
                "Tank" => new Tank(),
                "Swarm" => new Swarm(),
                "Flyer" => new Flyer(),
                _ => null
            };

            // Validate that enemy is an EnemyBase for proper functionality
            if (enemy != null && enemy is not EnemyBase)
            {
                GD.PrintErr($"CRITICAL: Enemy type {enemyType} does not inherit from EnemyBase! Skipping spawn.");
                return null; // Return null to prevent spawning invalid enemy
            }

            return enemy;
        }

        /// <summary>
        /// Get remaining enemies count (boss + support)
        /// </summary>
        public int GetRemainingEnemiesCount()
        {
            int count = 0;

            // Count boss if alive
            if (_spawnedBoss != null && IsInstanceValid(_spawnedBoss))
            {
                count++;
            }

            // Count support enemies
            _supportEnemies.RemoveAll(e => !IsInstanceValid(e));
            count += _supportEnemies.Count;

            return count;
        }

        /// <summary>
        /// Clear all spawned enemies
        /// </summary>
        public void ClearAllEnemies()
        {
            // Remove boss
            if (_spawnedBoss != null && IsInstanceValid(_spawnedBoss))
            {
                _spawnedBoss.QueueFree();
            }
            _spawnedBoss = null;

            // Remove support enemies
            foreach (var enemy in _supportEnemies)
            {
                if (IsInstanceValid(enemy))
                {
                    enemy.QueueFree();
                }
            }
            _supportEnemies.Clear();
        }
    }

    /// <summary>
    /// Event data for boss spawned event
    /// </summary>
    public class BossSpawnedEventData
    {
        public string BossName { get; set; }
        public int WaveNumber { get; set; }
    }
}
