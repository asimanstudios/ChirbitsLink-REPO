using System;
using System.Collections.Generic;
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
    /// TCP Network Server for mobile client connections.
    /// Handles client connections and message routing with proper error handling.
    /// </summary>
    public class TcpNetworkServer : MonoBehaviour
    {
        public static TcpNetworkServer Instance { get; private set; }
        
        [Header("Network Configuration")]
        public int port = DEFAULT_PORT;
        public ChibitsLink.UI.LobbyUI lobbyUI;
        
        [Header("Connection Settings")]
        public int maxConnections = DEFAULT_MAX_CONNECTIONS;
        public float connectionTimeout = DEFAULT_CONNECTION_TIMEOUT;
        public int bufferSize = DEFAULT_BUFFER_SIZE;
        
        // Network components
        private TcpListener _listener;
        private readonly List<TcpClient> _connectedClients = new List<TcpClient>();
        private readonly Dictionary<string, TcpClient> _userIdToClient = new Dictionary<string, TcpClient>();
        private readonly Dictionary<TcpClient, StringBuilder> _clientBuffers = new Dictionary<TcpClient, StringBuilder>();
        
        // Player data
        private readonly Dictionary<string, string> _idToName = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _idToCharacterId = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _idToLevel = new Dictionary<string, int>();
        
        // Session persistence
        private readonly Dictionary<string, string> _sessionNames = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _sessionCharacters = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _sessionLevels = new Dictionary<string, int>();
        
        // State
        private readonly List<string> _activePlayerIds = new List<string>();
        private string _currentRoomCode;
        private bool _isRunning;
        private bool _isInitialized;
        
        // Constants
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
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Failed to initialize server: {ex.Message}");
                throw new NetworkServiceException("Failed to initialize TCP server", ex);
            }
        }

        public async Task<bool> StartServerAsync()
        {
            if (_isRunning)
            {
                Debug.LogWarning("[TcpNetworkServer] Server is already running");
                return true;
            }

            try
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;
                
                Debug.Log($"[TcpNetworkServer] Server started on port {port}");
                
                // Start accepting connections
                _ = Task.Run(AcceptConnectionsAsync);
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Failed to start server: {ex.Message}");
                throw new NetworkServiceException("Failed to start TCP server", ex);
            }
        }

        public void StopServer()
        {
            if (_isRunning)
            {
                try
                {
                    _isRunning = false;
                    _listener?.Stop();
                    
                    // Disconnect all clients
                    foreach (var client in _connectedClients.ToArray())
                    {
                        DisconnectClient(client);
                    }
                    
                    _connectedClients.Clear();
                    _userIdToClient.Clear();
                    _clientBuffers.Clear();
                    
                    Debug.Log("[TcpNetworkServer] Server stopped successfully");
                }
                catch (System.Exception ex)
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
                catch (System.Exception ex) when (_isRunning)
                {
                    Debug.LogError($"[TcpNetworkServer] Error accepting connection: {ex.Message}");
                }
            }
        }

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
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error handling client: {ex.Message}");
            }
            finally
            {
                DisconnectClient(client);
                stream?.Close();
            }
        }

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
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error processing client data: {ex.Message}");
                throw new NetworkServiceException("Failed to process client data", ex);
            }
        }

        private void ProcessMessage(TcpClient client, string message)
        {
            try
            {
                Debug.Log($"[TcpNetworkServer] Received message: {message}");
                
                // Parse message format: "COMMAND:DATA"
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
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error processing message: {ex.Message}");
                throw new NetworkServiceException("Failed to process message", ex);
            }
        }

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
                    
                    // Store player data
                    _idToName[userId] = userName;
                    _idToCharacterId[userId] = characterId;
                    _idToLevel[userId] = level;
                    _userIdToClient[userId] = client;
                    
                    // Update session data
                    _sessionNames[userId] = userName;
                    _sessionCharacters[userId] = characterId;
                    _sessionLevels[userId] = level;
                    
                    if (!_activePlayerIds.Contains(userId))
                    {
                        _activePlayerIds.Add(userId);
                    }
                    
                    Debug.Log($"[TcpNetworkServer] Player connected: {userName} ({userId})");
                    
                    // Notify lobby UI
                    lobbyUI?.OnPlayerConnected(userId, userName, characterId, level);
                    
                    // Send confirmation
                    SendMessageToClient(client, "CONNECTED:OK");
                }
                else
                {
                    Debug.LogWarning("[TcpNetworkServer] Invalid connect data format");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error handling connect command: {ex.Message}");
                throw new NetworkServiceException("Failed to handle connect command", ex);
            }
        }

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
                    
                    // Notify lobby UI
                    lobbyUI?.OnPlayerDisconnected(userId, userName);
                }
                
                DisconnectClient(client);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error handling disconnect command: {ex.Message}");
                throw new NetworkServiceException("Failed to handle disconnect command", ex);
            }
        }

        private void HandleInputCommand(TcpClient client, string data)
        {
            try
            {
                // Find user ID for this client
                string userId = null;
                foreach (var kvp in _userIdToClient)
                {
                    if (kvp.Value == client)
                    {
                        userId = kvp.Key;
                        break;
                    }
                }
                
                if (!string.IsNullOrEmpty(userId))
                {
                    // Forward input to PlayerManager
                    var playerManager = ChibitsLink.GameSide.PlayerManager.Instance;
                    if (playerManager != null)
                    {
                        // Parse input data: "joystick:x:y" or "button:buttonId:state"
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
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error handling input command: {ex.Message}");
                throw new NetworkServiceException("Failed to handle input command", ex);
            }
        }

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
                
                // Remove from user mappings
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
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error disconnecting client: {ex.Message}");
            }
        }

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
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error sending message to client: {ex.Message}");
            }
        }

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
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error sending message to user: {ex.Message}");
            }
        }

        public void BroadcastMessage(string message)
        {
            try
            {
                foreach (var client in _connectedClients.ToArray())
                {
                    SendMessageToClient(client, message);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[TcpNetworkServer] Error broadcasting message: {ex.Message}");
            }
        }

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

        // Public API methods
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
