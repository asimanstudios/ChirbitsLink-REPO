using System;
using UnityEngine;

namespace Chirbits.Core.Networking
{
    /// <summary>
    /// Argumentos de evento para mensajes de red.
    /// Contiene información sobre mensajes recibidos de clientes.
    /// </summary>
    public class NetworkMessageEventArgs : EventArgs
    {
        /// <summary>ID del usuario que envió el mensaje</summary>
        public string UserId { get; set; }
        /// <summary>Mensaje sin procesar</summary>
        public string RawMessage { get; set; }
        /// <summary>Comando del mensaje</summary>
        public string Command { get; set; }
        /// <summary>Payload del mensaje</summary>
        public string[] Payload { get; set; }
    }

    /// <summary>
    /// Gestor centralizado de eventos de red.
    /// Proporciona eventos estáticos para comunicación de red.
    /// </summary>
    /// <remarks>
    /// Utiliza eventos estáticos para acceso global.
    /// Facilita la comunicación entre componentes de red.
    /// </remarks>
    public static class NetworkingEvents
    {
        /// <summary>Evento cuando un cliente se conecta</summary>
        public static event Action<string, string> OnClientConnected;
        /// <summary>Evento cuando un cliente se desconecta</summary>
        public static event Action<string> OnClientDisconnected;
        /// <summary>Evento cuando se recibe un mensaje</summary>
        public static event Action<NetworkMessageEventArgs> OnMessageReceived;

        /// <summary>
        /// Dispara el evento de cliente conectado.
        /// </summary>
        /// <param name="userId">ID del cliente conectado</param>
        /// <param name="endpoint">Endpoint de conexión</param>
        public static void RaiseConnected(string userId, string endpoint) => OnClientConnected?.Invoke(userId, endpoint);
        
        /// <summary>
        /// Dispara el evento de cliente desconectado.
        /// </summary>
        /// <param name="userId">ID del cliente desconectado</param>
        public static void RaiseDisconnected(string userId) => OnClientDisconnected?.Invoke(userId);
        
        /// <summary>
        /// Dispara el evento de mensaje recibido.
        /// </summary>
        /// <param name="args">Argumentos del mensaje recibido</param>
        public static void RaiseMessageReceived(NetworkMessageEventArgs args) => OnMessageReceived?.Invoke(args);
    }
}
