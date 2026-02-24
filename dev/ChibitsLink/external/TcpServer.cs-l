using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine; // Asumiendo que se usa en Unity

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

            try
            {
                while (_isRunning && client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    // Procesar mensaje
                    if (message.StartsWith("SYNC_CHAR|"))
                    {
                        var parts = message.Split('|');
                        if (parts.Length >= 3)
                        {
                            currentUserId = parts[1];
                            string charId = parts[2];
                            playerManager.HandlePlayerJoin(currentUserId, charId);
                        }
                    }
                    else if (message.Contains("\"type\":")) // Probablemente JSON de mando
                    {
                        // Aquí podrías usar un parser de JSON
                        // Para simplicidad en este script de ejemplo:
                        playerManager.HandleControllerInput(currentUserId, message);
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                if (currentUserId != null) playerManager.HandlePlayerDisconnect(currentUserId);
                _clients.Remove(client);
                client.Close();
            }
        }

        void OnApplicationQuit() => StopServer();

        public void StopServer()
        {
            _isRunning = false;
            _listener?.Stop();
            foreach (var client in _clients) client.Close();
            _clients.Clear();
        }
    }
}
