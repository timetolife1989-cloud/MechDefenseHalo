using Godot;
using System;

/// <summary>
/// First-person mech controller with support for both PC and mobile controls.
/// Handles movement, camera rotation, and player state management.
/// </summary>
public partial class PlayerMechController : CharacterBody3D
{
    #region Exported Properties
    
    [Export] public float WalkSpeed = 5.0f;
    [Export] public float SprintSpeed = 8.0f;
    [Export] public float MouseSensitivity = 0.002f;
    [Export] public float MaxHealth = 100f;
    [Export] public float MaxEnergy = 100f;
    
    #endregion
    
    #region Private Fields
    
    private Node3D _cameraMount;
    private Camera3D _camera;
    private float _gravity;
    private float _cameraPitch = 0f;
    private const float MaxPitchAngle = 1.0472f; // ~60 degrees in radians
    
    private float _currentHealth;
    private float _currentEnergy;
    
    // Mobile controls input
    private Vector2 _mobileMovementInput = Vector2.Zero;
    private Vector2 _mobileCameraDelta = Vector2.Zero;
    private bool _isMobilePlatform = false;
    private bool _mobileSprint = false;
    
    #endregion
    
    #region Godot Lifecycle Methods
    
    public override void _Ready()
    {
        // Get camera nodes
        _cameraMount = GetNode<Node3D>("CameraMount");
        _camera = GetNode<Camera3D>("CameraMount/Camera3D");
        
        // Initialize stats
        _currentHealth = MaxHealth;
        _currentEnergy = MaxEnergy;
        
        // Get gravity from project settings
        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
        
        // Detect platform
        string osName = OS.GetName();
        _isMobilePlatform = osName == "Android" || osName == "iOS";
        
        // Capture mouse on PC
        if (!_isMobilePlatform)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            GD.Print("PlayerMechController ready - Mouse captured");
        }
        else
        {
            GD.Print($"PlayerMechController initialized on {osName}");
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        // ESC = Release mouse
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
        
        // Mouse motion
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            // Horizontal rotation (body)
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            
            // Vertical rotation (camera mount)
            var cameraMount = GetNode<Node3D>("CameraMount");
            cameraMount.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
            
            // Clamp vertical angle
            var rotation = cameraMount.Rotation;
            rotation.X = Mathf.Clamp(rotation.X, Mathf.DegToRad(-60), Mathf.DegToRad(60));
            cameraMount.Rotation = rotation;
        }
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        // Click to recapture mouse
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            if (Input.MouseMode == Input.MouseModeEnum.Visible)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        // Apply gravity
        Vector3 velocity = Velocity;
        if (!IsOnFloor())
        {
            velocity.Y -= _gravity * (float)delta;
        }
        
        // Get movement input
        Vector2 inputDir = GetMovementInput();
        
        // Calculate movement direction relative to camera
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        
        // Apply movement speed - handle sprint for both PC and mobile
        bool isSprinting = _isMobilePlatform ? _mobileSprint : Input.IsActionPressed("sprint");
        float currentSpeed = isSprinting ? SprintSpeed : WalkSpeed;
        
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * currentSpeed;
            velocity.Z = direction.Z * currentSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, currentSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, currentSpeed);
        }
        
        Velocity = velocity;
        MoveAndSlide();
        
        // Handle mobile camera rotation
        if (_isMobilePlatform && _mobileCameraDelta != Vector2.Zero)
        {
            HandleCameraRotation(_mobileCameraDelta);
            _mobileCameraDelta = Vector2.Zero;
        }
    }
    
    #endregion
    
    #region Input Handling
    
    /// <summary>
    /// Gets movement input from either keyboard (PC) or virtual joystick (mobile)
    /// </summary>
    private Vector2 GetMovementInput()
    {
        if (_isMobilePlatform)
        {
            return _mobileMovementInput;
        }
        else
        {
            // PC keyboard input
            Vector2 inputDir = Vector2.Zero;
            
            if (Input.IsActionPressed("move_forward"))
                inputDir.Y -= 1;
            if (Input.IsActionPressed("move_backward"))
                inputDir.Y += 1;
            if (Input.IsActionPressed("move_left"))
                inputDir.X -= 1;
            if (Input.IsActionPressed("move_right"))
                inputDir.X += 1;
            
            return inputDir.Normalized();
        }
    }
    
    /// <summary>
    /// Handles camera rotation from mouse or touch input
    /// </summary>
    private void HandleCameraRotation(Vector2 mouseDelta)
    {
        // Horizontal rotation (yaw) - rotate entire character
        RotateY(-mouseDelta.X * MouseSensitivity);
        
        // Vertical rotation (pitch) - rotate camera mount only
        _cameraPitch -= mouseDelta.Y * MouseSensitivity;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -MaxPitchAngle, MaxPitchAngle);
        
        if (_cameraMount != null)
        {
            _cameraMount.Rotation = new Vector3(_cameraPitch, 0, 0);
        }
    }
    
    #endregion
    
    #region Mobile Input API
    
    /// <summary>
    /// Called by MobileControlsUI to set movement input from virtual joystick
    /// </summary>
    public void SetMobileMovementInput(Vector2 input)
    {
        _mobileMovementInput = input;
    }
    
    /// <summary>
    /// Called by MobileControlsUI to set camera rotation delta from touch
    /// </summary>
    public void SetMobileCameraDelta(Vector2 delta)
    {
        _mobileCameraDelta = delta;
    }
    
    /// <summary>
    /// Called by MobileControlsUI to toggle sprint on mobile
    /// </summary>
    public void SetMobileSprint(bool sprint)
    {
        _mobileSprint = sprint;
    }
    
    #endregion
    
    #region Health and Energy Management
    
    public float CurrentHealth => _currentHealth;
    public float CurrentEnergy => _currentEnergy;
    public float HealthPercent => _currentHealth / MaxHealth;
    public float EnergyPercent => _currentEnergy / MaxEnergy;
    
    public void TakeDamage(float amount)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        GD.Print($"Mech took {amount} damage. Health: {_currentHealth}/{MaxHealth}");
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void ConsumeEnergy(float amount)
    {
        _currentEnergy = Mathf.Max(0, _currentEnergy - amount);
    }
    
    public void RegenerateEnergy(float amount)
    {
        _currentEnergy = Mathf.Min(MaxEnergy, _currentEnergy + amount);
    }
    
    private void Die()
    {
        GD.Print("Mech destroyed!");
        // TODO: Implement death sequence
    }
    
    #endregion
}
