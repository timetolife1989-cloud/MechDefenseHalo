using Godot;
using System;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Loot
{
    /// <summary>
    /// Global modifiers for loot drop rates and bad luck protection
    /// </summary>
    public static class LootModifiers
    {
        #region Private Fields

        private static float _globalLuckModifier = 1.0f;
        private static float _difficultyMultiplier = 1.0f;
        private static float _clanPerkBonus = 0.0f;
        private static float _battlePassBonus = 0.0f;
        private static float _eventBonus = 0.0f;

        // Bad luck protection
        private static int _noLegendaryCounter = 0;
        private const int PITY_TIMER = 100; // Guaranteed legendary after 100 kills without one

        #endregion

        #region Public Properties

        public static int NoLegendaryCounter => _noLegendaryCounter;
        public static int PityTimer => PITY_TIMER;

        #endregion

        #region Public Methods - Global Modifiers

        /// <summary>
        /// Get the combined global luck modifier
        /// </summary>
        /// <returns>Total luck multiplier</returns>
        public static float GetGlobalLuckModifier()
        {
            return _globalLuckModifier * _difficultyMultiplier * 
                   (1.0f + _clanPerkBonus + _battlePassBonus + _eventBonus);
        }

        /// <summary>
        /// Set the base player luck modifier
        /// </summary>
        /// <param name="luckStat">Player's luck stat value</param>
        public static void SetPlayerLuck(float luckStat)
        {
            // +0.1% Legendary per luck point (represented as multiplier)
            _globalLuckModifier = 1.0f + (luckStat * 0.001f);
            GD.Print($"Player luck modifier set to: {_globalLuckModifier:F3}");
        }

        /// <summary>
        /// Set difficulty multiplier for drop rates
        /// </summary>
        /// <param name="difficulty">Difficulty level</param>
        public static void SetDifficultyMultiplier(string difficulty)
        {
            _difficultyMultiplier = difficulty.ToLower() switch
            {
                "easy" => 0.8f,
                "normal" => 1.0f,
                "hard" => 1.5f,
                "nightmare" => 2.0f,
                _ => 1.0f
            };

            GD.Print($"Difficulty multiplier set to: {_difficultyMultiplier:F1}x ({difficulty})");
        }

        /// <summary>
        /// Set clan perk bonus
        /// </summary>
        /// <param name="bonus">Bonus percentage (0.0 to 1.0)</param>
        public static void SetClanPerkBonus(float bonus)
        {
            _clanPerkBonus = Mathf.Clamp(bonus, 0.0f, 1.0f);
            GD.Print($"Clan perk bonus set to: +{_clanPerkBonus * 100:F1}%");
        }

        /// <summary>
        /// Set Battle Pass premium bonus
        /// </summary>
        /// <param name="hasPremium">Whether player has premium Battle Pass</param>
        public static void SetBattlePassBonus(bool hasPremium)
        {
            _battlePassBonus = hasPremium ? 0.10f : 0.0f; // +10% Epic+ drops
            GD.Print($"Battle Pass bonus: {(hasPremium ? "+10%" : "None")}");
        }

        /// <summary>
        /// Set temporary event bonus
        /// </summary>
        /// <param name="bonus">Bonus percentage (0.0 to 1.0)</param>
        public static void SetEventBonus(float bonus)
        {
            _eventBonus = Mathf.Clamp(bonus, 0.0f, 2.0f); // Max 2x (200%)
            GD.Print($"Event bonus set to: +{_eventBonus * 100:F1}%");
        }

        #endregion

        #region Public Methods - Bad Luck Protection

        /// <summary>
        /// Check if a legendary drop should be forced due to bad luck protection
        /// </summary>
        /// <returns>True if legendary should be guaranteed</returns>
        public static bool ShouldForceLegendary()
        {
            return _noLegendaryCounter >= PITY_TIMER;
        }

        /// <summary>
        /// Increment the no-legendary counter
        /// </summary>
        public static void IncrementNoLegendaryCounter()
        {
            _noLegendaryCounter++;
            
            if (_noLegendaryCounter >= PITY_TIMER)
            {
                GD.Print($"[BAD LUCK PROTECTION] Pity timer reached! Next drop will be Legendary.");
            }
            else if (_noLegendaryCounter >= PITY_TIMER - 10)
            {
                GD.Print($"[BAD LUCK PROTECTION] {PITY_TIMER - _noLegendaryCounter} kills until guaranteed Legendary");
            }
        }

        /// <summary>
        /// Reset the no-legendary counter (called when legendary drops)
        /// </summary>
        public static void ResetNoLegendaryCounter()
        {
            if (_noLegendaryCounter >= PITY_TIMER)
            {
                GD.Print("[BAD LUCK PROTECTION] Pity timer activated - Legendary dropped!");
            }
            
            _noLegendaryCounter = 0;
        }

        /// <summary>
        /// Notify that a specific rarity was dropped (for bad luck protection tracking)
        /// </summary>
        /// <param name="rarity">The rarity that dropped</param>
        public static void NotifyDrop(ItemRarity rarity)
        {
            if (rarity >= ItemRarity.Legendary)
            {
                ResetNoLegendaryCounter();
            }
            else
            {
                IncrementNoLegendaryCounter();
            }
        }

        #endregion

        #region Public Methods - Utility

        /// <summary>
        /// Reset all modifiers to default
        /// </summary>
        public static void ResetAllModifiers()
        {
            _globalLuckModifier = 1.0f;
            _difficultyMultiplier = 1.0f;
            _clanPerkBonus = 0.0f;
            _battlePassBonus = 0.0f;
            _eventBonus = 0.0f;
            _noLegendaryCounter = 0;
            
            GD.Print("All loot modifiers reset to default");
        }

        /// <summary>
        /// Get a summary of all active modifiers
        /// </summary>
        /// <returns>Human-readable summary</returns>
        public static string GetModifiersSummary()
        {
            return $"Loot Modifiers:\n" +
                   $"  Player Luck: {_globalLuckModifier:F3}x\n" +
                   $"  Difficulty: {_difficultyMultiplier:F1}x\n" +
                   $"  Clan Perk: +{_clanPerkBonus * 100:F1}%\n" +
                   $"  Battle Pass: +{_battlePassBonus * 100:F1}%\n" +
                   $"  Event: +{_eventBonus * 100:F1}%\n" +
                   $"  TOTAL: {GetGlobalLuckModifier():F3}x\n" +
                   $"  Bad Luck Protection: {_noLegendaryCounter}/{PITY_TIMER}";
        }

        #endregion
    }
}
