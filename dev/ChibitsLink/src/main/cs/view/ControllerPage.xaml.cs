namespace ChibitsLink.main.cs.view;

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.controller;
using ChibitsLink.main.cs.net;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;

public partial class ControllerPage : ContentPage
{
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
            MainThread.BeginInvokeOnMainThread(() => LatencyLabel.Text = $"Ping: {ms}ms");
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Security check
        var user = _accountService.GetCurrentUser();
        if (user == null)
        {
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        _orientationService?.SetLandscape();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _orientationService?.SetPortrait();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        // Optional: Disconnect logic if needed here
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

    private async void OnJoystickPanUpdated(object sender, PanUpdatedEventArgs e)
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

                // Fire event for the controller
                await _controller.HandleJoystickMoved((float)(totalX / maxRadius), (float)(-totalY / maxRadius));
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                // Snap back to center
                await JoystickKnob.TranslateTo(0, 0, 100, Easing.SpringOut);
                await _controller.HandleJoystickMoved(0, 0);
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
