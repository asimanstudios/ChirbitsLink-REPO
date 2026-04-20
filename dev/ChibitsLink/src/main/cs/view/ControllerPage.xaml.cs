namespace ChibitsLink.main.cs.view;

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.controller;
using ChibitsLink.main.cs.net;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;

[QueryProperty(nameof(RoomCode), "code")]
public partial class ControllerPage : ContentPage
{
    private string _roomCode = "";

    public string RoomCode
    {
        get => _roomCode;
        set => _roomCode = value;
    }

    private readonly ControllerController _controller;
    private readonly Connection _connection;
    private readonly AccountService _accountService;
    private readonly IOrientationService? _orientationService;
    private double _baseX, _baseY;

    public event Action<float, float>? OnJoystickMoved;
    public event Action<string>? OnButtonPressed;

    public ControllerPage(ControllerController controller, Connection connection, AccountService accountService, IOrientationService? orientationService = null)
    {
        InitializeComponent();
        _controller = controller;
        _connection = connection;
        _accountService = accountService;
        _orientationService = orientationService;

        _connection.LatencyUpdated += (ms) => 
        {
            MainThread.BeginInvokeOnMainThread(() => LatencyLabel.Text = $"{ms}ms");
        };
        _connection.Disconnected += OnUnexpectedDisconnect;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _orientationService?.SetPortrait();
        _connection.Disconnected -= OnUnexpectedDisconnect;
        _connection.MessageReceived -= OnMessageReceived;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _connection.MessageReceived += OnMessageReceived;
        
        // Security check
        var user = _accountService.GetCurrentUser();
        if (user == null)
        {
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        UsernameLabel.Text = (user.Username ?? "USER").ToUpper();
        UserLevelLabel.Text = $"LVL. {user.Level}";

        _orientationService?.SetLandscape();
    }

    private void OnUnexpectedDisconnect()
    {
        MainThread.BeginInvokeOnMainThread(async () => 
        {
            await DisplayAlert("Conexión Perdida", "Se ha perdido la conexión con el reino de ChirBits.", "OK");
            await Shell.Current.GoToAsync("//MainMenuPage");
        });
    }

    private void OnMessageReceived(string message)
    {
        string trimmed = message.Trim();
        if (trimmed == "GOTO_LOBBY" || trimmed == "GAME_OVER|LOBBY")
        {
            MainThread.BeginInvokeOnMainThread(async () => 
            {
                // Navegación absoluta con el código preservado para evitar el BUG del 000000
                await Shell.Current.GoToAsync($"//LobbyPage?code={_roomCode}"); 
            });
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        var user = _accountService.GetCurrentUser();
        
        // Desuscribirse para evitar aviso de desconexión inesperada
        _connection.Disconnected -= OnUnexpectedDisconnect;

        if (user != null && _connection.IsConnected)
        {
            await _connection.SendMessageAsync($"LEAVE|{user.Id}");
            await _connection.DisconnectAsync();
        }
        await Shell.Current.GoToAsync("//MainMenuPage");
    }

    private void OnTiltToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.Game);
            }
        }
        else
        {
            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.Stop();
                Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
            }
        }
    }

    private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        // R03F02T01P01 - Enviar valores de inclinación
        _ = _controller.HandleJoystickMoved(e.Reading.Acceleration.X, e.Reading.Acceleration.Y);
    }

    private void OnJoystickPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _baseX = JoystickKnob.TranslationX;
                _baseY = JoystickKnob.TranslationY;
                break;

            case GestureStatus.Running:
                // Limit movement within boundary
                double totalX = e.TotalX;
                double totalY = e.TotalY;
                double distance = Math.Sqrt(totalX * totalX + totalY * totalY);
                double maxRadius = 60;

                if (distance > maxRadius)
                {
                    totalX = (totalX / distance) * maxRadius;
                    totalY = (totalY / distance) * maxRadius;
                }

                JoystickKnob.TranslationX = totalX;
                JoystickKnob.TranslationY = totalY;

                // Fire event for the controller WITHOUT await to avoid UI flicker
                _ = _controller.HandleJoystickMoved((float)(totalX / maxRadius), (float)(-totalY / maxRadius));
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                // Snap back to center
                _ = JoystickKnob.TranslateTo(0, 0, 100, Easing.SpringOut);
                _ = _controller.HandleJoystickMoved(0, 0);
                break;
        }
    }


    private async void OnButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            string buttonId = button.CommandParameter?.ToString() ?? "Unknown";
            // Visual feedback
            button.Opacity = 0.5;
            await button.FadeTo(1, 100);
            
            await _controller.HandleButtonPressed(buttonId);
        }
    }
}
