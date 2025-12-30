using Godot;
using System.Linq;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.AI
{
    /// <summary>
    /// Main AI Director that manages adaptive enemy spawning and difficulty.
    /// Creates emergent gameplay by responding to player performance in real-time.
    /// </summary>
    public partial class AIDirector : Node
    {
        private PlayerPerformanceTracker performanceTracker;
        private AdaptiveDifficultyController difficultyController;
        private SwarmIntelligence swarmIntelligence;
        
        private float currentTension = 0.5f; // 0.0 = easy, 1.0 = intense
        private float tensionChangeRate = 0.1f;
        
        [Export] private float updateInterval = 2.0f;
        private float timeSinceUpdate = 0f;
        
        [Export] public bool EnableAdaptiveDifficulty { get; set; } = true;
        [Export] public bool EnableDynamicSpawning { get; set; } = false; // Disabled by default to not interfere with existing wave system
        
        public override void _Ready()
        {
            // Get or create child nodes
            performanceTracker = GetNodeOrNull<PlayerPerformanceTracker>("PerformanceTracker");
            if (performanceTracker == null)
            {
                performanceTracker = new PlayerPerformanceTracker();
                performanceTracker.Name = "PerformanceTracker";
                AddChild(performanceTracker);
            }
            
            difficultyController = GetNodeOrNull<AdaptiveDifficultyController>("DifficultyController");
            if (difficultyController == null)
            {
                difficultyController = new AdaptiveDifficultyController();
                difficultyController.Name = "DifficultyController";
                AddChild(difficultyController);
            }
            
            swarmIntelligence = GetNodeOrNull<SwarmIntelligence>("SwarmIntelligence");
            if (swarmIntelligence == null)
            {
                swarmIntelligence = new SwarmIntelligence();
                swarmIntelligence.Name = "SwarmIntelligence";
                AddChild(swarmIntelligence);
            }
            
            ConnectToGameEvents();
            GD.Print("AI Director initialized");
        }
        
        public override void _Process(double delta)
        {
            if (!EnableAdaptiveDifficulty)
                return;
                
            timeSinceUpdate += (float)delta;
            
            if (timeSinceUpdate >= updateInterval)
            {
                EvaluateGameState();
                AdjustTension();
                timeSinceUpdate = 0f;
            }
        }
        
        /// <summary>
        /// Connect to game events for AI awareness
        /// </summary>
        private void ConnectToGameEvents()
        {
            EventBus.On(EventBus.EnemyKilled, OnEnemyKilled);
            EventBus.On(EventBus.PlayerDied, OnPlayerDied);
        }
        
        public override void _ExitTree()
        {
            EventBus.Off(EventBus.EnemyKilled, OnEnemyKilled);
            EventBus.Off(EventBus.PlayerDied, OnPlayerDied);
        }
        
        private void OnEnemyKilled(object data)
        {
            // Track enemy for swarm intelligence if it's a Node3D
            // (Already handled by PerformanceTracker for statistics)
        }
        
        private void OnPlayerDied(object data)
        {
            // Player died - reduce tension immediately
            currentTension = Mathf.Max(0.2f, currentTension - 0.3f);
            GD.Print("AI Director: Player died, reducing tension");
        }
        
        /// <summary>
        /// Evaluate current game state and adjust AI behavior
        /// </summary>
        private void EvaluateGameState()
        {
            var performance = performanceTracker.GetCurrentPerformance();
            
            // Player dominating? Increase tension
            if (performance.KillRate > 10 && performance.HealthPercent > 0.8f)
            {
                currentTension += tensionChangeRate;
                GD.Print("AI Director: Player dominating, increasing intensity");
            }
            // Player struggling? Decrease tension
            else if (performance.HealthPercent < 0.3f && performance.DeathsRecent > 2)
            {
                currentTension -= tensionChangeRate;
                GD.Print("AI Director: Player struggling, easing difficulty");
            }
            // Player idle/camping? Send reminder wave
            else if (performance.TimeSinceLastKill > 30f && EnableDynamicSpawning)
            {
                SpawnReminderWave();
            }
            
            currentTension = Mathf.Clamp(currentTension, 0.2f, 1.0f);
            difficultyController.SetDifficultyLevel(currentTension);
        }
        
        /// <summary>
        /// Adjust spawn rates based on tension level
        /// </summary>
        private void AdjustTension()
        {
            if (!EnableDynamicSpawning)
                return;
                
            // Adjust spawn rates based on tension
            int enemyCount = GetTree().GetNodesInGroup("enemies").Count;
            int desiredCount = Mathf.RoundToInt(10 + (currentTension * 20));
            
            if (enemyCount < desiredCount)
            {
                SpawnAdaptiveEnemy();
            }
        }
        
        /// <summary>
        /// Spawn an enemy with adaptive personality
        /// </summary>
        private void SpawnAdaptiveEnemy()
        {
            // Don't spawn scripted enemy types, generate based on situation
            var personality = GeneratePersonalityForSituation();
            
            // Note: Actual enemy spawning would require enemy scene references
            // This is a placeholder that demonstrates the system
            GD.Print($"AI Director: Would spawn enemy with personality - " +
                    $"Speed: {personality.Speed:F2}, Aggression: {personality.Aggression:F2}");
            
            // Use swarm intelligence to decide spawn location
            Vector3 spawnPos = swarmIntelligence.GetOptimalSpawnPosition();
            GD.Print($"AI Director: Spawn position calculated at {spawnPos}");
        }
        
        /// <summary>
        /// Generate enemy personality based on current game situation
        /// </summary>
        private EnemyPersonality GeneratePersonalityForSituation()
        {
            var performance = performanceTracker.GetCurrentPerformance();
            
            return new EnemyPersonality
            {
                Speed = GD.Randf() * (0.5f + currentTension * 0.5f),
                Aggression = performance.HealthPercent > 0.5f ? 0.8f : 0.4f,
                Caution = performance.KillRate > 5 ? 0.7f : 0.3f,
                Teamwork = swarmIntelligence.GetAllyCount() > 3 ? 0.9f : 0.2f,
                Range = performance.IsPlayerAggressive ? 0.8f : 0.3f
            };
        }
        
        /// <summary>
        /// Spawn reminder wave for idle players
        /// </summary>
        private void SpawnReminderWave()
        {
            GD.Print("AI Director: Player idle, sending reminder wave");
            
            // Spawn 3-5 aggressive enemies to re-engage player
            for (int i = 0; i < GD.RandRange(3, 5); i++)
            {
                var personality = new EnemyPersonality
                {
                    Speed = 0.8f,
                    Aggression = 1.0f,
                    Caution = 0.1f,
                    Teamwork = 0.5f,
                    Range = 0.3f
                };
                
                GD.Print($"AI Director: Spawning reminder enemy #{i + 1}");
                // Actual spawning would happen here with enemy scene instantiation
            }
        }
        
        /// <summary>
        /// Get current tension level
        /// </summary>
        public float GetCurrentTension()
        {
            return currentTension;
        }
        
        /// <summary>
        /// Get current difficulty level from controller
        /// </summary>
        public float GetDifficultyLevel()
        {
            return difficultyController?.GetDifficultyLevel() ?? 0.5f;
        }
        
        /// <summary>
        /// Register an enemy with the swarm intelligence system
        /// </summary>
        public void RegisterEnemy(Node3D enemy)
        {
            swarmIntelligence?.RegisterEnemy(enemy);
        }
    }
}
