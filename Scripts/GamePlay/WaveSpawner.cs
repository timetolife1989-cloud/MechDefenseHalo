using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Enemies;
using MechDefenseHalo.Progression;

namespace MechDefenseHalo.GamePlay
{
    /// <summary>
    /// Manages wave-based enemy spawning.
    /// Handles wave progression, difficulty scaling, and spawn points.
    /// </summary>
    public partial class WaveSpawner : Node3D
    {
        #region Exported Properties

        [Export] public int StartWave { get; set; } = 1;
        [Export] public float TimeBetweenWaves { get; set; } = 10f;
        [Export] public float SpawnDelay { get; set; } = 1f;
        [Export] public bool AutoStartFirstWave { get; set; } = true;

        #endregion

        #region Public Properties

        public int CurrentWave { get; private set; } = 0;
        public bool IsWaveActive { get; private set; } = false;
        public int EnemiesRemaining => _activeEnemies.Count;

        #endregion

        #region Private Fields

        private List<Node3D> _spawnPoints = new List<Node3D>();
        private List<Node3D> _activeEnemies = new List<Node3D>();
        private List<WaveDefinition> _waves = new List<WaveDefinition>();
        private float _waveTimer = 0f;
        private int _enemiesToSpawn = 0;
        private float _spawnTimer = 0f;
        private Queue<EnemySpawnData> _spawnQueue = new Queue<EnemySpawnData>();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Find spawn points
            var spawnPointsNode = GetNodeOrNull("SpawnPoints");
            if (spawnPointsNode != null)
            {
                foreach (Node child in spawnPointsNode.GetChildren())
                {
                    if (child is Node3D spawnPoint)
                    {
                        _spawnPoints.Add(spawnPoint);
                    }
                }
            }

            if (_spawnPoints.Count == 0)
            {
                GD.PrintErr("No spawn points found! Add Node3D children under SpawnPoints node.");
                // Create a default spawn point
                var defaultSpawn = new Node3D();
                defaultSpawn.GlobalPosition = GlobalPosition;
                _spawnPoints.Add(defaultSpawn);
            }

            // Initialize wave definitions
            InitializeWaves();

            // Start first wave if auto-start enabled
            if (AutoStartFirstWave)
            {
                CallDeferred(nameof(StartNextWave));
            }

            GD.Print($"WaveSpawner initialized with {_spawnPoints.Count} spawn points and {_waves.Count} waves");
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // Clean up dead enemies
            _activeEnemies.RemoveAll(enemy => !IsInstanceValid(enemy));

            if (IsWaveActive)
            {
                // Spawn enemies from queue
                if (_spawnQueue.Count > 0)
                {
                    _spawnTimer -= dt;
                    if (_spawnTimer <= 0)
                    {
                        SpawnNextEnemy();
                        _spawnTimer = SpawnDelay;
                    }
                }
                else if (_activeEnemies.Count == 0)
                {
                    // Wave completed
                    CompleteWave();
                }
            }
            else
            {
                // Wait between waves
                _waveTimer -= dt;
                if (_waveTimer <= 0)
                {
                    StartNextWave();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the next wave manually
        /// </summary>
        public void StartNextWave()
        {
            if (IsWaveActive)
            {
                GD.PrintErr("Cannot start new wave - current wave is active!");
                return;
            }

            CurrentWave++;

            if (CurrentWave > _waves.Count)
            {
                // All waves completed
                EventBus.Emit(EventBus.AllWavesCompleted, null);
                GD.Print("All waves completed!");
                return;
            }

            IsWaveActive = true;
            _waveTimer = 0f;

            // Get wave definition
            var wave = _waves[CurrentWave - 1];
            
            // Build spawn queue
            BuildSpawnQueue(wave);

            EventBus.Emit(EventBus.WaveStarted, new WaveStartedData
            {
                WaveNumber = CurrentWave,
                TotalEnemies = _spawnQueue.Count
            });

            GD.Print($"Wave {CurrentWave} started! Enemies: {_spawnQueue.Count}");
        }

        /// <summary>
        /// Force complete current wave
        /// </summary>
        public void ForceCompleteWave()
        {
            if (!IsWaveActive)
                return;

            // Kill all active enemies
            foreach (var enemy in _activeEnemies)
            {
                if (IsInstanceValid(enemy))
                {
                    enemy.QueueFree();
                }
            }
            _activeEnemies.Clear();
            _spawnQueue.Clear();

            CompleteWave();
        }

        #endregion

        #region Private Methods

        private void InitializeWaves()
        {
            // Wave 1: Easy intro
            _waves.Add(new WaveDefinition
            {
                Enemies = new List<EnemySpawnData>
                {
                    new EnemySpawnData { EnemyType = typeof(Grunt), Count = 5 },
                    new EnemySpawnData { EnemyType = typeof(Swarm), Count = 3 }
                }
            });

            // Wave 2: Mixed enemies
            _waves.Add(new WaveDefinition
            {
                Enemies = new List<EnemySpawnData>
                {
                    new EnemySpawnData { EnemyType = typeof(Grunt), Count = 8 },
                    new EnemySpawnData { EnemyType = typeof(Shooter), Count = 2 },
                    new EnemySpawnData { EnemyType = typeof(Swarm), Count = 5 }
                }
            });

            // Wave 3: Introducing tank
            _waves.Add(new WaveDefinition
            {
                Enemies = new List<EnemySpawnData>
                {
                    new EnemySpawnData { EnemyType = typeof(Grunt), Count = 10 },
                    new EnemySpawnData { EnemyType = typeof(Shooter), Count = 3 },
                    new EnemySpawnData { EnemyType = typeof(Tank), Count = 1 },
                    new EnemySpawnData { EnemyType = typeof(Swarm), Count = 8 }
                }
            });

            // Wave 4: Flyers arrive
            _waves.Add(new WaveDefinition
            {
                Enemies = new List<EnemySpawnData>
                {
                    new EnemySpawnData { EnemyType = typeof(Grunt), Count = 12 },
                    new EnemySpawnData { EnemyType = typeof(Shooter), Count = 4 },
                    new EnemySpawnData { EnemyType = typeof(Flyer), Count = 3 },
                    new EnemySpawnData { EnemyType = typeof(Swarm), Count = 10 }
                }
            });

            // Wave 5: Challenging
            _waves.Add(new WaveDefinition
            {
                Enemies = new List<EnemySpawnData>
                {
                    new EnemySpawnData { EnemyType = typeof(Grunt), Count = 15 },
                    new EnemySpawnData { EnemyType = typeof(Shooter), Count = 5 },
                    new EnemySpawnData { EnemyType = typeof(Tank), Count = 2 },
                    new EnemySpawnData { EnemyType = typeof(Flyer), Count = 4 },
                    new EnemySpawnData { EnemyType = typeof(Swarm), Count = 12 }
                }
            });
        }

        private void BuildSpawnQueue(WaveDefinition wave)
        {
            _spawnQueue.Clear();

            foreach (var spawnData in wave.Enemies)
            {
                for (int i = 0; i < spawnData.Count; i++)
                {
                    _spawnQueue.Enqueue(spawnData);
                }
            }

            _spawnTimer = SpawnDelay;
        }

        private void SpawnNextEnemy()
        {
            if (_spawnQueue.Count == 0)
                return;

            var spawnData = _spawnQueue.Dequeue();
            
            // Get random spawn point
            var spawnPoint = _spawnPoints[GD.Randi() % _spawnPoints.Count];

            // Create enemy
            var enemy = Activator.CreateInstance(spawnData.EnemyType) as EnemyBase;
            if (enemy == null)
            {
                GD.PrintErr($"Failed to create enemy of type {spawnData.EnemyType}");
                return;
            }

            // Add to scene
            GetTree().Root.AddChild(enemy);
            enemy.GlobalPosition = spawnPoint.GlobalPosition;
            enemy.AddToGroup("player"); // Temp: enemies need target

            // Track enemy
            _activeEnemies.Add(enemy);

            GD.Print($"Spawned {enemy.EnemyName} at {spawnPoint.GlobalPosition}");
        }

        private void CompleteWave()
        {
            IsWaveActive = false;
            _waveTimer = TimeBetweenWaves;

            // Grant XP for wave completion (100 XP * wave number)
            int xpAmount = 100 * CurrentWave;
            PlayerLevel.AddXP(xpAmount, $"Wave {CurrentWave} completion");

            EventBus.Emit(EventBus.WaveCompleted, new WaveCompletedData
            {
                WaveNumber = CurrentWave
            });

            GD.Print($"Wave {CurrentWave} completed! Next wave in {TimeBetweenWaves} seconds");
        }

        #endregion
    }

    #region Data Structures

    public class WaveDefinition
    {
        public List<EnemySpawnData> Enemies { get; set; } = new List<EnemySpawnData>();
    }

    public class EnemySpawnData
    {
        public Type EnemyType { get; set; }
        public int Count { get; set; }
    }

    public class WaveStartedData
    {
        public int WaveNumber { get; set; }
        public int TotalEnemies { get; set; }
    }

    public class WaveCompletedData
    {
        public int WaveNumber { get; set; }
    }

    #endregion
}
