using UnityEngine;
using ChibiCocina.Models;

namespace ChibitsLink.Controllers
{
    /// <summary>
    /// Controlador de animaciones del jugador basado en su estado de movimiento y acciones.
    /// Sincroniza animaciones con el modelo del jugador y eventos de juego.
    /// Gestiona transiciones entre estados de movimiento, salto e interacción.
    /// </summary>
    /// <remarks>
    /// Requiere que el GameObject tenga un componente Animator.
    /// Los parámetros del Animator deben llamarse: "saltar", "andar", "correr", "interactua".
    /// </remarks>
    public class PlayerAnimationController : MonoBehaviour
    {
        /// <summary>Componente Animator para controlar las animaciones</summary>
        private Animator _animator;
        /// <summary>Modelo de salto para detectar estado en el aire</summary>
        private JumpModel _jumpModel;
        /// <summary>Modelo de movimiento para detectar velocidad y estado de carrera</summary>
        private MovementModel _movementModel;
        
        /// <summary>
        /// Inicializa el controlador de animaciones.
        /// Obtiene el componente Animator requerido para el funcionamiento.
        /// </summary>
        private void Awake()
        {
            InitializeComponents();
        }
        
        /// <summary>
        /// Inicializa los componentes necesarios para el controlador.
        /// Busca y valida el componente Animator en el GameObject.
        /// </summary>
        private void InitializeComponents()
        {
            _animator = GetComponent<Animator>();
        }
        
        /// <summary>
        /// Configura los modelos de datos que controlan las animaciones.
        /// Debe llamarse después de inicializar los modelos del jugador.
        /// </summary>
        /// <param name="movementModel">Modelo que contiene datos de movimiento y velocidad</param>
        /// <param name="jumpModel">Modelo que contiene datos de salto y estado en el aire</param>
        public void InitializeModels(MovementModel movementModel, JumpModel jumpModel)
        {
            _movementModel = movementModel;
            _jumpModel = jumpModel;
        }
        
        /// <summary>
        /// Actualiza todas las animaciones basadas en el estado actual de los modelos.
        /// Debe llamarse cada frame para mantener sincronizadas las animaciones.
        /// </summary>
        /// <remarks>
        /// Si no hay Animator disponible, el método retorna sin hacer nada.
        /// Prioriza la animación de salto sobre las de movimiento.
        /// </remarks>
        public void UpdateAnimator()
        {
            bool hasAnimator = _animator != null;
            if (!hasAnimator) return;
            
            UpdateJumpAnimation();
            UpdateMovementAnimation();
        }
        
        /// <summary>
        /// Actualiza la animación de salto basada en el estado del JumpModel.
        /// Activa/desactiva el parámetro "saltar" según si el jugador está en el aire.
        /// </summary>
        /// <remarks>
        /// Cuando está saltando, desactiva las animaciones de movimiento para evitar conflictos.
        /// </remarks>
        private void UpdateJumpAnimation()
        {
            bool isJumping = !_jumpModel.IsGrounded || _jumpModel.VerticalVelocity.y > 0.1f;
            _animator.SetBool("saltar", isJumping);
            
            if (isJumping)
            {
                _animator.SetBool("andar", false);
                _animator.SetBool("correr", false);
            }
        }
        
        /// <summary>
        /// Actualiza las animaciones de movimiento (caminar/correr) cuando está en el suelo.
        /// Basa la animación en la velocidad actual y el estado de carrera.
        /// </summary>
        /// <remarks>
        /// Solo se ejecuta si el jugador está en el suelo para evitar animaciones incorrectas.
        /// Utiliza la magnitud de la velocidad para determinar si se está moviendo.
        /// </remarks>
        private void UpdateMovementAnimation()
        {
            bool isGrounded = _jumpModel.IsGrounded;
            if (!isGrounded) return;
            
            float speedScale = _movementModel.CurrentVelocity.magnitude;
            bool isMoving = speedScale > 0.1f;
            
            _animator.SetBool("andar", isMoving && !_movementModel.IsRunning);
            _animator.SetBool("correr", isMoving && _movementModel.IsRunning);
        }
        
        /// <summary>
        /// Activa manualmente la animación de salto.
        /// Útil para sincronizar con eventos externos o forzar la animación.
        /// </summary>
        /// <remarks>
        /// Establece directamente el parámetro "saltar" a true.
        /// El Animator se encargará de la transición y retorno automático.
        /// </remarks>
        public void TriggerJump()
        {
            if (_animator != null)
            {
                _animator.SetBool("saltar", true);
            }
        }
        
        /// <summary>
        /// Activa la animación de interacción.
        /// Dispara el trigger "interactua" para reproducir la animación correspondiente.
        /// </summary>
        /// <remarks>
        /// Utiliza un trigger en lugar de un booleano para permitir animaciones one-shot.
        /// El trigger se consume automáticamente después de la transición.
        /// </remarks>
        public void TriggerInteract()
        {
            if (_animator != null)
            {
                _animator.SetTrigger("interactua");
            }
        }
    }
}
