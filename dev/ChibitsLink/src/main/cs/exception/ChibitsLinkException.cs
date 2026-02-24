using System;

namespace ChibitsLink.main.cs.exception;

/// <summary>
/// Excepción base de la aplicación ChibitsLink.
/// Todas las excepciones del dominio heredan de esta clase.
/// </summary>
public class ChibitsLinkException : Exception
{
    public ChibitsLinkException(string message) : base(message) { }

    public ChibitsLinkException(string message, Exception innerException) : base(message, innerException) { }
}
