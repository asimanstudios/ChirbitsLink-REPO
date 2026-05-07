using UnityEngine;
using UnityEngine.InputSystem;
using ChibitsLink.GameSide;
using ChibiCocina.Models;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour, PlayerManager.IChibitsController, IPushable
{
    [Header("Movement Configuration")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float acceleration = 12f;
    public float friction = 10f;
    public float airControl = 0.6f;
    public float rotationSpeed = 15f;
    public float lerpSpeed = 25f;

    [Header("Jump Configuration")]
    public float jumpForce = 3f;
    public float gravity = -15f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.2f;

    [Header("External Forces")]
    public float forceDecayFactor = 5f;

    // Components
    private CharacterController _controller;
    
    // Models
    private MovementModel _movementModel;
    private JumpModel _jumpModel;
    
    // Controllers
    private PlayerAudioController _audioController;
    private PlayerAnimationController _animationController;
    private PlayerCombatController _combatController;
    
    // State
    private float _lastMobileInputTime;
    private const float MOBILE_INPUT_TIMEOUT = 0.5f;

    private void Awake()
    {
        Debug.Log($"[PlayerMovementController] Initializing on {gameObject.name}");
        InitializeComponents();
        InitializeModels();
        InitializeControllers();
        ApplySceneSpecificStats();
    }
    
    private void InitializeComponents()
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null)
        {
            throw new System.InvalidOperationException("CharacterController component required");
        }
    }
    
    private void InitializeModels()
    {
        _movementModel = new MovementModel();
        _jumpModel = new JumpModel();
        UpdateModelsFromInspector();
    }
    
    private void InitializeControllers()
    {
        _audioController = GetComponent<PlayerAudioController>();
        if (_audioController == null)
        {
            _audioController = gameObject.AddComponent<PlayerAudioController>();
        }
        
        _animationController = GetComponent<PlayerAnimationController>();
        if (_animationController == null)
        {
            _animationController = gameObject.AddComponent<PlayerAnimationController>();
        }
        
        _combatController = GetComponent<PlayerCombatController>();
        if (_combatController == null)
        {
            _combatController = gameObject.AddComponent<PlayerCombatController>();
        }
        
        // Initialize models in controllers
        _animationController.InitializeModels(_movementModel, _jumpModel);
        _combatController.InitializeModels(_movementModel, _jumpModel);
    }
    
    private void ApplySceneSpecificStats()
    {
        _combatController.ApplySceneSpecificStats();
    }
    private void UpdateModelsFromInspector()
    {
        _movementModel.WalkSpeed = walkSpeed;
        _movementModel.RunSpeed = runSpeed;
        _movementModel.Acceleration = acceleration;
        _movementModel.AirControl = airControl;
        _movementModel.RotationSpeed = rotationSpeed;
        _movementModel.LerpSpeed = lerpSpeed;
        _movementModel.Friction = friction;
        
        _jumpModel.JumpForce = jumpForce;
        _jumpModel.Gravity = gravity;
        _jumpModel.FallMultiplier = fallMultiplier;
        _jumpModel.LowJumpMultiplier = lowJumpMultiplier;
        _jumpModel.CoyoteTime = coyoteTime;
        _jumpModel.JumpBufferTime = jumpBufferTime;
    }
    
    public void OnJump(InputValue value)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isCombatScene = sceneName.Contains("Push") || sceneName.Contains("Smash");
        
        if (value.isPressed)
        {
            _jumpModel.JumpBufferCounter = _jumpModel.JumpBufferTime;
            bool canAirJump = isCombatScene && !_jumpModel.IsGrounded && _jumpModel.AirJumpsRemaining > 0;
            if (canAirJump)
            {
                _jumpModel.AirJumpsRemaining--;
                ApplyDoubleJump(_jumpModel.JumpForce * 0.85f);
            }
        }
        else
        {
            Vector3 currentVelocity = _jumpModel.VerticalVelocity;
            if (currentVelocity.y > 0)
            {
                currentVelocity.y *= 0.5f;
                _jumpModel.VerticalVelocity = currentVelocity;
            }
        }
    }

    public void SetStats(float walk, float run, float acc, float air, float jump)
    {
        walkSpeed = walk;
        runSpeed = run;
        acceleration = acc;
        airControl = air;
        jumpForce = jump;
        UpdateModelsFromInspector();
    }

    public void EnableController(bool active)
    {
        if (_controller != null) 
        {
            _controller.enabled = active;
        }
        
        if (!active)
        {
            _movementModel.ResetVelocity();
            _jumpModel.ResetJumpState();
        }
    }

    public void CancelJumpBuffer() 
    { 
        _jumpModel.JumpBufferCounter = 0; 
    }
    
    public bool IsGrounded => _jumpModel.IsGrounded;

    private void Update()
    {
        bool canProcessUpdate = _controller != null && _controller.enabled;
        if (!canProcessUpdate) return;
        
        SmoothMovementInput();
        CheckCombatGround();
        ApplyExternalForces();
        ApplyMovement();
        ApplyGravity();
        UpdateAnimator();
        UpdateStepAudio();
    }

    private void SmoothMovementInput()
    {
        _movementModel.MoveInput = Vector2.Lerp(
            _movementModel.MoveInput, 
            _movementModel.TargetMoveInput, 
            Time.deltaTime * _movementModel.LerpSpeed
        );
    }

    private void ApplyExternalForces()
    {
        bool hasExternalForces = _movementModel.ExternalForce.magnitude > 0.01f;
        if (hasExternalForces)
        {
            _movementModel.ExternalForce = Vector3.Lerp(
                _movementModel.ExternalForce, 
                Vector3.zero, 
                Time.deltaTime * forceDecayFactor
            );
        }
        else
        {
            _movementModel.ExternalForce = Vector3.zero;
        }
    }

    public void ApplyPush(Vector3 force, float duration)
    {
        _movementModel.ExternalForce += force;
        Debug.Log($"[PlayerMovementController] {gameObject.name} pushed with force: {force}");
    }

    public void ApplyDoubleJump(float force)
    {
        Vector3 newVelocity = _jumpModel.VerticalVelocity;
        newVelocity.y = Mathf.Sqrt(force * -2f * _jumpModel.Gravity);
        _jumpModel.VerticalVelocity = newVelocity;
        
        _animationController.TriggerJump();
        _audioController.PlayJumpSound();
        Debug.Log($"[PlayerMovementController] {gameObject.name} – double jump!");
    }

    private void ProcessJumpInput()
    {
        if (_jumpModel.IsGrounded)
        {
            Vector3 newVelocity = _jumpModel.VerticalVelocity;
            newVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            _jumpModel.VerticalVelocity = newVelocity;
            
            _animationController.TriggerJump();
            _audioController.PlayJumpSound();
        }
    }
    
    private void ProcessJumpRelease()
    {
        // Variable jump height disabled - simplified
    }

    private void UpdateAnimator()
    {
        _animationController.UpdateAnimator();
    }
}