using UnityEngine;

namespace ChibitsLink.Core
{
    /// <summary>
    /// Gestor de tiempo para partidas de juego.
    /// Controla temporizadores de preparación y juego con eventos de actualización.
    /// </summary>
    /// <remarks>
    /// Proporciona eventos para sincronización con UI y otros sistemas.
    /// Maneja múltiples fases temporales (preparación y juego).
    /// </remarks>
    public class GameTimer : MonoBehaviour
    {
        [Header("Configuración del Temporizador")]
        /// <summary>Tiempo de preparación antes del juego (segundos)</summary>
        public float preparationTime = 5f;
        /// <summary>Duración del juego (segundos)</summary>
        public float gameTime = 300f;
        
        /// <summary>Tiempo restante actual</summary>
        private float _remainingTime;
        /// <summary>Indica si el temporizador está activo</summary>
        private bool _isRunning;
        
        /// <summary>Evento cuando el tiempo se actualiza</summary>
        public System.Action<float> OnTimeUpdated;
        
        /// <summary>
        /// Inicializa el temporizador.
        /// Establece el tiempo inicial de preparación.
        /// </summary>
        public void Initialize()
        {
            _remainingTime = preparationTime;
            _isRunning = false;
        }
        
        /// <summary>
        /// Inicia el temporizador con una duración específica.
        /// </summary>
        /// <param name="duration">Duración del temporizador</param>
        public void StartTimer(float duration)
        {
            _remainingTime = duration;
            _isRunning = true;
        }
        
        /// <summary>
        /// Detiene el temporizador.
        /// Mantiene el tiempo actual pero pausa el conteo.
        /// </summary>
        public void StopTimer()
        {
            _isRunning = false;
        }
        
        /// <summary>
        /// Actualiza el temporizador cada frame.
        /// Debe llamarse desde el Update del componente principal.
        /// </summary>
        public void UpdateTimer()
        {
            if (_isRunning)
            {
                _remainingTime -= Time.deltaTime;
                OnTimeUpdated?.Invoke(_remainingTime);
            }
        }
        
        /// <summary>
        /// Verifica si el tiempo ha expirado.
        /// </summary>
        /// <returns>True si el tiempo es menor o igual a cero</returns>
        public bool IsTimeExpired()
        {
            return _remainingTime <= 0f;
        }
        
        /// <summary>
        /// Obtiene el tiempo restante actual.
        /// </summary>
        /// <returns>Tiempo restante en segundos</returns>
        public float GetRemainingTime()
        {
            return _remainingTime;
        }
        
        /// <summary>
        /// Establece el tiempo de preparación.
        /// Reinicia al tiempo configurado para la fase de preparación.
        /// </summary>
        public void SetPreparationTime()
        {
            _remainingTime = preparationTime;
        }
        
        /// <summary>
        /// Establece el tiempo de juego.
        /// Reinicia al tiempo configurado para la fase de juego.
        /// </summary>
        public void SetGameTime()
        {
            _remainingTime = gameTime;
        }
    }
}
