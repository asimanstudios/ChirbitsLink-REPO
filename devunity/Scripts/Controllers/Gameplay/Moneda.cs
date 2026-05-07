using UnityEngine;
using ChibitsLink.GameSide.Models;

namespace ChibiCocina.CoinCollector
{
    /// <summary>
    /// Controlador de monedas coleccionables para el minijuego CoinCollector.
    /// Hereda de BaseCollectible y gestiona la lógica de recolección de monedas.
    /// </summary>
    /// <remarks>
    /// Solo permite recolección cuando el juego está en estado activo.
    /// Registra puntuación en el gestor específico del minijuego.
    /// </remarks>
    public class Moneda : BaseCollectible
    {
        /// <summary>
        /// Determina si la moneda puede ser recolectada.
        /// Verifica que el juego esté en estado activo.
        /// </summary>
        /// <returns>True si la moneda puede ser recolectada</returns>
        protected override bool CanBeCollected()
        {
            // Verificar si el juego está activo desde el GestorCoinCollector
            return GestorCoinCollector.Instancia != null && 
                   GestorCoinCollector.Instancia.estadoActual == GameState.InGame;
        }

        /// <summary>
        /// Maneja el evento de recolección de la moneda.
        /// Registra la puntuación en el gestor del minijuego.
        /// </summary>
        /// <param name="userId">ID del usuario que recolectó la moneda</param>
        protected override void OnCollect(string userId)
        {
            // Registrar puntuación en el gestor local específico de monedas
            if (GestorCoinCollector.Instancia != null)
            {
                GestorCoinCollector.Instancia.RegistrarMonedaRecogida(userId, valor);
            }
        }
    }
}
