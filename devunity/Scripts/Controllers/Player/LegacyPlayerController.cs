using UnityEngine;
using ChibitsLink.GameSide;
using ChibiCocina.Models;
using ChibiCocina.Core.Exceptions;

namespace ChibitsLink.Controllers
{
    /// <summary>
    /// Controlador legacy para el movimiento del jugador con soporte para input móvil y teclado.
    /// Maneja física básica de movimiento, salto e interacciones con objetos del mundo.
    /// Implementa interfaz IChibitsController para compatibilidad con sistema de control.
    /// </summary>
    /// <remarks>
    /// Requiere componentes Rigidbody y Collider en el GameObject.
    /// Utiliza patrón de timeout para evitar conflictos entre input móvil y teclado.
    /// </remarks>
    [RequireComponent(typeof(Rigidbody))]
    public class LegacyPlayerController : MonoBehaviour, PlayerManager.IChibitsController
    {
        [Header("Movement Configuration")]
        /// <summary>Velocidad de movimiento del jugador</summary>
        public float speed = 5f;
        /// <summary>Fuerza aplicada al saltar</summary>
        public float jumpForce = 6f;
        
        [Header("Ground Detection")]
        /// <summary>LayerMask para detectar superficies transitables</summary>
        public LayerMask groundMask = ~0;
        /// <summary>Distancia máxima para detección de suelo</summary>
        public float groundCheckDistance = 0.1f;

        // Components
        /// <summary>Componente Rigidbody para física de movimiento</summary>
        private Rigidbody _rigidbody;
        /// <summary>Componente Collider para detección de colisiones</summary>
        private Collider _collider;
        
        // Models
        /// <summary>Modelo que almacena estado y datos del jugador</summary>
        private PlayerModel _playerModel;
        
        // State
        /// <summary>Último tiempo en que se recibió input móvil</summary>
        private float _lastMobileInputTime;
        /// <summary>Tiempo de espera para evitar conflictos de input</summary>
        private const float MOBILE_INPUT_TIMEOUT = 0.5f;

        /// <summary>
        /// Inicializa el controlador del jugador.
        /// Configura componentes y modelos necesarios para el funcionamiento.
        /// </summary>
        private void Awake()
        {
            InitializeComponents();
            InitializeModels();
        }
        
        /// <summary>
        /// Inicializa los componentes requeridos para el controlador.
        /// Valida la presencia de Rigidbody y Collider necesarios.
        /// </summary>
        /// <exception cref="ComponentNotFoundException">Si falta Rigidbody o Collider</exception>
        private void InitializeComponents()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            
            if (_rigidbody == null)
            {
                throw new ComponentNotFoundException("Rigidbody component required");
            }
                
            if (_collider == null)
            {
                throw new ComponentNotFoundException("Collider component required");
            }
                
            _rigidbody.freezeRotation = true;
        }
        
        /// <summary>
        /// Inicializa los modelos de datos del jugador.
        /// Crea una instancia de PlayerModel para gestionar estado.
        /// </summary>
        private void InitializeModels()
        {
            _playerModel = new PlayerModel();
        }

        /// <summary>
        /// Actualiza el estado del jugador cada frame.
        /// Procesa input de teclado si no hay conflicto con input móvil.
        /// </summary>
        /// <remarks>
        /// Utiliza un timeout para evitar que el input móvil y teclado
        /// se procesen simultáneamente y causen movimientos erráticos.
        /// </remarks>
        private void Update()
        {
            bool canProcessKeyboard = Time.time - _lastMobileInputTime > MOBILE_INPUT_TIMEOUT;
            if (canProcessKeyboard)
            {
                ProcessKeyboardInput();
            }
        }
        
        /// <summary>
        /// Procesa el input de teclado para movimiento y salto.
        /// Actualiza el PlayerModel con los valores de entrada detectados.
        /// </summary>
        /// <remarks>
        /// Utiliza Input.GetAxisRaw para respuesta inmediata sin suavizado.
        /// Normaliza el vector de movimiento para velocidad consistente.
        /// </remarks>
        private void ProcessKeyboardInput()
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            
            bool hasMovementInput = Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f;
            
            if (hasMovementInput)
            {
                Vector2 moveInput = new Vector2(horizontalInput, verticalInput).normalized;
                _playerModel.MoveInput = moveInput;
            }
            else
            {
                _playerModel.MoveInput = Vector2.zero;
            }

            bool jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
            if (jumpPressed)
            {
                _playerModel.JumpRequested = true;
            }
        }

        /// <summary>
        /// Actualiza la física del jugador en intervalos fijos.
        /// Procesa detección de suelo, movimiento y salto de forma consistente.
        /// </summary>
        /// <remarks>
        /// FixedUpdate se llama a intervalos regulares independientes del frame rate,
        /// lo que garantiza física consistente en diferentes hardware.
        /// </remarks>
        private void FixedUpdate()
        {
            CheckGrounded();
            ApplyMovement();
            ApplyJump();
        }
        
        /// <summary>
        /// Verifica si el jugador está en contacto con el suelo.
        /// Realiza un raycast hacia abajo para detectar superficies transitables.
        /// </summary>
        /// <remarks>
        /// Considera la extensión del collider más la distancia de chequeo.
        /// Utiliza la LayerMask configurada para filtrar objetos válidos.
        /// </remarks>
        private void CheckGrounded()
        {
            float checkDistance = _collider.bounds.extents.y + groundCheckDistance;
            bool isGrounded = Physics.Raycast(transform.position, Vector3.down, checkDistance, groundMask);
            _playerModel.IsGrounded = isGrounded;
        }
        
        /// <summary>
        /// Aplica el movimiento horizontal basado en el input del jugador.
        /// Mantiene la velocidad vertical actual y rota hacia la dirección de movimiento.
        /// </summary>
        /// <remarks>
        /// Utiliza Slerp para rotación suave y natural.
        /// Solo rota si hay movimiento significativo para evitar vibraciones.
        /// </remarks>
        private void ApplyMovement()
        {
            Vector3 movement = new Vector3(_playerModel.MoveInput.x, 0f, _playerModel.MoveInput.y) * speed;
            Vector3 currentVelocity = _rigidbody.linearVelocity;
            _rigidbody.linearVelocity = new Vector3(movement.x, currentVelocity.y, movement.z);

            bool shouldRotate = movement.magnitude > 0.1f;
            if (shouldRotate)
            {
                Vector3 forwardDirection = new Vector3(movement.x, 0, movement.z);
                Quaternion targetRotation = Quaternion.LookRotation(forwardDirection);
                transform.forward = Vector3.Slerp(transform.forward, forwardDirection, Time.fixedDeltaTime * 10f);
            }
        }
        
        /// <summary>
        /// Aplica la fuerza de salto si el jugador lo solicita y está en el suelo.
        /// Resetea el flag de salto después de ejecutar la acción.
        /// </summary>
        /// <remarks>
        /// Solo permite saltar si está en el suelo para evitar saltos múltiples.
        /// Usa ForceMode.Impulse para aplicación instantánea de fuerza.
        /// </remarks>
        private void ApplyJump()
        {
            bool canJump = _playerModel.JumpRequested && _playerModel.IsGrounded;
            if (canJump)
            {
                _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _playerModel.JumpRequested = false;
            }
        }

        // ─── IChibitsController Implementation ────────────────────────────────────────

        /// <summary>
        /// Procesa el input de joystick para movimiento del jugador.
        /// Implementación de la interfaz IChibitsController para compatibilidad móvil.
        /// </summary>
        /// <param name="x">Coordenada X del joystick (-1 a 1)</param>
        /// <param name="y">Coordenada Y del joystick (-1 a 1)</param>
        /// <remarks>
        /// Actualiza el timestamp de input móvil para evitar conflictos con teclado.
        /// Los valores se asignan directamente al PlayerModel.
        /// </remarks>
        public void ProcessJoystick(float x, float y)
        {
            Debug.Log($"[LegacyPlayerController] Joystick Input: ({x}, {y})");
            _lastMobileInputTime = Time.time;
            _playerModel.MoveInput = new Vector2(x, y);
        }

        /// <summary>
        /// Procesa eventos de botones del controlador móvil.
        /// Implementación de la interfaz IChibitsController para botones táctiles.
        /// </summary>
        /// <param name="buttonId">Identificador del botón presionado</param>
        /// <param name="state">Estado del botón ("pressed" o "released")</param>
        /// <remarks>
        /// Solo procesa eventos de tipo "pressed".
        /// Actualiza el timestamp para evitar conflicto con input de teclado.
        /// </remarks>
        public void ProcessButton(string buttonId, string state)
        {
            Debug.Log($"[LegacyPlayerController] Button Input: {buttonId} state: {state}");
            _lastMobileInputTime = Time.time;
            bool isPressedState = state == "pressed";
            
            if (isPressedState)
            {
                ProcessMobileButton(buttonId);
            }
        }
        
        /// <summary>
        /// Procesa botones específicos del controlador móvil.
        /// Mapea IDs de botones a acciones del jugador (salto, interacción).
        /// </summary>
        /// <param name="buttonId">ID del botón a procesar</param>
        /// <remarks>
        /// Soporta múltiples aliases: "jump"/"a" para salto, "interact"/"b" para interacción.
        /// Los IDs se normalizan a minúsculas para consistencia.
        /// </remarks>
        private void ProcessMobileButton(string buttonId)
        {
            string normalizedButtonId = buttonId.ToLower();
            
            switch (normalizedButtonId)
            {
                case "jump":
                case "a":
                    _playerModel.JumpRequested = true;
                    break;

                case "interact":
                case "b":
                    TryInteract();
                    break;

                default:
                    Debug.Log($"[LegacyPlayerController] Unmapped button: {buttonId}");
                    break;
            }
        }

        /// <summary>
        /// Intenta interactuar con objetos en la dirección frontal del jugador.
        /// Realiza un raycast corto para detectar objetos interactivos.
        /// </summary>
        /// <remarks>
        /// Solo interactúa con objetos que implementen la interfaz IInteractable.
        /// La distancia de interacción está fija en 1.5 unidades.
        /// </remarks>
        private void TryInteract()
        {
            bool hitDetected = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1.5f);
            if (hitDetected)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                interactable?.Interact(gameObject);
                Debug.Log($"[LegacyPlayerController] Interacted with: {hit.collider.name}");
            }
        }
    }

    /// <summary>
    /// Interfaz para objetos del mundo que los jugadores pueden interactuar.
    /// Define el contrato para objetos interactivos en el escenario.
    /// </summary>
    /// <remarks>
    /// Cualquier objeto que quiera ser interactuable debe implementar esta interfaz.
    /// El método Interact será llamado cuando el jugador pulse el botón de interacción.
    /// </remarks>
    public interface IInteractable
    {
        /// <summary>
        /// Ejecuta la acción de interacción del objeto.
        /// </summary>
        /// <param name="source">GameObject del jugador que interactúa</param>
        /// <remarks>
        /// La implementación debe definir qué hace el objeto cuando interactúan con él.
        /// Puede incluir animaciones, cambios de estado, activación de mecanismos, etc.
        /// </remarks>
        void Interact(GameObject source);
    }
}
