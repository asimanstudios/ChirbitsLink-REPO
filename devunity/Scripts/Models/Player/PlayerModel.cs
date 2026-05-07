using UnityEngine;

namespace ChibiCocina.Models
{
    /// <summary>
    /// Modelo de datos que representa el estado y input del jugador.
    /// Almacena información de movimiento, acciones y estado físico del jugador.
    /// Utilizado como contenedor central de datos para sistemas de control.
    /// </summary>
    /// <remarks>
    /// Actúa como intermediario entre input y sistemas de movimiento/animación.
    /// Proporciona estado centralizado para múltiples componentes.
    /// </remarks>
    public class PlayerModel
    {
        /// <summary>Vector de input de movimiento (X, Y)</summary>
        public Vector2 MoveInput { get; set; }
        /// <summary>Indica si el jugador está corriendo</summary>
        public bool IsRunning { get; set; }
        /// <summary>Indica si el jugador solicitó saltar</summary>
        public bool JumpRequested { get; set; }
        /// <summary>Indica si el jugador solicitó interactuar</summary>
        public bool InteractRequested { get; set; }
        /// <summary>Velocidad vertical actual del jugador</summary>
        public float VerticalVelocity { get; set; }
        /// <summary>Indica si el jugador está en el suelo</summary>
        public bool IsGrounded { get; set; }
        /// <summary>Objeto que el jugador está sosteniendo</summary>
        public GameObject HeldObject { get; set; }
        
        /// <summary>
        /// Resetea todos los inputs del jugador a valores iniciales.
        /// Utilizado para limpiar estado entre frames o al cambiar de control.
        /// </summary>
        /// <remarks>
        /// No resetea HeldObject ni estado físico (IsGrounded, VerticalVelocity).
        /// Solo limpia inputs de acción del jugador.
        /// </remarks>
        public void ResetInputs()
        {
            MoveInput = Vector2.zero;
            JumpRequested = false;
            InteractRequested = false;
        }
    }
}

