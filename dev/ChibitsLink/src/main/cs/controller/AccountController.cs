using System;
using System.Threading.Tasks;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.view;

namespace ChibitsLink.main.cs.controller;

/// <summary>
/// Controla el sistema de autenticación de usuarios.
/// Intermediario entre las páginas de login/registro y <see cref="AccountService"/>.
/// </summary>
public class AccountController
{
    private readonly AccountService _service;

    public AccountController(AccountService service)
    {
        _service = service;
    }

    /// <summary>
    /// Gestiona el intento de inicio de sesión. Actualiza la UI de la vista con el resultado.
    /// </summary>
    public async Task Login(LoginPage view, string username, string password)
    {
        view.ShowLoading(true);
        var (success, errorMessage) = await _service.Login(username, password);
        view.ShowLoading(false);

        if (success)
        {
            await view.NavigateToSelection();
        }
        else
        {
            await view.DisplayAlert("Error", errorMessage ?? "Credenciales no válidas.", "OK");
        }
    }

    /// <summary>
    /// Gestiona el intento de registro. Actualiza la UI de la vista con el resultado.
    /// </summary>
    public async Task Register(RegisterPage view, string realName, string username, string email, string password)
    {
        view.ShowLoading(true);
        var (success, errorMessage) = await _service.RegisterAsync(realName, username, email, password);
        view.ShowLoading(false);

        if (success)
        {
            await view.NavigateToLogin();
        }
        else
        {
            await view.DisplayAlert("Error", errorMessage ?? "Error al registrar la cuenta.", "OK");
        }
    }
}