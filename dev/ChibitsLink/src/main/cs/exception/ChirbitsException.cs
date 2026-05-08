using System;

namespace ChibitsLink.main.cs.exception;

/// <summary>
/// Clase base para todas las excepciones del ecosistema Chirbits.
/// </summary>
public class ChirbitsException : Exception
{
    public ChirbitsException(string message) : base(message) { }

    public ChirbitsException(string message, Exception inner) : base(message, inner) { }
}
