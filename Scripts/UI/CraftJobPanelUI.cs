using Godot;
using System;
using MechDefenseHalo.Crafting;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Displays a single crafting job with progress bar and controls.
    /// 
    /// REQUIRED SCENE STRUCTURE (create manually in Godot):
    /// 
    /// Panel (CraftJobPanelUI) - Script: CraftJobPanelUI.cs, custom_minimum_size: (280, 100)
    /// ├─ MarginContainer (margins: 8px all sides)
    /// │  └─ VBoxContainer (separation: 6)
    /// │     ├─ HBoxContainer (TopRow)
    /// │     │  ├─ TextureRect (ItemIcon) - custom_minimum_size: (48, 48), expand_mode: FIT_TO_RECT
    /// │     │  ├─ VBoxContainer (InfoContainer)
    /// │     │  │  ├─ Label (ItemNameLabel) - theme_override_font_sizes/font_size: 14
    /// │     │  │  └─ Label (TimeRemainingLabel) - theme_override_font_sizes/font_size: 11
    /// │     │  │                                   modulate: Color(0.8, 0.8, 0.8)
    /// │     ├─ ProgressBar (ProgressBar) - custom_minimum_size: height 12
    /// │     │                              show_percentage: true
    /// │     └─ HBoxContainer (ButtonRow)
    /// │        ├─ Button (InstantFinishButton) - text: "⚡ Instant (X Cores)", size_flags_h: EXPAND_FILL
    /// │        │                                 theme_override_colors/font_color: Color(1, 0.8, 0.2)
    /// │        └─ Button (CancelButton) - text: "✖ Cancel", size_flags_h: SHRINK_END
    /// </summary>
    public partial class CraftJobPanelUI : Panel
    {
        #region Export Variables

        /// <summary>Icon showing the item being crafted</summary>
        [Export] public TextureRect ItemIcon { get; set; }
        
        /// <summary>Label showing the item name and rarity</summary>
        [Export] public Label ItemNameLabel { get; set; }
        
        /// <summary>Label showing remaining time</summary>
        [Export] public Label TimeRemainingLabel { get; set; }
        
        /// <summary>Progress bar for craft completion</summary>
        [Export] public ProgressBar ProgressBar { get; set; }
        
        /// <summary>Button to instantly complete the craft</summary>
        [Export] public Button InstantFinishButton { get; set; }
        
        /// <summary>Button to cancel the craft</summary>
        [Export] public Button CancelButton { get; set; }

        #endregion

        #region Properties

        /// <summary>The crafting job this panel represents</summary>
        public CraftingJob Job { get; private set; }

        #endregion

        #region Events

        /// <summary>Fired when instant finish button is clicked</summary>
        public event Action<string> InstantFinishRequested;
        
        /// <summary>Fired when cancel button is clicked</summary>
        public event Action<string> CancelRequested;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Connect button signals
            if (InstantFinishButton != null)
            {
                InstantFinishButton.Pressed += OnInstantFinishPressed;
            }

            if (CancelButton != null)
            {
                CancelButton.Pressed += OnCancelPressed;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the panel with a crafting job
        /// </summary>
        /// <param name="job">The crafting job to display</param>
        public void SetJob(CraftingJob job)
        {
            Job = job;
            RefreshDisplay();
        }

        /// <summary>
        /// Update the progress display
        /// </summary>
        public void UpdateProgress()
        {
            if (Job == null) return;

            // Update progress bar
            if (ProgressBar != null)
            {
                ProgressBar.Value = Job.Progress * 100f;
            }

            // Update time remaining
            if (TimeRemainingLabel != null)
            {
                TimeRemainingLabel.Text = FormatTimeRemaining(Job.TimeRemaining);
            }
        }

        /// <summary>
        /// Refresh the entire display
        /// </summary>
        public void RefreshDisplay()
        {
            if (Job == null || Job.Blueprint == null) return;

            // Set item name with rarity color
            if (ItemNameLabel != null)
            {
                ItemNameLabel.Text = Job.Blueprint.DisplayName;
                ItemNameLabel.Modulate = RarityConfig.GetColor(Job.Blueprint.ResultRarity);
            }

            // Set icon (would need to get from ItemDatabase in real implementation)
            if (ItemIcon != null)
            {
                // TODO: Load icon from ItemDatabase using ResultItemID
                // For now, hide if no texture
                ItemIcon.Visible = false;
            }

            // Set progress bar
            if (ProgressBar != null)
            {
                ProgressBar.MinValue = 0;
                ProgressBar.MaxValue = 100;
                ProgressBar.Value = Job.Progress * 100f;
                
                // Color progress bar by rarity
                var rarityColor = RarityConfig.GetColor(Job.Blueprint.ResultRarity);
                ProgressBar.Modulate = rarityColor;
            }

            // Update time remaining
            if (TimeRemainingLabel != null)
            {
                TimeRemainingLabel.Text = FormatTimeRemaining(Job.TimeRemaining);
            }

            // Update instant finish button cost
            if (InstantFinishButton != null)
            {
                int coreCost = CalculateInstantFinishCost();
                InstantFinishButton.Text = $"⚡ Instant ({coreCost} Cores)";
            }

            // Set panel background color based on rarity (subtle)
            var bgColor = RarityConfig.GetColor(Job.Blueprint.ResultRarity);
            bgColor.A = 0.2f; // Very transparent
            Modulate = new Color(1, 1, 1, 1); // Reset modulate
            // Note: Actual background color would be set via StyleBox in .tscn
        }

        #endregion

        #region Private Methods

        private string FormatTimeRemaining(float seconds)
        {
            if (seconds <= 0)
            {
                return "Completing...";
            }

            int totalSeconds = Mathf.CeilToInt(seconds);
            
            if (totalSeconds < 60)
            {
                return $"{totalSeconds}s";
            }
            else if (totalSeconds < 3600)
            {
                int minutes = totalSeconds / 60;
                int remainingSeconds = totalSeconds % 60;
                return $"{minutes}m {remainingSeconds}s";
            }
            else
            {
                int hours = totalSeconds / 3600;
                int minutes = (totalSeconds % 3600) / 60;
                return $"{hours}h {minutes}m";
            }
        }

        private int CalculateInstantFinishCost()
        {
            if (Job == null) return 0;

            // Cost formula: 1 core per 60 seconds remaining (minimum 1 core)
            int timeInMinutes = Mathf.CeilToInt(Job.TimeRemaining / 60f);
            return Mathf.Max(1, timeInMinutes);
        }

        private void OnInstantFinishPressed()
        {
            if (Job != null)
            {
                InstantFinishRequested?.Invoke(Job.JobID);
            }
        }

        private void OnCancelPressed()
        {
            if (Job != null)
            {
                CancelRequested?.Invoke(Job.JobID);
            }
        }

        #endregion
    }
}
