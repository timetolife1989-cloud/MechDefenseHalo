using Godot;
using System;

namespace MechDefenseHalo.WaveSystem
{
    /// <summary>
    /// Marker for enemy spawn locations with various spawn patterns
    /// </summary>
    public partial class SpawnPoint : Marker3D
    {
        /// <summary>
        /// Spawn pattern types for enemy positioning
        /// </summary>
        public enum SpawnPattern
        {
            Circle,      // Enemies spawn in circle around player
            Line,        // Linear formation
            Surround,    // Encircle player
            Random       // Random positions within radius
        }

        [Export] public float SpawnRadius { get; set; } = 20f;
        [Export] public float RandomSpawnRadius { get; set; } = 5f;

        /// <summary>
        /// Get spawn position based on pattern
        /// </summary>
        /// <param name="pattern">Spawn pattern to use</param>
        /// <param name="index">Index of enemy in spawn group</param>
        /// <param name="total">Total enemies in spawn group</param>
        /// <returns>World position for spawning</returns>
        public Vector3 GetSpawnPosition(SpawnPattern pattern, int index, int total)
        {
            Vector3 basePosition = GlobalPosition;

            switch (pattern)
            {
                case SpawnPattern.Circle:
                    return GetCirclePosition(basePosition, index, total);

                case SpawnPattern.Line:
                    return GetLinePosition(basePosition, index, total);

                case SpawnPattern.Surround:
                    return GetSurroundPosition(basePosition, index, total);

                case SpawnPattern.Random:
                default:
                    return GetRandomPosition(basePosition);
            }
        }

        /// <summary>
        /// Get position in a circle formation
        /// </summary>
        private Vector3 GetCirclePosition(Vector3 center, int index, int total)
        {
            if (total == 0) return center;

            float angleStep = 360f / total;
            float angle = Mathf.DegToRad(angleStep * index);

            return center + new Vector3(
                Mathf.Cos(angle) * SpawnRadius,
                0,
                Mathf.Sin(angle) * SpawnRadius
            );
        }

        /// <summary>
        /// Get position in a line formation
        /// </summary>
        private Vector3 GetLinePosition(Vector3 center, int index, int total)
        {
            if (total == 0) return center;

            float spacing = SpawnRadius * 2 / total;
            float offset = -SpawnRadius + (spacing * index) + (spacing * 0.5f);

            return center + new Vector3(offset, 0, 0);
        }

        /// <summary>
        /// Get position surrounding a point (full circle around player)
        /// </summary>
        private Vector3 GetSurroundPosition(Vector3 center, int index, int total)
        {
            if (total == 0) return center;

            float angleStep = 360f / total;
            float angle = Mathf.DegToRad(angleStep * index);

            // Use larger radius for surround pattern
            float surroundRadius = SpawnRadius * 1.5f;

            return center + new Vector3(
                Mathf.Cos(angle) * surroundRadius,
                0,
                Mathf.Sin(angle) * surroundRadius
            );
        }

        /// <summary>
        /// Get random position within spawn radius
        /// </summary>
        private Vector3 GetRandomPosition(Vector3 center)
        {
            float randomAngle = GD.Randf() * Mathf.Tau;
            float randomDistance = GD.Randf() * RandomSpawnRadius;

            return center + new Vector3(
                Mathf.Cos(randomAngle) * randomDistance,
                0,
                Mathf.Sin(randomAngle) * randomDistance
            );
        }

        /// <summary>
        /// Parse spawn pattern from string
        /// </summary>
        public static SpawnPattern ParseSpawnPattern(string patternString)
        {
            return patternString?.ToLower() switch
            {
                "circle" => SpawnPattern.Circle,
                "line" => SpawnPattern.Line,
                "surround" => SpawnPattern.Surround,
                "random" => SpawnPattern.Random,
                _ => SpawnPattern.Random
            };
        }
    }
}
