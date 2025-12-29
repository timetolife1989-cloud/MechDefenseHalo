using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Items;
using MechDefenseHalo.Items.Sets;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// Displays active set bonuses and set progress for equipped items.
    /// 
    /// REQUIRED SCENE STRUCTURE (create manually in Godot):
    /// 
    /// Panel (SetBonusPanelUI) - Script: SetBonusPanelUI.cs
    /// ├─ VBoxContainer (MainContainer) - separation: 8
    /// │  ├─ Label (Title) - text: "SET BONUSES", horizontal_alignment: CENTER
    /// │  │                  theme_override_font_sizes/font_size: 18
    /// │  ├─ HSeparator
    /// │  ├─ ScrollContainer (ScrollContainer) - horizontal_scroll_mode: DISABLED
    /// │  │  └─ VBoxContainer (SetListContainer) - separation: 12
    /// │  └─ Label (EmptyLabel) - text: "No active set bonuses", visible: false
    /// │                           horizontal_alignment: CENTER, modulate: Color(0.6, 0.6, 0.6)
    /// </summary>
    public partial class SetBonusPanelUI : Panel
    {
        #region Export Variables

        /// <summary>Container for individual set bonus entries</summary>
        [Export] public VBoxContainer SetListContainer { get; set; }
        
        /// <summary>Label shown when no sets are active</summary>
        [Export] public Label EmptyLabel { get; set; }
        
        /// <summary>Title label at the top</summary>
        [Export] public Label Title { get; set; }

        #endregion

        #region Private Fields

        private SetManager _setManager;
        private List<Control> _setBonusEntries = new();

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            GD.Print("SetBonusPanelUI initialized");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the panel with a SetManager reference
        /// </summary>
        /// <param name="setManager">SetManager to track bonuses from</param>
        public void Initialize(SetManager setManager)
        {
            _setManager = setManager;
            RefreshDisplay();
        }

        /// <summary>
        /// Refresh the entire set bonus display
        /// </summary>
        public void RefreshDisplay()
        {
            ClearAllEntries();

            if (_setManager == null)
            {
                ShowEmptyState();
                return;
            }

            var setProgress = _setManager.GetSetProgress();

            if (setProgress.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            // Hide empty label
            if (EmptyLabel != null)
                EmptyLabel.Hide();

            // Create entry for each active set
            foreach (var kvp in setProgress)
            {
                CreateSetBonusEntry(kvp.Value);
            }
        }

        #endregion

        #region Private Methods

        private void ClearAllEntries()
        {
            if (SetListContainer == null) return;

            foreach (var entry in _setBonusEntries)
            {
                entry.QueueFree();
            }
            _setBonusEntries.Clear();
        }

        private void ShowEmptyState()
        {
            if (EmptyLabel != null)
                EmptyLabel.Show();
        }

        private void CreateSetBonusEntry(SetProgressInfo setInfo)
        {
            if (SetListContainer == null) return;

            // Create container for this set
            var entryContainer = new VBoxContainer();
            entryContainer.AddThemeConstantOverride("separation", 4);

            // Set header with name and progress
            var headerLabel = new Label();
            headerLabel.Text = $"{setInfo.SetName} ({setInfo.EquippedPieces}/{setInfo.TotalPieces})";
            headerLabel.AddThemeFontSizeOverride("font_size", 16);
            
            // Color header by whether bonus is active
            if (setInfo.ActiveBonus != null)
            {
                headerLabel.Modulate = new Color(0.3f, 1.0f, 0.3f); // Green for active
            }
            else
            {
                headerLabel.Modulate = new Color(0.8f, 0.8f, 0.8f); // Grey for inactive
            }
            
            entryContainer.AddChild(headerLabel);

            // Show active bonus if present
            if (setInfo.ActiveBonus != null)
            {
                var bonusLabel = new Label();
                bonusLabel.Text = $"▸ {setInfo.ActiveBonus.BonusName}";
                bonusLabel.AddThemeFontSizeOverride("font_size", 14);
                bonusLabel.Modulate = new Color(1.0f, 0.9f, 0.5f); // Gold
                entryContainer.AddChild(bonusLabel);

                var descLabel = new Label();
                descLabel.Text = $"  {setInfo.ActiveBonus.Description}";
                descLabel.AddThemeFontSizeOverride("font_size", 12);
                descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
                descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
                entryContainer.AddChild(descLabel);

                // Show stat bonuses
                if (setInfo.ActiveBonus.StatBonuses != null && setInfo.ActiveBonus.StatBonuses.Count > 0)
                {
                    var statsLabel = new Label();
                    statsLabel.Text = "  " + GetStatBonusesText(setInfo.ActiveBonus.StatBonuses);
                    statsLabel.AddThemeFontSizeOverride("font_size", 11);
                    statsLabel.Modulate = new Color(0.5f, 0.8f, 1.0f); // Light blue
                    statsLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
                    entryContainer.AddChild(statsLabel);
                }
            }

            // Add progress bar
            var progressBar = new ProgressBar();
            progressBar.MinValue = 0;
            progressBar.MaxValue = setInfo.TotalPieces;
            progressBar.Value = setInfo.EquippedPieces;
            progressBar.ShowPercentage = false;
            progressBar.CustomMinimumSize = new Vector2(0, 8);
            entryContainer.AddChild(progressBar);

            // Add separator
            var separator = new HSeparator();
            separator.Modulate = new Color(0.4f, 0.4f, 0.4f);
            entryContainer.AddChild(separator);

            SetListContainer.AddChild(entryContainer);
            _setBonusEntries.Add(entryContainer);
        }

        private string GetStatBonusesText(Dictionary<StatType, float> statBonuses)
        {
            var stats = new List<string>();
            
            foreach (var kvp in statBonuses)
            {
                string statName = kvp.Key.ToString();
                float value = kvp.Value;
                
                // Format based on stat type (percentage vs absolute)
                if (IsPercentageStat(kvp.Key))
                {
                    stats.Add($"+{value * 100:F1}% {statName}");
                }
                else
                {
                    stats.Add($"+{value:F0} {statName}");
                }
            }

            return string.Join(", ", stats);
        }

        private bool IsPercentageStat(StatType stat)
        {
            return stat switch
            {
                StatType.CritChance => true,
                StatType.Dodge => true,
                StatType.PhysicalResist => true,
                StatType.FireResist => true,
                StatType.IceResist => true,
                StatType.ElectricResist => true,
                StatType.ToxicResist => true,
                StatType.Accuracy => true,
                StatType.EnergyEfficiency => true,
                _ => false
            };
        }

        #endregion
    }
}
