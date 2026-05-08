using System;

namespace ChibitsLink.main.cs.exception;

// ⚠️ DUPLICADO — Esta clase cumple el mismo rol que ChirbitsException.
// Usar ChirbitsException como base en su lugar.
// Se mantiene para compatibilidad hasta una limpieza completa.

/// <summary>
/// Excepción base de la aplicación ChibitsLink.
/// <br/>⚠️ <b>Obsoleto</b>: usar <see cref="ChirbitsException"/> en su lugar.
/// </summary>
[Obsolete("Usar ChirbitsException en su lugar. Esta clase es un duplicado.")]
public class ChibitsLinkException : Exception
{
    public ChibitsLinkException(string message) : base(message) { }

    public ChibitsLinkException(string message, Exception innerException) : base(message, innerException) { }
}
