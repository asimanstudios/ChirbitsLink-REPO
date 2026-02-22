using System.Collections.ObjectModel;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.net;
using ChibitsLink.main.repository;
using System.Text.Json;

namespace ChibitsLink.main.cs.view;

[QueryProperty(nameof(RoomCode), "code")]
public partial class LobbyPage : ContentPage
{
    private readonly AccountService _accountService;
    private readonly Connection _connection;
    private readonly Database _database;
    private string _roomCode = "";
    private bool _isReady = false;
    public ObservableCollection<Character> Characters { get; set; } = new();
    public ObservableCollection<PlayerInfo> Players { get; set; } = new();

    public string RoomCode
    {
        get => _roomCode;
        set
        {
            _roomCode = value;
            RoomTitleLabel.Text = $"SALA #{_roomCode}";
        }
    }

    public LobbyPage(AccountService accountService, Connection connection, Database database)
    {
        InitializeComponent();
        _accountService = accountService;
        _connection = connection;
        _database = database;
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
            
            // Sincronizar con el juego
            await SyncCharacterWithGame(user.SelectedCharacterId);
            
            // Actualizar lista de jugadores
            await RefreshPlayersList();
        }
        else
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

    private void UpdateProfileDisplay(string characterId)
    {
        var character = Characters.FirstOrDefault(c => c.Name == characterId) 
                        ?? Characters.FirstOrDefault();
        
        if (character != null)
        {
            ProfileImage.Source = character.ImageUrl;
            CharacterNameLabel.Text = character.Name;
        }
    }

    private void LoadCharacters()
    {
        Characters.Clear();
        Characters.Add(new Character { Id = "VALIENTE", Name = "VALIENTE", ImageUrl = "dotnet_bot.png" });
        Characters.Add(new Character { Id = "MAGA", Name = "MAGA", ImageUrl = "dotnet_bot.png" });
        Characters.Add(new Character { Id = "PICARO", Name = "PICARO", ImageUrl = "dotnet_bot.png" });
        Characters.Add(new Character { Id = "BRUJO", Name = "BRUJO", ImageUrl = "dotnet_bot.png" });
        
        CharactersCollection.ItemsSource = Characters;
    }

    private void OnProfileTapped(object sender, EventArgs e)
    {
        CharacterSelectionLayout.IsVisible = !CharacterSelectionLayout.IsVisible;
    }

    private async void OnCharacterSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Character character)
        {
            CharacterSelectionLayout.IsVisible = false;
            UpdateProfileDisplay(character.Name);
            
            var user = _accountService.GetCurrentUser();
            if (user != null)
            {
                user.SelectedCharacterId = character.Name;
                await _accountService.UpdateUser(user);
                
                // Sincronizar personaje con el juego
                await SyncCharacterWithGame(character.Name);
            }
            
            CharactersCollection.SelectedItem = null;
        }
    }

    /// <summary>
    /// Sincroniza el personaje seleccionado con el juego via TCP.
    /// </summary>
    private async Task SyncCharacterWithGame(string characterId)
    {
        if (_connection.IsConnected)
        {
            try
            {
                var user = _accountService.GetCurrentUser();
                if (user != null)
                {
                    // Formato: SYNC_CHAR|roomCode|userId|charId
                    string syncMessage = $"SYNC_CHAR|{_roomCode}|{user.Id}|{characterId}";
                    await _connection.SendMessageAsync(syncMessage);
                    
                    System.Diagnostics.Debug.WriteLine($"[LobbyPage] Sincronizando personaje: {syncMessage}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LobbyPage] Error sincronizando personaje: {ex.Message}");
                await DisplayAlert("Error", "No se pudo sincronizar el personaje con el juego.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Sin conexión", "No hay conexión con el juego.", "OK");
        }
    }

    /// <summary>
    /// Actualiza la lista de jugadores en la sala.
    /// </summary>
    private async Task RefreshPlayersList()
    {
        try
        {
            var party = await _database.GetParty(_roomCode);
            if (party != null)
            {
                Players.Clear();
                
                // Mostrar hasta 4 jugadores
                int displayCount = Math.Min(party.PlayerIds?.Count ?? 0, 4);
                for (int i = 0; i < displayCount; i++)
                {
                    var playerId = party.PlayerIds[i];
                    var playerUser = await _database.GetUser(playerId);
                    
                    Players.Add(new PlayerInfo 
                    { 
                        Slot = i + 1,
                        PlayerName = playerUser?.Username ?? $"Jugador {i + 1}",
                        Character = playerUser?.SelectedCharacterId ?? "Sin seleccionar",
                        IsReady = i == 0 // El host se considera listo
                    });
                }
                
                PlayersList.ItemsSource = Players;
                PlayerCountLabel.Text = $"{Players.Count}/4 JUGADORES";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LobbyPage] Error actualizando jugadores: {ex.Message}");
        }
    }

    private async void OnReadyClicked(object sender, EventArgs e)
    {
        _isReady = !_isReady;
        
        if (_isReady)
        {
            ReadyButton.Text = "¡LISTO!";
            ReadyButton.BackgroundColor = Color.FromArgb("#00B894");
            
            // Verificar que hay conexión antes de proceed
            if (_connection.IsConnected)
            {
                // Aquí podrías enviar un mensaje de "listo" al juego
                var user = _accountService.GetCurrentUser();
                if (user != null)
                {
                    try
                    {
                        string readyMessage = $"PLAYER_READY|{_roomCode}|{user.Id}";
                        await _connection.SendMessageAsync(readyMessage);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LobbyPage] Error enviando listo: {ex.Message}");
                    }
                }
                
                // Navegar al ControllerPage
                await Shell.Current.GoToAsync("//ControllerPage");
            }
            else
            {
                await DisplayAlert("Sin conexión", "Se perdió la conexión con el juego.", "OK");
                ReadyButton.Text = "MARCAR COMO LISTO";
                ReadyButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                _isReady = false;
            }
        }
        else
        {
            ReadyButton.Text = "MARCAR COMO LISTO";
            ReadyButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
        }
    }

    private async void OnLeaveClicked(object sender, EventArgs e)
    {
        // Desconectar del juego
        if (_connection.IsConnected)
        {
            try
            {
                var user = _accountService.GetCurrentUser();
                if (user != null)
                {
                    string leaveMessage = $"PLAYER_LEAVE|{_roomCode}|{user.Id}";
                    await _connection.SendMessageAsync(leaveMessage);
                }
                
                await _connection.DisconnectAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LobbyPage] Error desconectando: {ex.Message}");
            }
        }
        
        await Shell.Current.GoToAsync("..");
    }
}

/// <summary>
/// Modelo para mostrar información del jugador en la UI.
/// </summary>
public class PlayerInfo
{
    public int Slot { get; set; }
    public string PlayerName { get; set; } = "";
    public string Character { get; set; } = "";
    public bool IsReady { get; set; }
}
