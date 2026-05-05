using UnityEngine;
using ChibitsLink.GameSide;
using ChibiCocina.Models;
using ChibiCocina.Core.Exceptions;

namespace ChibitsLink.Jugador
{
    [RequireComponent(typeof(Rigidbody))]
    public class ControladorLegacy : MonoBehaviour, PlayerManager.IChibitsController
    {
        [Header("Movimiento")]
        public float speed = 5f;
        public float jumpForce = 6f;

        [Header("Detección de suelo")]
        public LayerMask groundMask = ~0;
        public float groundCheckDistance = 0.1f;

        // Componentes
        private Rigidbody _rb;
        private Collider _collider;
        
        // Modelos
        private PlayerModel playerModel;
        
        // Estado
        private float _lastMobileInputTime;
        private const float MOBILE_INPUT_TIMEOUT = 0.5f;

        private void Awake()
        {
            try
            {
                InitializeComponents();
                InitializeModels();
            }
            catch (System.Exception ex)
            {
                throw new ComponentNotFoundException("Fallo al inicializar ControladorLegacy", ex);
            }
        }
        
        private void InitializeComponents()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            
            if (_rb == null)
                throw new ComponentNotFoundException("Rigidbody");
                
            if (_collider == null)
                throw new ComponentNotFoundException("Collider");
                
            _rb.freezeRotation = true;
        }
        
        private void InitializeModels()
        {
            playerModel = new PlayerModel();
        }

        private void Update()
        {
            // Solo procesar teclado si no hay input reciente del móvil (prioridad al móvil)
            if (Time.time - _lastMobileInputTime > MOBILE_INPUT_TIMEOUT)
            {
                ProcessKeyboardInput();
            }
        }
        
        private void ProcessKeyboardInput()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            
            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                playerModel.MoveInput = new Vector2(h, v).normalized;
            }
            else
            {
                playerModel.MoveInput = Vector2.zero;
            }

            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            {
                playerModel.JumpRequested = true;
            }
        }

        private void FixedUpdate()
        {
            CheckGrounded();
            ApplyMovement();
            ApplyJump();
        }
        
        private void CheckGrounded()
        {
            playerModel.IsGrounded = Physics.Raycast(transform.position, Vector3.down, 
                _collider.bounds.extents.y + groundCheckDistance, groundMask);
        }
        
        private void ApplyMovement()
        {
            Vector3 move = new Vector3(playerModel.MoveInput.x, 0f, playerModel.MoveInput.y) * speed;
            _rb.linearVelocity = new Vector3(move.x, _rb.linearVelocity.y, move.z);

            if (move.magnitude > 0.1f)
            {
                transform.forward = Vector3.Slerp(transform.forward, new Vector3(move.x, 0, move.z), Time.fixedDeltaTime * 10f);
            }
        }
        
        private void ApplyJump()
        {
            if (playerModel.JumpRequested && playerModel.IsGrounded)
            {
                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                playerModel.JumpRequested = false;
            }
        }

        // ─── IChibitsController ────────────────────────────────────────────────

        public void ProcessJoystick(float x, float y)
        {
            Debug.Log($"[PlayerController] Joystick Input: ({x}, {y})");
            _lastMobileInputTime = Time.time;
            playerModel.MoveInput = new Vector2(x, y);
        }

        public void ProcessButton(string buttonId, string state)
        {
            Debug.Log($"[PlayerController] Button Input: {buttonId} state: {state}");
            _lastMobileInputTime = Time.time;
            bool isPressedState = state == "pressed";
            if (isPressedState)
            {
                switch (buttonId.ToLower())
                {
                    case "jump":
                    case "a":
                        playerModel.JumpRequested = true;
                        break;

                    case "interact":
                    case "b":
                        TryInteract();
                        break;

                    default:
                        Debug.Log($"[PlayerController] Botón no mapeado en switch: {buttonId}");
                        break;
                }
            }
        }

        private void TryInteract()
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1.5f))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                interactable?.Interact(gameObject);
                Debug.Log($"[PlayerController] Interacción con: {hit.collider.name}");
            }
        }
    }

    /// <summary>Interfaz para objetos del mundo con los que el jugador puede interactuar.</summary>
    public interface IInteractable
    {
        void Interact(GameObject source);
    }
}
