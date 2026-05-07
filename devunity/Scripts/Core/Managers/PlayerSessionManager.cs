using UnityEngine;

namespace ChibitsLink.Core
{
    /// <summary>
    /// Gestor de sesión de jugadores.
    /// Controla el número de jugadores conectados y validaciones de partida.
    /// </summary>
    /// <remarks>
    /// Proporciona eventos para actualización de UI.
    /// Maneja validaciones básicas para inicio de juego.
    /// </remarks>
    public class PlayerSessionManager : MonoBehaviour
    {
        [Header("Configuración de Jugadores")]
        /// <summary>Número máximo de jugadores permitidos</summary>
        public int maxPlayers = 4;
        
        /// <summary>Número de jugadores conectados actualmente</summary>
        private int _connectedPlayers;
        
        /// <summary>Evento cuando se actualiza el número de jugadores</summary>
        public System.Action<int> OnPlayersUpdated;
        
        /// <summary>
        /// Inicializa el gestor de sesión.
        /// Resetea el contador de jugadores conectados.
        /// </summary>
        public void Initialize()
        {
            _connectedPlayers = 0;
        }
        
        /// <summary>
        /// Registra la conexión de un nuevo jugador.
        /// Incrementa el contador y dispara evento.
        /// </summary>
        public void PlayerConnected()
        {
            _connectedPlayers++;
            OnPlayersUpdated?.Invoke(_connectedPlayers);
            
            Debug.Log($"[PlayerSessionManager] Player connected. Total: {_connectedPlayers}/{maxPlayers}");
        }
        
        /// <summary>
        /// Registra la desconexión de un jugador.
        /// Decrementa el contador y dispara evento.
        /// </summary>
        public void PlayerDisconnected()
        {
            _connectedPlayers = Mathf.Max(0, _connectedPlayers - 1);
            OnPlayersUpdated?.Invoke(_connectedPlayers);
            
            Debug.Log($"[PlayerSessionManager] Player disconnected. Total: {_connectedPlayers}/{maxPlayers}");
        }
        
        /// <summary>
        /// Obtiene el número de jugadores conectados.
        /// </summary>
        /// <returns>Número de jugadores conectados</returns>
        public int GetConnectedPlayers()
        {
            return _connectedPlayers;
        }
        
        /// <summary>
        /// Obtiene el número máximo de jugadores permitidos.
        /// </summary>
        /// <returns>Número máximo de jugadores</returns>
        public int GetMaxPlayers()
        {
            return maxPlayers;
        }
        
        /// <summary>
        /// Verifica si se puede iniciar el juego.
        /// Requiere mínimo 2 jugadores.
        /// </summary>
        /// <returns>True si hay suficientes jugadores</returns>
        public bool CanStartGame()
        {
            return _connectedPlayers >= 2;
        }
        
        /// <summary>
        /// Verifica si la sala está llena.
        /// </summary>
        /// <returns>True si se alcanzó el máximo de jugadores</returns>
        public bool IsFull()
        {
            return _connectedPlayers >= maxPlayers;
        }
    }
}
