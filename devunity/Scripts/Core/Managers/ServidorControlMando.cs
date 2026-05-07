using UnityEngine;
using Unity.Netcode;
using ChibitsLink.Models;

namespace ChibitsLink.Core
{
    /// <summary>
    /// Servidor para control de mandos móviles.
    /// Gestiona conexiones de clientes y comunicación de red.
    /// Implementa patrón Singleton para acceso global.
    /// </summary>
    /// <remarks>
    /// Utiliza Unity Netcode para GameObjects.
    /// Maneja eventos de conexión y desconexión.
    /// Proporciona API para gestión de clientes.
    /// </remarks>
    /// <seealso cref="https://docs-multiplayer.unity3d.com/">
    /// Documentación de Unity Netcode
    /// </seealso>
    public class ServidorControlMando : MonoBehaviour
    {
        /// <summary>Instancia global del servidor (patrón Singleton)</summary>
        public static ServidorControlMando Instance { get; private set; }
        
        [Header("Configuración del Servidor")]
        /// <summary>Puerto del servidor</summary>
        public int port = 7777;
        /// <summary>Número máximo de conexiones</summary>
        public int maxConnections = 8;
        /// <summary>Timeout para conexiones</summary>
        public float connectionTimeout = 10f;
        
        // Estado del servidor
        /// <summary>Indica si el servidor está activo</summary>
        private bool _isServerActive;
        /// <summary>Número de clientes conectados</summary>
        private int _connectedClients;
        /// <summary>Información de clientes conectados</summary>
        private System.Collections.Generic.Dictionary<ulong, ClientInfo> _clients;
        
        // Eventos
        /// <summary>Evento cuando se actualiza el número de clientes</summary>
        public System.Action<int> OnClientsUpdated;
        /// <summary>Evento cuando un cliente se conecta</summary>
        public System.Action<ulong> OnClientConnected;
        /// <summary>Evento cuando un cliente se desconecta</summary>
        public System.Action<ulong> OnClientDisconnected;
        /// <summary>Evento cuando se recibe un mensaje</summary>
        public System.Action<string> OnMessageReceived;
        
        /// <summary>
        /// Inicializa el servidor y establece el patrón Singleton.
        /// Configura NetworkManager y persiste entre escenas.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeServer();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Inicializa los componentes del servidor.
        /// Configura NetworkManager y eventos.
        /// </summary>
        private void InitializeServer()
        {
            _clients = new System.Collections.Generic.Dictionary<ulong, ClientInfo>();
            _connectedClients = 0;
            _isServerActive = false;
            
            // Subscribe to NetworkManager events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            }
            
            Debug.Log("[ServidorControlMando] Server initialized");
        }
        
        /// <summary>
        /// Limpia recursos al destruir el objeto.
        /// Remueve listeners de NetworkManager.
        /// </summary>
        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }
        
        /// <summary>
        /// Inicia el servidor de red.
        /// Configura transporte y comienza a escuchar conexiones.
        /// </summary>
        /// <returns>True si el servidor se inició correctamente</returns>
        public bool StartServer()
        {
            if (_isServerActive)
            {
                Debug.LogWarning("[ServidorControlMando] Server is already active");
                return false;
            }
            
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[ServidorControlMando] NetworkManager not found");
                return false;
            }
            
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.ConnectionData.Port = (ushort)port;
            
            if (NetworkManager.Singleton.StartServer())
            {
                _isServerActive = true;
                Debug.Log($"[ServidorControlMando] Server started on port {port}");
                return true;
        }
        
        public void StopServer()
        {
            if (!_isServerActive)
            {
                Debug.LogWarning("[ServidorControlMando] Server is already stopped");
            }
            else
            {
                if (NetworkManager.Singleton != null)
                {
                    NetworkManager.Singleton.Shutdown();
                }
                
                _isServerActive = false;
                _clients.Clear();
                _connectedClients = 0;
                
                Debug.Log("[ServidorControlMando] Server stopped");
            }
        }
        
        private void HandleClientConnected(ulong clientId)
        {
            if (_clients.ContainsKey(clientId))
            {
                Debug.LogWarning($"[ServidorControlMando] Client {clientId} is already connected");
            }
            else
            {
                var newClient = new ClientInfo
                {
                    Id = clientId,
                    Name = $"Player_{clientId}",
                    IsConnected = true,
                    ConnectionTime = Time.time
                };
                
                _clients[clientId] = newClient;
                _connectedClients++;
                
                OnClientConnected?.Invoke(clientId);
                OnClientsUpdated?.Invoke(_connectedClients);
                
                Debug.Log($"[ServidorControlMando] Client connected: {clientId} ({newClient.Name})");
            }
        }
        
        private void HandleClientDisconnected(ulong clientId)
        {
            if (!_clients.ContainsKey(clientId))
            {
                Debug.LogWarning($"[ServidorControlMando] Client {clientId} is not connected");
            }
            else
            {
                var client = _clients[clientId];
                client.IsConnected = false;
                client.DisconnectionTime = Time.time;
                
                _connectedClients--;
                
                OnClientDisconnected?.Invoke(clientId);
                OnClientsUpdated?.Invoke(_connectedClients);
                
                Debug.Log($"[ServidorControlMando] Client disconnected: {clientId} ({client.Name})");
            }
        }
        
        public void SendMessageToAll(string message)
        {
            if (!_isServerActive)
            {
                Debug.LogWarning("[ServidorControlMando] Cannot send message - server inactive");
            }
            else
            {
                // Send message to all connected clients
                Debug.Log($"[ServidorControlMando] Sending message to all: {message}");
            }
        }
        
        public void SendMessageToClient(ulong clientId, string message)
        {
            if (!_isServerActive)
            {
                Debug.LogWarning("[ServidorControlMando] Cannot send message - server inactive");
            }
            else if (!_clients.ContainsKey(clientId))
            {
                Debug.LogWarning($"[ServidorControlMando] Client {clientId} does not exist");
            }
            else
            {
                // Send message to specific client
                Debug.Log($"[ServidorControlMando] Sending message to {clientId}: {message}");
            }
        }
        
        public bool IsServerActive()
        {
            return _isServerActive;
        }
        
        public int GetConnectedClients()
        {
            return _connectedClients;
        }
        
        public ClientInfo? GetClientInfo(ulong clientId)
        {
            return _clients.ContainsKey(clientId) ? _clients[clientId] : null;
        }
        
        public System.Collections.Generic.IEnumerable<ClientInfo> GetAllClients()
        {
            return _clients.Values;
        }
        
        private void Update()
        {
            if (_isServerActive)
            {
                // Unity Netcode handles network updates automatically
                // No custom update logic needed for this server manager
            }
        }
        
        private void OnGUI()
        {
            if (_isServerActive)
            {
                GUILayout.BeginArea(new Rect(10, 320, 300, 150));
                GUILayout.Label($"Server Active - Port: {port}");
                GUILayout.Label($"Clients: {_connectedClients}/{maxConnections}");
                
                if (GUILayout.Button("Stop Server"))
                {
                    StopServer();
                }
                GUILayout.EndArea();
            }
            else
            {
                GUILayout.BeginArea(new Rect(10, 320, 300, 150));
                GUILayout.Label($"Server Inactive - Port: {port}");
                
                if (GUILayout.Button("Start Server"))
                {
                    StartServer();
                }
                GUILayout.EndArea();
            }
        }
    }
    
    [System.Serializable]
    public class ClientInfo
    {
        public ulong Id;
        public string Name;
        public bool IsConnected;
        public float ConnectionTime;
        public float DisconnectionTime;
        public int Ping;
    }
}
