using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ChibitsLink.main.cs.controller;

namespace ChibitsLink.main.cs.view;

/// <summary>
/// Página de registro de nuevos usuarios.
/// Recoge nombre completo, alias, email y contraseña (sin confirmación — XAML no tiene ese campo).
/// </summary>
public partial class RegisterPage : ContentPage
{
    private readonly AccountController _controller;

    public RegisterPage(AccountController controller)
    {
        InitializeComponent();
        _controller = controller;
    }

    public void ShowLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
    }

    /// <summary>Navega a la pantalla de login tras un registro exitoso.</summary>
    public async Task NavigateToLogin()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string realName = RealNameEntry.Text;
        string username = UsernameEntry.Text;
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(realName) || string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Campos vacíos", "Por favor rellena todos los campos.", "OK");
            return;
        }

        await _controller.Register(this, realName, username, email, password);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
