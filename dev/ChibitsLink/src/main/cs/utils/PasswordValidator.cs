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
        bool isValid = false;
        string message = string.Empty;

        if (string.IsNullOrWhiteSpace(password))
        {
            message = "La contraseña no puede estar vacía.";
        }
        else if (password.Length < 8)
        {
            message = "La contraseña debe tener al menos 8 caracteres.";
        }
        else if (!Regex.IsMatch(password, @"[0-9]"))
        {
            message = "La contraseña debe contener al menos un número.";
        }
        else if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            message = "La contraseña debe contener al menos una mayúscula.";
        }
        else if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
        {
            message = "La contraseña debe contener al menos un carácter especial.";
        }
        else
        {
            isValid = true;
        }

        return (isValid, message);
    }
}
