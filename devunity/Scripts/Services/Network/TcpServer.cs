using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ChibitsLink.Utils;
using Chirbits.Core.Networking;

namespace ChibitsLink.GameSide
{
    /// <summary>
    /// Servidor TCP (ServerSocket) para el lado del juego.
    /// Maneja conexiones de la App cliente y retransmisión de mensajes.
    /// </summary>
    public class TcpServer : MonoBehaviour
    {
        public static TcpServer Instance { get; private set; }
        
        public int port = 11000;
        public ChibitsLink.UI.LobbyUI lobbyUI; // Asignar en el inspector
        
        private TcpListener _listener;
        private List<TcpClient> _clients = new List<TcpClient>();
        private Dictionary<string, string> _idToName = new Dictionary<string, string>();
        private Dictionary<string, string> _idToCharId = new Dictionary<string, string>();
        private Dictionary<string, int> _idToLevel = new Dictionary<string, int>();
        
        // Registro persistente para el historial (no se limpia al desconectar)
        private Dictionary<string, string> _sessionNames = new Dictionary<string, string>();
        private Dictionary<string, string> _sessionChars = new Dictionary<string, string>();
        private Dictionary<string, int> _sessionLevels = new Dictionary<string, int>();
        
        private List<string> _activePlayerIds = new List<string>();
        // Buffer por cliente: acumula bytes hasta encontrar \n (framing TCP correcto)
        private Dictionary<TcpClient, System.Text.StringBuilder> _clientBuffers = new Dictionary<TcpClient, System.Text.StringBuilder>();
        private Dictionary<string, TcpClient> _userIdToClient = new Dictionary<string, TcpClient>();
        private string _currentRoomCode;
        private bool _isRunning;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetRoomCode(string code) => _currentRoomCode = code;
        public string GetRoomCode() => _currentRoomCode;

        public Dictionary<string, string> GetSessionNames() => new Dictionary<string, string>(_sessionNames);
        public Dictionary<string, string> GetSessionChars() => new Dictionary<string, string>(_sessionChars);

        void Start()
        {
            // El servidor ahora se inicia bajo demanda desde LobbyUI.cs
            // StartServer();
        }

        public void StartServer()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;
                Debug.Log($"[TCP Server] Servidor iniciado en el puerto {port}");
                _ = AcceptClientsAsync();
            }
            catch (SocketException ex)
            {
                Debug.LogError($"[TCP Server] Error de socket al iniciar: {ex.Message}");
            }
        }

        private async Task AcceptClientsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    Debug.Log($"[TCP Server] Cliente conectado desde {client.Client.RemoteEndPoint}");
                    
                    // Configurar Keep-Alive para detectar desconexiones bruscas
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    
                    lock (_syncLock)
                    {
                        _clients.Add(client);
                        _clientBuffers[client] = new System.Text.StringBuilder();
                    }
                    NetworkingEvents.RaiseConnected("Unknown", client.Client.RemoteEndPoint.ToString());
                    _ = HandleClientAsync(client);
                }
                catch (ObjectDisposedException) { }
                catch (SocketException ex)
                {
                    Debug.LogError($"[TCP Server] Error de socket al aceptar cliente: {ex.Message}");
                }
            }
        }

        [Serializable]
        public class ControllerInput
        {
            public string type;
            public string id;       // Para botones
            public string state;    // Para botones
            public float x;         // Para joystick
            public float y;         // Para joystick
            public string userId;
            public string sensor;   // Para sensores
            public float value;    // Para sensores
        }

        private readonly object _syncLock = new object();

        /// <summary>
        /// Extrae mensajes completos del buffer acumulado de un cliente.
        /// MODO HÍBRIDO:
        ///   1) Si hay '\n', divide por líneas (correcto para JSON del joystick).
        ///   2) Si no hay '\n' pero el contenido empieza por un prefijo conocido
        ///      (SYNC_CHAR|, READY|, VOTE|, {) lo entrega de inmediato, sin esperar '\n'.
        ///      Esto cubre la app móvil que no añade '\n' al final de sus mensajes.
        /// </summary>
        private List<string> ExtractLines(TcpClient client, string incoming)
        {
            var messages = new List<string>();
            System.Text.StringBuilder sb;
            lock (_syncLock)
            {
                if (!_clientBuffers.TryGetValue(client, out sb)) return messages;
                sb.Append(incoming);
            }

            string accumulated;
            lock (_syncLock) { accumulated = sb.ToString(); }

            // ── Paso 1: Prioridad absoluta a mensajes con \n ─────────────────
            int newlineIdx;
            while ((newlineIdx = accumulated.IndexOf('\n')) >= 0)
            {
                string line = accumulated.Substring(0, newlineIdx).TrimEnd('\r');
                if (!string.IsNullOrWhiteSpace(line)) messages.Add(line);
                accumulated = accumulated.Substring(newlineIdx + 1);
            }

            // ── Paso 2: Manejo de mensajes sin \n (Especialmente para la App móvil) ──
            // Si el buffer contiene patrones conocidos, los extraemos aunque no tengan \n
            string[] prefixes = { "SYNC_CHAR|", "READY|", "VOTE|", "LEAVE|", "PING", "{" };
            
            bool found;
            do
            {
                found = false;
                string current = accumulated.TrimStart();
                int startOffset = accumulated.Length - current.Length;

                string matchedPrefix = null;
                foreach (var p in prefixes)
                {
                    if (current.StartsWith(p)) { matchedPrefix = p; break; }
                }

                if (matchedPrefix != null)
                {
                    // Buscar el inicio del SIGUIENTE mensaje para saber dónde termina este
                    int nextStart = -1;
                    foreach (var p in prefixes)
                    {
                        int idx = current.IndexOf(p, matchedPrefix.Length);
                        if (idx != -1 && (nextStart == -1 || idx < nextStart)) nextStart = idx;
                    }

                    if (nextStart != -1)
                    {
                        // Mensaje completo seguido de otro (ej: PING{"type":"joystick"...})
                        messages.Add(current.Substring(0, nextStart).Trim());
                        accumulated = accumulated.Substring(startOffset + nextStart);
                        found = true;
                    }
                    else
                    {
                        // No hay un siguiente mensaje. ¿Podemos dar por cerrado este?
                        if (matchedPrefix == "PING")
                        {
                            messages.Add("PING");
                            accumulated = accumulated.Substring(startOffset + matchedPrefix.Length);
                            found = true;
                        }
                        else if (matchedPrefix == "{" && current.EndsWith("}"))
                        {
                            messages.Add(current);
                            accumulated = "";
                            found = true;
                        }
                        else if (matchedPrefix.EndsWith("|") && current.Split('|').Length >= 3)
                        {
                            // SYNC_CHAR|UID|CHAR -> al menos 3 partes, lo damos por bueno
                            messages.Add(current);
                            accumulated = "";
                            found = true;
                        }
                    }
                }
            } while (found && !string.IsNullOrEmpty(accumulated));

            lock (_syncLock)
            {
                sb.Clear();
                sb.Append(accumulated);
            }

            return messages;
        }


        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[8192];
            string currentUserId = null;
            HashSet<string> sessionUserIds = new HashSet<string>();
            
            const int TIMEOUT_MS = 15000; // 15 segundos de inactividad

            try
            {
                while (_isRunning && client.Connected)
                {
                    Task<int> readTask = stream.ReadAsync(buffer, 0, buffer.Length);
                    Task timeoutTask = Task.Delay(TIMEOUT_MS);

                    Task completedTask = await Task.WhenAny(readTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        Debug.LogWarning($"[TCP Server] Timeout de inactividad alcanzado ({TIMEOUT_MS}ms). Cerrando socket.");
                        break;
                    }

                    int bytesRead = await (Task<int>)completedTask;
                    if (bytesRead == 0) break;

                    string rawData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var extractedMessages = ExtractLines(client, rawData);

                    foreach (var message in extractedMessages)
                    {
                        string trimmedMsg = message.Trim();
                        if (string.IsNullOrWhiteSpace(trimmedMsg) || trimmedMsg == "PING") continue;

                        // Fase 5: Notificar recepción para desacoplamiento
                        NetworkingEvents.RaiseMessageReceived(new NetworkMessageEventArgs 
                        { 
                            UserId = currentUserId, 
                            RawMessage = trimmedMsg 
                        });

                        if (trimmedMsg.StartsWith("SYNC_CHAR|"))
                        {
                            var parts = trimmedMsg.Split('|');
                            if (parts.Length >= 3)
                            {
                                string userId = parts[1].Trim(); 
                                if (string.IsNullOrEmpty(userId)) continue;

                                string charId = parts[2].Trim();
                                string username = parts.Length >= 4 ? parts[3].Trim() : "Jugador";

                                // SISTEMA DE EXPULSIÓN DE DUPLICADOS
                                lock (_syncLock)
                                {
                                    if (_userIdToClient.TryGetValue(userId, out var oldClient))
                                    {
                                        if (oldClient != client)
                                        {
                                            Debug.Log($"[TCP Server] UID {userId} ya conectado en otro socket. Expulsando antiguo.");
                                            try { oldClient.Close(); } catch { }
                                            _userIdToClient.Remove(userId);
                                        }
                                    }
                                    _userIdToClient[userId] = client;
                                }

                                string finalUsername = username;
                                int finalLevel = 1;

                                if (LobbyManager.Instance != null)
                                {
                                    UserData userData = await LobbyManager.Instance.FetchUserDataAsync(userId);
                                    if (userData != null)
                                    {
                                        if (userData.username != "Jugador") finalUsername = userData.username;
                                        finalLevel = userData.level;
                                    }
                                }

                                if (currentUserId == null) currentUserId = userId;
                                 
                                lock (_syncLock)
                                {
                                    sessionUserIds.Add(userId);
                                    if (!_activePlayerIds.Contains(userId))
                                    {
                                        _activePlayerIds.Add(userId);
                                        _idToName[userId] = finalUsername;
                                        _idToCharId[userId] = charId;
                                        _idToLevel[userId] = finalLevel;
                                        
                                        // Guardar en el registro persistente de la sesión
                                        _sessionNames[userId] = finalUsername;
                                        _sessionChars[userId] = charId;
                                        _sessionLevels[userId] = finalLevel;

                                        SyncParticipantsToFirestore();
                                        RefreshUIPlayerList();
                                    }
                                    else
                                    {
                                        // Actualizar datos si ya estaba pero los cambió (ej: reconexión o cambio de skin)
                                        _idToName[userId] = finalUsername;
                                        _idToCharId[userId] = charId;
                                        _idToLevel[userId] = finalLevel;
                                        
                                        _sessionNames[userId] = finalUsername;
                                        _sessionChars[userId] = charId;
                                        _sessionLevels[userId] = finalLevel;

                                        SyncParticipantsToFirestore();
                                        RefreshUIPlayerList();
                                    }
                                }

                                if (PlayerManager.Instance != null) {
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => {
                                        PlayerManager.Instance.HandlePlayerJoin(userId, charId, finalUsername, finalLevel);
                                    });
                                }
                            }
                        }
                        else if (trimmedMsg.StartsWith("READY|"))
                        {
                            var parts = trimmedMsg.Split('|');
                            if (parts.Length >= 3 && LobbyManager.Instance != null)
                            {
                                string userId = parts[1].Trim();
                                bool isReady = parts[2].ToLower().Contains("true");
                                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                                    _ = LobbyManager.Instance.ToggleReadyAsync(_currentRoomCode, userId, isReady);
                                });
                            }
                        }
                        else if (trimmedMsg.StartsWith("VOTE|"))
                        {
                            var parts = trimmedMsg.Split('|');
                            if (parts.Length >= 2 && LobbyManager.Instance != null)
                            {
                                string gameId = parts[1].Trim();
                                string userId = currentUserId ?? client.Client.RemoteEndPoint.ToString();
                                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                                    LobbyManager.Instance.HandleVote(userId, gameId);
                                });
                            }
                        }
                        else if (trimmedMsg.StartsWith("LEAVE|"))
                        {
                            var parts = trimmedMsg.Split('|');
                            if (parts.Length >= 2)
                            {
                                string userId = parts[1].Trim();
                                Debug.Log($"[TCP Server] Mensaje LEAVE recibido para {userId}. Cerrando conexión.");
                                break; // Rompe el bucle para ir al finally, limpiar y cerrar socket
                            }
                        }
                        else if (trimmedMsg.StartsWith("{") && trimmedMsg.EndsWith("}"))
                        {
                            try 
                            {
                                var input = JsonUtility.FromJson<ControllerInput>(trimmedMsg);
                                if (input != null)
                                {
                                    string uid = string.IsNullOrEmpty(input.userId) ? currentUserId : input.userId.Trim();
                                    if (uid != null && PlayerManager.Instance != null)
                                    {
                                        string capturedUid = uid;
                                        string capturedMsg = trimmedMsg;
                                        UnityMainThreadDispatcher.Instance().Enqueue(() => {
                                            PlayerManager.Instance.HandleControllerInput(capturedUid, capturedMsg);
                                        });
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (System.IO.IOException ex)
            {
                Debug.LogWarning($"[TCP Server] Cierre de I/O de cliente: {ex.Message}");
            }
            finally
            {
                bool shouldReturnToLobby = false;
                string roomToClose = _currentRoomCode;

                lock (_syncLock)
                {
                    Debug.Log($"[TCP Server] Cleanup FINALLY. UIDs: {sessionUserIds.Count}");
                    foreach (var uid in sessionUserIds)
                    {
                        if (string.IsNullOrEmpty(uid)) continue;

                        if (_userIdToClient.TryGetValue(uid, out var ownerClient) && ownerClient == client)
                        {
                            _userIdToClient.Remove(uid);
                        }

                        _activePlayerIds.Remove(uid);
                        _idToName.Remove(uid);
                        
                        if (PlayerManager.Instance != null)
                        {
                            string capturedUid = uid;
                            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                                PlayerManager.Instance.HandlePlayerDisconnect(capturedUid);
                            });
                        }
                    }

                    if (_activePlayerIds.Count == 0 && !string.IsNullOrEmpty(SceneManager.GetActiveScene().name))
                    {
                        string sceneName = SceneManager.GetActiveScene().name.ToLower();
                        if (!sceneName.Contains("lobby") && !sceneName.Contains("menu")) shouldReturnToLobby = true;
                    }

                }
                // Sync fuera del lock para evitar deadlocks en llamadas async
                SyncParticipantsToFirestore();
                RefreshUIPlayerList();
                
                _clients.Remove(client);
                lock (_syncLock) { _clientBuffers.Remove(client); }
                client.Close();

                    if (shouldReturnToLobby && LobbyManager.Instance != null && !string.IsNullOrEmpty(roomToClose))
                    {
                        _ = LobbyManager.Instance.ReturnToLobbyAsync(roomToClose);
                    }
                    NetworkingEvents.RaiseDisconnected(currentUserId);
                    Debug.Log("[TCP Server] Socket cerrado.");
            }
        }

        /// <summary>
        /// Registra un bot localmente para que aparezca en el lobby de Firestore y en la App.
        /// </summary>
        public void RegisterBot(string botId, string charId, string username, int level)
        {
            if (!_activePlayerIds.Contains(botId))
            {
                _activePlayerIds.Add(botId);
            }
            
            _idToName[botId] = username;
            _idToCharId[botId] = charId;
            _idToLevel[botId] = level;
            
            _sessionNames[botId] = username;
            _sessionChars[botId] = charId;
            _sessionLevels[botId] = level;

            SyncParticipantsToFirestore();
            RefreshUIPlayerList();
        }

        private void SyncParticipantsToFirestore()
        {
            if (LobbyManager.Instance != null && !string.IsNullOrEmpty(_currentRoomCode))
            {
                var names = new Dictionary<string, string>(_sessionNames);
                var chars = new Dictionary<string, string>(_sessionChars);
                var levels = new Dictionary<string, int>(_sessionLevels);
                
                _ = LobbyManager.Instance.UpdateParticipantsAsync(_currentRoomCode, new List<string>(_activePlayerIds), names, chars, levels);
            }
        }

        void OnApplicationQuit() => StopServer();

        public void StopServer()
        {
            _isRunning = false;

            // Notificar a las Apps y cerrar la sala en Firestore ANTES de destruir sockets
            if (LobbyManager.Instance != null && !string.IsNullOrEmpty(_currentRoomCode))
            {
                BroadcastToAll("STOP_SESSION");

                var finalNames = new Dictionary<string, string>(_sessionNames);
                var finalChars = new Dictionary<string, string>(_sessionChars);
                var finalLevels = new Dictionary<string, int>(_sessionLevels);
                var finalScores = new Dictionary<string, int>(LobbyManager.Instance.SessionScores);
                var finalGames = new List<string>(LobbyManager.Instance.SessionPlayedGames);
                
                _ = LobbyManager.Instance.CloseLobbyAsync(_currentRoomCode, finalNames, finalChars, finalScores, finalLevels, finalGames);
                _currentRoomCode = null;
            }

            _listener?.Stop();

            // Cerrar cada socket individualmente para que uno roto no bloquee el resto
            List<TcpClient> toClose;
            lock (_syncLock)
            {
                toClose = new List<TcpClient>(_clients);
                _clients.Clear();
                _clientBuffers.Clear();
                _userIdToClient.Clear();
                _activePlayerIds.Clear();
                _sessionNames.Clear();
                _sessionChars.Clear();
                _sessionLevels.Clear();
            }
            foreach (var c in toClose)
            {
                try { c?.Close(); } catch { }
            }
            Debug.Log("[TCP Server] Servidor detenido. Recursos liberados.");
        }

        public void RefreshUIPlayerList()
        {
            if (lobbyUI == null) return;
            
            List<string> names = new List<string>();
            lock (_syncLock)
            {
                var mgr = LobbyManager.Instance;
                foreach (var id in _activePlayerIds)
                {
                    string displayName = _idToName.TryGetValue(id, out string n) ? n : "Jugador";
                    
                    // Intentar sacar nivel de PlayerManager o del cache de LobbyManager
                    int level = 1;
                    if (PlayerManager.Instance != null)
                    {
                        level = PlayerManager.Instance.GetPlayerLevel(id);
                    }

                    // Sacar puntos de la sesión actual del cache de LobbyManager
                    int score = 0;
                    if (mgr != null && mgr.SessionScores.TryGetValue(id, out int s))
                    {
                        score = s;
                    }

                    names.Add($"[Lv. {level}] {displayName} - {score} pts");
                }
            }
            // Ejecutar en hilo principal de Unity
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                if (lobbyUI != null) lobbyUI.UpdatePlayerList(names);
            });
        }

        public void BroadcastToAll(string message)
        {
            if (!_isRunning) return;
            byte[] bytes = Encoding.UTF8.GetBytes(message + "\n");
            List<TcpClient> snapshot;
            lock (_syncLock) { snapshot = new List<TcpClient>(_clients); }

            foreach (var client in snapshot)
            {
                if (client == null || !client.Connected) continue;
                try
                {
                    // Timeout de escritura de 2s para no bloquear en sockets zombie
                    client.SendTimeout = 2000;
                    client.GetStream().Write(bytes, 0, bytes.Length);
                }
                catch (System.IO.IOException ex)
                {
                    Debug.LogWarning($"[TCP Server] BroadcastToAll: fallo de I/O en cliente ({ex.Message}). Ignorando.");
                }
            }
        }
    }
}
