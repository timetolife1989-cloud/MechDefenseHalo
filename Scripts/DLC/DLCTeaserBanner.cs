using Godot;
using System;

namespace MechDefenseHalo.DLC
{
    /// <summary>
    /// Displays DLC teaser banner in main menu
    /// </summary>
    public partial class DLCTeaserBanner : Control
    {
        #region Exported Fields
        
        [Export] private TextureRect bannerImage;
        [Export] private Label titleLabel;
        [Export] private Button viewButton;
        
        #endregion
        
        #region Private Fields
        
        private DLCData featuredDLC;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            ShowNextDLC();
            
            if (viewButton != null)
            {
                viewButton.Pressed += OnViewPressed;
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void ShowNextDLC()
        {
            if (DLCManager.Instance == null)
            {
                Visible = false;
                return;
            }
            
            var upcomingDLCs = DLCManager.Instance.GetUpcomingDLCs();
            
            if (upcomingDLCs.Count > 0)
            {
                featuredDLC = upcomingDLCs[0];
                
                if (titleLabel != null)
                    titleLabel.Text = $"Coming Soon: {featuredDLC.Name}";
                
                Visible = true;
            }
            else
            {
                Visible = false;
            }
        }
        
        private void OnViewPressed()
        {
            if (featuredDLC == null)
                return;
            
            var teaserSystem = GetNode<DLCTeaserSystem>("/root/DLCTeaserSystem");
            if (teaserSystem != null)
            {
                teaserSystem.ShowTeaser(featuredDLC.Id);
            }
            else
            {
                GD.PrintErr("DLCTeaserSystem not found in scene tree");
            }
        }
        
        #endregion
    }
}
