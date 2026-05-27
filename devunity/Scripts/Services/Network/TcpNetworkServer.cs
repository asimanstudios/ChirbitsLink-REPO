using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ChibitsLink.Core.Exceptions;
using ChibitsLink.Core.Networking;

namespace ChibitsLink.Services.Network
{
    /// <summary>
    /// Servidor de red TCP para conexiones de clientes móviles.
    /// Maneja conexiones de clientes y enrutamiento de mensajes con manejo de errores.
    /// Implementa patrón Singleton para acceso global.
    /// </summary>
    /// <remarks>
    /// Gestiona múltiples clientes simultáneamente.
    /// Proporciona persistencia de sesión entre escenas.
    /// Maneja desconexiones y reconexiones automáticas.
    /// </remarks>
    public class TcpNetworkServer : MonoBehaviour
    {
        /// <summary>Instancia global del servidor (patrón Singleton)</summary>
        public static TcpNetworkServer Instance { get; private set; }
        
        [Header("Configuración de Red")]
        /// <summary>Puerto del servidor</summary>
        public int port = DEFAULT_PORT;
        /// <summary>Referencia a la UI del lobby</summary>
        public ChibitsLink.UI.LobbyUI lobbyUI;
        
        [Header("Configuración de Conexión")]
        /// <summary>Número máximo de conexiones</summary>
        public int maxConnections = DEFAULT_MAX_CONNECTIONS;
        /// <summary>Timeout de conexión</summary>
        public float connectionTimeout = DEFAULT_CONNECTION_TIMEOUT;
        /// <summary>Tamaño del buffer</summary>
        public int bufferSize = DEFAULT_BUFFER_SIZE;
        
        // Componentes de red
        /// <summary>Listener TCP para aceptar conexiones</summary>
        private TcpListener _listener;
        /// <summary>Lista de clientes conectados</summary>
        private readonly List<TcpClient> _connectedClients = new List<TcpClient>();
        /// <summary>Mapeo de ID de usuario a cliente</summary>
        private readonly Dictionary<string, TcpClient> _userIdToClient = new Dictionary<string, TcpClient>();
        /// <summary>Buffers de mensajes por cliente</summary>
        private readonly Dictionary<TcpClient, StringBuilder> _clientBuffers = new Dictionary<TcpClient, StringBuilder>();
        
        // Datos de jugadores
        /// <summary>Mapeo de ID a nombre</summary>
        private readonly Dictionary<string, string> _idToName = new Dictionary<string, string>();
        /// <summary>Mapeo de ID a personaje</summary>
        private readonly Dictionary<string, string> _idToCharacterId = new Dictionary<string, string>();
        /// <summary>Mapeo de ID a nivel</summary>
        private readonly Dictionary<string, int> _idToLevel = new Dictionary<string, int>();
        
        // Persistencia de sesión
        /// <summary>Nombres de sesión persistente</summary>
        private readonly Dictionary<string, string> _sessionNames = new Dictionary<string, string>();
        /// <summary>Personajes de sesión persistente</summary>
        private readonly Dictionary<string, string> _sessionCharacters = new Dictionary<string, string>();
        /// <summary>Niveles de sesión persistente</summary>
        private readonly Dictionary<string, int> _sessionLevels = new Dictionary<string, int>();
        
        // Estado
        /// <summary>IDs de jugadores activos</summary>
        private readonly List<string> _activePlayerIds = new List<string>();
        /// <summary>Código de sala actual</summary>
        private string _currentRoomCode;
        /// <summary>Indica si el servidor está en ejecución</summary>
        private bool _isRunning;
        private bool _isInitialized;
        
        private const int DEFAULT_PORT = 11000;
        private const int DEFAULT_MAX_CONNECTIONS = 8;
        private const float DEFAULT_CONNECTION_TIMEOUT = 30f;
        private const int DEFAULT_BUFFER_SIZE = 4096;

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
        
        private void InitializeServer()
        {
            try
            {
                _isInitialized = true;
                Debug.Log("[TcpNetworkServer] Server initialized successfully");
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError($"[TcpNetworkServer] Failed to initialize server: {ex.Message}");
                throw new NetworkServiceException("Failed to initialize TCP server", ex);
            }
        }

        public async Task<bool> StartServerAsync()
        {
            bool result = true;
            if (!_isRunning)
            {
                try
                {
                    _listener = new TcpListener(IPAddress.Any, port);
                    _listener.Start();
                    _isRunning = true;
                    
                    Debug.Log($"[TcpNetworkServer] Server started on port {port}");
                    
                    _ = Task.Run(AcceptConnectionsAsync);
                }
                catch (SocketException ex)
                {
                    Debug.LogError($"[TcpNetworkServer] Failed to start server: {ex.Message}");
                    throw new NetworkServiceException("Failed to start TCP server", ex);
                }
            }
            else
            {
                Debug.LogWarning("[TcpNetworkServer] Server is already running");
            }
            return result;
        }

        /// <summary>
        /// Detiene el servidor TCP.
        /// Cierra todas las conexiones y libera recursos.
        /// </summary>
        public void StopServer()
        {
            if (_isRunning)
            {
                try
                {
                    _isRunning = false;
                    _listener?.Stop();
                    
                    foreach (var client in _connectedClients.ToArray())
                    {
                        DisconnectClient(client);
                    }
                    
                    _connectedClients.Clear();
                    _userIdToClient.Clear();
                    _clientBuffers.Clear();
                    
                    Debug.Log("[TcpNetworkServer] Server stopped successfully");
                }
                catch (ObjectDisposedException ex)
                {
                    Debug.LogError($"[TcpNetworkServer] Error stopping server: {ex.Message}");
                    throw new NetworkServiceException("Failed to stop TCP server", ex);
                }
            }
            else
            {
                Debug.LogWarning("[TcpNetworkServer] Server is not running");
            }
        }

        /// <summary>
        /// Acepta conexiones entrantes de forma asíncrona.
        /// Maneja cada cliente en un hilo separado.
        /// </summary>
        private async Task AcceptConnectionsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    
                    if (_connectedClients.Count >= maxConnections)
                    {
                        Debug.LogWarning("[TcpNetworkServer] Maximum connections reached, rejecting client");
                        client.Close();
                        continue;
                    }
                    
                    _connectedClients.Add(client);
                    _clientBuffers[client] = new StringBuilder();
                    
                    Debug.Log($"[TcpNetworkServer] Client connected: {GetClientEndpoint(client)}");
                    
                    // Start handling this client
                    _ = Task.Run(() => HandleClientAsync(client));
                }
                catch (SocketException ex) when (_isRunning)
                {
                    Debug.LogError($"[TcpNetworkServer] Error accepting connection: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Maneja un cliente TCP de forma asíncrona.
        /// Lee datos del stream y procesa mensajes completos.
        /// </summary>
        /// <param name="client">Cliente TCP a manejar</param>
        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                byte[] buffer = new byte[bufferSize];
                
                while (_isRunning && client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    
                    if (bytesRead == 0)
                    {
                        // Client disconnected - exit loop naturally
                        _isRunning = false;
                    }
                    else
                    {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        ProcessClientData(client, receivedData);
                    }
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error handling client: {ex.Message}");
            }
            finally
            {
                DisconnectClient(client);
                stream?.Close();
            }
        }

        /// <summary>
        /// Procesa los datos recibidos de un cliente.
        /// Almacena en buffer y procesa mensajes completos separados por newline.
        /// </summary>
        /// <param name="client">Cliente TCP origen de los datos</param>
        /// <param name="data">Datos recibidos del cliente</param>
        private void ProcessClientData(TcpClient client, string data)
        {
            try
            {
                StringBuilder clientBuffer = _clientBuffers[client];
                clientBuffer.Append(data);
                
                // Process complete messages (terminated by newline)
                string bufferContent = clientBuffer.ToString();
                string[] messages = bufferContent.Split('\n');
                
                // Keep the last incomplete message
                clientBuffer.Clear();
                if (messages.Length > 0)
                {
                    clientBuffer.Append(messages[messages.Length - 1]);
                    
                    // Process complete messages (except the last one which might be incomplete)
                    for (int i = 0; i < messages.Length - 1; i++)
                    {
                        if (!string.IsNullOrEmpty(messages[i]))
                        {
                            ProcessMessage(client, messages[i].Trim());
                        }
                    }
                }
            }
            catch (KeyNotFoundException ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error processing client data: {ex.Message}");
                throw new NetworkServiceException("Failed to process client data", ex);
            }
        }

        /// <summary>
        /// Procesa un mensaje individual recibido.
        /// Parsea el formato COMMAND:DATA y ejecuta el comando correspondiente.
        /// </summary>
        /// <param name="client">Cliente TCP que envió el mensaje</param>
        /// <param name="message">Mensaje en formato COMMAND:DATA</param>
        private void ProcessMessage(TcpClient client, string message)
        {
            try
            {
                Debug.Log($"[TcpNetworkServer] Received message: {message}");
                
                string[] parts = message.Split(':', 2);
                if (parts.Length >= 2)
                {
                    string command = parts[0].ToUpper();
                    string data = parts[1];
                    
                    switch (command)
                    {
                        case "CONNECT":
                            HandleConnectCommand(client, data);
                            break;
                        case "DISCONNECT":
                            HandleDisconnectCommand(client, data);
                            break;
                        case "INPUT":
                            HandleInputCommand(client, data);
                            break;
                        default:
                            Debug.LogWarning($"[TcpNetworkServer] Unknown command: {command}");
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning($"[TcpNetworkServer] Invalid message format: {message}");
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error processing message: {ex.Message}");
                throw new NetworkServiceException("Failed to process message", ex);
            }
        }

        /// <summary>
        /// Maneja el comando de conexión de un cliente.
        /// Parsea datos del jugador y lo registra en el sistema.
        /// </summary>
        /// <param name="client">Cliente TCP que se conecta</param>
        /// <param name="data">Datos del jugador en formato JSON</param>
        private void HandleConnectCommand(TcpClient client, string data)
        {
            try
            {
                // Parse connection data: "userId:userName:characterId:level"
                string[] parts = data.Split(':');
                if (parts.Length >= 4)
                {
                
                string userId = parts[0];
                string userName = parts[1];
                string characterId = parts[2];
                
                    if (!int.TryParse(parts[3], out int level))
                    {
                        level = 1;
                    }
                    
                    _idToName[userId] = userName;
                    _idToCharacterId[userId] = characterId;
                    _idToLevel[userId] = level;
                    _userIdToClient[userId] = client;
                    
                    _sessionNames[userId] = userName;
                    _sessionCharacters[userId] = characterId;
                    _sessionLevels[userId] = level;
                    
                    if (!_activePlayerIds.Contains(userId))
                    {
                        _activePlayerIds.Add(userId);
                    }
                    
                    Debug.Log($"[TcpNetworkServer] Player connected: {userName} ({userId})");
                    
                    lobbyUI?.OnPlayerConnected(userId, userName, characterId, level);
                    
                    SendMessageToClient(client, "CONNECTED:OK");
                }
                else
                {
                    Debug.LogWarning("[TcpNetworkServer] Invalid connect data format");
                }
            }
            catch (FormatException ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error handling connect command: {ex.Message}");
                throw new NetworkServiceException("Failed to handle connect command", ex);
            }
        }

        /// <summary>
        /// Maneja el comando de desconexión de un cliente.
        /// Busca al cliente y lo desconecta del servidor.
        /// </summary>
        /// <param name="client">Cliente TCP que se desconecta</param>
        /// <param name="data">Datos de desconexión (opcional)</param>
        private void HandleDisconnectCommand(TcpClient client, string data)
        {
            try
            {
                string userId = data.Trim();
                
                if (_userIdToClient.ContainsKey(userId))
                {
                    _userIdToClient.Remove(userId);
                }
                
                if (_idToName.ContainsKey(userId))
                {
                    string userName = _idToName[userId];
                    _idToName.Remove(userId);
                    _idToCharacterId.Remove(userId);
                    _idToLevel.Remove(userId);
                    
                    _activePlayerIds.Remove(userId);
                    
                    Debug.Log($"[TcpNetworkServer] Player disconnected: {userName} ({userId})");
                    
                    lobbyUI?.OnPlayerDisconnected(userId, userName);
                }
                
                DisconnectClient(client);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error handling disconnect command: {ex.Message}");
                throw new NetworkServiceException("Failed to handle disconnect command", ex);
            }
        }

        /// <summary>
        /// Maneja el comando de input de un cliente.
        /// Procesa datos de controlador (joystick, botones, sensores).
        /// </summary>
        /// <param name="client">Cliente TCP que envía input</param>
        /// <param name="data">Datos del input en formato JSON</param>
        private void HandleInputCommand(TcpClient client, string data)
        {
            try
            {
                // Find user ID for this client
                string userId = _userIdToClient.FirstOrDefault(kvp => kvp.Value == client).Key;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    var playerManager = ChibitsLink.GameSide.PlayerManager.Instance;
                    if (playerManager != null)
                    {
                        string[] inputParts = data.Split(':');
                        if (inputParts.Length >= 2)
                        {
                            string inputType = inputParts[0];
                            
                            if (inputType == "joystick" && inputParts.Length >= 3)
                            {
                                if (float.TryParse(inputParts[1], out float x) && float.TryParse(inputParts[2], out float y))
                                {
                                    playerManager.ProcessJoystick(userId, x, y);
                                }
                            }
                            else if (inputType == "button" && inputParts.Length >= 3)
                            {
                                string buttonId = inputParts[1];
                                string state = inputParts[2];
                                playerManager.ProcessButton(userId, buttonId, state);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[TcpNetworkServer] Received input from unauthenticated client");
                }
            }
            catch (FormatException ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error handling input command: {ex.Message}");
                throw new NetworkServiceException("Failed to handle input command", ex);
            }
        }

        /// <summary>
        /// Desconecta un cliente específico del servidor.
        /// Cierra la conexión y limpia todos los datos asociados.
        /// </summary>
        /// <param name="client">Cliente TCP a desconectar</param>
        private void DisconnectClient(TcpClient client)
        {
            try
            {
                if (client.Connected)
                {
                    client.Close();
                }
                
                _connectedClients.Remove(client);
                _clientBuffers.Remove(client);
                
                var keysToRemove = new List<string>();
                foreach (var kvp in _userIdToClient)
                {
                    if (kvp.Value == client)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
                
                foreach (var key in keysToRemove)
                {
                    _userIdToClient.Remove(key);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error disconnecting client: {ex.Message}");
            }
        }

        /// <summary>
        /// Envía un mensaje a un cliente específico.
        /// Escribe el mensaje en el stream del cliente TCP.
        /// </summary>
        /// <param name="client">Cliente TCP destinatario</param>
        /// <param name="message">Mensaje a enviar</param>
        public void SendMessageToClient(TcpClient client, string message)
        {
            try
            {
                if (client != null && client.Connected)
                {
                    NetworkStream stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                    stream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error sending message to client: {ex.Message}");
            }
        }

        /// <summary>
        /// Envía un mensaje a un usuario específico por ID.
        /// Busca el cliente asociado al ID y le envía el mensaje.
        /// </summary>
        /// <param name="userId">ID del usuario destinatario</param>
        /// <param name="message">Mensaje a enviar</param>
        public void SendMessageToUser(string userId, string message)
        {
            try
            {
                if (_userIdToClient.ContainsKey(userId))
                {
                    SendMessageToClient(_userIdToClient[userId], message);
                }
                else
                {
                    Debug.LogWarning($"[TcpNetworkServer] User {userId} not found");
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error sending message to user: {ex.Message}");
            }
        }

        /// <summary>
        /// Envía un mensaje a todos los clientes conectados.
        /// Itera sobre la lista de clientes y envía el mismo mensaje.
        /// </summary>
        /// <param name="message">Mensaje a broadcast</param>
        public void BroadcastMessage(string message)
        {
            try
            {
                foreach (var client in _connectedClients.ToArray())
                {
                    SendMessageToClient(client, message);
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error broadcasting message: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el endpoint de un cliente TCP.
        /// Extrae la dirección IP y puerto del cliente remoto.
        /// </summary>
        /// <param name="client">Cliente TCP del cual obtener endpoint</param>
        /// <returns>String con dirección IP:puerto o "Unknown" si hay error</returns>
        private string GetClientEndpoint(TcpClient client)
        {
            try
            {
                return client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        public bool IsRunning => _isRunning;
        public int ConnectedClientCount => _connectedClients.Count;
        public List<string> ActivePlayerIds => new List<string>(_activePlayerIds);
        public string GetPlayerName(string userId) => _idToName.TryGetValue(userId, out string name) ? name : "Unknown";
        public string GetPlayerCharacterId(string userId) => _idToCharacterId.TryGetValue(userId, out string charId) ? charId : "Default";
        public int GetPlayerLevel(string userId) => _idToLevel.TryGetValue(userId, out int level) ? level : 1;

        private void OnDestroy()
        {
            StopServer();
        }
    }
}
