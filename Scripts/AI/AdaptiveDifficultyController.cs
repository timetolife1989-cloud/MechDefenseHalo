using Godot;

namespace MechDefenseHalo.AI
{
    /// <summary>
    /// Controls adaptive difficulty scaling based on player performance.
    /// Adjusts spawn rates, enemy stats, and other difficulty parameters.
    /// </summary>
    public partial class AdaptiveDifficultyController : Node
    {
        private float _currentDifficulty = 0.5f; // 0.0 = easy, 1.0 = hard
        
        [Export] public float MinDifficulty { get; set; } = 0.2f;
        [Export] public float MaxDifficulty { get; set; } = 1.0f;
        
        /// <summary>
        /// Set the current difficulty level
        /// </summary>
        public void SetDifficultyLevel(float level)
        {
            _currentDifficulty = Mathf.Clamp(level, MinDifficulty, MaxDifficulty);
            GD.Print($"Difficulty adjusted to: {_currentDifficulty:F2}");
        }
        
        /// <summary>
        /// Get the current difficulty level
        /// </summary>
        public float GetDifficultyLevel()
        {
            return _currentDifficulty;
        }
        
        /// <summary>
        /// Get spawn rate multiplier based on difficulty
        /// </summary>
        public float GetSpawnRateMultiplier()
        {
            return Mathf.Lerp(0.5f, 2.0f, _currentDifficulty);
        }
        
        /// <summary>
        /// Get enemy health multiplier based on difficulty
        /// </summary>
        public float GetEnemyHealthMultiplier()
        {
            return Mathf.Lerp(0.7f, 1.5f, _currentDifficulty);
        }
        
        /// <summary>
        /// Get enemy damage multiplier based on difficulty
        /// </summary>
        public float GetEnemyDamageMultiplier()
        {
            return Mathf.Lerp(0.8f, 1.3f, _currentDifficulty);
        }
    }
}
