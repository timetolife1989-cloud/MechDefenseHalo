using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Crafting;
using MechDefenseHalo.Inventory;
using MechDefenseHalo.Items;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Crafting UI system with blueprint list, requirements, and queue
    /// </summary>
    public partial class CraftingUI : Control
    {
        #region Nodes
        
        [Export] public ItemList BlueprintList { get; set; }
        [Export] public Panel BlueprintDetails { get; set; }
        [Export] public Label BlueprintName { get; set; }
        [Export] public VBoxContainer MaterialRequirements { get; set; }
        [Export] public Label CreditCost { get; set; }
        [Export] public Label CraftTime { get; set; }
        [Export] public Button CraftButton { get; set; }
        [Export] public VBoxContainer CraftQueueContainer { get; set; }
        [Export] public OptionButton FilterDropdown { get; set; }
        [Export] public Button CloseButton { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private CraftingManager _craftingManager;
        private InventoryManager _inventoryManager;
        private Blueprint _selectedBlueprint;
        private ItemRarity _filterRarity = ItemRarity.Common;
        private bool _showAll = true;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Initially hidden
            Visible = false;
            
            // Connect signals
            if (BlueprintList != null)
            {
                BlueprintList.ItemSelected += OnBlueprintSelected;
            }
            
            if (FilterDropdown != null)
            {
                PopulateFilterDropdown();
                FilterDropdown.ItemSelected += OnFilterSelected;
            }
            
            if (CraftButton != null)
            {
                CraftButton.Pressed += OnCraftButtonPressed;
            }
            
            if (CloseButton != null)
            {
                CloseButton.Pressed += OnClosePressed;
            }
            
            // Listen for crafting events
            EventBus.On(EventBus.CraftStarted, OnCraftStarted);
            EventBus.On(EventBus.CraftCompleted, OnCraftCompleted);
            
            GD.Print("CraftingUI initialized");
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.C)
                {
                    ToggleVisibility();
                }
                else if (keyEvent.Keycode == Key.Escape && Visible)
                {
                    Hide();
                }
            }
        }
        
        public override void _Process(double delta)
        {
            if (Visible && _craftingManager != null)
            {
                UpdateCraftQueue();
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize with manager references
        /// </summary>
        public void Initialize(CraftingManager craftingManager, InventoryManager inventoryManager)
        {
            _craftingManager = craftingManager;
            _inventoryManager = inventoryManager;
            RefreshDisplay();
        }
        
        /// <summary>
        /// Toggle crafting UI visibility
        /// </summary>
        public void ToggleVisibility()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshDisplay();
            }
        }
        
        /// <summary>
        /// Refresh the entire crafting display
        /// </summary>
        public void RefreshDisplay()
        {
            if (_craftingManager == null) return;
            
            UpdateBlueprintList();
            UpdateCraftQueue();
            
            if (_selectedBlueprint != null)
            {
                DisplayBlueprintDetails(_selectedBlueprint);
            }
        }
        
        #endregion
        
        #region Private Methods - UI Setup
        
        private void PopulateFilterDropdown()
        {
            FilterDropdown.Clear();
            FilterDropdown.AddItem("All");
            FilterDropdown.AddItem("Common");
            FilterDropdown.AddItem("Uncommon");
            FilterDropdown.AddItem("Rare");
            FilterDropdown.AddItem("Epic");
            FilterDropdown.AddItem("Legendary");
            FilterDropdown.Selected = 0;
        }
        
        #endregion
        
        #region Private Methods - Display Update
        
        private void UpdateBlueprintList()
        {
            if (BlueprintList == null || _craftingManager == null) return;
            
            BlueprintList.Clear();
            
            var blueprints = _craftingManager.GetAllBlueprints();
            
            // Apply filter
            if (!_showAll)
            {
                blueprints = blueprints.Where(bp => bp.ResultRarity == _filterRarity).ToList();
            }
            
            foreach (var blueprint in blueprints)
            {
                string displayText = $"{blueprint.DisplayName} ({blueprint.ResultRarity})";
                BlueprintList.AddItem(displayText);
                
                // Store blueprint ID as metadata
                int index = BlueprintList.ItemCount - 1;
                BlueprintList.SetItemMetadata(index, blueprint.BlueprintID);
            }
        }
        
        private void DisplayBlueprintDetails(Blueprint blueprint)
        {
            if (BlueprintDetails == null || _inventoryManager == null) return;
            
            // Update blueprint name
            if (BlueprintName != null)
            {
                BlueprintName.Text = blueprint.DisplayName;
            }
            
            // Update material requirements
            if (MaterialRequirements != null)
            {
                // Clear existing
                foreach (var child in MaterialRequirements.GetChildren())
                {
                    child.QueueFree();
                }
                
                foreach (var material in blueprint.RequiredMaterials)
                {
                    int have = _inventoryManager.GetItemQuantity(material.Key);
                    int need = material.Value;
                    bool hasEnough = have >= need;
                    
                    var label = new Label();
                    label.Text = $"{need}x {material.Key} ({(hasEnough ? "✓" : "✗")} Have {have})";
                    label.Modulate = hasEnough ? Colors.Green : Colors.Red;
                    
                    MaterialRequirements.AddChild(label);
                }
            }
            
            // Update credit cost
            if (CreditCost != null)
            {
                CreditCost.Text = $"Cost: {blueprint.CreditCost:N0} Credits";
            }
            
            // Update craft time
            if (CraftTime != null)
            {
                int minutes = blueprint.CraftingTimeSeconds / 60;
                int seconds = blueprint.CraftingTimeSeconds % 60;
                
                if (minutes > 0)
                {
                    CraftTime.Text = $"Time: {minutes}m {seconds}s";
                }
                else
                {
                    CraftTime.Text = $"Time: {seconds}s";
                }
            }
            
            // Update craft button
            if (CraftButton != null)
            {
                bool canCraft = blueprint.CanCraft(_inventoryManager, 1, Economy.CurrencyManager.Credits);
                CraftButton.Disabled = !canCraft;
                CraftButton.Text = canCraft ? "CRAFT" : "Cannot Craft";
                CraftButton.Modulate = canCraft ? Colors.Green : Colors.Red;
            }
        }
        
        private void UpdateCraftQueue()
        {
            if (CraftQueueContainer == null || _craftingManager == null) return;
            
            // Clear existing
            foreach (var child in CraftQueueContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var activeJobs = _craftingManager.GetActiveJobs();
            
            foreach (var job in activeJobs)
            {
                var jobPanel = CreateCraftJobPanel(job);
                CraftQueueContainer.AddChild(jobPanel);
            }
            
            // Show empty message if no jobs
            if (activeJobs.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "No active crafting jobs";
                emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
                CraftQueueContainer.AddChild(emptyLabel);
            }
        }
        
        private Panel CreateCraftJobPanel(CraftingJob job)
        {
            var panel = new Panel();
            panel.CustomMinimumSize = new Vector2(400, 80);
            
            var hbox = new HBoxContainer();
            panel.AddChild(hbox);
            
            // Item name
            var nameLabel = new Label();
            nameLabel.Text = job.Blueprint.DisplayName;
            nameLabel.CustomMinimumSize = new Vector2(200, 0);
            hbox.AddChild(nameLabel);
            
            // Progress bar
            var vbox = new VBoxContainer();
            hbox.AddChild(vbox);
            
            var progressBar = new ProgressBar();
            progressBar.CustomMinimumSize = new Vector2(200, 20);
            progressBar.MaxValue = 100;
            progressBar.Value = job.PercentComplete;
            vbox.AddChild(progressBar);
            
            // Time remaining
            var timeLabel = new Label();
            int minutes = (int)job.TimeRemaining / 60;
            int seconds = (int)job.TimeRemaining % 60;
            timeLabel.Text = $"{minutes}m {seconds}s remaining";
            vbox.AddChild(timeLabel);
            
            // Instant finish button
            var instantButton = new Button();
            int coreCost = Economy.PricingConfig.GetInstantCraftCost((int)job.TimeRemaining);
            instantButton.Text = $"⚡ Instant ({coreCost} Cores)";
            instantButton.Pressed += () => OnInstantFinishPressed(job.JobID);
            hbox.AddChild(instantButton);
            
            // Cancel button
            var cancelButton = new Button();
            cancelButton.Text = "✕";
            cancelButton.Pressed += () => OnCancelCraftPressed(job.JobID);
            hbox.AddChild(cancelButton);
            
            return panel;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnBlueprintSelected(long index)
        {
            if (BlueprintList == null || _craftingManager == null) return;
            
            string blueprintID = BlueprintList.GetItemMetadata((int)index).AsString();
            _selectedBlueprint = _craftingManager.GetBlueprint(blueprintID);
            
            if (_selectedBlueprint != null)
            {
                DisplayBlueprintDetails(_selectedBlueprint);
            }
        }
        
        private void OnFilterSelected(long index)
        {
            _showAll = (index == 0);
            
            if (!_showAll)
            {
                _filterRarity = (ItemRarity)(index - 1);
            }
            
            UpdateBlueprintList();
        }
        
        private void OnCraftButtonPressed()
        {
            if (_selectedBlueprint == null || _craftingManager == null || _inventoryManager == null)
            {
                return;
            }
            
            bool success = _craftingManager.TryStartCraft(_selectedBlueprint.BlueprintID, _inventoryManager, 1);
            
            if (success)
            {
                RefreshDisplay();
            }
        }
        
        private void OnInstantFinishPressed(string jobID)
        {
            if (_craftingManager != null)
            {
                _craftingManager.InstantFinishCraft(jobID);
            }
        }
        
        private void OnCancelCraftPressed(string jobID)
        {
            if (_craftingManager != null)
            {
                _craftingManager.CancelCraft(jobID);
                UpdateCraftQueue();
            }
        }
        
        private void OnClosePressed()
        {
            Hide();
        }
        
        private void OnCraftStarted(object data)
        {
            RefreshDisplay();
        }
        
        private void OnCraftCompleted(object data)
        {
            RefreshDisplay();
        }
        
        #endregion
    }
}
