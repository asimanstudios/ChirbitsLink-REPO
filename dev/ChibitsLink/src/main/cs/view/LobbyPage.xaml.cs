using ChibitsLink.main.cs.net;
using System.Diagnostics;
using System.Collections.ObjectModel;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;

namespace ChibitsLink.main.cs.view;

[QueryProperty(nameof(RoomCode), "code")]
public partial class LobbyPage : ContentPage
{
    private readonly AccountService _accountService;
    private readonly GameService _gameService;
    private readonly Connection _connection;
    private IDisposable? _lobbyListener;
    
    private string _roomCode = "";
    private bool _isReady = false;
    public ObservableCollection<Character> Characters { get; set; } = new();
    public ObservableCollection<Game> AvailableGames { get; set; } = new();

    public string RoomCode
    {
        get => _roomCode;
        set
        {
            _roomCode = value;
            RoomTitleLabel.Text = $"SALA #{_roomCode}";
            StartLobbyListener();
        }
    }

    public LobbyPage(AccountService accountService, GameService gameService, Connection connection)
    {
        InitializeComponent();
        _accountService = accountService;
        _gameService = gameService;
        _connection = connection;
        
        LoadCharacters();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var user = _accountService.GetCurrentUser();
        if (user != null)
        {
            UpdateProfileDisplay(user.SelectedCharacterId);
            _connection.Disconnected += OnUnexpectedDisconnect; // Suscribirse
            
            // Sincronizar personaje inicial con el servidor
            await _connection.SendMessageAsync($"SYNC_CHAR|{user.Id}|{user.SelectedCharacterId}");
        }
        else
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _lobbyListener?.Dispose();
        _connection.Disconnected -= OnUnexpectedDisconnect; // Des-suscribirse
    }

    private void StartLobbyListener()
    {
        if (string.IsNullOrEmpty(_roomCode)) return;

        _lobbyListener?.Dispose();
        _lobbyListener = _gameService.ListenToLobby(_roomCode, (party) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (party == null) return;
                
                // Conectar al servidor TCP si no lo estamos
                if (!_connection.IsConnected)
                {
                    try 
                    {
                        Debug.WriteLine($"[LobbyPage] Intentando conexión TCP a {party.IpAddress}:{party.Port}");
                        await _connection.ConnectTcpAsync(party.IpAddress, party.Port);
                        
                        // Una vez conectados, sincronizar personaje
                        var user = _accountService.GetCurrentUser();
                        if (user != null)
                        {
                            await _connection.SendMessageAsync($"SYNC_CHAR|{user.Id}|{user.SelectedCharacterId}|{user.Username}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[LobbyPage] Error conectando al servidor del juego: {ex.Message}");
                    }
                }

                // Si el estado cambia a VOTING, mostramos la UI de votación
                if (party.GameState == "VOTING" && !VotingOverlay.IsVisible)
                {
                    await LoadAvailableGames();
                    VotingOverlay.IsVisible = true;
                }
                else if (party.GameState == "LOBBY")
                {
                    VotingOverlay.IsVisible = false;
                }
                else if (party.GameState == "IN_GAME")
                {
                    await Shell.Current.GoToAsync("//ControllerPage");
                }
            });
        });
    }

    private async Task LoadAvailableGames()
    {
        try
        {
            var games = await _gameService.GetAvailableGames();
            AvailableGames.Clear();
            foreach (var g in games) AvailableGames.Add(g);
            GamesCollection.ItemsSource = AvailableGames;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cargando juegos: {ex.Message}");
        }
    }

    private async void OnGameVoted(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Game game)
        {
            var user = _accountService.GetCurrentUser();
            if (user != null)
            {
                await _connection.SendMessageAsync($"VOTE|{user.Id}|{game.Id}");
                await DisplayAlert("Voto Registrado", $"Has votado por: {game.Name}", "OK");
            }
            GamesCollection.SelectedItem = null;
        }
    }

    private void UpdateProfileDisplay(string? characterId)
    {
        Debug.WriteLine($"[LobbyPage] Actualizando display para: {characterId}");
        var character = Characters.FirstOrDefault(c => c.Id == characterId) 
                        ?? Characters.FirstOrDefault();
        
        if (character != null)
        {
            var imageUrl = string.IsNullOrEmpty(character.ImageUrl) ? "char_placeholder.png" : character.ImageUrl;
            
            // Temporary fix for legacy database entries or missing assets
            if (imageUrl == "dotnet_bot.png" || imageUrl.Contains("char_") && !imageUrl.Contains("placeholder"))
            {
                // If it's a specific character image but we know we only have the placeholder
                imageUrl = "char_placeholder.png";
            }

            ProfileImage.Source = imageUrl;
            CharacterNameLabel.Text = character.Name;
        }

        var user = _accountService.GetCurrentUser();
        if (user != null)
        {
            UserLevelLabel.Text = $"LVL. {user.Level}";
            UsernameLabel.Text = user.Username.ToUpper();
        }
    }

    private async void OnUnexpectedDisconnect()
    {
        MainThread.BeginInvokeOnMainThread(async () => 
        {
            await DisplayAlert("Conexión Perdida", "Se ha perdido la conexión con el reino de ChirBits.", "OK");
            await Shell.Current.GoToAsync("//MainMenuPage");
        });
    }

    private async void LoadCharacters()
    {
        try 
        {
            Debug.WriteLine("[LobbyPage] Cargando personajes...");
            var list = await _gameService.GetCharacters();
            Characters.Clear();
            foreach(var c in list) 
            {
                // Sanitize ImageUrl for placeholders/missing assets
                if (string.IsNullOrEmpty(c.ImageUrl) || c.ImageUrl == "dotnet_bot.png" || (c.ImageUrl.Contains("char_") && !c.ImageUrl.Contains("placeholder")))
                {
                    c.ImageUrl = "char_placeholder.png";
                }
                Characters.Add(c);
            }
            
            CharactersCollection.ItemsSource = Characters;
            Debug.WriteLine($"[LobbyPage] {Characters.Count} personajes cargados.");

            var user = _accountService.GetCurrentUser();
            if (user != null) UpdateProfileDisplay(user.SelectedCharacterId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cargando personajes: {ex.Message}");
        }
    }

    private void OnProfileTapped(object sender, EventArgs e)
    {
        Debug.WriteLine("[LobbyPage] Tapa en perfil detectada.");
        CharacterSelectionLayout.IsVisible = !CharacterSelectionLayout.IsVisible;
    }

    private async void OnCharacterSelected(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Character character)
        {
            Debug.WriteLine($"[LobbyPage] Personaje seleccionado mediante Tap: {character.Id}");
            CharacterSelectionLayout.IsVisible = false;
            UpdateProfileDisplay(character.Id);
            
            var user = _accountService.GetCurrentUser();
            if (user != null)
            {
                user.SelectedCharacterId = character.Id;
                await _accountService.UpdateUser(user);
                
                // Informar al servidor Unity del cambio de modelo incluyendo el username
                if (_connection.IsConnected)
                {
                    string syncMsg = $"SYNC_CHAR|{user.Id}|{character.Id}|{user.Username}";
                    await _connection.SendMessageAsync(syncMsg);
                    Debug.WriteLine($"[LobbyPage] Sync enviado: {syncMsg}");
                }
            }
        }
    }

    private async void OnReadyClicked(object sender, EventArgs e)
    {
        var user = _accountService.GetCurrentUser();
        if (user == null) return;

        _isReady = !_isReady;
        
        // Enviar estado de Listo al servidor Unity vía TCP
        await _connection.SendMessageAsync($"READY|{user.Id}|{_isReady}");

        if (_isReady)
        {
            ReadyButton.Text = "¡LISTO!";
            ReadyButton.BackgroundColor = Color.FromArgb("#00B894");
        }
        else
        {
            ReadyButton.Text = "MARCAR COMO LISTO";
            if (Application.Current?.Resources.TryGetValue("Primary", out var primaryColor) == true)
            {
                ReadyButton.BackgroundColor = (Color)primaryColor;
            }
        }
    }

    private async void OnLeaveClicked(object sender, EventArgs e)
    {
        if (_connection.IsConnected)
        {
            await _connection.DisconnectAsync();
        }
        await Shell.Current.GoToAsync("..");
    }
}
