using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Crafting;
using MechDefenseHalo.Inventory;
using MechDefenseHalo.Items;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Crafting UI with blueprint browser, requirements, and crafting queue.
    /// 
    /// REQUIRED SCENE STRUCTURE (create manually in Godot):
    /// 
    /// Control (CraftingUI) - Script: CraftingUI.cs
    /// ├─ Panel (Background)
    /// │  ├─ Label (Title) - text: "CRAFTING"
    /// │  ├─ HBoxContainer (MainContainer)
    /// │  │  ├─ VBoxContainer (LeftPanel) - Blueprint List
    /// │  │  │  ├─ HBoxContainer (FilterBar)
    /// │  │  │  │  ├─ Label - text: "Filter:"
    /// │  │  │  │  └─ OptionButton (FilterDropdown)
    /// │  │  │  └─ ItemList (BlueprintList) - size_flags_v: EXPAND_FILL
    /// │  │  ├─ VBoxContainer (MiddlePanel) - Blueprint Details
    /// │  │  │  ├─ Label (BlueprintName) - theme_override_font_sizes/font_size: 18
    /// │  │  │  ├─ HSeparator
    /// │  │  │  ├─ Label - text: "Materials Required:"
    /// │  │  │  ├─ VBoxContainer (MaterialRequirements)
    /// │  │  │  ├─ Label (CreditCost)
    /// │  │  │  ├─ Label (CraftTime)
    /// │  │  │  └─ Button (CraftButton) - text: "CRAFT"
    /// │  │  └─ VBoxContainer (RightPanel) - Crafting Queue
    /// │  │     ├─ Label (QueueCountLabel) - text: "Crafting Queue (0/3)"
    /// │  │     ├─ HSeparator
    /// │  │     └─ VBoxContainer (CraftQueueContainer) - separation: 8
    /// │  └─ Button (CloseButton) - text: "Close"
    /// </summary>
    public partial class CraftingUI : Control
    {
        #region Export Variables (Wire these in Godot Editor)

        [Export] public ItemList BlueprintList { get; set; }
        [Export] public Panel BlueprintDetails { get; set; }
        [Export] public Label BlueprintName { get; set; }
        [Export] public VBoxContainer MaterialRequirements { get; set; }
        [Export] public Label CreditCost { get; set; }
        [Export] public Label CraftTime { get; set; }
        [Export] public Button CraftButton { get; set; }
        [Export] public VBoxContainer CraftQueueContainer { get; set; }
        [Export] public Label QueueCountLabel { get; set; }
        [Export] public OptionButton FilterDropdown { get; set; }
        [Export] public Button CloseButton { get; set; }
        [Export] public PackedScene CraftJobPanelPrefab { get; set; } // Reference to CraftJobPanel.tscn

        #endregion

        #region Private Fields

        private CraftingManager _craftingManager;
        private InventoryManager _inventoryManager;
        private Blueprint _selectedBlueprint;
        private ItemRarity _filterRarity = ItemRarity.Common;
        private bool _showAll = true;
        private List<CraftJobPanelUI> _jobPanels = new();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get manager references
            _craftingManager = GetNode<CraftingManager>("/root/CraftingManager");
            _inventoryManager = GetNode<InventoryManager>("/root/InventoryManager");

            // Connect signals
            if (BlueprintList != null)
                BlueprintList.ItemSelected += OnBlueprintSelected;

            if (FilterDropdown != null)
            {
                PopulateFilterDropdown();
                FilterDropdown.ItemSelected += OnFilterSelected;
            }

            if (CraftButton != null)
                CraftButton.Pressed += OnCraftButtonPressed;

            if (CloseButton != null)
                CloseButton.Pressed += OnClosePressed;

            // Listen for crafting events
            EventBus.On(EventBus.CraftStarted, OnCraftStartedEvent);
            EventBus.On(EventBus.CraftCompleted, OnCraftCompletedEvent);

            // Initial display
            RefreshDisplay();

            // Hide by default
            Hide();

            GD.Print("CraftingUI initialized");
        }

        public override void _Input(InputEvent @event)
        {
            // Toggle with 'C' key
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.C)
            {
                ToggleVisibility();
                GetViewport().SetInputAsHandled();
            }
        }

        public override void _Process(double delta)
        {
            if (Visible && _craftingManager != null)
            {
                UpdateCraftQueueProgress();
            }
        }

        #endregion

        #region Public Methods

        public void ToggleVisibility()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshDisplay();
            }
        }

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
                BlueprintName.Modulate = RarityConfig.GetColor(blueprint.ResultRarity);
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
                bool canAfford = CurrencyManager.CurrentCredits >= blueprint.CreditCost;
                CreditCost.Text = $"Cost: {blueprint.CreditCost:N0} Credits";
                CreditCost.Modulate = canAfford ? Colors.White : Colors.Red;
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
                bool canCraft = blueprint.CanCraft(_inventoryManager, 1, CurrencyManager.CurrentCredits);
                CraftButton.Disabled = !canCraft;
                CraftButton.Text = canCraft ? "CRAFT" : "Cannot Craft";
                CraftButton.Modulate = canCraft ? Colors.Green : Colors.Red;
            }
        }

        private void UpdateCraftQueue()
        {
            if (CraftQueueContainer == null || _craftingManager == null) return;

            // Clear existing panels
            foreach (var panel in _jobPanels)
            {
                panel.QueueFree();
            }
            _jobPanels.Clear();

            var activeJobs = _craftingManager.GetActiveJobs();

            // Update queue count label
            if (QueueCountLabel != null)
            {
                QueueCountLabel.Text = $"Crafting Queue ({activeJobs.Count}/3)";
            }

            // Create job panels
            if (CraftJobPanelPrefab != null)
            {
                foreach (var job in activeJobs)
                {
                    var jobPanel = CraftJobPanelPrefab.Instantiate<CraftJobPanelUI>();
                    jobPanel.SetJob(job);

                    // Connect events
                    jobPanel.InstantFinishRequested += OnInstantFinishPressed;
                    jobPanel.CancelRequested += OnCancelCraftPressed;

                    CraftQueueContainer.AddChild(jobPanel);
                    _jobPanels.Add(jobPanel);
                }
            }

            // Show empty message if no jobs
            if (activeJobs.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "No active crafting jobs";
                emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
                emptyLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
                CraftQueueContainer.AddChild(emptyLabel);
            }
        }

        private void UpdateCraftQueueProgress()
        {
            // Update progress on all job panels
            foreach (var panel in _jobPanels)
            {
                panel.UpdateProgress();
            }
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

        private void OnCraftStartedEvent(object data)
        {
            RefreshDisplay();
        }

        private void OnCraftCompletedEvent(object data)
        {
            RefreshDisplay();

            // Show notification
            if (data is CraftEventData craftData)
            {
                GD.Print($"Craft completed: {craftData.ItemName}");
                // TODO: Show toast notification
            }
        }

        #endregion
    }
}
