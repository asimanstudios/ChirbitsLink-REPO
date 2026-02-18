namespace ChibitsLink.main.cs.controller;
/*
    Controla al sistema de usuarios
*/
using System;
using ChibitsLink.main.cs.view;
using ChibitsLink.main.cs.service;
using System.Threading.Tasks;

public class AccountController
{
    private readonly AccountService _service;

    public AccountController(AccountService service)
    {
        _service = service;
    }

    public async Task Login(LoginPage view, string username, string password)
    {
        view.ShowLoading(true);
        var (success, errorMessage) = await _service.Login(username, password);
        view.ShowLoading(false);

        if (success)
        {
            await view.NavigateToSelection(); // This should be renamed or redirected
        }
        else
        {
            await view.DisplayAlert("Error", errorMessage ?? "Invalid credentials", "OK");
        }
    }

    public async Task Register(RegisterPage view, string realName, string username, string email, string password)
    {
        view.ShowLoading(true);
        var (success, errorMessage) = await _service.Register(realName, username, email, password);
        view.ShowLoading(false);

        if (success)
        {
            await view.NavigateToLogin();
        }
        else
        {
            await view.DisplayAlert("Error", errorMessage ?? "Registration failed", "OK");
        }
    }
}