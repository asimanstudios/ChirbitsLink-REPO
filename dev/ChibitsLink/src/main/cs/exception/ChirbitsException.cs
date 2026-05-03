using System;

namespace ChibitsLink.main.cs.exception
{
    /// <summary>
    /// Clase base para todas las excepciones del ecosistema Chirbits.
    /// </summary>
    public class ChirbitsException : Exception
    {
        public ChirbitsException(string message) : base(message) { }
        public ChirbitsException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Errores relacionados con la base de datos (Firestore).
    /// </summary>
    public class DatabaseException : ChirbitsException
    {
        public string Collection { get; }
        public string DocumentId { get; }

        public DatabaseException(string message, string collection = "", string docId = "") 
            : base(message)
        {
            Collection = collection;
            DocumentId = docId;
        }

        public DatabaseException(string message, Exception inner, string collection = "", string docId = "") 
            : base(message, inner)
        {
            Collection = collection;
            DocumentId = docId;
        }
    }

    /// <summary>
    /// Excepción lanzada cuando un documento no se encuentra en Firestore.
    /// </summary>
    public class RecordNotFoundException : DatabaseException
    {
        public RecordNotFoundException(string collection, string id)
            : base($"El registro con ID '{id}' no fue encontrado en la colección '{collection}'.", collection, id) { }
    }

    /// <summary>
    /// Errores de autenticación (Firebase Auth).
    /// </summary>
    public class AuthException : ChirbitsException
    {
        public string ErrorCode { get; }
        public AuthException(string message, string errorCode = "") : base(message) 
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// Errores de conexión TCP con el servidor Unity.
    /// </summary>
    public class NetworkException : ChirbitsException
    {
        public NetworkException(string message) : base(message) { }
        public NetworkException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Errores de sesión o estado de juego inválido.
    /// </summary>
    public class SessionException : ChirbitsException
    {
        public SessionException(string message) : base(message) { }
    }
}
