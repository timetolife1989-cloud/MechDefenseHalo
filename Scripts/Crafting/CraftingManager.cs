using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;
using MechDefenseHalo.Items;
using MechDefenseHalo.Inventory;
using MechDefenseHalo.Economy;

namespace MechDefenseHalo.Crafting
{
    /// <summary>
    /// Manages crafting queue and blueprint system
    /// </summary>
    public partial class CraftingManager : Node
    {
        #region Constants

        private const int MAX_CONCURRENT_CRAFTS = 3;

        #endregion

        #region Private Fields

        private List<CraftingJob> _activeJobs = new();
        private Dictionary<string, Blueprint> _blueprints = new();
        private int _nextJobID = 0;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            LoadBlueprints();
            GD.Print($"CraftingManager initialized with {_blueprints.Count} blueprints");
        }

        public override void _Process(double delta)
        {
            UpdateCraftingProgress(delta);
        }

        #endregion

        #region Public Methods - Crafting

        /// <summary>
        /// Start crafting an item
        /// </summary>
        /// <param name="blueprintID">Blueprint to craft</param>
        /// <param name="inventory">Player inventory</param>
        /// <param name="playerLevel">Player level</param>
        /// <returns>True if craft started successfully</returns>
        public bool TryStartCraft(string blueprintID, InventoryManager inventory, int playerLevel)
        {
            if (_activeJobs.Count >= MAX_CONCURRENT_CRAFTS)
            {
                GD.PrintErr($"Cannot start craft: Queue is full ({MAX_CONCURRENT_CRAFTS} max)");
                return false;
            }

            if (!_blueprints.TryGetValue(blueprintID, out var blueprint))
            {
                GD.PrintErr($"Blueprint not found: {blueprintID}");
                return false;
            }

            if (!blueprint.CanCraft(inventory, playerLevel, CurrencyManager.CurrentCredits))
            {
                GD.PrintErr($"Cannot craft {blueprint.DisplayName}: Requirements not met");
                return false;
            }

            // Consume materials
            foreach (var material in blueprint.RequiredMaterials)
            {
                inventory.RemoveItem(material.Key, material.Value);
            }

            // Consume credits
            CurrencyManager.SpendCredits(blueprint.CreditCost, $"crafting {blueprint.DisplayName}");

            // Create crafting job
            var job = new CraftingJob
            {
                JobID = $"craft_{_nextJobID++}",
                BlueprintID = blueprintID,
                Blueprint = blueprint,
                TimeRemaining = blueprint.CraftingTimeSeconds,
                TotalTime = blueprint.CraftingTimeSeconds
            };

            _activeJobs.Add(job);
            
            GD.Print($"Started crafting: {blueprint.DisplayName} (ID: {job.JobID}, Time: {job.TotalTime}s)");

            EventBus.Emit(EventBus.CraftStarted, new CraftEventData
            {
                JobID = job.JobID,
                BlueprintID = blueprintID,
                ItemName = blueprint.DisplayName
            });

            return true;
        }

        /// <summary>
        /// Instantly finish a craft using premium currency
        /// </summary>
        /// <param name="jobID">Crafting job ID</param>
        /// <returns>True if completed</returns>
        public bool InstantFinishCraft(string jobID)
        {
            var job = _activeJobs.FirstOrDefault(j => j.JobID == jobID);
            if (job == null)
            {
                GD.PrintErr($"Craft job not found: {jobID}");
                return false;
            }

            int coreCost = PricingConfig.GetInstantCraftCost((int)job.TimeRemaining);

            if (!CurrencyManager.SpendCores(coreCost, $"instant craft {job.Blueprint.DisplayName}"))
            {
                return false;
            }

            CompleteCraft(job);
            return true;
        }

        /// <summary>
        /// Cancel a crafting job
        /// </summary>
        /// <param name="jobID">Job ID to cancel</param>
        /// <returns>True if cancelled</returns>
        public bool CancelCraft(string jobID)
        {
            var job = _activeJobs.FirstOrDefault(j => j.JobID == jobID);
            if (job == null)
            {
                GD.PrintErr($"Craft job not found: {jobID}");
                return false;
            }

            _activeJobs.Remove(job);
            GD.Print($"Cancelled craft: {job.Blueprint.DisplayName}");
            
            // Could refund partial materials here
            return true;
        }

        /// <summary>
        /// Get all active crafting jobs
        /// </summary>
        /// <returns>List of active jobs</returns>
        public List<CraftingJob> GetActiveJobs()
        {
            return new List<CraftingJob>(_activeJobs);
        }

        /// <summary>
        /// Get a blueprint by ID
        /// </summary>
        /// <param name="blueprintID">Blueprint identifier</param>
        /// <returns>Blueprint or null</returns>
        public Blueprint GetBlueprint(string blueprintID)
        {
            return _blueprints.TryGetValue(blueprintID, out var blueprint) ? blueprint : null;
        }

        /// <summary>
        /// Get all available blueprints
        /// </summary>
        /// <returns>List of blueprints</returns>
        public List<Blueprint> GetAllBlueprints()
        {
            return new List<Blueprint>(_blueprints.Values);
        }

        #endregion

        #region Private Methods

        private void UpdateCraftingProgress(double delta)
        {
            var completedJobs = new List<CraftingJob>();

            foreach (var job in _activeJobs)
            {
                job.TimeRemaining -= (float)delta;

                if (job.TimeRemaining <= 0)
                {
                    completedJobs.Add(job);
                }
            }

            foreach (var job in completedJobs)
            {
                CompleteCraft(job);
            }
        }

        private void CompleteCraft(CraftingJob job)
        {
            _activeJobs.Remove(job);

            GD.Print($"Craft completed: {job.Blueprint.DisplayName}");

            EventBus.Emit(EventBus.CraftCompleted, new CraftEventData
            {
                JobID = job.JobID,
                BlueprintID = job.BlueprintID,
                ItemName = job.Blueprint.DisplayName,
                ResultItemID = job.Blueprint.ResultItemID
            });

            // The inventory manager should listen to this event and add the item
        }

        private void LoadBlueprints()
        {
            _blueprints.Clear();

            // Create sample blueprints programmatically
            CreateSampleBlueprints();

            GD.Print($"Loaded {_blueprints.Count} blueprints");
        }

        private void CreateSampleBlueprints()
        {
            // Common weapon blueprint
            var commonWeapon = new Blueprint
            {
                BlueprintID = "bp_common_rifle",
                DisplayName = "Basic Assault Rifle",
                ResultItemID = "weapon_assault_rifle_basic",
                ResultRarity = ItemRarity.Common,
                CreditCost = 100,
                CraftingTimeSeconds = 60,
                RequiredPlayerLevel = 1
            };
            commonWeapon.RequiredMaterials["scrap_metal"] = 10;
            commonWeapon.RequiredMaterials["circuits"] = 5;
            _blueprints[commonWeapon.BlueprintID] = commonWeapon;

            // Rare armor blueprint
            var rareArmor = new Blueprint
            {
                BlueprintID = "bp_rare_armor",
                DisplayName = "Advanced Mech Plating",
                ResultItemID = "armor_advanced_plating",
                ResultRarity = ItemRarity.Rare,
                CreditCost = 500,
                CraftingTimeSeconds = 300,
                RequiredPlayerLevel = 10
            };
            rareArmor.RequiredMaterials["alloy_plates"] = 20;
            rareArmor.RequiredMaterials["plasma_core"] = 5;
            _blueprints[rareArmor.BlueprintID] = rareArmor;

            // Legendary weapon blueprint
            var legendaryWeapon = new Blueprint
            {
                BlueprintID = "bp_legendary_cannon",
                DisplayName = "Plasma Devastator",
                ResultItemID = "weapon_plasma_devastator",
                ResultRarity = ItemRarity.Legendary,
                CreditCost = 10000,
                CraftingTimeSeconds = 3600,
                RequiredPlayerLevel = 30
            };
            legendaryWeapon.RequiredMaterials["void_crystal"] = 10;
            legendaryWeapon.RequiredMaterials["quantum_chips"] = 15;
            legendaryWeapon.RequiredMaterials["plasma_core"] = 25;
            _blueprints[legendaryWeapon.BlueprintID] = legendaryWeapon;
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Represents an active crafting job
    /// </summary>
    public class CraftingJob
    {
        public string JobID { get; set; }
        public string BlueprintID { get; set; }
        public Blueprint Blueprint { get; set; }
        public float TimeRemaining { get; set; }
        public float TotalTime { get; set; }

        public float Progress => 1.0f - (TimeRemaining / TotalTime);
        public int PercentComplete => Mathf.RoundToInt(Progress * 100);
    }

    /// <summary>
    /// Data for craft events
    /// </summary>
    public class CraftEventData
    {
        public string JobID { get; set; }
        public string BlueprintID { get; set; }
        public string ItemName { get; set; }
        public string ResultItemID { get; set; }
    }

    #endregion
}
