using System;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using ChibitsLink.main.cs.service;

namespace ChibitsLink.main.cs.view;

public partial class JoinRoomPage : ContentPage
{
    private readonly AccountService _accountService;

    public JoinRoomPage(AccountService accountService)
    {
        InitializeComponent();
        _accountService = accountService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var user = _accountService.GetCurrentUser();
        if (user == null)
        {
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        string code = RoomCodeEntry.Text;
        if (string.IsNullOrEmpty(code) || code.Length != 6)
        {
            await DisplayAlert("Ups", "El código debe ser de 6 dígitos mágicos.", "Vale");
            return;
        }

        LoadingIndicator.IsRunning = true;
        
        // Simular conexión
        await Task.Delay(1500);
        
        LoadingIndicator.IsRunning = false;
        
        // Navegar al Lobby
        await Shell.Current.GoToAsync($"LobbyPage?code={code}");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
