using UnityEngine;

namespace ChibiCocina.Models
{
    /// <summary>
    /// Modelo de datos para el sistema de movimiento del jugador.
    /// Almacena parámetros de movimiento y estado actual de locomoción.
    /// Configurable para diferentes modos de juego (combate vs lobby).
    /// </summary>
    /// <remarks>
    /// Contiene tanto configuración estática como estado dinámico.
    /// Permite aplicar diferentes estadísticas según el contexto del juego.
    /// </remarks>
    public class MovementModel
    {
        /// <summary>Velocidad de caminata base</summary>
        public float WalkSpeed { get; set; } = 4f;
        /// <summary>Velocidad de carrera base</summary>
        public float RunSpeed { get; set; } = 7f;
        /// <summary>Tasa de aceleración del movimiento</summary>
        public float Acceleration { get; set; } = 12f;
        /// <summary>Fricción aplicada cuando no hay input</summary>
        public float Friction { get; set; } = 10f;
        /// <summary>Control de movimiento en el aire (0-1)</summary>
        public float AirControl { get; set; } = 0.6f;
        /// <summary>Velocidad de rotación del jugador</summary>
        public float RotationSpeed { get; set; } = 15f;
        /// <summary>Velocidad de interpolación para suavizado</summary>
        public float LerpSpeed { get; set; } = 25f;
        
        // Estado dinámico
        /// <summary>Velocidad horizontal actual del jugador</summary>
        public Vector3 CurrentVelocity { get; set; }
        /// <summary>Fuerza externa aplicada al jugador</summary>
        public Vector3 ExternalForce { get; set; }
        /// <summary>Input de movimiento actual</summary>
        public Vector2 MoveInput { get; set; }
        /// <summary>Input objetivo para interpolación suave</summary>
        public Vector2 TargetMoveInput { get; set; }
        /// <summary>Indica si el jugador está corriendo actualmente</summary>
        public bool IsRunning { get; set; }
        
        /// <summary>
        /// Aplica estadísticas de combate al modelo de movimiento.
        /// Configura parámetros agresivos para modo competitivo.
        /// </summary>
        /// <remarks>
        /// Utilizado por PlayerCombatController para cambiar entre modos.
        /// Establece valores más altos para jugabilidad rápida.
        /// </remarks>
        public void ApplyCombatStats()
        {
            WalkSpeed = 6.5f;
            RunSpeed = 11f;
            Acceleration = 35f;
            AirControl = 1.0f;
            RotationSpeed = 25f;
        }
        
        /// <summary>
        /// Resetea las velocidades del modelo a cero.
        /// Utilizado para detener movimiento bruscamente.
        /// </summary>
        /// <remarks>
        /// Limpia tanto velocidad actual como fuerzas externas.
        /// No afecta parámetros de configuración.
        /// </remarks>
        public void ResetVelocity()
        {
            CurrentVelocity = Vector3.zero;
            ExternalForce = Vector3.zero;
        }
    }
}
