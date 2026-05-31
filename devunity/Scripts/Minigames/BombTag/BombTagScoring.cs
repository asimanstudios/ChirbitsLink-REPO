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
            
            GameObject player;
            PlayerIdentity identity;
            int score;
            string playerName;
            bool hasIdentity;
            
            // Score from first eliminated (lowest score) to last survivor (highest score)
            for (int i = 0; i < _eliminationOrder.Count; i++)
            {
                player = _eliminationOrder[i];
                hasIdentity = player != null && _playerIdentities.TryGetValue(player, out identity);
                if (hasIdentity)
                {
                    score = CalculateScore(i, _eliminationOrder.Count);
                    playerName = GetPlayerName(player);
                    
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
            bool isPlayerActive;
            
            foreach (var kvp in _playerIdentities)
            {
                player = kvp.Key;
                isPlayerActive = player != null && player.activeInHierarchy;
                if (isPlayerActive)
                {
                    alive.Add(player);
                }
            }
            return alive;
        }
        
        public string GetPlayerName(GameObject player)
        {
            string result = "Unknown";
            
            if (player != null)
            {
                bool nameFound = false;
                PlayerIdentity identity;
                bool hasCachedIdentity;
                bool hasUsername;
                string repositoryName;
                bool isNotDefaultName;
                var manualIdentity;
                bool hasManualIdentity;
                
                // Priority 1: Use cached identity username
                hasCachedIdentity = _playerIdentities.TryGetValue(player, out identity);
                if (hasCachedIdentity)
                {
                    hasUsername = !string.IsNullOrEmpty(identity.username);
                    if (hasUsername)
                    {
                        result = identity.username;
                        nameFound = true;
                    }
                    else if (PlayerManager.Instance != null && !string.IsNullOrEmpty(identity.userId))
                    {
                        // Priority 2: Fetch from central repository
                        repositoryName = PlayerManager.Instance.GetPlayerName(identity.userId);
                        isNotDefaultName = repositoryName != "Jugador";
                        if (isNotDefaultName)
                        {
                            result = repositoryName;
                            nameFound = true;
                        }
                    }
                }
                
                // Priority 3: Manual search fallback
                if (!nameFound)
                {
                    manualIdentity = player.GetComponent<PlayerIdentity>() ?? player.GetComponentInParent<PlayerIdentity>();
                    hasManualIdentity = manualIdentity != null && PlayerManager.Instance != null;
                    if (hasManualIdentity)
                    {
                        result = PlayerManager.Instance.GetPlayerName(manualIdentity.userId);
                        nameFound = true;
                    }
                }
                
                if (!nameFound)
                {
                    result = player.name;
                }
            }
            
            return result;
        }
    }
}
