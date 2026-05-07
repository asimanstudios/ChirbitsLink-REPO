using UnityEngine;

namespace ChibiCocina.Models
{
    /// <summary>
    /// Modelo de datos para el sistema de salto del jugador.
    /// Almacena parámetros de física de salto y estado de coyote time.
    /// Implementa mecánicas avanzadas para saltos tolerantes.
    /// </summary>
    /// <remarks>
    /// Soporta coyote time y jump buffer para mejor jugabilidad.
    /// Configurable para diferentes modos de juego.
    /// Maneja saltos múltiples y control de altura variable.
    /// </remarks>
    public class JumpModel
    {
        // Parámetros de configuración
        /// <summary>Fuerza aplicada al saltar</summary>
        public float JumpForce { get; set; } = 5f;
        /// <summary>Fuerza de gravedad aplicada</summary>
        public float Gravity { get; set; } = -20f;
        /// <summary>Multiplicador de caída para realismo</summary>
        public float FallMultiplier { get; set; } = 2.5f;
        /// <summary>Multiplicador para saltos bajos (control de altura)</summary>
        public float LowJumpMultiplier { get; set; } = 2f;
        /// <summary>Tiempo permitido para saltar después de dejar el suelo</summary>
        public float CoyoteTime { get; set; } = 0.15f;
        /// <summary>Tiempo de anticipación para saltos</summary>
        public float JumpBufferTime { get; set; } = 0.2f;
        
        // Estado dinámico
        /// <summary>Velocidad vertical actual del jugador</summary>
        public Vector3 VerticalVelocity { get; set; }
        /// <summary>Indica si el jugador está en el suelo</summary>
        public bool IsGrounded { get; set; }
        /// <summary>Contador de tiempo para coyote time</summary>
        public float CoyoteTimeCounter { get; set; }
        /// <summary>Contador de tiempo para jump buffer</summary>
        public float JumpBufferCounter { get; set; }
        /// <summary>Saltos en el aire restantes</summary>
        public int AirJumpsRemaining { get; set; } = 1;
        /// <summary>Indica si estaba en el suelo en el frame anterior</summary>
        public bool WasGrounded { get; set; }
        
        /// <summary>
        /// Aplica estadísticas de combate al modelo de salto.
        /// Configura parámetros específicos para modo competitivo.
        /// </summary>
        /// <remarks>
        /// Utilizado por PlayerCombatController para cambiar entre modos.
        /// Establece valores ajustados para jugabilidad de combate.
        /// </remarks>
        public void ApplyCombatStats()
        {
            Gravity = -20f;
            JumpForce = 5f;
            FallMultiplier = 2.5f;
            LowJumpMultiplier = 2f;
        }
        
        /// <summary>
        /// Resetea el estado completo del sistema de salto.
        /// Limpia velocidades y contadores de tiempo.
        /// </summary>
        /// <remarks>
        /// Utilizado al respawnear o cambiar de escena.
        /// Reinicia todos los contadores a valores iniciales.
        /// </remarks>
        public void ResetJumpState()
        {
            VerticalVelocity = Vector3.zero;
            CoyoteTimeCounter = 0f;
            JumpBufferCounter = 0f;
            AirJumpsRemaining = 1;
        }
        
        /// <summary>
        /// Maneja el evento de aterrizaje del jugador.
        /// Restablece saltos en el aire y coyote time.
        /// </summary>
        /// <remarks>
        /// Debe llamarse cuando el jugador toca el suelo.
        /// Permite saltar inmediatamente después de aterrizar.
        /// </remarks>
        public void OnLanded()
        {
            CoyoteTimeCounter = CoyoteTime;
            AirJumpsRemaining = 1;
        }
    }
}
