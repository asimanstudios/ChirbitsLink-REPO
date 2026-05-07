using UnityEngine;
using UnityEngine.InputSystem;
using ChibitsLink.GameSide;
using ChibiCocina.Models;

/// <summary>
/// Controlador principal de movimiento del jugador.
/// Gestiona movimiento físico, salto, fuerzas externas y coordinación con otros controladores.
/// Implementa interfaces para input y empuje.
/// </summary>
/// <remarks>
/// Requiere CharacterController para funcionamiento.
/// Coordina con controladores de audio, animación y combate.
/// Soporta saltos dobles en escenas de combate.
/// </remarks>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour, PlayerManager.IChibitsController, IPushable
{
    [Header("Configuración de Movimiento")]
    /// <summary>Velocidad de caminata del jugador</summary>
    public float walkSpeed = 4f;
    /// <summary>Velocidad de carrera del jugador</summary>
    public float runSpeed = 7f;
    /// <summary>Tasa de aceleración del movimiento</summary>
    public float acceleration = 12f;
    /// <summary>Fricción aplicada cuando no hay input</summary>
    public float friction = 10f;
    /// <summary>Control de movimiento en el aire (0-1)</summary>
    public float airControl = 0.6f;
    /// <summary>Velocidad de rotación del jugador</summary>
    public float rotationSpeed = 15f;
    /// <summary>Velocidad de interpolación para suavizado</summary>
    public float lerpSpeed = 25f;

    [Header("Configuración de Salto")]
    /// <summary>Fuerza aplicada al saltar</summary>
    public float jumpForce = 3f;
    /// <summary>Fuerza de gravedad aplicada</summary>
    public float gravity = -15f;
    /// <summary>Multiplicador de caída para realismo</summary>
    public float fallMultiplier = 2.5f;
    /// <summary>Multiplicador para saltos bajos</summary>
    public float lowJumpMultiplier = 2f;
    /// <summary>Tiempo permitido para saltar después de dejar el suelo</summary>
    public float coyoteTime = 0.15f;
    /// <summary>Tiempo de anticipación para saltos</summary>
    public float jumpBufferTime = 0.2f;

    [Header("Fuerzas Externas")]
    /// <summary>Factor de decaimiento de fuerzas externas</summary>
    public float forceDecayFactor = 5f;
    
    // Componentes
    /// <summary>Componente CharacterController para física</summary>
    private CharacterController _controller;
    
    // Modelos
    /// <summary>Modelo de datos de movimiento</summary>
    private MovementModel _movementModel;
    /// <summary>Modelo de datos de salto</summary>
    private JumpModel _jumpModel;
    
    // Controladores
    /// <summary>Controlador de audio del jugador</summary>
    private PlayerAudioController _audioController;
    /// <summary>Controlador de animaciones del jugador</summary>
    private PlayerAnimationController _animationController;
    /// <summary>Controlador de estadísticas de combate</summary>
    private PlayerCombatController _combatController;
    
    // Estado
    /// <summary>Tiempo del último input móvil recibido</summary>
    private float _lastMobileInputTime;
    /// <summary>Timeout para input móvil</summary>
    private const float MOBILE_INPUT_TIMEOUT = 0.5f;

    /// <summary>
    /// Inicializa el controlador de movimiento.
    /// Configura componentes, modelos y controladores relacionados.
    /// </summary>
    private void Awake()
    {
        Debug.Log($"[PlayerMovementController] Initializing on {gameObject.name}");
        InitializeComponents();
        InitializeModels();
        InitializeControllers();
        ApplySceneSpecificStats();
    }
    
    /// <summary>
    /// Inicializa los componentes requeridos.
    /// Obtiene y valida el CharacterController.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Si no hay CharacterController</exception>
    private void InitializeComponents()
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null)
        {
            throw new System.InvalidOperationException("CharacterController component required");
        }
    }
    
    /// <summary>
    /// Inicializa los modelos de datos.
    /// Crea instancias de MovementModel y JumpModel.
    /// </summary>
    private void InitializeModels()
    {
        _movementModel = new MovementModel();
        _jumpModel = new JumpModel();
        UpdateModelsFromInspector();
    }
    
    /// <summary>
    /// Inicializa los controladores relacionados.
    /// Añade componentes de audio, animación y combate si no existen.
    /// </summary>
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
    
    /// <summary>
    /// Aplica estadísticas específicas de la escena.
    /// Utiliza el controlador de combate para ajustar parámetros.
    /// </summary>
    private void ApplySceneSpecificStats()
    {
        _combatController.ApplySceneSpecificStats();
    }
    /// <summary>
    /// Actualiza los modelos desde los valores del inspector.
    /// Sincroniza parámetros de configuración con los modelos.
    /// </summary>
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
    
    /// <summary>
    /// Maneja el input de salto del jugador.
    /// Procesa tanto saltos normales como dobles en escenas de combate.
    /// </summary>
    /// <param name="value">Valor del input de salto</param>
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

    /// <summary>
    /// Establece estadísticas personalizadas de movimiento.
    /// Permite configuración dinámica de parámetros.
    /// </summary>
    /// <param name="walk">Velocidad de caminata</param>
    /// <param name="run">Velocidad de carrera</param>
    /// <param name="acc">Aceleración</param>
    /// <param name="air">Control aéreo</param>
    /// <param name="jump">Fuerza de salto</param>
    public void SetStats(float walk, float run, float acc, float air, float jump)
    {
        walkSpeed = walk;
        runSpeed = run;
        acceleration = acc;
        airControl = air;
        jumpForce = jump;
        UpdateModelsFromInspector();
    }

    /// <summary>
    /// Habilita o deshabilita el controlador.
    /// Resetea estado al deshabilitar.
    /// </summary>
    /// <param name="active">Estado del controlador</param>
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

    /// <summary>
    /// Cancela el buffer de salto.
    /// Utilizado para evitar saltos no deseados.
    /// </summary>
    public void CancelJumpBuffer() 
    { 
        _jumpModel.JumpBufferCounter = 0; 
    }
    
    /// <summary>
    /// Indica si el jugador está en el suelo.
    /// Proporciona acceso al estado del modelo de salto.
    /// </summary>
    public bool IsGrounded => _jumpModel.IsGrounded;

    /// <summary>
    /// Actualización principal del controlador.
    /// Procesa input, física y actualizaciones de componentes.
    /// </summary>
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

    /// <summary>
    /// Suaviza el input de movimiento.
    /// Aplica interpolación para movimiento fluido.
    /// </summary>
    private void SmoothMovementInput()
    {
        _movementModel.MoveInput = Vector2.Lerp(
            _movementModel.MoveInput, 
            _movementModel.TargetMoveInput, 
            Time.deltaTime * _movementModel.LerpSpeed
        );
    }

    /// <summary>
    /// Aplica fuerzas externas al movimiento.
    /// Decrementa gradualmente las fuerzas aplicadas.
    /// </summary>
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

    /// <summary>
    /// Aplica una fuerza de empuje al jugador.
    /// Implementación de la interfaz IPushable.
    /// </summary>
    /// <param name="force">Fuerza a aplicar</param>
    /// <param name="duration">Duración de la fuerza</param>
    public void ApplyPush(Vector3 force, float duration)
    {
        _movementModel.ExternalForce += force;
        Debug.Log($"[PlayerMovementController] {gameObject.name} pushed with force: {force}");
    }

    /// <summary>
    /// Aplica un salto doble.
    /// Utilizado en escenas de combate para saltos adicionales.
    /// </summary>
    /// <param name="force">Fuerza del salto doble</param>
    public void ApplyDoubleJump(float force)
    {
        Vector3 newVelocity = _jumpModel.VerticalVelocity;
        newVelocity.y = Mathf.Sqrt(force * -2f * _jumpModel.Gravity);
        _jumpModel.VerticalVelocity = newVelocity;
        
        _animationController.TriggerJump();
        _audioController.PlayJumpSound();
        Debug.Log($"[PlayerMovementController] {gameObject.name} – double jump!");
    }

    /// <summary>
    /// Procesa el input de salto.
    /// Aplica la física del salto normal.
    /// </summary>
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
    
    /// <summary>
    /// Procesa la liberación del input de salto.
    /// Actualmente deshabilitado para simplificar.
    /// </summary>
    private void ProcessJumpRelease()
    {
        // Variable jump height disabled - simplified
    }

    /// <summary>
    /// Actualiza el animador del jugador.
    /// Delega al controlador de animaciones.
    /// </summary>
    private void UpdateAnimator()
    {
        _animationController.UpdateAnimator();
    }
}