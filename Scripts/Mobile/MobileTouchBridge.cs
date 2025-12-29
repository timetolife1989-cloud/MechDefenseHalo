using Godot;
using System;
using MechDefenseHalo.Mobile;
using MechDefenseHalo.Player;

/// <summary>
/// Bridges TouchController with PlayerMechController and WeaponManager
/// Add this script to your main scene or player node
/// </summary>
public partial class MobileTouchBridge : Node
{
    [Export] public NodePath PlayerControllerPath;
    [Export] public NodePath WeaponManagerPath;
    [Export] public string TouchControlsNodeName = "TouchControls";
    
    private TouchController touchController;
    private PlayerMechController playerController;
    private WeaponManager weaponManager;
    private bool isMobilePlatform;
    private bool lastFireState = false;
    
    public override void _Ready()
    {
        // Detect platform
        string osName = OS.GetName();
        isMobilePlatform = osName == "Android" || osName == "iOS";
        
        if (!isMobilePlatform)
        {
            GD.Print("MobileTouchBridge: Not on mobile platform, disabling");
            SetProcess(false);
            return;
        }
        
        // Get player controller
        if (PlayerControllerPath != null && !PlayerControllerPath.IsEmpty)
        {
            playerController = GetNode<PlayerMechController>(PlayerControllerPath);
        }
        
        // Get weapon manager
        if (WeaponManagerPath != null && !WeaponManagerPath.IsEmpty)
        {
            weaponManager = GetNode<WeaponManager>(WeaponManagerPath);
        }
        
        // Find or create touch controller
        CallDeferred(nameof(SetupTouchControls));
        
        GD.Print("MobileTouchBridge initialized on mobile platform");
    }
    
    private void SetupTouchControls()
    {
        // Look for existing touch controller by configured name
        touchController = GetTree().Root.GetNodeOrNull<TouchController>(TouchControlsNodeName);
        
        // If not found, create it
        if (touchController == null)
        {
            var touchScene = ResourceLoader.Load<PackedScene>("res://Scenes/Mobile/TouchControls.tscn");
            if (touchScene != null)
            {
                touchController = touchScene.Instantiate<TouchController>();
                GetTree().Root.AddChild(touchController);
                GD.Print("Touch controls loaded and added to scene");
            }
            else
            {
                GD.PrintErr("Failed to load TouchControls.tscn");
            }
        }
    }
    
    public override void _Process(double delta)
    {
        if (touchController == null)
            return;
            
        // Update player movement
        if (playerController != null)
        {
            playerController.SetMobileMovementInput(touchController.MovementInput);
        }
        
        // Handle weapon firing
        // Note: WeaponManager.FireCurrentWeapon() already respects weapon fire rate
        // and handles automatic vs semi-automatic behavior internally
        if (weaponManager != null)
        {
            bool currentFireState = touchController.IsFirePressed;
            
            // Fire on initial press (for semi-auto weapons)
            if (currentFireState && !lastFireState)
            {
                weaponManager.FireCurrentWeapon();
            }
            // Continue calling FireCurrentWeapon for full-auto weapons
            // The WeaponManager/WeaponBase will handle fire rate limiting
            else if (currentFireState && weaponManager.CurrentWeapon != null && weaponManager.CurrentWeapon.IsAutomatic)
            {
                weaponManager.FireCurrentWeapon();
            }
            
            lastFireState = currentFireState;
        }
    }
}
