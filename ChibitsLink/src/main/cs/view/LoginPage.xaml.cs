namespace ChibitsLink.main.cs.view;

using System;
using Microsoft.Maui.Controls;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.controller;

public partial class LoginPage : ContentPage
{
    private readonly AccountController _controller;

    public LoginPage(AccountController controller)
    {
        InitializeComponent();
        _controller = controller;
    }

    public event Action<string, string>? OnLoginRequested;

    public void ShowLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter credentials", "OK");
            return;
        }

        await _controller.Login(this, username, password);
    }

    public async Task NavigateToSelection()
    {
        await Shell.Current.GoToAsync("//MainMenuPage");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("RegisterPage");
    }
}
