using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MechDefenseHalo.Core;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Enemies;

namespace MechDefenseHalo.WaveSystem
{
    /// <summary>
    /// Main wave management system
    /// Handles wave progression, spawning, and completion
    /// </summary>
    public partial class WaveManager : Node
    {
        #region Exports

        [Export] public NodePath SpawnPointsPath { get; set; }
        [Export] public float WaveBreakTimer { get; set; } = 30f;
        [Export] public bool AutoStartFirstWave { get; set; } = false;
        [Export] public string WaveDefinitionsPath { get; set; } = "res://Data/Waves/wave_definitions.json";

        #endregion

        #region Public Properties

        public int CurrentWave { get; private set; } = 0;
        public int EnemiesRemaining => _activeEnemies.Count;
        public bool IsWaveActive { get; private set; } = false;
        public float CurrentBreakTime { get; private set; } = 0f;

        #endregion

        #region Private Fields

        private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
        private List<Node3D> _activeEnemies = new List<Node3D>();
        private Dictionary<int, WaveDefinition> _waveDefinitions = new Dictionary<int, WaveDefinition>();
        private BossWaveController _bossWaveController;
        private Queue<SpawnQueueItem> _spawnQueue = new Queue<SpawnQueueItem>();
        private float _spawnTimer = 0f;

        #endregion

        #region Godot Lifecycle

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

            if (_spawnPoints.Count == 0)
            {
                GD.PrintErr("No spawn points found! Add SpawnPoint children to the SpawnPoints node.");
            }

            // Get or create boss wave controller
            _bossWaveController = GetNodeOrNull<BossWaveController>("BossWaveController");
            if (_bossWaveController == null)
            {
                _bossWaveController = new BossWaveController();
                _bossWaveController.Name = "BossWaveController";
                _bossWaveController.SpawnPointsPath = SpawnPointsPath;
                AddChild(_bossWaveController);
            }

            // Load wave definitions
            LoadWaveDefinitions();

            // Auto-start first wave if enabled
            if (AutoStartFirstWave)
            {
                CallDeferred(nameof(StartNextWave));
            }

            GD.Print($"WaveManager initialized with {_spawnPoints.Count} spawn points and {_waveDefinitions.Count} wave definitions");
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // Clean up dead enemies
            _activeEnemies.RemoveAll(enemy => !IsInstanceValid(enemy));

            if (IsWaveActive)
            {
                // Process spawn queue
                if (_spawnQueue.Count > 0)
                {
                    _spawnTimer -= dt;
                    if (_spawnTimer <= 0)
                    {
                        ProcessNextSpawn();
                        _spawnTimer = 1.0f; // Default spawn delay
                    }
                }
                // Check if wave is complete
                else if (EnemiesRemaining <= 0)
                {
                    CompleteWave();
                }
            }
            else
            {
                // Count down break timer
                if (CurrentBreakTime > 0)
                {
                    CurrentBreakTime -= dt;
                    if (CurrentBreakTime <= 0)
                    {
                        StartNextWave();
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the next wave
        /// </summary>
        public void StartNextWave()
        {
            if (IsWaveActive)
            {
                GD.PrintErr("Cannot start new wave while current wave is active!");
                return;
            }

            CurrentWave++;

            // Check if all waves are complete
            if (CurrentWave > _waveDefinitions.Count)
            {
                EventBus.Emit(EventBus.AllWavesCompleted, null);
                GD.Print("All waves completed!");
                return;
            }

            IsWaveActive = true;
            CurrentBreakTime = 0f;

            // Get wave definition
            WaveDefinition waveDef = LoadWaveDefinition(CurrentWave);

            int totalEnemies = 0;
            bool isBossWave = waveDef.IsBossWave || CurrentWave % 10 == 0;

            // Check if this is a boss wave
            if (isBossWave)
            {
                totalEnemies = SpawnBossWave(waveDef);
            }
            else
            {
                SpawnNormalWave(waveDef);
                totalEnemies = _spawnQueue.Count;
            }

            // Emit wave started event
            EventBus.Emit(EventBus.WaveStarted, new WaveStartedEventData
            {
                WaveNumber = CurrentWave,
                TotalEnemies = totalEnemies,
                IsBossWave = isBossWave
            });

            GD.Print($"Wave {CurrentWave} started! Enemies: {totalEnemies}");
        }

        /// <summary>
        /// Called when an enemy dies
        /// </summary>
        public void OnEnemyKilled(Node3D enemy)
        {
            _activeEnemies.Remove(enemy);

            if (EnemiesRemaining <= 0 && IsWaveActive && _spawnQueue.Count == 0)
            {
                CompleteWave();
            }
        }

        #endregion

        #region Private Methods - Wave Loading

        /// <summary>
        /// Load wave definitions from JSON file or use fallback
        /// </summary>
        private void LoadWaveDefinitions()
        {
            // Try to load from JSON file
            if (FileAccess.FileExists(WaveDefinitionsPath))
            {
                try
                {
                    using var file = FileAccess.Open(WaveDefinitionsPath, FileAccess.ModeFlags.Read);
                    string jsonContent = file.GetAsText();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var wavesDict = JsonSerializer.Deserialize<Dictionary<string, WaveDefinition>>(jsonContent, options);
                    
                    if (wavesDict != null)
                    {
                        foreach (var kvp in wavesDict)
                        {
                            // Parse wave number from key (e.g., "wave_1" -> 1)
                            if (int.TryParse(kvp.Key.Replace("wave_", ""), out int waveNum))
                            {
                                kvp.Value.WaveNumber = waveNum;
                                _waveDefinitions[waveNum] = kvp.Value;
                            }
                        }
                        
                        GD.Print($"Loaded {_waveDefinitions.Count} wave definitions from JSON");
                        return;
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Failed to load wave definitions from JSON: {e.Message}");
                }
            }

            // Fallback: Generate basic wave definitions
            GenerateFallbackWaves();
        }

        /// <summary>
        /// Generate fallback wave definitions if JSON fails to load
        /// </summary>
        private void GenerateFallbackWaves()
        {
            GD.Print("Generating fallback wave definitions");

            for (int i = 1; i <= 50; i++)
            {
                var wave = new WaveDefinition
                {
                    WaveNumber = i,
                    IsBossWave = (i % 10 == 0)
                };

                if (wave.IsBossWave)
                {
                    // Boss wave
                    wave.BossType = GetBossTypeForWave(i);
                    wave.SupportEnemies = new List<SupportEnemy>
                    {
                        new SupportEnemy { EnemyType = "Grunt", Count = 5 }
                    };
                }
                else
                {
                    // Normal wave
                    wave.SpawnGroups = GenerateWaveSpawnGroups(i);
                }

                _waveDefinitions[i] = wave;
            }
        }

        /// <summary>
        /// Generate spawn groups for a wave based on wave number
        /// </summary>
        private List<SpawnGroup> GenerateWaveSpawnGroups(int waveNumber)
        {
            var groups = new List<SpawnGroup>();

            // Waves 1-10: Tutorial
            if (waveNumber <= 10)
            {
                groups.Add(new SpawnGroup
                {
                    EnemyType = "Grunt",
                    Count = DifficultyScaler.ScaleEnemyCount(5 + waveNumber, waveNumber),
                    Delay = 1.5f
                });

                if (waveNumber >= 5)
                {
                    groups.Add(new SpawnGroup
                    {
                        EnemyType = "Shooter",
                        Count = DifficultyScaler.ScaleEnemyCount(waveNumber / 2, waveNumber),
                        Delay = 2.0f
                    });
                }
            }
            // Waves 11-30: Progression
            else if (waveNumber <= 30)
            {
                groups.Add(new SpawnGroup
                {
                    EnemyType = "Grunt",
                    Count = DifficultyScaler.ScaleEnemyCount(10 + waveNumber / 2, waveNumber),
                    Delay = 1.0f
                });
                groups.Add(new SpawnGroup
                {
                    EnemyType = "Shooter",
                    Count = DifficultyScaler.ScaleEnemyCount(5 + waveNumber / 3, waveNumber),
                    Delay = 1.5f
                });
                groups.Add(new SpawnGroup
                {
                    EnemyType = "Tank",
                    Count = DifficultyScaler.ScaleEnemyCount(2 + waveNumber / 5, waveNumber),
                    Delay = 2.0f
                });

                if (waveNumber >= 15)
                {
                    groups.Add(new SpawnGroup
                    {
                        EnemyType = "Flyer",
                        Count = DifficultyScaler.ScaleEnemyCount(3 + waveNumber / 4, waveNumber),
                        Delay = 1.5f
                    });
                }
            }
            // Waves 31-50: Endgame
            else
            {
                groups.Add(new SpawnGroup
                {
                    EnemyType = "Grunt",
                    Count = DifficultyScaler.ScaleEnemyCount(20 + waveNumber / 2, waveNumber),
                    Delay = 0.8f
                });
                groups.Add(new SpawnGroup
                {
                    EnemyType = "Shooter",
                    Count = DifficultyScaler.ScaleEnemyCount(10 + waveNumber / 3, waveNumber),
                    Delay = 1.0f
                });
                groups.Add(new SpawnGroup
                {
                    EnemyType = "Tank",
                    Count = DifficultyScaler.ScaleEnemyCount(5 + waveNumber / 4, waveNumber),
                    Delay = 1.5f
                });
                groups.Add(new SpawnGroup
                {
                    EnemyType = "Flyer",
                    Count = DifficultyScaler.ScaleEnemyCount(8 + waveNumber / 3, waveNumber),
                    Delay = 1.0f
                });
                groups.Add(new SpawnGroup
                {
                    EnemyType = "Swarm",
                    Count = DifficultyScaler.ScaleEnemyCount(15 + waveNumber / 2, waveNumber),
                    Delay = 0.5f
                });
            }

            return groups;
        }

        /// <summary>
        /// Get boss type for a specific wave
        /// </summary>
        private string GetBossTypeForWave(int wave)
        {
            return wave switch
            {
                10 => "FrostTitan",
                20 => "InfernoColossus",
                30 => "VoidWraith",
                40 => "StormLord",
                50 => "ChaosBringer",
                _ => "FrostTitan"
            };
        }

        /// <summary>
        /// Load wave definition for specific wave number
        /// </summary>
        private WaveDefinition LoadWaveDefinition(int waveNumber)
        {
            if (_waveDefinitions.TryGetValue(waveNumber, out WaveDefinition def))
            {
                return def;
            }

            // Return empty wave if not found
            return new WaveDefinition
            {
                WaveNumber = waveNumber,
                SpawnGroups = new List<SpawnGroup>()
            };
        }

        #endregion

        #region Private Methods - Wave Spawning

        /// <summary>
        /// Spawn a normal (non-boss) wave
        /// </summary>
        private void SpawnNormalWave(WaveDefinition def)
        {
            _spawnQueue.Clear();

            foreach (var spawnGroup in def.SpawnGroups)
            {
                for (int i = 0; i < spawnGroup.Count; i++)
                {
                    _spawnQueue.Enqueue(new SpawnQueueItem
                    {
                        EnemyType = spawnGroup.EnemyType,
                        Delay = spawnGroup.Delay,
                        SpawnPattern = SpawnPoint.ParseSpawnPattern(spawnGroup.SpawnPattern),
                        GroupIndex = i,
                        GroupTotal = spawnGroup.Count
                    });
                }
            }

            _spawnTimer = 0.5f; // Initial spawn delay
        }

        /// <summary>
        /// Spawn a boss wave
        /// Returns the total number of enemies (boss + support)
        /// </summary>
        private int SpawnBossWave(WaveDefinition def)
        {
            _spawnQueue.Clear();
            
            // Get boss and support enemies from boss wave controller
            var bossEnemies = _bossWaveController.SpawnBossWave(CurrentWave, def);
            
            // Track all boss wave enemies
            foreach (var enemy in bossEnemies)
            {
                _activeEnemies.Add(enemy);
            }
            
            return bossEnemies.Count;
        }

        /// <summary>
        /// Process next spawn from queue
        /// </summary>
        private void ProcessNextSpawn()
        {
            if (_spawnQueue.Count == 0)
                return;

            var spawnItem = _spawnQueue.Dequeue();

            // Create enemy
            Node3D enemy = CreateEnemy(spawnItem.EnemyType);
            if (enemy == null)
            {
                GD.PrintErr($"Failed to create enemy: {spawnItem.EnemyType}");
                return;
            }

            // Get spawn position
            Vector3 spawnPos = GetSpawnPosition(spawnItem);

            // Apply difficulty scaling
            ApplyDifficultyScaling(enemy);

            // Add to scene
            GetTree().Root.AddChild(enemy);
            enemy.GlobalPosition = spawnPos;

            // Track enemy
            _activeEnemies.Add(enemy);

            // Set next spawn timer
            _spawnTimer = spawnItem.Delay;
        }

        /// <summary>
        /// Create enemy instance by type with validation
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
                GD.PrintErr($"CRITICAL: Enemy type {enemyType} does not inherit from EnemyBase! Difficulty scaling will not work. Skipping spawn.");
                return null; // Return null to prevent spawning invalid enemy
            }

            return enemy;
        }

        /// <summary>
        /// Get spawn position for enemy
        /// </summary>
        private Vector3 GetSpawnPosition(SpawnQueueItem item)
        {
            if (_spawnPoints.Count == 0)
                return Vector3.Zero;

            // Get random spawn point
            var spawnPoint = _spawnPoints[GD.Randi() % _spawnPoints.Count];

            return spawnPoint.GetSpawnPosition(
                item.SpawnPattern,
                item.GroupIndex,
                item.GroupTotal
            );
        }

        /// <summary>
        /// Apply difficulty scaling to spawned enemy
        /// </summary>
        private void ApplyDifficultyScaling(Node3D enemyNode)
        {
            if (enemyNode is EnemyBase enemy)
            {
                // Scale HP
                int baseHP = (int)enemy.MaxHealth;
                int scaledHP = DifficultyScaler.ScaleEnemyHP(baseHP, CurrentWave);
                scaledHP = (int)(scaledHP * DifficultyScaler.GetEliteHPMultiplier(CurrentWave));
                enemy.MaxHealth = scaledHP;

                // Scale damage
                int baseDamage = (int)enemy.AttackDamage;
                int scaledDamage = DifficultyScaler.ScaleEnemyDamage(baseDamage, CurrentWave);
                scaledDamage = (int)(scaledDamage * DifficultyScaler.GetEliteDamageMultiplier(CurrentWave));
                enemy.AttackDamage = scaledDamage;
            }
        }

        #endregion

        #region Private Methods - Wave Completion

        /// <summary>
        /// Complete the current wave
        /// </summary>
        private void CompleteWave()
        {
            IsWaveActive = false;
            CurrentBreakTime = WaveBreakTimer;

            // Calculate rewards
            int creditsReward = DifficultyScaler.CalculateCreditsReward(CurrentWave);
            int xpReward = DifficultyScaler.CalculateXPReward(CurrentWave);

            // Grant credits
            CurrencyManager.AddCredits(creditsReward, "wave_complete");

            // TODO: Integrate with player level/XP system when implemented
            // The xpReward is calculated and included in the event for future use

            // Emit wave completed event
            EventBus.Emit(EventBus.WaveCompleted, new WaveCompletedEventData
            {
                WaveNumber = CurrentWave,
                CreditsReward = creditsReward,
                XPReward = xpReward
            });

            GD.Print($"Wave {CurrentWave} completed! Credits: +{creditsReward}, XP: +{xpReward} (XP system pending)");
            GD.Print($"Next wave in {WaveBreakTimer} seconds");
        }

        #endregion

        #region Helper Classes

        private class SpawnQueueItem
        {
            public string EnemyType { get; set; }
            public float Delay { get; set; }
            public SpawnPoint.SpawnPattern SpawnPattern { get; set; }
            public int GroupIndex { get; set; }
            public int GroupTotal { get; set; }
        }

        #endregion
    }
}
