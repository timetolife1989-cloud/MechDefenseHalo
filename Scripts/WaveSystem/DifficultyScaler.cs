using Godot;
using System;

namespace MechDefenseHalo.WaveSystem
{
    /// <summary>
    /// Handles difficulty scaling for waves
    /// Scales enemy HP, damage, and count based on wave number
    /// </summary>
    public static class DifficultyScaler
    {
        /// <summary>
        /// Scale enemy HP based on wave number
        /// Linear scaling for waves 1-10, exponential after wave 10
        /// </summary>
        public static int ScaleEnemyHP(int baseHP, int waveNumber)
        {
            if (waveNumber <= 0)
                return baseHP;

            if (waveNumber <= 10)
            {
                // Linear scaling: +50 HP per wave
                return baseHP + (waveNumber * 50);
            }

            // Exponential scaling after wave 10: +15% per wave
            float multiplier = 1 + (waveNumber - 10) * 0.15f;
            return Mathf.RoundToInt(baseHP * multiplier);
        }

        /// <summary>
        /// Scale enemy damage based on wave number
        /// No scaling for waves 1-10, +10% per wave after
        /// </summary>
        public static int ScaleEnemyDamage(int baseDamage, int waveNumber)
        {
            if (waveNumber <= 0)
                return baseDamage;

            if (waveNumber <= 10)
            {
                // No damage scaling in tutorial waves
                return baseDamage;
            }

            // +10% damage per wave after wave 10
            float multiplier = 1 + (waveNumber - 10) * 0.10f;
            return Mathf.RoundToInt(baseDamage * multiplier);
        }

        /// <summary>
        /// Scale enemy count based on wave number
        /// +10% enemies per 5 waves
        /// </summary>
        public static int ScaleEnemyCount(int baseCount, int waveNumber)
        {
            if (waveNumber <= 0)
                return baseCount;

            // +10% enemies per 5 waves
            float multiplier = 1 + Mathf.Floor(waveNumber / 5) * 0.10f;
            return Mathf.RoundToInt(baseCount * multiplier);
        }

        /// <summary>
        /// Calculate credits reward for wave completion
        /// </summary>
        public static int CalculateCreditsReward(int waveNumber)
        {
            return waveNumber * 50;
        }

        /// <summary>
        /// Calculate XP reward for wave completion
        /// </summary>
        public static int CalculateXPReward(int waveNumber)
        {
            return waveNumber * 100;
        }

        /// <summary>
        /// Check if wave should be an elite wave (2x HP, 1.5x damage)
        /// Waves 31-50 have elite variants
        /// </summary>
        public static bool IsEliteWave(int waveNumber)
        {
            return waveNumber >= 31;
        }

        /// <summary>
        /// Get elite multiplier for HP
        /// </summary>
        public static float GetEliteHPMultiplier(int waveNumber)
        {
            return IsEliteWave(waveNumber) ? 2.0f : 1.0f;
        }

        /// <summary>
        /// Get elite multiplier for damage
        /// </summary>
        public static float GetEliteDamageMultiplier(int waveNumber)
        {
            return IsEliteWave(waveNumber) ? 1.5f : 1.0f;
        }
    }
}
