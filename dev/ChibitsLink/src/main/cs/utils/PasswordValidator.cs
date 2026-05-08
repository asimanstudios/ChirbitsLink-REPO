using System.Text.RegularExpressions;

namespace ChibitsLink.main.cs.utils;

/// <summary>
/// Valida que una contraseña cumpla los requisitos mínimos de seguridad de la aplicación.
/// </summary>
public static class PasswordValidator
{
    /// <summary>
    /// Valida que la contraseña tenga al menos 8 caracteres, 1 número, 1 mayúscula y 1 carácter especial.
    /// </summary>
    public static (bool IsValid, string Message) Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "La contraseña no puede estar vacía.");

        if (password.Length < 8)
            return (false, "La contraseña debe tener al menos 8 caracteres.");

        if (!Regex.IsMatch(password, @"[0-9]"))
            return (false, "La contraseña debe contener al menos un número.");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return (false, "La contraseña debe contener al menos una mayúscula.");

        if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            return (false, "La contraseña debe contener al menos un carácter especial.");

        return (true, string.Empty);
    }
}
