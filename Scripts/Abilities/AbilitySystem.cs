using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.Abilities
{
    /// <summary>
    /// Main ability system controller that manages all player abilities.
    /// Handles ability registration, execution, cooldowns, and energy costs.
    /// 
    /// SETUP:
    /// Add as a child node to the PlayerMechController in the scene hierarchy.
    /// The system will automatically register all abilities on _Ready.
    /// 
    /// USAGE:
    /// - Call UseAbility(index) to use an ability by its slot index
    /// - Call UseAbilityById(id) to use an ability by its ID
    /// - Query GetAbility(index) or GetAbilityById(id) to get ability info
    /// - Check GetAbilityReady(index) to see if an ability is ready to use
    /// </summary>
    public partial class AbilitySystem : Node
    {
        #region Signals
        
        [Signal]
        public delegate void AbilityUsedEventHandler(string abilityId, string abilityName);
        
        [Signal]
        public delegate void AbilityCooldownStartedEventHandler(string abilityId, float duration);
        
        [Signal]
        public delegate void AbilityUpgradedEventHandler(string abilityId, int newLevel);
        
        #endregion
        
        #region Private Fields
        
        private List<AbilityBase> _abilities = new();
        private CooldownManager _cooldownManager;
        private Node3D _user; // The player/entity using abilities
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Create and add cooldown manager
            _cooldownManager = new CooldownManager
            {
                Name = "CooldownManager"
            };
            AddChild(_cooldownManager);
            
            // Get reference to the user (parent node, usually PlayerMechController)
            _user = GetParent<Node3D>();
            
            // Register all available abilities
            RegisterAbilities();
            
            GD.Print($"[AbilitySystem] Initialized with {_abilities.Count} abilities");
        }
        
        public override void _Input(InputEvent @event)
        {
            // Handle ability hotkeys
            if (@event.IsActionPressed("weapon_1"))
                UseAbility(0);
            else if (@event.IsActionPressed("weapon_2"))
                UseAbility(1);
            else if (@event.IsActionPressed("weapon_3"))
                UseAbility(2);
            else if (@event.IsActionPressed("weapon_4"))
                UseAbility(3);
        }
        
        #endregion
        
        #region Ability Registration
        
        /// <summary>
        /// Register all available abilities in the system
        /// </summary>
        private void RegisterAbilities()
        {
            _abilities.Clear();
            
            // Register the 4 core abilities
            _abilities.Add(new DashAbility());
            _abilities.Add(new ShieldAbility());
            _abilities.Add(new EMPAbility());
            _abilities.Add(new TimeSlowAbility());
            
            GD.Print("[AbilitySystem] Registered abilities:");
            for (int i = 0; i < _abilities.Count; i++)
            {
                var ability = _abilities[i];
                GD.Print($"  [{i}] {ability.AbilityName} (ID: {ability.AbilityId})");
            }
        }
        
        #endregion
        
        #region Public Methods - Ability Usage
        
        /// <summary>
        /// Use an ability by its slot index
        /// </summary>
        /// <param name="index">Ability slot (0-3)</param>
        /// <returns>True if ability was used successfully</returns>
        public bool UseAbility(int index)
        {
            if (index < 0 || index >= _abilities.Count)
            {
                GD.PrintErr($"[AbilitySystem] Invalid ability index: {index}");
                return false;
            }
            
            return UseAbilityInternal(_abilities[index]);
        }
        
        /// <summary>
        /// Use an ability by its ID
        /// </summary>
        /// <param name="abilityId">Unique ability identifier</param>
        /// <returns>True if ability was used successfully</returns>
        public bool UseAbilityById(string abilityId)
        {
            var ability = _abilities.Find(a => a.AbilityId == abilityId);
            
            if (ability == null)
            {
                GD.PrintErr($"[AbilitySystem] Ability not found: {abilityId}");
                return false;
            }
            
            return UseAbilityInternal(ability);
        }
        
        #endregion
        
        #region Private Methods - Ability Execution
        
        private bool UseAbilityInternal(AbilityBase ability)
        {
            // Check cooldown
            if (_cooldownManager.IsOnCooldown(ability.AbilityId))
            {
                float remaining = _cooldownManager.GetRemainingCooldown(ability.AbilityId);
                GD.Print($"[AbilitySystem] {ability.AbilityName} on cooldown! {remaining:F1}s remaining");
                return false;
            }
            
            // Check custom conditions
            if (!ability.CanUse(_user))
            {
                GD.Print($"[AbilitySystem] {ability.AbilityName} cannot be used right now");
                return false;
            }
            
            // Check energy cost
            float energyCost = ability.GetModifiedEnergyCost();
            if (!HasEnoughEnergy(energyCost))
            {
                GD.Print($"[AbilitySystem] Not enough energy! Need {energyCost}, have {GetCurrentEnergy()}");
                return false;
            }
            
            // Execute the ability
            try
            {
                ability.Execute(_user);
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"[AbilitySystem] Error executing {ability.AbilityName}: {ex.Message}");
                return false;
            }
            
            // Consume energy
            ConsumeEnergy(energyCost);
            
            // Start cooldown
            float cooldown = ability.GetModifiedCooldown();
            _cooldownManager.StartCooldown(ability.AbilityId, cooldown);
            
            // Emit signals
            EmitSignal(SignalName.AbilityUsed, ability.AbilityId, ability.AbilityName);
            EmitSignal(SignalName.AbilityCooldownStarted, ability.AbilityId, cooldown);
            
            GD.Print($"[AbilitySystem] Used {ability.AbilityName}! Energy: -{energyCost}, Cooldown: {cooldown}s");
            return true;
        }
        
        #endregion
        
        #region Energy Management
        
        /// <summary>
        /// Check if the user has enough energy
        /// </summary>
        private bool HasEnoughEnergy(float cost)
        {
            float currentEnergy = GetCurrentEnergy();
            return currentEnergy >= cost;
        }
        
        /// <summary>
        /// Get current energy from the player
        /// </summary>
        private float GetCurrentEnergy()
        {
            // Try to get energy from PlayerMechController
            if (_user != null && _user.HasMethod("get_CurrentEnergy"))
            {
                var result = _user.Call("get_CurrentEnergy");
                return result.AsSingle();
            }
            
            // Try property access
            if (_user != null)
            {
                var property = _user.Get("CurrentEnergy");
                if (property.VariantType != Variant.Type.Nil)
                {
                    return property.AsSingle();
                }
            }
            
            return 100f; // Default fallback
        }
        
        /// <summary>
        /// Consume energy from the player
        /// </summary>
        private void ConsumeEnergy(float cost)
        {
            if (_user != null && _user.HasMethod("ConsumeEnergy"))
            {
                _user.Call("ConsumeEnergy", cost);
            }
        }
        
        #endregion
        
        #region Public Query Methods
        
        /// <summary>
        /// Get an ability by index
        /// </summary>
        public AbilityBase GetAbility(int index)
        {
            if (index >= 0 && index < _abilities.Count)
                return _abilities[index];
            return null;
        }
        
        /// <summary>
        /// Get an ability by ID
        /// </summary>
        public AbilityBase GetAbilityById(string abilityId)
        {
            return _abilities.Find(a => a.AbilityId == abilityId);
        }
        
        /// <summary>
        /// Get all registered abilities
        /// </summary>
        public List<AbilityBase> GetAllAbilities()
        {
            return new List<AbilityBase>(_abilities);
        }
        
        /// <summary>
        /// Check if an ability is ready to use (not on cooldown)
        /// </summary>
        public bool GetAbilityReady(int index)
        {
            if (index < 0 || index >= _abilities.Count)
                return false;
                
            var ability = _abilities[index];
            return !_cooldownManager.IsOnCooldown(ability.AbilityId);
        }
        
        /// <summary>
        /// Get cooldown progress for an ability (0-1, where 1 = ready)
        /// </summary>
        public float GetAbilityCooldownProgress(int index)
        {
            if (index < 0 || index >= _abilities.Count)
                return 1f;
                
            var ability = _abilities[index];
            return _cooldownManager.GetCooldownProgress(ability.AbilityId);
        }
        
        /// <summary>
        /// Get remaining cooldown time for an ability
        /// </summary>
        public float GetAbilityRemainingCooldown(int index)
        {
            if (index < 0 || index >= _abilities.Count)
                return 0f;
                
            var ability = _abilities[index];
            return _cooldownManager.GetRemainingCooldown(ability.AbilityId);
        }
        
        /// <summary>
        /// Get total number of abilities
        /// </summary>
        public int GetAbilityCount()
        {
            return _abilities.Count;
        }
        
        #endregion
        
        #region Upgrade System
        
        /// <summary>
        /// Upgrade an ability to increase its effectiveness
        /// </summary>
        /// <param name="index">Ability slot index</param>
        /// <returns>True if upgrade was successful</returns>
        public bool UpgradeAbility(int index)
        {
            if (index < 0 || index >= _abilities.Count)
                return false;
                
            var ability = _abilities[index];
            ability.UpgradeLevel++;
            
            EmitSignal(SignalName.AbilityUpgraded, ability.AbilityId, ability.UpgradeLevel);
            
            GD.Print($"[AbilitySystem] Upgraded {ability.AbilityName} to level {ability.UpgradeLevel}");
            return true;
        }
        
        /// <summary>
        /// Upgrade an ability by ID
        /// </summary>
        public bool UpgradeAbilityById(string abilityId)
        {
            var ability = GetAbilityById(abilityId);
            if (ability == null)
                return false;
                
            int index = _abilities.IndexOf(ability);
            return UpgradeAbility(index);
        }
        
        #endregion
    }
}
