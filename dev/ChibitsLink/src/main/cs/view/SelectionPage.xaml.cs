using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ChibitsLink.main.cs.controller;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.net;
using ChibitsLink.main.cs.service;

namespace ChibitsLink.main.cs.view;

/// <summary>
/// Página de selección de juego y método de conexión (Wi-Fi, Bluetooth, QR).
/// </summary>
public partial class SelectionPage : ContentPage
{
    private readonly GameService _gameService;
    private readonly BluetoothService _bluetoothService;
    private readonly Connection _connection;
    private readonly AccountService _accountService;
    private Game? _selectedGame;

    public SelectionPage(GameService gameService, BluetoothService bluetoothService, Connection connection, AccountService accountService)
    {
        InitializeComponent();
        _gameService = gameService;
        _bluetoothService = bluetoothService;
        _connection = connection;
        _accountService = accountService;
        LoadGames();
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

    private async void LoadGames()
    {
        GamesCollection.ItemsSource = await _gameService.GetAvailableGames();
    }

    private void OnGameSelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedGame = e.CurrentSelection.FirstOrDefault() as Game;
        StartButton.IsEnabled = _selectedGame != null;
    }

    private async void OnQRClicked(object sender, EventArgs e) =>
        await DisplayAlert("QR", "Abriendo escáner de cámara...", "OK");

    private async void OnBluetoothClicked(object sender, EventArgs e)
    {
        try
        {
            var hasPermission = await _bluetoothService.RequestBluetoothPermissions();
            if (!hasPermission)
            {
                await DisplayAlert("Permisos", "Se necesitan permisos de ubicación/dispositivos cercanos para buscar el juego.", "OK");
            }
            else
            {
                await _bluetoothService.ScanDevicesAsync();
                var deviceNames = _bluetoothService.DiscoveredDevices
                    .Select((Plugin.BLE.Abstractions.Contracts.IDevice d) => d.Name ?? d.Id.ToString())
                    .ToArray();

                if (deviceNames.Length == 0)
                {
                    await DisplayAlert("Bluetooth", "No se encontraron dispositivos. Asegúrate de que el juego Unity esté en modo emparejamiento.", "OK");
                }
                else
                {
                    var selectedName = await DisplayActionSheet("Seleccionar Host ChirBits", "Cancelar", null, deviceNames);

                    if (selectedName != "Cancelar" && selectedName != null)
                    {
                        var selectedDevice = _bluetoothService.DiscoveredDevices
                            .FirstOrDefault(d => (d.Name ?? d.Id.ToString()) == selectedName);

                        if (selectedDevice != null)
                        {
                            bool connected = await _bluetoothService.ConnectToDeviceAsync(selectedDevice);
                            if (connected)
                            {
                                _connection.SetBluetoothDevice(selectedDevice);
                                await DisplayAlert("Éxito", "¡Conectado vía Bluetooth!", "OK");
                                await Shell.Current.GoToAsync("//ControllerPage");
                            }
                            else
                            {
                                await DisplayAlert("Error", "No se pudo conectar al dispositivo.", "OK");
                            }
                        }
                    }
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlert("Error Bluetooth", $"Bluetooth no disponible o permisos insuficientes: {ex.Message}", "OK");
        }
        catch (TimeoutException ex)
        {
            await DisplayAlert("Tiempo Agotado", $"El escaneo Bluetooth tardó demasiado: {ex.Message}", "OK");
        }
    }

    private async void OnWebSocketClicked(object sender, EventArgs e)
    {
        string url = await DisplayPromptAsync(
            "Conectar WebSocket",
            "Introduce la URL del servidor de juego",
            "Conectar", "Cancelar",
            "ws://192.168.1.100:8080");

        if (!string.IsNullOrEmpty(url))
        {
            try
            {
                await _connection.ConnectWebSocketAsync(url);
                await DisplayAlert("Conectado", "Enlace WebSocket establecido.", "OK");
                await Shell.Current.GoToAsync("//ControllerPage");
            }
            catch (ChibitsLink.main.cs.exception.NetworkException ex)
            {
                await DisplayAlert("Conexión Fallida", $"No se pudo conectar al servidor WebSocket: {ex.Message}", "OK");
            }
        }
    }

    private async void OnJoinLobbyClicked(object sender, EventArgs e)
    {
        string code = RoomCodeEntry.Text;
        bool isCodeValid = !string.IsNullOrEmpty(code) && code.Length == 6;

        if (!isCodeValid)
        {
            await DisplayAlert("Error", "Introduce un código válido de 6 dígitos.", "OK");
        }
        else
        {
            try
            {
                bool lobbyExists = await _gameService.ValidateLobbyAsync(code!);
                if (lobbyExists)
                {
                    await Shell.Current.GoToAsync($"LobbyPage?code={code}");
                }
                else
                {
                    await DisplayAlert("Error", "La sala no existe o el código es incorrecto.", "Vale");
                }
            }
            catch (ChibitsLink.main.cs.exception.DatabaseException ex)
            {
                await DisplayAlert("Error de Conexión", $"No hemos podido verificar la sala: {ex.Message}", "Vale");
            }
        }
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        if (_selectedGame != null)
        {
            await Shell.Current.GoToAsync("//ControllerPage");
        }
    }
}
