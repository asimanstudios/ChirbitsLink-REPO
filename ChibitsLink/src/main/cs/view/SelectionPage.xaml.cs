namespace ChibitsLink.main.cs.view;

using System;
using System.Linq;
using Microsoft.Maui.Controls;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.controller;
using ChibitsLink.main.cs.net;
using System.Threading.Tasks;

public partial class SelectionPage : ContentPage
{
    private readonly GameService _gameService;
    private readonly BluetoothService _bluetoothService;
    private readonly Connection _connection;
    private Game? _selectedGame;

    public SelectionPage(GameService gameService, BluetoothService bluetoothService, Connection connection)
    {
        InitializeComponent();
        _gameService = gameService;
        _bluetoothService = bluetoothService;
        _connection = connection;
        LoadGames();
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

    private async void OnQRClicked(object sender, EventArgs e) => await DisplayAlert("QR", "Opening Camera Scan...", "OK");
    
    private async void OnBluetoothClicked(object sender, EventArgs e)
    {
        try
        {
            var hasPermission = await _bluetoothService.RequestBluetoothPermissions();
            if (!hasPermission)
            {
                await DisplayAlert("Permisos", "Se necesitan permisos de ubicación/dispositivos cercanos para buscar el juego.", "OK");
                return;
            }

            await _bluetoothService.ScanDevicesAsync();
            var deviceNames = _bluetoothService.DiscoveredDevices
                .Select((Plugin.BLE.Abstractions.Contracts.IDevice d) => d.Name ?? d.Id.ToString())
                .ToArray();

            if (deviceNames.Length == 0)
            {
                await DisplayAlert("Bluetooth", "No items found. Make sure Unity Game is in Pairing Mode.", "OK");
                return;
            }

            string[] buttons = deviceNames;
            var selectedName = await DisplayActionSheet("Select Chibits Host", "Cancel", null, buttons);

            if (selectedName != "Cancel" && selectedName != null)
            {
                var selectedDevice = _bluetoothService.DiscoveredDevices.FirstOrDefault(d => (d.Name ?? d.Id.ToString()) == selectedName);
                if (selectedDevice != null)
                {
                    bool connected = await _bluetoothService.ConnectToDeviceAsync(selectedDevice);
                    if (connected)
                    {
                        _connection.SetBluetoothDevice(selectedDevice);
                        await DisplayAlert("Success", "Connected via Bluetooth!", "OK");
                        await Shell.Current.GoToAsync("//ControllerPage");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Could not connect to device", "OK");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Bluetooth Error", ex.Message, "OK");
        }
    }

    private async void OnWebSocketClicked(object sender, EventArgs e)
    {
        string url = await DisplayPromptAsync("WebSocket Connect", "Enter Game Server URL", "Connect", "Cancel", "ws://192.168.1.100:8080");
        if (!string.IsNullOrEmpty(url))
        {
            try 
            {
                await _connection.ConnectWebSocketAsync(url);
                await DisplayAlert("Connected", "WebSocket link established.", "OK");
                await Shell.Current.GoToAsync("//ControllerPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Connection Failed", ex.Message, "OK");
            }
        }
    }

    private async void OnJoinLobbyClicked(object sender, EventArgs e)
    {
        string code = RoomCodeEntry.Text;
        if (string.IsNullOrEmpty(code) || code.Length < 6)
        {
            await DisplayAlert("Error", "Enter a valid 6-digit code", "OK");
            return;
        }

        // R02F02T01 - Verificar conexión al lobby
        await DisplayAlert("Lobby", $"Joining {code}...", "OK");
        
        // Navigation directly to controller for that lobby
        await Shell.Current.GoToAsync("//ControllerPage");
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        if (_selectedGame != null)
        {
            await Shell.Current.GoToAsync("//ControllerPage");
        }
    }
}
