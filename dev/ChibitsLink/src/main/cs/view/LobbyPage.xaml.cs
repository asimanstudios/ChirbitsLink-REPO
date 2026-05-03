using ChibitsLink.main.cs.net;
using System.Diagnostics;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.viewmodel;
using ChibitsLink.main.cs.model;
using System.Linq;

namespace ChibitsLink.main.cs.view;

[QueryProperty(nameof(RoomCode), "code")]
public partial class LobbyPage : ContentPage
{
    private readonly LobbyViewModel _viewModel;
    private readonly Connection _connection;
    
    public string RoomCode
    {
        get => _viewModel.RoomCode;
        set => _viewModel.RoomCode = value;
    }

    public LobbyPage(LobbyViewModel viewModel, Connection connection)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _connection = connection;
        
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
        _connection.Disconnected += OnServerDisconnected;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _connection.Disconnected -= OnServerDisconnected;
        _viewModel.Cleanup();
    }

    private void OnServerDisconnected()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync("//MainMenuPage");
        });
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainMenuPage");
    }

    private async void OnVoteClicked(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string gameId)
        {
            await _connection.SendMessageAsync($"VOTE|{gameId}");
            await DisplayAlert("¡Voto Registrado!", $"Has votado por {gameId}. ¡Suerte!", "Vale");
        }
    }

    private async void OnLeaveClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Abandonar", "¿Seguro que quieres salir de la sala?", "Sí", "No");
        if (confirm)
        {
            await _viewModel.LeaveLobbyAsync();
            await Shell.Current.GoToAsync("//MainMenuPage");
        }
    }

    private async void OnGameSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Game game)
        {
            await _connection.SendMessageAsync($"VOTE|{game.Id}");
            await DisplayAlert("Voto Registrado", $"Has votado por {game.Name}", "OK");
            
            if (sender is ListView lv) lv.SelectedItem = null;
        }
    }
}
