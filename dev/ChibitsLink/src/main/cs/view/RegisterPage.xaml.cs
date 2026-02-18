namespace ChibitsLink.main.cs.view;

using System;
using Microsoft.Maui.Controls;
using ChibitsLink.main.cs.controller;
using System.Threading.Tasks;

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

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string realName = RealNameEntry.Text;
        string username = UsernameEntry.Text;
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(realName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Por favor, completa todos los campos", "Vale");
            return;
        }

        await _controller.Register(this, realName, username, email, password);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    public async Task NavigateToLogin()
    {
        await DisplayAlert("Success", "Account created! Please login.", "OK");
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
