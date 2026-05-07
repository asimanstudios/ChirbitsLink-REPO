using UnityEngine;

namespace ChibitsLink.Core
{
    public class PlayerSessionManager : MonoBehaviour
    {
        [Header("Player Configuration")]
        public int maxPlayers = 4;
        
        private int _connectedPlayers;
        
        public System.Action<int> OnPlayersUpdated;
        
        public void Initialize()
        {
            _connectedPlayers = 0;
        }
        
        public void PlayerConnected()
        {
            _connectedPlayers++;
            OnPlayersUpdated?.Invoke(_connectedPlayers);
            
            Debug.Log($"[PlayerSessionManager] Player connected. Total: {_connectedPlayers}/{maxPlayers}");
        }
        
        public void PlayerDisconnected()
        {
            _connectedPlayers = Mathf.Max(0, _connectedPlayers - 1);
            OnPlayersUpdated?.Invoke(_connectedPlayers);
            
            Debug.Log($"[PlayerSessionManager] Player disconnected. Total: {_connectedPlayers}/{maxPlayers}");
        }
        
        public int GetConnectedPlayers()
        {
            return _connectedPlayers;
        }
        
        public int GetMaxPlayers()
        {
            return maxPlayers;
        }
        
        public bool CanStartGame()
        {
            return _connectedPlayers >= 2;
        }
        
        public bool IsFull()
        {
            return _connectedPlayers >= maxPlayers;
        }
    }
}
