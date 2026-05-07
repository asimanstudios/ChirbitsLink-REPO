using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ChibitsLink.GameSide;

namespace ChibitsLink.Minigames.BombTag
{
    /// <summary>
    /// Sistema de puntuación para el minijuego BombTag.
    /// Gestiona el orden de eliminación y calcula puntuaciones finales.
    /// </summary>
    /// <remarks>
    /// Asigna puntos basados en el orden de eliminación.
    /// Los supervivientes obtienen mejores puntuaciones.
    /// Proporciona callback para reportar puntuaciones al lobby.
    /// </remarks>
    public class BombTagScoring : MonoBehaviour
    {
        /// <summary>Orden de eliminación de jugadores</summary>
        private List<GameObject> _eliminationOrder = new List<GameObject>();
        /// <summary>Identidades de jugadores para obtener IDs</summary>
        private Dictionary<GameObject, PlayerIdentity> _playerIdentities = new Dictionary<GameObject, PlayerIdentity>();
        
        /// <summary>
        /// Inicializa el sistema de puntuación.
        /// Configura identidades de jugadores y limpia estado anterior.
        /// </summary>
        /// <param name="playerIdentities">Diccionario de identidades de jugadores</param>
        public void Initialize(Dictionary<GameObject, PlayerIdentity> playerIdentities)
        {
            _playerIdentities = playerIdentities ?? new Dictionary<GameObject, PlayerIdentity>();
            _eliminationOrder.Clear();
        }
        
        /// <summary>
        /// Registra la eliminación de un jugador.
        /// Añade al orden de eliminación si no está ya presente.
        /// </summary>
        /// <param name="player">GameObject del jugador eliminado</param>
        public void AddElimination(GameObject player)
        {
            if (player != null && !_eliminationOrder.Contains(player))
            {
                _eliminationOrder.Add(player);
                Debug.Log($"[BombTagScoring] Added elimination: {GetPlayerName(player)}");
            }
        }
        
        /// <summary>
        /// Procesa las puntuaciones finales del juego.
        /// Calcula puntos para todos los jugadores y ejecuta callback.
        /// </summary>
        /// <param name="scoreCallback">Callback para reportar puntuaciones (userId, score)</param>
        public void ProcessFinalScoring(System.Action<string, int> scoreCallback)
        {
            var survivors = GetAlivePlayers();
            
            // Add survivors to elimination order (they get better scores)
            foreach (var survivor in survivors)
            {
                if (!_eliminationOrder.Contains(survivor))
                {
                    _eliminationOrder.Add(survivor);
                }
            }
            
            Debug.Log($"[BombTagScoring] Processing final scores. Ranking count: {_eliminationOrder.Count}");
            
            // Score from first eliminated (lowest score) to last survivor (highest score)
            for (int i = 0; i < _eliminationOrder.Count; i++)
            {
                GameObject player = _eliminationOrder[i];
                if (player != null && _playerIdentities.TryGetValue(player, out PlayerIdentity identity))
                {
                    int score = CalculateScore(i, _eliminationOrder.Count);
                    string playerName = GetPlayerName(player);
                    
                    Debug.Log($"[BombTagScoring] {playerName}: {score} points");
                    scoreCallback?.Invoke(identity.userId, score);
                }
            }
        }
        
        private int CalculateScore(int position, int totalPlayers)
        {
            // Last position gets 10 points, first gets (totalPlayers * 10)
            return (totalPlayers - position) * 10;
        }
        
        public List<GameObject> GetEliminationOrder()
        {
            return new List<GameObject>(_eliminationOrder);
        }
        
        public List<GameObject> GetWinners()
        {
            return GetAlivePlayers();
        }
        
        private List<GameObject> GetAlivePlayers()
        {
            var alive = new List<GameObject>();
            GameObject player;
            
            foreach (var kvp in _playerIdentities)
            {
                player = kvp.Key;
                if (player != null && player.activeInHierarchy)
                {
                    alive.Add(player);
                }
            }
            return alive;
        }
        
        public string GetPlayerName(GameObject player)
        {
            if (player == null) return "Unknown";
            
            // Priority 1: Use cached identity username
            if (_playerIdentities.TryGetValue(player, out PlayerIdentity identity))
            {
                if (!string.IsNullOrEmpty(identity.username)) 
                    return identity.username;
                
                // Priority 2: Fetch from central repository
                if (PlayerManager.Instance != null && !string.IsNullOrEmpty(identity.userId))
                {
                    string repositoryName = PlayerManager.Instance.GetPlayerName(identity.userId);
                    if (repositoryName != "Jugador") 
                        return repositoryName;
                }
            }

            // Priority 3: Manual search fallback
            var manualIdentity = player.GetComponent<PlayerIdentity>() ?? player.GetComponentInParent<PlayerIdentity>();
            if (manualIdentity != null && PlayerManager.Instance != null)
            {
                return PlayerManager.Instance.GetPlayerName(manualIdentity.userId);
            }

            return player.name;
        }
    }
}
