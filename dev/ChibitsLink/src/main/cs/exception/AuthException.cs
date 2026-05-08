namespace ChibitsLink.main.cs.exception;

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
