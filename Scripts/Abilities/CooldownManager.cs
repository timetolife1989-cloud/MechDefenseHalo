using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.Abilities
{
    /// <summary>
    /// Manages cooldowns for all abilities.
    /// Tracks when abilities can be used again and handles cooldown timers.
    /// </summary>
    public partial class CooldownManager : Node
    {
        private Dictionary<string, float> _cooldowns = new();
        private Dictionary<string, float> _cooldownTimers = new();
        
        public override void _Process(double delta)
        {
            // Update all active cooldown timers
            List<string> completedCooldowns = new();
            
            foreach (var kvp in _cooldownTimers)
            {
                string abilityId = kvp.Key;
                float timeRemaining = kvp.Value - (float)delta;
                
                if (timeRemaining <= 0)
                {
                    completedCooldowns.Add(abilityId);
                }
                else
                {
                    _cooldownTimers[abilityId] = timeRemaining;
                }
            }
            
            // Remove completed cooldowns
            foreach (string abilityId in completedCooldowns)
            {
                _cooldownTimers.Remove(abilityId);
            }
        }
        
        /// <summary>
        /// Start a cooldown for an ability
        /// </summary>
        /// <param name="abilityId">Unique ability identifier</param>
        /// <param name="duration">Cooldown duration in seconds</param>
        public void StartCooldown(string abilityId, float duration)
        {
            _cooldowns[abilityId] = duration;
            _cooldownTimers[abilityId] = duration;
        }
        
        /// <summary>
        /// Check if an ability is currently on cooldown
        /// </summary>
        /// <param name="abilityId">Ability to check</param>
        /// <returns>True if on cooldown, false if ready to use</returns>
        public bool IsOnCooldown(string abilityId)
        {
            return _cooldownTimers.ContainsKey(abilityId);
        }
        
        /// <summary>
        /// Get remaining cooldown time for an ability
        /// </summary>
        /// <param name="abilityId">Ability to check</param>
        /// <returns>Seconds remaining, or 0 if not on cooldown</returns>
        public float GetRemainingCooldown(string abilityId)
        {
            return _cooldownTimers.TryGetValue(abilityId, out float remaining) ? remaining : 0f;
        }
        
        /// <summary>
        /// Get cooldown progress as a percentage (0-1)
        /// </summary>
        /// <param name="abilityId">Ability to check</param>
        /// <returns>Progress from 0 (just started) to 1 (ready)</returns>
        public float GetCooldownProgress(string abilityId)
        {
            if (!_cooldownTimers.ContainsKey(abilityId))
                return 1f; // Ready
                
            if (!_cooldowns.ContainsKey(abilityId))
                return 1f;
                
            float total = _cooldowns[abilityId];
            float remaining = _cooldownTimers[abilityId];
            
            return Mathf.Clamp((total - remaining) / total, 0f, 1f);
        }
        
        /// <summary>
        /// Reset a specific ability's cooldown (make it available immediately)
        /// </summary>
        /// <param name="abilityId">Ability to reset</param>
        public void ResetCooldown(string abilityId)
        {
            _cooldownTimers.Remove(abilityId);
        }
        
        /// <summary>
        /// Reset all cooldowns
        /// </summary>
        public void ResetAllCooldowns()
        {
            _cooldownTimers.Clear();
        }
    }
}
