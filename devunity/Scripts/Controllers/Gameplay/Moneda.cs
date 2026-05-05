using UnityEngine;
using ChibitsLink.GameSide.Models;

namespace ChibiCocina.CoinCollector
{
    public class Moneda : BaseCollectible
    {
        protected override bool CanBeCollected()
        {
            // Verificar si el juego está activo desde el GestorCoinCollector
            return GestorCoinCollector.Instancia != null && 
                   GestorCoinCollector.Instancia.estadoActual == GameState.InGame;
        }

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
