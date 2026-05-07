using UnityEngine;

namespace ChibitsLink.Core
{
    public class GameDebugUI : MonoBehaviour
    {
        private GameManager _gameManager;
        private bool _showDebugInfo = true;
        
        public void Initialize(GameManager gameManager)
        {
            _gameManager = gameManager;
        }
        
        private void OnGUI()
        {
            if (_showDebugInfo)
            {
                DrawDebugPanel();
            }
        }
        
        private void DrawDebugPanel()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 200, 180));
            GUILayout.Label("=== GAME MANAGER DEBUG ===");
            GUILayout.Label($"State: {_gameManager.GetCurrentState()}");
            GUILayout.Label($"Players: {_gameManager.GetConnectedPlayers()}/{_gameManager.maxPlayers}");
            GUILayout.Label($"Time: {_gameManager.GetRemainingTime():F1}s");
            GUILayout.Label($"Can Start: {_gameManager.CanStartGame()}");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Start Game"))
            {
                _gameManager.StartGame();
            }
            
            if (GUILayout.Button("End Game"))
            {
                _gameManager.EndGame();
            }
            
            if (GUILayout.Button("Player Connected"))
            {
                _gameManager.PlayerConnected();
            }
            
            if (GUILayout.Button("Player Disconnected"))
            {
                _gameManager.PlayerDisconnected();
            }
            
            if (GUILayout.Button("Toggle Debug"))
            {
                _showDebugInfo = !_showDebugInfo;
            }
            
            GUILayout.EndArea();
        }
    }
}
