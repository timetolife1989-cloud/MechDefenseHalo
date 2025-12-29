using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.DLC
{
    /// <summary>
    /// Gates DLC content and shows teaser on interaction
    /// </summary>
    public partial class DLCContentGate : Node3D
    {
        #region Exported Fields
        
        [Export] private string requiredDLC;
        [Export] private Node3D lockedVisual;
        [Export] private Node3D unlockedContent;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            UpdateGateState();
            
            EventBus.On(EventBus.DLCUnlocked, OnDLCUnlocked);
        }
        
        public override void _ExitTree()
        {
            EventBus.Off(EventBus.DLCUnlocked, OnDLCUnlocked);
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateGateState()
        {
            bool isUnlocked = DLCManager.Instance?.IsDLCUnlocked(requiredDLC) ?? false;
            
            if (lockedVisual != null)
                lockedVisual.Visible = !isUnlocked;
            
            if (unlockedContent != null)
                unlockedContent.Visible = isUnlocked;
        }
        
        private void OnDLCUnlocked(object data)
        {
            string dlcId = data as string;
            if (dlcId == requiredDLC)
            {
                UpdateGateState();
                PlayUnlockAnimation();
            }
        }
        
        private void PlayUnlockAnimation()
        {
            // Portal opening animation
            GD.Print($"Opening DLC gate: {requiredDLC}");
        }
        
        private void ShowDLCTeaser()
        {
            var teaserSystem = GetNode<DLCTeaserSystem>("/root/DLCTeaserSystem");
            if (teaserSystem != null)
            {
                teaserSystem.ShowTeaser(requiredDLC);
            }
            else
            {
                GD.PrintErr("DLCTeaserSystem not found in scene tree");
            }
        }
        
        #endregion
        
        #region Input Handling
        
        public override void _InputEvent(Camera3D camera, InputEvent @event, Vector3 position, Vector3 normal, int shapeIdx)
        {
            if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
            {
                if (DLCManager.Instance != null && !DLCManager.Instance.IsDLCUnlocked(requiredDLC))
                {
                    ShowDLCTeaser();
                }
            }
        }
        
        #endregion
    }
}
