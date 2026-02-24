using System;
using System.Threading.Tasks;
using ChibitsLink.main.cs.service;

namespace ChibitsLink.main.cs.view;

/// <summary>
/// Página de unión a sala mediante código de 6 dígitos.
/// Valida el código contra Firestore antes de navegar al Lobby.
/// </summary>
public partial class JoinRoomPage : ContentPage
{
    private readonly AccountService _accountService;
    private readonly GameService _gameService;

    public JoinRoomPage(AccountService accountService, GameService gameService)
    {
        InitializeComponent();
        _accountService = accountService;
        _gameService = gameService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var user = _accountService.GetCurrentUser();
        if (user == null)
        {
            await Shell.Current.GoToAsync("//LoginPage");
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

        try
        {
            bool isValid = await _gameService.ValidateLobbyAsync(code);
            LoadingIndicator.IsRunning = false;

            if (isValid)
            {
                await Shell.Current.GoToAsync($"LobbyPage?code={code}");
            }
            else
            {
                await DisplayAlert("Error", "La sala no existe o el código es incorrecto.", "Vale");
            }
        }
        catch (Exception)
        {
            LoadingIndicator.IsRunning = false;
            await DisplayAlert("Error de Conexión", "No hemos podido verificar la sala. Comprueba tu conexión.", "Vale");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
