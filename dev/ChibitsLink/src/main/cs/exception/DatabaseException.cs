using System;

namespace ChibitsLink.main.cs.exception;

/// <summary>
/// Excepción lanzada cuando ocurre un error en las operaciones de base de datos (Firestore).
/// </summary>
public class DatabaseException : ChibitsLinkException
{
    public DatabaseException(string message) : base(message) { }

    public DatabaseException(string message, Exception innerException) : base(message, innerException) { }
}
