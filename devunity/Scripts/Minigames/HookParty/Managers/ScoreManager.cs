using UnityEngine;
using System.Collections.Generic;

namespace ChibitsLink.GameSide.HookParty
{
    /// <summary>
    /// Almacena un registro de de qué jugador tiene cuántos puntos en este minijuego.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private Dictionary<string, int> _scores = new Dictionary<string, int>();
        
        public System.Action<string, int> OnScoreUpdated;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddScore(string userId, int amount = 1)
        {
            if (!_scores.ContainsKey(userId))
            {
                _scores[userId] = 0;
            }
            _scores[userId] += amount;
            
            // Notificar al UI para actualizar texto de puntos del jugador
            OnScoreUpdated?.Invoke(userId, _scores[userId]);
            Debug.Log($"[HookParty] ¡Puntos sumados para {userId}! Total: {_scores[userId]}");
        }

        public int GetScore(string userId)
        {
            if (_scores.TryGetValue(userId, out int s)) return s;
            return 0;
        }

        public Dictionary<string, int> GetAllScores()
        {
            return _scores;
        }
    }
}
