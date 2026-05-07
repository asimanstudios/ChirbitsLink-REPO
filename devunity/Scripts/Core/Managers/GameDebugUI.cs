using UnityEngine;

namespace ChibitsLink.Core
{
    /// <summary>
    /// Interfaz de depuración para el gestor del juego.
    /// Muestra información en tiempo real y permite control manual durante desarrollo.
    /// </summary>
    /// <remarks>
    /// Solo debe utilizarse durante el desarrollo.
    /// Proporciona acceso directo a funciones del GameManager.
    /// </remarks>
    public class GameDebugUI : MonoBehaviour
    {
        /// <summary>Referencia al gestor principal del juego</summary>
        private GameManager _gameManager;
        /// <summary>Control de visibilidad del panel de depuración</summary>
        private bool _showDebugInfo = true;
        
        /// <summary>
        /// Inicializa la UI de depuración.
        /// </summary>
        /// <param name="gameManager">Referencia al GameManager</param>
        public void Initialize(GameManager gameManager)
        {
            _gameManager = gameManager;
        }
        
        /// <summary>
        /// Dibuja la interfaz de depuración.
        /// Se ejecuta automáticamente cada frame.
        /// </summary>
        private void OnGUI()
        {
            if (_showDebugInfo)
            {
                DrawDebugPanel();
            }
        }
        
        /// <summary>
        /// Dibuja el panel de información de depuración.
        /// Muestra estado del juego y botones de control.
        /// </summary>
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
