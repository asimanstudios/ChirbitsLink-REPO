using System;

namespace Chirbits.Core.Exceptions
{
    /// <summary>
    /// Excepción base para el juego Chirbits.
    /// Representa errores generales del sistema de juego.
    /// </summary>
    public class ChirbitsGameException : Exception
    {
        /// <summary>
        /// Inicializa una nueva instancia de ChirbitsGameException.
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        public ChirbitsGameException(string message) : base(message) { }
        
        /// <summary>
        /// Inicializa una nueva instancia con excepción interna.
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        /// <param name="inner">Excepción interna</param>
        public ChirbitsGameException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Excepción para errores de sincronización con Firestore.
    /// Ocurre cuando fallan operaciones de base de datos.
    /// </summary>
    public class FirestoreSyncException : ChirbitsGameException
    {
        /// <summary>Colección afectada por el error</summary>
        public string Collection { get; }
        
        /// <summary>
        /// Inicializa una nueva instancia de FirestoreSyncException.
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        /// <param name="collection">Colección afectada</param>
        public FirestoreSyncException(string message, string collection) : base(message)
        {
            Collection = collection;
        }
    }

    /// <summary>
    /// Excepción para errores de protocolo de socket.
    /// Ocurre cuando hay problemas con la comunicación de red.
    /// </summary>
    public class SocketProtocolException : ChirbitsGameException
    {
        /// <summary>Mensaje crudo que causó el error</summary>
        public string RawMessage { get; }
        
        /// <summary>
        /// Inicializa una nueva instancia de SocketProtocolException.
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        /// <param name="rawMessage">Mensaje crudo del error</param>
        public SocketProtocolException(string message, string rawMessage) : base(message)
        {
            RawMessage = rawMessage;
        }
    }

    /// <summary>
    /// Excepción para errores de lógica de sesión.
    /// Ocurre cuando hay problemas con la gestión de sesiones de juego.
    /// </summary>
    public class SessionLogicException : ChirbitsGameException
    {
        /// <summary>
        /// Inicializa una nueva instancia de SessionLogicException.
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        public SessionLogicException(string message) : base(message) { }
    }
}
