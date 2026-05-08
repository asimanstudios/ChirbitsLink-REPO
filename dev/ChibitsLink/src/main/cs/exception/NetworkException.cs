using System;

namespace ChibitsLink.main.cs.exception;

/// <summary>
/// Errores de conexión TCP con el servidor Unity.
/// </summary>
public class NetworkException : ChirbitsException
{
    public NetworkException(string message) : base(message) { }

    public NetworkException(string message, Exception inner) : base(message, inner) { }
}
