namespace ChibitsLink.main.cs.exception;

/// <summary>
/// Errores de sesión o estado de juego inválido.
/// </summary>
public class SessionException : ChirbitsException
{
    public SessionException(string message) : base(message) { }
}
