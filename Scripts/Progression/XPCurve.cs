using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Progression
{
    /// <summary>
    /// Calculates XP requirements for each level using a balanced quadratic curve.
    /// Level 1-10: Linear progression (100 XP per level)
    /// Level 11-30: Quadratic growth with moderate scaling
    /// Level 31-60: Steeper quadratic growth for mid-game
    /// Level 61-100: Very steep quadratic growth for endgame challenge
    /// Total XP to level 100: ~2.4 million
    /// </summary>
    public static class XPCurve
    {
        private static Dictionary<int, int> _xpCache = new Dictionary<int, int>();
        private static bool _initialized = false;

        /// <summary>
        /// Get XP required to reach a specific level from level 1
        /// </summary>
        public static int GetXPRequired(int level)
        {
            if (level <= 1) return 0;
            if (level > 100) level = 100;

            if (!_initialized)
            {
                InitializeCache();
            }

            return _xpCache.ContainsKey(level) ? _xpCache[level] : CalculateXPRequired(level);
        }

        /// <summary>
        /// Get XP required for next level (from current level to next)
        /// </summary>
        public static int GetXPForNextLevel(int currentLevel)
        {
            if (currentLevel >= 100) return 0;
            
            int xpToCurrentLevel = GetXPRequired(currentLevel);
            int xpToNextLevel = GetXPRequired(currentLevel + 1);
            
            return xpToNextLevel - xpToCurrentLevel;
        }

        /// <summary>
        /// Initialize XP cache for all levels
        /// </summary>
        private static void InitializeCache()
        {
            _xpCache.Clear();
            _xpCache[1] = 0;

            int totalXP = 0;

            for (int level = 2; level <= 100; level++)
            {
                int xpForLevel = CalculateXPForLevel(level);
                totalXP += xpForLevel;
                _xpCache[level] = totalXP;
            }

            _initialized = true;
            GD.Print($"XPCurve initialized. Total XP to level 100: {totalXP}");
        }

        /// <summary>
        /// Calculate XP required for a specific level
        /// </summary>
        private static int CalculateXPRequired(int level)
        {
            if (level <= 1) return 0;

            int totalXP = 0;
            for (int i = 2; i <= level; i++)
            {
                totalXP += CalculateXPForLevel(i);
            }

            return totalXP;
        }

        /// <summary>
        /// Calculate XP required to go from (level-1) to level
        /// </summary>
        private static int CalculateXPForLevel(int level)
        {
            if (level <= 1) return 0;

            // Level 1-10: Linear (100 XP per level)
            if (level <= 10)
            {
                return 100;
            }
            // Level 11-30: Quadratic growth starting at 150 XP
            else if (level <= 30)
            {
                int levelsAbove10 = level - 10;
                return 150 + (levelsAbove10 * levelsAbove10 * 10);
            }
            // Level 31-60: Steeper quadratic growth
            else if (level <= 60)
            {
                int levelsAbove30 = level - 30;
                return 2250 + (levelsAbove30 * levelsAbove30 * 25);
            }
            // Level 61-100: Very steep growth for endgame
            else
            {
                int levelsAbove60 = level - 60;
                return 24750 + (levelsAbove60 * levelsAbove60 * 50);
            }
        }

        /// <summary>
        /// Get level from total XP
        /// </summary>
        public static int GetLevelFromXP(int totalXP)
        {
            if (!_initialized)
            {
                InitializeCache();
            }

            for (int level = 100; level >= 1; level--)
            {
                if (totalXP >= GetXPRequired(level))
                {
                    return level;
                }
            }

            return 1;
        }
    }
}
