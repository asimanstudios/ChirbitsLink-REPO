using System;
using System.Threading.Tasks;
using ChibitsLink.main.cs.service;

namespace ChibitsLink.main.cs.view;

/// <summary>
/// Página de unión a sala mediante código de 6 dígitos.
/// Valida el código contra Firestore antes de navegar al LobbyPage.
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
        if (code != null)
        {
            code = code.Trim().ToUpperInvariant();
        }

        bool isCodeValid = !string.IsNullOrEmpty(code) && code.Length == 6;

        if (!isCodeValid)
        {
            await DisplayAlert("Ups", "El código debe ser de exactamente 6 dígitos mágicos.", "Vale");
        }
        else
        {
            LoadingIndicator.IsRunning = true;

            try
            {
                bool lobbyExists = await _gameService.ValidateLobbyAsync(code!);
                LoadingIndicator.IsRunning = false;

                if (lobbyExists)
                {
                    await Shell.Current.GoToAsync($"//LobbyPage?code={code}");
                }
                else
                {
                    await DisplayAlert("Error", "La sala no existe o el código es incorrecto.", "Vale");
                }
            }
            catch (ChibitsLink.main.cs.exception.DatabaseException dbEx)
            {
                LoadingIndicator.IsRunning = false;
                string detail = dbEx.InnerException != null ? dbEx.InnerException.Message : dbEx.Message;
                await DisplayAlert("Error de Conexión", $"No hemos podido verificar la sala: {detail}", "Vale");
            }
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainMenuPage");
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync("//MainMenuPage"));
        return true;
    }
}
