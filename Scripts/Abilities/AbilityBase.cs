using Godot;

namespace MechDefenseHalo.Abilities
{
    /// <summary>
    /// Base class for all player abilities.
    /// Defines common properties and behavior for special abilities.
    /// </summary>
    public abstract class AbilityBase
    {
        /// <summary>
        /// Unique identifier for this ability
        /// </summary>
        public string AbilityId { get; protected set; }
        
        /// <summary>
        /// Display name of the ability
        /// </summary>
        public string AbilityName { get; protected set; }
        
        /// <summary>
        /// Description of what the ability does
        /// </summary>
        public string Description { get; protected set; }
        
        /// <summary>
        /// Cooldown duration in seconds
        /// </summary>
        public float Cooldown { get; protected set; }
        
        /// <summary>
        /// Energy cost to use this ability
        /// </summary>
        public float EnergyCost { get; protected set; }
        
        /// <summary>
        /// Icon path for UI display
        /// </summary>
        public string IconPath { get; protected set; }
        
        /// <summary>
        /// Current upgrade level (0 = base level)
        /// </summary>
        public int UpgradeLevel { get; set; } = 0;
        
        /// <summary>
        /// Execute the ability effect
        /// </summary>
        /// <param name="user">The Node3D that is using the ability (usually the player)</param>
        public abstract void Execute(Node3D user);
        
        /// <summary>
        /// Check if the ability can be used (override for custom conditions)
        /// </summary>
        /// <param name="user">The user attempting to use the ability</param>
        /// <returns>True if ability can be used</returns>
        public virtual bool CanUse(Node3D user)
        {
            return true;
        }
        
        /// <summary>
        /// Get cooldown modified by upgrade level
        /// </summary>
        public float GetModifiedCooldown()
        {
            // Each upgrade reduces cooldown by 5%
            return Cooldown * Mathf.Max(0.5f, 1.0f - (UpgradeLevel * 0.05f));
        }
        
        /// <summary>
        /// Get energy cost modified by upgrade level
        /// </summary>
        public float GetModifiedEnergyCost()
        {
            // Each upgrade reduces cost by 5%
            return EnergyCost * Mathf.Max(0.5f, 1.0f - (UpgradeLevel * 0.05f));
        }
    }
}
