using System;

namespace ChibitsLink.main.cs.exception;

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
