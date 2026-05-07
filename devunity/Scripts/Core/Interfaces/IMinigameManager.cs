using System.Collections;

namespace Chirbits.Core
{
    /// <summary>
    /// Interfaz base para gestores de minijuegos.
    /// Define los métodos esenciales para el ciclo de vida de un minijuego.
    /// </summary>
    /// <remarks>
    /// Todos los gestores de minijuegos deben implementar esta interfaz.
    /// Proporciona una API consistente para control de minijuegos.
    /// </remarks>
    public interface IMinigameManager
    {
        /// <summary>
        /// Inicia el minijuego.
        /// Debe preparar el estado inicial y comenzar la jugabilidad.
        /// </summary>
        void StartGame();
        
        /// <summary>
        /// Finaliza el minijuego.
        /// Debe limpiar recursos y guardar resultados si es necesario.
        /// </summary>
        void EndGame();
        
        /// <summary>
        /// Indica si el minijuego está actualmente en ejecución.
        /// </summary>
        bool IsGameRunning { get; }
    }
}
