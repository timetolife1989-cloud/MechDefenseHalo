using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.UI.HUD
{
    /// <summary>
    /// Objective tracker UI component
    /// Displays current objectives and their completion status
    /// </summary>
    public partial class ObjectiveTrackerUI : Control
    {
        #region Exported Properties
        
        [Export] public NodePath ObjectiveListPath { get; set; }
        [Export] public PackedScene ObjectiveItemScene { get; set; }
        [Export] public int MaxVisibleObjectives { get; set; } = 5;
        
        #endregion
        
        #region Private Fields
        
        private VBoxContainer _objectiveList;
        private List<ObjectiveItem> _objectives = new List<ObjectiveItem>();
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Get UI nodes
            _objectiveList = GetNodeOrNull<VBoxContainer>(ObjectiveListPath);
            
            if (_objectiveList == null)
            {
                GD.PrintErr("ObjectiveTrackerUI: ObjectiveList not found!");
            }
            
            GD.Print("ObjectiveTrackerUI initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Add a new objective
        /// </summary>
        public void AddObjective(string id, string description, bool isOptional = false)
        {
            if (_objectiveList == null)
                return;
            
            // Check if objective already exists
            if (_objectives.Exists(o => o.Id == id))
            {
                GD.PrintErr($"Objective with id '{id}' already exists!");
                return;
            }
            
            // Create objective label
            var objectiveLabel = new Label();
            objectiveLabel.Text = $"• {description}";
            objectiveLabel.AddThemeColorOverride("font_color", isOptional ? Colors.Gray : Colors.White);
            
            _objectiveList.AddChild(objectiveLabel);
            
            var objective = new ObjectiveItem
            {
                Id = id,
                Description = description,
                IsOptional = isOptional,
                IsCompleted = false,
                LabelNode = objectiveLabel
            };
            
            _objectives.Add(objective);
            
            // Limit visible objectives
            UpdateVisibleObjectives();
            
            GD.Print($"Added objective: {description}");
        }
        
        /// <summary>
        /// Update objective progress
        /// </summary>
        public void UpdateObjective(string id, int current, int total)
        {
            var objective = _objectives.Find(o => o.Id == id);
            if (objective != null && objective.LabelNode != null)
            {
                objective.CurrentProgress = current;
                objective.TotalProgress = total;
                objective.LabelNode.Text = $"• {objective.Description} ({current}/{total})";
            }
        }
        
        /// <summary>
        /// Complete an objective
        /// </summary>
        public void CompleteObjective(string id)
        {
            var objective = _objectives.Find(o => o.Id == id);
            if (objective != null && objective.LabelNode != null)
            {
                objective.IsCompleted = true;
                objective.LabelNode.Text = $"✓ {objective.Description}";
                objective.LabelNode.AddThemeColorOverride("font_color", Colors.Green);
                
                // Fade out and remove after delay
                FadeOutObjective(objective);
                
                GD.Print($"Completed objective: {objective.Description}");
            }
        }
        
        /// <summary>
        /// Fail an objective
        /// </summary>
        public void FailObjective(string id)
        {
            var objective = _objectives.Find(o => o.Id == id);
            if (objective != null && objective.LabelNode != null)
            {
                objective.IsCompleted = false;
                objective.LabelNode.Text = $"✗ {objective.Description}";
                objective.LabelNode.AddThemeColorOverride("font_color", Colors.Red);
                
                // Fade out and remove after delay
                FadeOutObjective(objective);
                
                GD.Print($"Failed objective: {objective.Description}");
            }
        }
        
        /// <summary>
        /// Remove an objective
        /// </summary>
        public void RemoveObjective(string id)
        {
            var objective = _objectives.Find(o => o.Id == id);
            if (objective != null)
            {
                objective.LabelNode?.QueueFree();
                _objectives.Remove(objective);
                UpdateVisibleObjectives();
            }
        }
        
        /// <summary>
        /// Clear all objectives
        /// </summary>
        public void ClearAllObjectives()
        {
            foreach (var objective in _objectives)
            {
                objective.LabelNode?.QueueFree();
            }
            _objectives.Clear();
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Update visible objectives based on max limit
        /// </summary>
        private void UpdateVisibleObjectives()
        {
            if (_objectiveList == null)
                return;
            
            int visibleCount = 0;
            foreach (var objective in _objectives)
            {
                if (objective.LabelNode != null)
                {
                    objective.LabelNode.Visible = visibleCount < MaxVisibleObjectives;
                    visibleCount++;
                }
            }
        }
        
        /// <summary>
        /// Fade out and remove objective
        /// </summary>
        private void FadeOutObjective(ObjectiveItem objective)
        {
            if (objective.LabelNode == null)
                return;
            
            // Create tween for fade out
            var tween = CreateTween();
            tween.TweenProperty(objective.LabelNode, "modulate:a", 0.0, 1.0).SetDelay(2.0);
            tween.TweenCallback(Callable.From(() => 
            {
                // Check if objective still exists before removing
                if (_objectives.Contains(objective))
                {
                    RemoveObjective(objective.Id);
                }
            }));
        }
        
        #endregion
        
        #region Helper Classes
        
        /// <summary>
        /// Objective item data
        /// </summary>
        private class ObjectiveItem
        {
            public string Id { get; set; }
            public string Description { get; set; }
            public bool IsOptional { get; set; }
            public bool IsCompleted { get; set; }
            public int CurrentProgress { get; set; }
            public int TotalProgress { get; set; }
            public Label LabelNode { get; set; }
        }
        
        #endregion
    }
}
