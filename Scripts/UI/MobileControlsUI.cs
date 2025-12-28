using Godot;
using System;

/// <summary>
/// Handles mobile UI controls including virtual joystick, touch camera, and action buttons.
/// Automatically shows/hides based on platform detection.
/// </summary>
public partial class MobileControlsUI : CanvasLayer
{
    #region Node References
    
    private Control _leftSide;
    private Panel _virtualJoystick;
    private Panel _joystickThumb;
    private Control _rightSide;
    private Panel _touchArea;
    
    private Button _fireButton;
    private Button _shieldButton;
    private Button _abilityButton;
    private Button _weaponSwitchButton;
    private Button _sprintButton;
    
    private ProgressBar _healthBar;
    private ProgressBar _energyBar;
    
    #endregion
    
    #region Private Fields
    
    private bool _isMobilePlatform = false;
    private PlayerMechController _playerMech;
    private bool _isSprinting = false;
    
    // Joystick state
    private bool _joystickActive = false;
    private Vector2 _joystickCenter = Vector2.Zero;
    private int _joystickTouchIndex = -1;
    private const float JoystickRadius = 100f;
    private const float JoystickDeadZone = 0.1f;
    private const float JoystickPanelSize = 250f;
    private const float JoystickPanelHalfSize = 125f;
    private const float JoystickThumbSize = 80f;
    private const float JoystickThumbHalfSize = 40f;
    
    // Camera touch state
    private bool _cameraTouchActive = false;
    private int _cameraTouchIndex = -1;
    private Vector2 _lastCameraTouchPosition = Vector2.Zero;
    private float _touchCameraSensitivity = 0.5f;
    
    #endregion
    
    #region Godot Lifecycle Methods
    
    public override void _Ready()
    {
        // Get node references
        _leftSide = GetNode<Control>("LeftSide");
        _virtualJoystick = GetNode<Panel>("LeftSide/VirtualJoystick");
        _joystickThumb = GetNode<Panel>("LeftSide/VirtualJoystick/JoystickThumb");
        _rightSide = GetNode<Control>("RightSide");
        _touchArea = GetNode<Panel>("RightSide/TouchArea");
        
        _fireButton = GetNode<Button>("ActionButtons/FireButton");
        _shieldButton = GetNode<Button>("ActionButtons/ShieldButton");
        _abilityButton = GetNode<Button>("ActionButtons/AbilityButton");
        _weaponSwitchButton = GetNode<Button>("ActionButtons/WeaponSwitchButton");
        _sprintButton = GetNode<Button>("LeftSide/SprintButton");
        
        _healthBar = GetNode<ProgressBar>("HUD/HealthBar");
        _energyBar = GetNode<ProgressBar>("HUD/EnergyBar");
        
        // Connect button signals
        _fireButton.Pressed += OnFirePressed;
        _shieldButton.Pressed += OnShieldPressed;
        _abilityButton.Pressed += OnAbilityPressed;
        _weaponSwitchButton.Pressed += OnWeaponSwitchPressed;
        _sprintButton.Pressed += OnSprintPressed;
        
        // Platform detection
        string osName = OS.GetName();
        _isMobilePlatform = osName == "Android" || osName == "iOS";
        
        // Show/hide mobile controls based on platform
        if (_isMobilePlatform)
        {
            _leftSide.Visible = true;
            _rightSide.Visible = true;
            GetNode<Control>("ActionButtons").Visible = true;
            GD.Print("Mobile controls enabled");
        }
        else
        {
            _leftSide.Visible = false;
            _rightSide.Visible = false;
            GetNode<Control>("ActionButtons").Visible = false;
            GD.Print("PC controls enabled (mobile UI hidden)");
        }
        
        // Find player mech
        CallDeferred(nameof(FindPlayerMech));
    }
    
    private void FindPlayerMech()
    {
        // Wait for scene tree to be ready
        var root = GetTree().Root;
        _playerMech = root.GetNodeOrNull<PlayerMechController>("Main/PlayerMech");
        
        if (_playerMech == null)
        {
            GD.PrintErr("PlayerMech not found in scene tree!");
        }
        else
        {
            GD.Print("PlayerMech connected to mobile controls");
        }
    }
    
    public override void _Process(double delta)
    {
        // Update HUD
        if (_playerMech != null)
        {
            _healthBar.Value = _playerMech.HealthPercent * 100f;
            _energyBar.Value = _playerMech.EnergyPercent * 100f;
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (!_isMobilePlatform)
            return;
        
        if (@event is InputEventScreenTouch touchEvent)
        {
            HandleTouchEvent(touchEvent);
        }
        else if (@event is InputEventScreenDrag dragEvent)
        {
            HandleDragEvent(dragEvent);
        }
    }
    
    #endregion
    
    #region Touch Input Handling
    
    private void HandleTouchEvent(InputEventScreenTouch touchEvent)
    {
        Vector2 touchPos = touchEvent.Position;
        
        // Check if touch is on left or right side
        bool isLeftSide = touchPos.X < GetViewport().GetVisibleRect().Size.X / 2;
        
        if (touchEvent.Pressed)
        {
            if (isLeftSide)
            {
                // Start joystick
                StartJoystick(touchPos, touchEvent.Index);
            }
            else
            {
                // Start camera touch
                StartCameraTouch(touchPos, touchEvent.Index);
            }
        }
        else
        {
            // Release
            if (touchEvent.Index == _joystickTouchIndex)
            {
                StopJoystick();
            }
            else if (touchEvent.Index == _cameraTouchIndex)
            {
                StopCameraTouch();
            }
        }
    }
    
    private void HandleDragEvent(InputEventScreenDrag dragEvent)
    {
        if (dragEvent.Index == _joystickTouchIndex)
        {
            UpdateJoystick(dragEvent.Position);
        }
        else if (dragEvent.Index == _cameraTouchIndex)
        {
            UpdateCameraTouch(dragEvent.Position);
        }
    }
    
    #endregion
    
    #region Virtual Joystick Logic
    
    private void StartJoystick(Vector2 touchPos, int touchIndex)
    {
        _joystickActive = true;
        _joystickTouchIndex = touchIndex;
        _joystickCenter = touchPos;
        
        // Move joystick to touch position
        _virtualJoystick.Position = touchPos - new Vector2(JoystickPanelHalfSize, JoystickPanelHalfSize);
        
        GD.Print($"Joystick started at {touchPos}");
    }
    
    private void UpdateJoystick(Vector2 touchPos)
    {
        if (!_joystickActive)
            return;
        
        Vector2 delta = touchPos - _joystickCenter;
        float distance = delta.Length();
        
        // Clamp to joystick radius
        if (distance > JoystickRadius)
        {
            delta = delta.Normalized() * JoystickRadius;
        }
        
        // Update thumb position
        _joystickThumb.Position = new Vector2(JoystickPanelHalfSize, JoystickPanelHalfSize) + delta - new Vector2(JoystickThumbHalfSize, JoystickThumbHalfSize);
        
        // Calculate input vector
        Vector2 inputVector = delta / JoystickRadius;
        
        // Apply dead zone
        if (inputVector.Length() < JoystickDeadZone)
        {
            inputVector = Vector2.Zero;
        }
        
        // Send to player controller
        if (_playerMech != null)
        {
            _playerMech.SetMobileMovementInput(inputVector);
        }
    }
    
    private void StopJoystick()
    {
        _joystickActive = false;
        _joystickTouchIndex = -1;
        
        // Reset thumb position
        _joystickThumb.Position = new Vector2(JoystickPanelHalfSize - JoystickThumbHalfSize, JoystickPanelHalfSize - JoystickThumbHalfSize);
        
        // Stop movement
        if (_playerMech != null)
        {
            _playerMech.SetMobileMovementInput(Vector2.Zero);
        }
        
        GD.Print("Joystick stopped");
    }
    
    #endregion
    
    #region Touch Camera Logic
    
    private void StartCameraTouch(Vector2 touchPos, int touchIndex)
    {
        _cameraTouchActive = true;
        _cameraTouchIndex = touchIndex;
        _lastCameraTouchPosition = touchPos;
        
        GD.Print($"Camera touch started at {touchPos}");
    }
    
    private void UpdateCameraTouch(Vector2 touchPos)
    {
        if (!_cameraTouchActive)
            return;
        
        Vector2 delta = touchPos - _lastCameraTouchPosition;
        _lastCameraTouchPosition = touchPos;
        
        // Apply sensitivity and send to player controller
        if (_playerMech != null)
        {
            _playerMech.SetMobileCameraDelta(delta * _touchCameraSensitivity);
        }
    }
    
    private void StopCameraTouch()
    {
        _cameraTouchActive = false;
        _cameraTouchIndex = -1;
        
        GD.Print("Camera touch stopped");
    }
    
    #endregion
    
    #region Action Button Handlers
    
    private void OnFirePressed()
    {
        GD.Print("Fire button pressed!");
        // TODO: Implement fire action
    }
    
    private void OnShieldPressed()
    {
        GD.Print("Shield button pressed!");
        // TODO: Implement shield action
    }
    
    private void OnAbilityPressed()
    {
        GD.Print("Ability button pressed!");
        // TODO: Implement ability action
    }
    
    private void OnWeaponSwitchPressed()
    {
        GD.Print("Weapon switch button pressed!");
        // TODO: Implement weapon switch action
    }
    
    private void OnSprintPressed()
    {
        _isSprinting = !_isSprinting;
        
        if (_playerMech != null)
        {
            _playerMech.SetMobileSprint(_isSprinting);
        }
        
        // Update button appearance
        if (_isSprinting)
        {
            _sprintButton.Modulate = new Color(0.2f, 1f, 0.2f, 1f); // Bright green when active
            GD.Print("Sprint activated!");
        }
        else
        {
            _sprintButton.Modulate = new Color(0.5f, 0.8f, 0.5f, 0.6f); // Dimmed when inactive
            GD.Print("Sprint deactivated!");
        }
    }
    
    #endregion
}
