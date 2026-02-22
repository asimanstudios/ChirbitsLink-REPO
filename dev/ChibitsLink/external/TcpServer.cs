using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace ChibitsLink.GameSide
{
    /// <summary>
    /// Servidor TCP (ServerSocket) para el lado del juego.
    /// Maneja conexiones de la App cliente y retransmisión de mensajes.
    /// </summary>
    public class TcpServer : MonoBehaviour
    {
        public int port = 11000;
        public PlayerManager playerManager; // Asignar en el inspector
        
        private TcpListener _listener;
        private List<TcpClient> _clients = new List<TcpClient>();
        private bool _isRunning;
        
        // Diccionario para rastrear habitaciones y sus jugadores
        private Dictionary<string, RoomInfo> _rooms = new Dictionary<string, RoomInfo>();
        
        private class RoomInfo
        {
            public string RoomCode { get; set; } = "";
            public List<string> PlayerIds { get; set; } = new List<string>();
            public int MaxPlayers { get; set; } = 4;
            public DateTime CreatedAt { get; set; } = DateTime.Now;
        }

        void Start()
        {
            if (playerManager == null) playerManager = GetComponent<PlayerManager>();
            StartServer();
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
            catch (Exception ex)
            {
                Debug.LogError($"[TCP Server] Error al iniciar: {ex.Message}");
            }
        }

        private async Task AcceptClientsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _clients.Add(client);
                    _ = HandleClientAsync(client);
                }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    Debug.LogError($"[TCP Server] Error: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];
            string currentUserId = null;
            string currentRoomCode = null;

            try
            {
                while (_isRunning && client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log($"[TCP Server] Mensaje recibido: {message}");
                    
                    // Procesar mensaje
                    if (message.StartsWith("SYNC_CHAR|"))
                    {
                        var parts = message.Split('|');
                        // Formato: SYNC_CHAR|roomCode|userId|charId
                        if (parts.Length >= 4)
                        {
                            currentRoomCode = parts[1];
                            currentUserId = parts[2];
                            string charId = parts[3];
                            
                            // Validar habitación
                            if (!ValidateRoom(currentRoomCode, currentUserId))
                            {
                                // Enviar mensaje de error al cliente
                                byte[] errorMsg = Encoding.UTF8.GetBytes("ERROR|Sala no válida o llena");
                                await stream.WriteAsync(errorMsg, 0, errorMsg.Length);
                                Debug.LogWarning($"[TCP Server] Error: Sala {currentRoomCode} no válida o llena para usuario {currentUserId}");
                                continue;
                            }
                            
                            // Agregar jugador a la sala
                            AddPlayerToRoom(currentRoomCode, currentUserId);
                            
                            // Notificar al PlayerManager
                            playerManager.HandlePlayerJoin(currentUserId, charId);
                            
                            // Confirmar conexión exitosa
                            byte[] successMsg = Encoding.UTF8.GetBytes("OK|" + currentRoomCode);
                            await stream.WriteAsync(successMsg, 0, successMsg.Length);
                            
                            Debug.Log($"[TCP Server] Jugador {currentUserId} unido a la sala {currentRoomCode}");
                        }
                        else
                        {
                            Debug.LogWarning($"[TCP Server] Formato de mensaje inválido: {message}");
                        }
                    }
                    else if (message.StartsWith("CHECK_ROOM|"))
                    {
                        // Verificar estado de una sala
                        var parts = message.Split('|');
                        if (parts.Length >= 2)
                        {
                            string roomCode = parts[1];
                            if (_rooms.TryGetValue(roomCode, out RoomInfo room))
                            {
                                string response = $"ROOM_STATUS|{roomCode}|{room.PlayerIds.Count}|{room.MaxPlayers}";
                                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                            }
                            else
                            {
                                byte[] errorMsg = Encoding.UTF8.GetBytes("ROOM_STATUS|NOT_FOUND|0|0");
                                await stream.WriteAsync(errorMsg, 0, errorMsg.Length);
                            }
                        }
                    }
                    else if (message.StartsWith("CREATE_ROOM|"))
                    {
                        // Crear una nueva sala desde el juego
                        var parts = message.Split('|');
                        if (parts.Length >= 2)
                        {
                            string roomCode = parts[1];
                            if (!_rooms.ContainsKey(roomCode))
                            {
                                _rooms[roomCode] = new RoomInfo 
                                { 
                                    RoomCode = roomCode, 
                                    MaxPlayers = 4,
                                    PlayerIds = new List<string>()
                                };
                                Debug.Log($"[TCP Server] Sala {roomCode} creada");
                            }
                        }
                    }
                    else if (message.Contains("\"type\":")) // Probablemente JSON de mando
                    {
                        // Reenviar input al PlayerManager
                        if (currentUserId != null)
                        {
                            playerManager.HandleControllerInput(currentUserId, message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TCP Server] Error manejando cliente: {ex.Message}");
            }
            finally
            {
                // Limpiar jugador al desconectar
                if (currentUserId != null && currentRoomCode != null)
                {
                    RemovePlayerFromRoom(currentRoomCode, currentUserId);
                    playerManager.HandlePlayerDisconnect(currentUserId);
                }
                _clients.Remove(client);
                client.Close();
            }
        }
        
        /// <summary>
        /// Valida que la sala exista y no esté llena
        /// </summary>
        private bool ValidateRoom(string roomCode, string userId)
        {
            if (string.IsNullOrEmpty(roomCode))
            {
                Debug.LogWarning("[TCP Server] Código de sala vacío");
                return false;
            }
            
            if (!_rooms.ContainsKey(roomCode))
            {
                Debug.LogWarning($"[TCP Server] Sala {roomCode} no existe");
                return false;
            }
            
            var room = _rooms[roomCode];
            if (room.PlayerIds.Count >= room.MaxPlayers)
            {
                Debug.LogWarning($"[TCP Server] Sala {roomCode} está llena ({room.PlayerIds.Count}/{room.MaxPlayers})");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Agrega un jugador a la sala
        /// </summary>
        private void AddPlayerToRoom(string roomCode, string userId)
        {
            if (_rooms.TryGetValue(roomCode, out RoomInfo room))
            {
                if (!room.PlayerIds.Contains(userId))
                {
                    room.PlayerIds.Add(userId);
                    Debug.Log($"[TCP Server] Jugador {userId} agregado a sala {roomCode}. Total: {room.PlayerIds.Count}/{room.MaxPlayers}");
                }
            }
        }
        
        /// <summary>
        /// Remueve un jugador de la sala
        /// </summary>
        private void RemovePlayerFromRoom(string roomCode, string userId)
        {
            if (_rooms.TryGetValue(roomCode, out RoomInfo room))
            {
                room.PlayerIds.Remove(userId);
                Debug.Log($"[TCP Server] Jugador {userId} removido de sala {roomCode}. Total: {room.PlayerIds.Count}");
                
                // Eliminar sala si está vacía
                if (room.PlayerIds.Count == 0)
                {
                    _rooms.Remove(roomCode);
                    Debug.Log($"[TCP Server] Sala {roomCode} eliminada (vacía)");
                }
            }
        }
        
        /// <summary>
        /// Obtiene el número de jugadores en una sala
        /// </summary>
        public int GetPlayerCount(string roomCode)
        {
            if (_rooms.TryGetValue(roomCode, out RoomInfo room))
            {
                return room.PlayerIds.Count;
            }
            return 0;
        }

        void OnApplicationQuit() => StopServer();

        public void StopServer()
        {
            _isRunning = false;
            _listener?.Stop();
            foreach (var client in _clients) client.Close();
            _clients.Clear();
            _rooms.Clear();
        }
    }
}
