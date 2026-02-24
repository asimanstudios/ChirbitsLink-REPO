using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ChibitsLink.main.cs.controller;
using ChibitsLink.main.cs.service;

namespace ChibitsLink.main.cs.view;

/// <summary>
/// Página de inicio de sesión. Autentica al usuario con email y contraseña.
/// </summary>
public partial class LoginPage : ContentPage
{
    private readonly AccountController _controller;
    private readonly AccountService _accountService;

    public LoginPage(AccountController controller, AccountService accountService)
    {
        InitializeComponent();
        _controller = controller;
        _accountService = accountService;
    }

    public void ShowLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
    }

    /// <summary>Navega al menú principal tras un login exitoso.</summary>
    public async Task NavigateToSelection()
    {
        await Shell.Current.GoToAsync("//MainMenuPage");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        bool isActive = await _accountService.IsSessionActiveAsync();
        if (isActive)
        {
            await Shell.Current.GoToAsync("//MainMenuPage");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Campos vacíos", "Introduce tu correo y contraseña.", "OK");
            return;
        }

        await _controller.Login(this, username, password);
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("RegisterPage");
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Recuperar contraseña", "Contacta con soporte para recuperar tu cuenta.", "OK");
    }
}
