using UnityEngine;
using ChibitsLink.Models;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.Services.Gameplay
{
    /// <summary>
    /// Servicio responsable de la física y cálculos de movimiento del jugador.
    /// Maneja movimiento horizontal, salto, gravedad y rotación.
    /// Implementa mecánicas avanzadas como coyote time y jump buffer.
    /// </summary>
    /// <remarks>
    /// Utiliza CharacterController en lugar de Rigidbody para movimiento preciso.
    /// Proporciona controles mejorados para jugabilidad fluida.
    /// </remarks>
    public class MovementService : MonoBehaviour
    {
        [Header("Movement Configuration")]
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
        /// <summary>Fuerza de gravedad aplicada</summary>
        public float gravity = -20f;
        
        [Header("Jump Configuration")]
        /// <summary>Fuerza aplicada al saltar</summary>
        public float jumpForce = 5f;
        /// <summary>Multiplicador de caída para mayor realismo</summary>
        public float fallMultiplier = 2.5f;
        /// <summary>Multiplicador para saltos bajos (control de altura)</summary>
        public float lowJumpMultiplier = 2f;
        /// <summary>Tiempo permitido para saltar después de dejar el suelo</summary>
        public float coyoteTime = 0.15f;
        /// <summary>Tiempo de anticipación para saltos</summary>
        public float jumpBufferTime = 0.2f;

        // Components
        /// <summary>CharacterController para manejo de colisiones y movimiento</summary>
        private CharacterController _controller;
        /// <summary>Transform del jugador para aplicar rotación</summary>
        private Transform _playerTransform;
        /// <summary>Modelo del jugador con estado y input</summary>
        private PlayerModel _playerModel;

        // Movement state
        /// <summary>Velocidad horizontal actual del jugador</summary>
        private Vector3 _currentVelocity;
        /// <summary>Velocidad vertical actual del jugador</summary>
        private Vector3 _verticalVelocity;
        /// <summary>Contador de tiempo para coyote time</summary>
        private float _coyoteTimeCounter;
        /// <summary>Contador de tiempo para jump buffer</summary>
        private float _jumpBufferCounter;
        
        // Constants
        /// <summary>Velocidad vertical cuando está en el suelo</summary>
        private const float GROUNDED_VELOCITY_Y = -2f;
        /// <summary>Umbral mínimo para detectar movimiento</summary>
        private const float MOVEMENT_THRESHOLD = 0.01f;
        /// <summary>Umbral mínimo para detectar rotación</summary>
        private const float ROTATION_THRESHOLD = 0.001f;
        /// <summary>Tasa de decremento del jump buffer</summary>
        private const float JUMP_BUFFER_DECREMENT = 1f;

        /// <summary>
        /// Inicializa el servicio de movimiento con los componentes requeridos.
        /// Valida que todos los parámetros sean válidos antes de asignarlos.
        /// </summary>
        /// <param name="characterController">Controlador de personaje para física</param>
        /// <param name="playerTransform">Transform del jugador para rotación</param>
        /// <param name="playerModel">Modelo con estado e input del jugador</param>
        /// <exception cref="ArgumentNullException">Si algún parámetro es null</exception>
        public void Initialize(CharacterController characterController, Transform playerTransform, PlayerModel playerModel)
        {
            _controller = characterController ?? throw new ArgumentNullException(nameof(characterController));
            _playerTransform = playerTransform ?? throw new ArgumentNullException(nameof(playerTransform));
            _playerModel = playerModel ?? throw new ArgumentNullException(nameof(playerModel));
        }

        /// <summary>
        /// Actualiza los cálculos de movimiento y aplica la física.
        /// Debe llamarse cada frame para mantener movimiento consistente.
        /// </summary>
        /// <remarks>
        /// Procesa input, actualiza estado, aplica gravedad y movimiento.
        /// Solo ejecuta si el CharacterController está habilitado.
        /// </remarks>
        public void UpdateMovement()
        {
            bool isControllerValid = _controller != null && _controller.enabled;
            
            if (isControllerValid)
            {
                ProcessMovementInput();
                UpdateGroundState();
                ProcessJumpInput();
                ApplyGravityForce();
                ApplyMovementToController();
            }
        }
        
        /// <summary>
        /// Procesa el input de movimiento del jugador.
        /// Aplica movimiento horizontal o fricción según corresponda.
        /// </summary>
        private void ProcessMovementInput()
        {
            bool hasMovementInput = _playerModel.MoveInput.magnitude > MOVEMENT_THRESHOLD;
            
            if (hasMovementInput)
            {
                ApplyHorizontalMovement(_playerModel.MoveInput);
            }
            else
            {
                ApplyFriction();
            }
        }
        
        /// <summary>
        /// Actualiza el estado de suelo del jugador.
        /// Maneja el coyote time para saltos tolerantes.
        /// </summary>
        /// <remarks>
        /// Actualiza el estado grounded del PlayerModel.
        /// Mantiene el coyote time cuando está en el suelo.
        /// </remarks>
        private void UpdateGroundState()
        {
            _playerModel.IsGrounded = _controller.isGrounded;
            
            if (_playerModel.IsGrounded)
            {
                _coyoteTimeCounter = coyoteTime;
            }
            
            bool shouldDecayCoyoteTime = !_playerModel.IsGrounded && _coyoteTimeCounter > 0;
            if (shouldDecayCoyoteTime)
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }
        }
        
        /// <summary>
        /// Procesa el input de salto considerando coyote time y jump buffer.
        /// Permite saltos con timing tolerante.
        /// </summary>
        /// <remarks>
        /// Solo salta si hay coyote time y jump buffer activos.
        /// Actualiza el contador de jump buffer cada frame.
        /// </remarks>
        private void ProcessJumpInput()
        {
            bool canJump = _playerModel.JumpRequested && _coyoteTimeCounter > 0 && _jumpBufferCounter > 0;
            
            if (canJump)
            {
                ExecuteJump();
            }
            
            UpdateJumpBuffer();
        }
        
        /// <summary>
        /// Ejecuta el salto con la física apropiada.
        /// Calcula la velocidad vertical necesaria para alcanzar la altura deseada.
        /// </summary>
        /// <remarks>
        /// Utiliza fórmula de física: v = sqrt(2gh).
        /// Resetea todos los contadores relacionados con el salto.
        /// </remarks>
        private void ExecuteJump()
        {
            _verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            _playerModel.JumpRequested = false;
            _jumpBufferCounter = 0;
            _coyoteTimeCounter = 0;
        }
        
        /// <summary>
        /// Actualiza el contador de jump buffer.
        /// Decrementa el tiempo restante para anticipación de saltos.
        /// </summary>
        private void UpdateJumpBuffer()
        {
            if (_jumpBufferCounter > 0)
            {
                _jumpBufferCounter -= Time.deltaTime * JUMP_BUFFER_DECREMENT;
            }
        }
        
        /// <summary>
        /// Aplica el movimiento calculado al CharacterController.
        /// Combina velocidades horizontal y vertical.
        /// </summary>
        /// <remarks>
        /// Utiliza Move del CharacterController para manejo de colisiones.
        /// La velocidad se multiplica por deltaTime para independencia de framerate.
        /// </remarks>
        private void ApplyMovementToController()
        {
            Vector3 totalVelocity = _currentVelocity + _verticalVelocity;
            _controller.Move(totalVelocity * Time.deltaTime);
        }

        /// <summary>
        /// Aplica movimiento horizontal basado en el input y estado actual.
        /// Calcula velocidad objetivo, dirección y aceleración apropiados.
        /// </summary>
        /// <param name="input">Vector de input de movimiento normalizado</param>
        /// <remarks>
        /// Considera si el jugador está corriendo o en el aire.
        /// Actualiza la rotación del jugador hacia la dirección de movimiento.
        /// </remarks>
        private void ApplyHorizontalMovement(Vector2 input)
        {
            float targetSpeed = CalculateTargetSpeed(input);
            Vector3 movementDirection = CalculateMovementDirection(input);
            float currentAcceleration = CalculateAcceleration();
            
            _currentVelocity = Vector3.Lerp(_currentVelocity, movementDirection * targetSpeed, currentAcceleration * Time.deltaTime);
            
            UpdatePlayerRotation(movementDirection);
        }
        
        /// <summary>
        /// Calcula la velocidad objetivo según el input y estado de carrera.
        /// </summary>
        /// <param name="input">Vector de input de movimiento</param>
        /// <returns>Velocidad objetivo calculada</returns>
        /// <remarks>
        /// Usa walkSpeed o runSpeed según el estado del PlayerModel.
        /// Multiplica por la magnitud del input para velocidad variable.
        /// </remarks>
        private float CalculateTargetSpeed(Vector2 input)
        {
            float baseSpeed = _playerModel.IsRunning ? runSpeed : walkSpeed;
            return baseSpeed * input.magnitude;
        }
        
        /// <summary>
        /// Calcula la dirección de movimiento 3D desde el input 2D.
        /// </summary>
        /// <param name="input">Vector de input 2D</param>
        /// <returns>Dirección de movimiento 3D normalizada</returns>
        /// <remarks>
        /// Convierte X->X y Y->Z para movimiento en el plano horizontal.
        /// </remarks>
        private Vector3 CalculateMovementDirection(Vector2 input)
        {
            return new Vector3(input.x, 0, input.y).normalized;
        }
        
        /// <summary>
        /// Calcula la aceleración apropiada según el estado del jugador.
        /// </summary>
        /// <returns>Aceleración calculada</returns>
        /// <remarks>
        /// Usa airControl cuando está en el aire para menor control.
        /// Usa aceleración completa cuando está en el suelo.
        /// </remarks>
        private float CalculateAcceleration()
        {
            return _playerModel.IsGrounded ? acceleration : acceleration * airControl;
        }
        
        /// <summary>
        /// Actualiza la rotación del jugador hacia la dirección de movimiento.
        /// </summary>
        /// <param name="direction">Dirección hacia la cual rotar</param>
        /// <remarks>
        /// Solo rota si hay movimiento significativo para evitar vibraciones.
        /// Usa Slerp para rotación suave y natural.
        /// </remarks>
        private void UpdatePlayerRotation(Vector3 direction)
        {
            bool shouldRotate = direction.sqrMagnitude > ROTATION_THRESHOLD;
            
            if (shouldRotate)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _playerTransform.rotation = Quaternion.Slerp(_playerTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Aplica fricción para reducir la velocidad horizontal gradualmente.
        /// </summary>
        /// <remarks>
        /// Utiliza Lerp hacia cero para desaceleración suave.
        /// Se aplica cuando no hay input de movimiento.
        /// </remarks>
        private void ApplyFriction()
        {
            _currentVelocity = Vector3.Lerp(_currentVelocity, Vector3.zero, friction * Time.deltaTime);
        }

        /// <summary>
        /// Aplica fuerza de gravedad con diferentes multiplicadores.
        /// Maneja caída más rápida y control de altura de salto.
        /// </summary>
        /// <remarks>
        /// Resetea la velocidad vertical cuando aterriza.
        /// Aplica diferentes multiplicadores según si está cayendo o subiendo.
        /// </remarks>
        private void ApplyGravityForce()
        {
            ResetVerticalVelocityWhenGrounded();
            
            float gravityMultiplier = CalculateGravityMultiplier();
            _verticalVelocity.y += gravity * gravityMultiplier * Time.deltaTime;
        }
        
        /// <summary>
        /// Resetea la velocidad vertical cuando el jugador aterriza.
        /// Previene acumulación de velocidad vertical negativa.
        /// </summary>
        /// <remarks>
        /// Solo se aplica si está en el suelo y cayendo.
        /// Establece una velocidad ligera hacia abajo para mejor detección de suelo.
        /// </remarks>
        private void ResetVerticalVelocityWhenGrounded()
        {
            bool isGroundedAndFalling = _playerModel.IsGrounded && _verticalVelocity.y < 0;
            
            if (isGroundedAndFalling)
            {
                _verticalVelocity.y = GROUNDED_VELOCITY_Y;
            }
        }
        
        /// <summary>
        /// Calcula el multiplicador de gravedad apropiado.
        /// Diferencia entre caída, salto bajo y salto normal.
        /// </summary>
        /// <returns>Multiplicador de gravedad calculado</returns>
        /// <remarks>
        /// Usa fallMultiplier cuando está cayendo para caída más rápida.
        /// Usa lowJumpMultiplier cuando suelta el botón de salto.
        /// Retorna 1f para salto normal.
        /// </remarks>
        private float CalculateGravityMultiplier()
        {
            bool isFalling = _verticalVelocity.y < 0;
            if (isFalling)
            {
                return fallMultiplier;
            }
            
            bool isRisingWithoutJumpHeld = _verticalVelocity.y > 0 && !IsJumpButtonHeld();
            if (isRisingWithoutJumpHeld)
            {
                return lowJumpMultiplier;
            }
            
            return 1f;
        }
        
        /// <summary>
        /// Verifica si el botón de salto está siendo presionado.
        /// </summary>
        /// <returns>True si el botón de salto está presionado</returns>
        /// <remarks>
        /// Consulta el estado del PlayerModel para consistencia.
        /// </remarks>
        private bool IsJumpButtonHeld()
        {
            return _playerModel.JumpRequested;
        }
    }
}
