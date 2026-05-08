using System.Diagnostics;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.net;
using ChibitsLink.main.cs.viewmodel;

namespace ChibitsLink.main.cs.view;

public partial class MainMenuPage : ContentPage
{
    private readonly MainMenuViewModel _viewModel;
    private readonly AccountService _accountService;
    private readonly Connection _connection;

    public MainMenuPage(MainMenuViewModel viewModel, AccountService accountService, Connection connection)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _accountService = accountService;
        _connection = connection;
        
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.InitializeAsync();
        
        // Animación de entrada secuencial para fluidez premium
        await AnimateEntrance();
    }

    private async Task AnimateEntrance()
    {
        // Reset inicial para el efecto "slide"
        ProfileSection.TranslationY = -30;
        ContentStack.TranslationY = 30;
        FooterStack.TranslationY = 30;

        // 1. Aparece el perfil (desde arriba)
        await Task.WhenAll(
            ProfileSection.FadeTo(1, 450, Easing.CubicOut),
            ProfileSection.TranslateTo(0, 0, 450, Easing.CubicOut)
        );

        // 2. Aparece el contenido y el pie (desde abajo)
        await Task.WhenAll(
            ContentStack.FadeTo(1, 400, Easing.CubicOut),
            ContentStack.TranslateTo(0, 0, 400, Easing.CubicOut),
            FooterStack.FadeTo(1, 400, Easing.CubicOut),
            FooterStack.TranslateTo(0, 0, 400, Easing.CubicOut)
        );
    }

    // ── Handlers (UI-specific navigation) ─────────────────────────────────────

    private async void OnControllerClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ControllerPage");
    }

    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HistoryPage");
    }

    private async void OnJoinRoomClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//JoinRoomPage");
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SettingsPage");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cerrar Sesión", "¿Seguro que quieres abandonar el reino?", "Sí, salir", "Me quedo");
        if (confirm)
        {
            if (_connection.IsConnected) await _connection.DisconnectAsync();
            _accountService.Logout();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
