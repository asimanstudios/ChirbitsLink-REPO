using UnityEngine;
using UnityEngine.InputSystem;
using ChibitsLink.GameSide;
using ChibiCocina.Models;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class MovimientoPersonaje : MonoBehaviour, PlayerManager.IChibitsController, IPushable
{
    [Header("Movimiento")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float acceleration = 12f;
    public float friction = 10f;
    public float airControl = 0.6f;
    public float rotationSpeed = 15f;
    public float lerpSpeed = 25f;

    [Header("Salto")]
    public float jumpForce = 3f;
    public float gravity = -15f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.2f;

    [Header("Audio")]
    public AudioClip sonidoSalto;
    public AudioClip[] sonidosPasos;
    public float intervaloPasos = 0.4f;
    public float intervaloPasosCorrer = 0.3f;

    [Header("Fuerzas Externas (Knockback)")]
    public float factorDecaimientoFuerza = 5f;

    // Componentes
    private CharacterController controller;
    private Animator animator;
    private AudioSource audioSource;
    
    // Modelos
    private MovementModel movementModel;
    private JumpModel jumpModel;
    
    // Servicios
    private AudioService audioService;
    
    // Estado
    private float _lastMobileInputTime;
    private const float MOBILE_INPUT_TIMEOUT = 0.5f;

    void Awake()
    {
        Debug.Log($"[Movimiento] Inicializando MovimientoPersonaje en {gameObject.name}");
        InitializeComponents();
        InitializeModels();
        InitializeServices();
        ApplyCombatStatsIfNeeded();
    }
    
    private void InitializeComponents()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    
    private void InitializeModels()
    {
        movementModel = new MovementModel();
        jumpModel = new JumpModel();
    }
    
    private void InitializeServices()
    {
        audioService = gameObject.AddComponent<AudioService>();
        audioService.Initialize(audioSource);
    }
    
    private void ApplyCombatStatsIfNeeded()
    {
        // DESACTIVADO - para que no sobrescriba los valores del salto
        // Ahora los valores del Inspector se respetan
        
        /*
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isCombatScene = sceneName.Contains("Push") || sceneName.Contains("Smash");
        bool isLobby = sceneName.Contains("menu") || sceneName.Contains("lobby");
        
        if (isCombatScene)
        {
            // ESCENAS DE COMBATE - valores ajustados para Push/Smash
            walkSpeed = 6.5f;
            runSpeed = 11f;
            acceleration = 35f;
            airControl = 1.0f;
            rotationSpeed = 25f;
            gravity = -22f;      // ← ESTO SOBRESCRIBÍA TUS VALORES
            jumpForce = 8f;     // ← ESTO SOBRESCRIBÍA TUS VALORES
            fallMultiplier = 1.5f;
            lowJumpMultiplier = 1f;
            
            movementModel.ApplyCombatStats();
            
            // Sincronizar jumpModel con los valores del script para consistencia
            jumpModel.Gravity = gravity;
            jumpModel.JumpForce = jumpForce;
            jumpModel.FallMultiplier = fallMultiplier;
            jumpModel.LowJumpMultiplier = lowJumpMultiplier;
        }
        else if (isLobby)
        {
            // ESCENAS DE LOBBY - valores normales para movimiento casual
            walkSpeed = 4f;
            runSpeed = 7f;
            acceleration = 20f;
            airControl = 0.8f;
            rotationSpeed = 15f;
            gravity = -18f;      // ← ESTO SOBRESCRIBÍA TUS VALORES
            jumpForce = 6f;     // ← ESTO SOBRESCRIBÍA TUS VALORES
            fallMultiplier = 2f;
            lowJumpMultiplier = 1.5f;
            
            // NO aplicar stats de combate en lobby
            // Usar valores normales del jumpModel
            jumpModel.Gravity = gravity;
            jumpModel.JumpForce = jumpForce;
            jumpModel.FallMultiplier = fallMultiplier;
            jumpModel.LowJumpMultiplier = lowJumpMultiplier;
        }
        else
        {
            // OTRAS ESCENAS (minijuegos normales) - valores estándar
            walkSpeed = 5f;
            runSpeed = 8f;
            acceleration = 25f;
            airControl = 0.9f;
            rotationSpeed = 20f;
            gravity = -20f;      // ← ESTO SOBRESCRIBÍA TUS VALORES
            jumpForce = 7f;     // ← ESTO SOBRESCRIBÍA TUS VALORES
            fallMultiplier = 2.5f;
            lowJumpMultiplier = 2f;
            
            // Valores estándar del jumpModel
            jumpModel.Gravity = gravity;
            jumpModel.JumpForce = jumpForce;
            jumpModel.FallMultiplier = fallMultiplier;
            jumpModel.LowJumpMultiplier = lowJumpMultiplier;
        }
        */
        
        UpdateModelsFromInspector();
    }
    
    private void UpdateModelsFromInspector()
    {
        movementModel.WalkSpeed = walkSpeed;
        movementModel.RunSpeed = runSpeed;
        movementModel.Acceleration = acceleration;
        movementModel.AirControl = airControl;
        movementModel.RotationSpeed = rotationSpeed;
        movementModel.LerpSpeed = lerpSpeed;
        
        jumpModel.JumpForce = jumpForce;
        jumpModel.Gravity = gravity;
        jumpModel.FallMultiplier = fallMultiplier;
        jumpModel.LowJumpMultiplier = lowJumpMultiplier;
        jumpModel.CoyoteTime = coyoteTime;
        jumpModel.JumpBufferTime = jumpBufferTime;
    }
    
    public void OnJump(InputValue value)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isCombatScene = sceneName.Contains("Push") || sceneName.Contains("Smash");
        
        if (value.isPressed)
        {
            jumpModel.JumpBufferCounter = jumpModel.JumpBufferTime;
            if (isCombatScene && !jumpModel.IsGrounded && jumpModel.AirJumpsRemaining > 0)
            {
                jumpModel.AirJumpsRemaining--;
                AplicarDobleSalto(jumpModel.JumpForce * 0.85f);
            }
        }
        else
        {
            Vector3 currentVelocity = jumpModel.VerticalVelocity;
            if (currentVelocity.y > 0)
            {
                currentVelocity.y *= 0.5f;
                jumpModel.VerticalVelocity = currentVelocity;
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
        if (controller != null) controller.enabled = active;
        if (!active)
        {
            movementModel.ResetVelocity();
            jumpModel.ResetJumpState();
        }
    }

    public void CancelJumpBuffer() 
    { 
        jumpModel.JumpBufferCounter = 0; 
    }
    
    public bool IsGrounded => jumpModel.IsGrounded;

    void Update()
    {
        bool canProcessUpdate = controller != null && controller.enabled;
        if (canProcessUpdate)
        {
            // Suavizar el input para evitar saltos de red
            movementModel.MoveInput = Vector2.Lerp(movementModel.MoveInput, movementModel.TargetMoveInput, Time.deltaTime * movementModel.LerpSpeed);
            
            CheckCombatGround();
            ApplyExternalForces();
            ApplyMovement();
            ApplyGravity();
            UpdateAnimator();
            UpdateStepAudio();

            // Sin buffers - simplificado
        }
    }

    private void ApplyExternalForces()
    {
        if (movementModel.ExternalForce.magnitude > 0.01f)
        {
            movementModel.ExternalForce = Vector3.Lerp(movementModel.ExternalForce, Vector3.zero, Time.deltaTime * factorDecaimientoFuerza);
        }
        else
        {
            movementModel.ExternalForce = Vector3.zero;
        }
    }

    public void ApplyPush(Vector3 force, float duration)
    {
        movementModel.ExternalForce += force;
        Debug.Log($"[Movimiento] {gameObject.name} empujado con fuerza: {force}");
    }

    public void AplicarDobleSalto(float fuerza)
    {
        Vector3 newVelocity = jumpModel.VerticalVelocity;
        newVelocity.y = Mathf.Sqrt(fuerza * -2f * jumpModel.Gravity);
        jumpModel.VerticalVelocity = newVelocity;
        if (animator != null) animator.SetBool("saltar", true);
        audioService.PlayJumpSound(sonidoSalto);
        Debug.Log($"[Movimiento] {gameObject.name} – doble salto!");
    }

    private void ProcessJumpInput()
    {
        // SALTO SIMPLE - sin complejidad innecesaria
        if (jumpModel.IsGrounded)
        {
            Vector3 newVelocity = jumpModel.VerticalVelocity;
            newVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpModel.VerticalVelocity = newVelocity;
            
            if (animator != null) animator.SetBool("saltar", true);
            audioService.PlayJumpSound(sonidoSalto);
        }
    }
    
    private void ProcessJumpRelease()
    {
        // Sin variable jump height - simplificado
    }

    private void UpdateStepAudio()
    {
        audioService.UpdateStepAudio(jumpModel.IsGrounded, movementModel.CurrentVelocity.magnitude, movementModel.IsRunning);
    }

    private void CheckCombatGround()
    {
        jumpModel.IsGrounded = controller.isGrounded;
        if (jumpModel.IsGrounded && !jumpModel.WasGrounded)
        {
            jumpModel.AirJumpsRemaining = 1;
            jumpModel.OnLanded();
        }
        jumpModel.WasGrounded = jumpModel.IsGrounded;
    }

    public void OnMove(InputValue value)
    {
        if (Time.time - _lastMobileInputTime > MOBILE_INPUT_TIMEOUT)
        {
            movementModel.TargetMoveInput = value.Get<Vector2>();
        }
    }

    public void OnRun(InputValue value)
    {
        movementModel.IsRunning = value.isPressed;
    }

    public void OnJumpCombat(InputValue value)
    {
        // Unificar con ProcessButton para evitar conflictos
        if (value.isPressed)
        {
            ProcessJumpInput();
        }
        else
        {
            ProcessJumpRelease();
        }
    }

    public void OnInteractua(InputValue value)
    {
        if (value.isPressed && animator != null)
        {
            animator.SetTrigger("interactua");
        }
    }

    // ─── IChibitsController ────────────────────────────────────────────────

    public void ProcessJoystick(float x, float y)
    {
        _lastMobileInputTime = Time.time;
        const float deadzone = 0.05f;
        float magnitude = Mathf.Sqrt(x * x + y * y);
        movementModel.TargetMoveInput = magnitude > deadzone ? new Vector2(x, y) : Vector2.zero;
    }

    public void ProcessButton(string buttonId, string state)
    {
        _lastMobileInputTime = Time.time;
        bool isPressedState = state == "pressed";
        if (isPressedState)
        {
            // Fighter mode for Smash/Push minigames
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isCombatMode = sceneName.Contains("Push") || sceneName.Contains("Smash");

            switch (buttonId.ToLower())
            {
                case "jump":
                case "a":
                    ProcessJumpInput();
                    break;

                case "interact":
                case "b":
                    if (animator != null) animator.SetTrigger("interactua");
                    // Fighter Systems desvinculado
                    break;
            }
        }
    }

    void ApplyMovement()
    {
        float inputMagnitude = movementModel.MoveInput.magnitude;
        float targetSpeed = (movementModel.IsRunning ? movementModel.RunSpeed : movementModel.WalkSpeed) * inputMagnitude;
        Vector3 moveDirection = new Vector3(movementModel.MoveInput.x, 0, movementModel.MoveInput.y);
        Vector3 normalizedDirection = moveDirection.normalized;

        float currentAcc = jumpModel.IsGrounded ? movementModel.Acceleration : movementModel.Acceleration * movementModel.AirControl;
        float currentFric = jumpModel.IsGrounded ? movementModel.Friction : movementModel.Friction * 0.2f;

        if (inputMagnitude > 0.05f)
        {
            movementModel.CurrentVelocity = Vector3.Lerp(movementModel.CurrentVelocity, normalizedDirection * targetSpeed, currentAcc * Time.deltaTime);

            if (normalizedDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(normalizedDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * movementModel.RotationSpeed);
            }
        }
        else
        {
            movementModel.CurrentVelocity = Vector3.Lerp(movementModel.CurrentVelocity, Vector3.zero, currentFric * Time.deltaTime);
        }

        controller.Move((movementModel.CurrentVelocity + movementModel.ExternalForce) * Time.deltaTime);
    }

    void ApplyGravity()
    {
        // GRAVEDAD SIMPLE - sin complejidad innecesaria
        Vector3 currentVelocity = jumpModel.VerticalVelocity;
        
        if (jumpModel.IsGrounded && currentVelocity.y < 0)
        {
            currentVelocity.y = -2f;
            jumpModel.VerticalVelocity = currentVelocity;
            if (animator != null) animator.SetBool("saltar", false);
        }
        else
        {
            // Aplicar gravedad simple
            currentVelocity.y += gravity * Time.deltaTime;
            jumpModel.VerticalVelocity = currentVelocity;
        }
        
        controller.Move(currentVelocity * Time.deltaTime);
    }

    void UpdateAnimator()
    {
        bool hasAnimator = animator != null;
        if (hasAnimator)
        {
            bool saltando = !jumpModel.IsGrounded || jumpModel.VerticalVelocity.y > 0.1f;
            animator.SetBool("saltar", saltando);

            if (saltando)
            {
                animator.SetBool("andar", false);
                animator.SetBool("correr", false);
            }
            else
            {
                float speedScale = movementModel.CurrentVelocity.magnitude;
                bool moving = speedScale > 0.1f;
                animator.SetBool("andar", moving && !movementModel.IsRunning);
                animator.SetBool("correr", moving && movementModel.IsRunning);
            }
        }
    }
}
