using System;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.net;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository;

namespace ChibitsLink.main.cs.view;

public partial class JoinRoomPage : ContentPage
{
    private readonly AccountService _accountService;
    private readonly Database _database;
    private readonly Connection _connection;

    public JoinRoomPage(AccountService accountService, Database database, Connection connection)
    {
        InitializeComponent();
        _accountService = accountService;
        _database = database;
        _connection = connection;
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
        string code = RoomCodeEntry.Text?.ToUpperInvariant();
        
        if (string.IsNullOrEmpty(code) || code.Length != 6)
        {
            await DisplayAlert("Ups", "El código debe ser de 6 caracteres.", "Vale");
            return;
        }

        LoadingIndicator.IsRunning = true;
        StatusLabel.Text = "Verificando sala...";
        StatusLabel.IsVisible = true;

        try
        {
            // 1. Verificar que la sala existe en Firebase
            var party = await _database.GetParty(code);
            
            if (party == null)
            {
                LoadingIndicator.IsRunning = false;
                StatusLabel.Text = "";
                await DisplayAlert("Error", "El código de sala no es válido.", "Intentar de nuevo");
                return;
            }

            // 2. Verificar que la sala no esté llena (máximo 4 jugadores)
            if (party.CurrentPlayers >= party.MaxPlayers)
            {
                LoadingIndicator.IsRunning = false;
                StatusLabel.Text = "";
                await DisplayAlert("Sala llena", "Esta sala ya tiene el máximo de 4 jugadores.", "OK");
                return;
            }

            // 3. Intentar conexión TCP al juego
            StatusLabel.Text = "Conectando al juego...";
            
            try
            {
                await _connection.ConnectTcpAsync();
                
                if (!_connection.IsConnected)
                {
                    LoadingIndicator.IsRunning = false;
                    StatusLabel.Text = "";
                    await DisplayAlert("Sin conexión", "No se pudo conectar al juego. Asegúrate de que el juego esté abierto.", "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                LoadingIndicator.IsRunning = false;
                StatusLabel.Text = "";
                await DisplayAlert("Error de conexión", $"No se pudo conectar al juego: {ex.Message}", "OK");
                return;
            }

            // 4. Registrar al jugador en la sala
            var user = _accountService.GetCurrentUser();
            if (user != null)
            {
                await _database.JoinLobbyAsync(user.Id, code);
                
                // Actualizar contador de jugadores en la sala
                party.CurrentPlayers++;
                if (party.PlayerIds == null)
                    party.PlayerIds = new List<string>();
                party.PlayerIds.Add(user.Id);
                await _database.UpdateParty(party);
            }

            LoadingIndicator.IsRunning = false;
            StatusLabel.Text = "";
            
            // 5. Navegar al Lobby
            await Shell.Current.GoToAsync($"LobbyPage?code={code}");
        }
        catch (Exception ex)
        {
            LoadingIndicator.IsRunning = false;
            StatusLabel.Text = "";
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
