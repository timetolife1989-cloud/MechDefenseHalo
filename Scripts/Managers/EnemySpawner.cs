using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Managers
{
    /// <summary>
    /// Wave-based enemy spawning system that manages enemy generation, spawn points, difficulty scaling, and wave progression.
    /// </summary>
    public partial class EnemySpawner : Node3D
    {
        #region Singleton

        public static EnemySpawner Instance { get; private set; }

        #endregion

        #region Signals

        [Signal] public delegate void WaveStartedEventHandler(int waveNumber, int enemyCount);
        [Signal] public delegate void WaveCompletedEventHandler(int waveNumber);
        [Signal] public delegate void EnemySpawnedEventHandler(Node enemy);
        [Signal] public delegate void EnemyKilledEventHandler(int remaining);

        #endregion

        #region Exported Properties

        [ExportGroup("Wave Settings")]
        [Export] public int EnemiesPerWave { get; set; } = 5;
        [Export] public float WaveMultiplier { get; set; } = 1.5f;
        [Export] public float TimeBetweenWaves { get; set; } = 10.0f;

        [ExportGroup("Spawn Settings")]
        [Export] public float SpawnInterval { get; set; } = 2.0f;
        [Export] public NodePath[] SpawnPointPaths { get; set; }

        [ExportGroup("Debug")]
        [Export] public bool AutoStart { get; set; } = true;

        #endregion

        #region Public Properties

        [Export] public int CurrentWave { get; private set; } = 0;

        #endregion

        #region Private Fields

        private int enemiesRemaining = 0;
        private int enemiesAlive = 0;
        private bool waveActive = false;
        private List<Node3D> spawnPoints = new List<Node3D>();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Singleton pattern
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            Instance = this;

            // Initialize spawn points
            if (SpawnPointPaths != null)
            {
                foreach (var path in SpawnPointPaths)
                {
                    var point = GetNodeOrNull<Node3D>(path);
                    if (point != null)
                    {
                        spawnPoints.Add(point);
                    }
                }
            }

            if (spawnPoints.Count == 0)
            {
                GD.PrintErr("No spawn points configured!");
            }

            if (AutoStart)
            {
                StartWave();
            }

            GD.Print("EnemySpawner initialized");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Stop the spawning process
        /// </summary>
        public void StopSpawning()
        {
            waveActive = false;
            GD.Print("Spawning stopped");
        }

        /// <summary>
        /// Force the next wave to start immediately
        /// </summary>
        public void ForceNextWave()
        {
            if (waveActive) return;
            StartWave();
        }

        /// <summary>
        /// Get the number of enemies currently alive
        /// </summary>
        public int GetEnemiesAlive()
        {
            return enemiesAlive;
        }

        /// <summary>
        /// Get the current wave number
        /// </summary>
        public int GetCurrentWave()
        {
            return CurrentWave;
        }

        /// <summary>
        /// Called when an enemy dies
        /// </summary>
        public void OnEnemyDied(Node enemy)
        {
            enemiesAlive--;
            EmitSignal(SignalName.EnemyKilled, enemiesAlive);

            GD.Print($"Enemy killed! Remaining: {enemiesAlive}");

            // Check if wave complete
            if (enemiesAlive <= 0 && enemiesRemaining <= 0)
            {
                EndWave();
            }
        }

        #endregion

        #region Private Methods - Wave Management

        /// <summary>
        /// Start a new wave
        /// </summary>
        private async void StartWave()
        {
            CurrentWave++;
            waveActive = true;

            // Calculate enemies for this wave
            enemiesRemaining = Mathf.RoundToInt(EnemiesPerWave * Mathf.Pow(WaveMultiplier, CurrentWave - 1));
            enemiesAlive = 0;

            EmitSignal(SignalName.WaveStarted, CurrentWave, enemiesRemaining);
            GD.Print($"Wave {CurrentWave} started! Enemies: {enemiesRemaining}");

            // Spawn enemies with interval
            while (enemiesRemaining > 0 && waveActive)
            {
                SpawnEnemy();
                enemiesRemaining--;

                if (enemiesRemaining > 0)
                {
                    await ToSignal(GetTree().CreateTimer(SpawnInterval), SceneTreeTimer.SignalName.Timeout);
                }
            }
        }

        /// <summary>
        /// End the current wave and prepare for the next
        /// </summary>
        private async void EndWave()
        {
            waveActive = false;
            EmitSignal(SignalName.WaveCompleted, CurrentWave);
            GD.Print($"Wave {CurrentWave} completed! Next wave in {TimeBetweenWaves}s");

            await ToSignal(GetTree().CreateTimer(TimeBetweenWaves), SceneTreeTimer.SignalName.Timeout);

            if (waveActive == false)  // Only start next wave if not manually stopped
            {
                StartWave();
            }
        }

        #endregion

        #region Private Methods - Enemy Spawning

        /// <summary>
        /// Spawn a single enemy at a random spawn point
        /// </summary>
        private void SpawnEnemy()
        {
            if (spawnPoints.Count == 0)
            {
                GD.PrintErr("Cannot spawn enemy - no spawn points!");
                return;
            }

            // Pick random spawn point
            var spawnPoint = spawnPoints[GD.RandRange(0, spawnPoints.Count - 1)];

            // TODO: Load enemy scene when created
            // For now, create placeholder
            var enemy = new Node3D();
            enemiesAlive++;
            enemy.Name = $"Enemy_{CurrentWave}_{enemiesAlive}";
            enemy.GlobalPosition = spawnPoint.GlobalPosition;

            AddChild(enemy);

            EmitSignal(SignalName.EnemySpawned, enemy);
            GD.Print($"Spawned enemy at {spawnPoint.GlobalPosition}");

            // TODO: Connect to enemy death signal
            // enemy.Connect("Died", new Callable(this, nameof(OnEnemyDied)));
        }

        #endregion
    }
}
