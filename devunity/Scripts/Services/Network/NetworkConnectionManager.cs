using Unity.Netcode;
using UnityEngine;

namespace ChibitsLink.Services.Network
{
    /// <summary>
    /// Gestor de conexión de red utilizando Unity Netcode for GameObjects.
    /// Proporciona interfaz GUI para iniciar como Host, Client o Server.
    /// Implementa patrón Singleton para acceso global al estado de red.
    /// </summary>
    /// <remarks>
    /// Este componente está diseñado principalmente para desarrollo y pruebas.
    /// En producción, debería reemplazarse con una UI más robusta.
    /// </remarks>
    /// <seealso href="https://docs-multiplayer.unity3d.com/">Unity Netcode Documentation</seealso>
    public class NetworkManager : MonoBehaviour
    {
        /// <summary>Instancia global del gestor de red (patrón Singleton)</summary>
        public static NetworkManager Instance;

        /// <summary>
        /// Inicializa el gestor de red y establece el patrón Singleton.
        /// Asegura que solo exista una instancia del componente.
        /// </summary>
        private void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
            }
            else 
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Dibuja la interfaz de usuario para control de red.
        /// Muestra botones para iniciar como Host/Client/Server o para desconectarse.
        /// </summary>
        /// <remarks>
        /// OnGUI es llamado automáticamente por Unity cada frame.
        /// La interfaz solo se muestra cuando no hay conexión activa.
        /// </remarks>
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            
            bool isNotConnected = !Unity.Netcode.NetworkManager.Singleton.IsClient && !Unity.Netcode.NetworkManager.Singleton.IsServer;
            
            if (isNotConnected)
            {
                if (GUILayout.Button("Start as Host")) 
                {
                    Unity.Netcode.NetworkManager.Singleton.StartHost();
                }
                
                if (GUILayout.Button("Start as Client")) 
                {
                    Unity.Netcode.NetworkManager.Singleton.StartClient();
                }
                
                if (GUILayout.Button("Start as Server")) 
                {
                    Unity.Netcode.NetworkManager.Singleton.StartServer();
                }
            }
            else
            {
                string mode = Unity.Netcode.NetworkManager.Singleton.IsHost ? "Host" : 
                             Unity.Netcode.NetworkManager.Singleton.IsServer ? "Server" : "Client";
                
                GUILayout.Label("Mode: " + mode);
                
                if (GUILayout.Button("Disconnect")) 
                {
                    Unity.Netcode.NetworkManager.Singleton.Shutdown();
                }
            }
            
            GUILayout.EndArea();
        }
    }
}
