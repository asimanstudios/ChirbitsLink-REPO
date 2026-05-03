using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.repository.interfaces;
using ChibitsLink.main.cs.net;

namespace ChibitsLink.main.cs.viewmodel;

public class PlayerItem : BaseViewModel
{
    private string _name = string.Empty;
    private string _characterImage = "char_placeholder";
    private bool _isReady = false;
    private int _level = 1;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string CharacterImage { get => _characterImage; set => SetProperty(ref _characterImage, value); }
    public bool IsReady { get => _isReady; set => SetProperty(ref _isReady, value); }
    public int Level { get => _level; set => SetProperty(ref _level, value); }
    public string LevelDisplay => $"LVL. {Level}";
}

public class LobbyViewModel : BaseViewModel
{
    private readonly GameService _gameService;
    private readonly AccountService _accountService;
    private readonly IMasterDataRepository _masterRepo;
    private readonly ILobbyRepository _lobbyRepo;
    private readonly IUserRepository _userRepo;
    private readonly Connection _connection;

    private string _roomCode = string.Empty;
    private bool _isReady = false;
    private string _selectedCharacterName = "VALIENTE";
    private string _selectedCharacterImage = "char_placeholder";
    private string _username = string.Empty;
    private string _levelDisplay = "LVL. 1";
    private bool _isCharacterListVisible = false;
    private bool _isVoting = false;
    private IDisposable? _lobbyListener;

    public string RoomCode 
    { 
        get => _roomCode; 
        set 
        { 
            if (SetProperty(ref _roomCode, value)) StartLobbyListener(); 
        } 
    }

    public bool IsReady { get => _isReady; set => SetProperty(ref _isReady, value); }
    public bool IsVoting { get => _isVoting; set => SetProperty(ref _isVoting, value); }
    public string SelectedCharacterName { get => _selectedCharacterName; set => SetProperty(ref _selectedCharacterName, value); }
    public string SelectedCharacterImage { get => _selectedCharacterImage; set => SetProperty(ref _selectedCharacterImage, value); }
    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string LevelDisplay { get => _levelDisplay; set => SetProperty(ref _levelDisplay, value); }
    public bool IsCharacterListVisible { get => _isCharacterListVisible; set => SetProperty(ref _isCharacterListVisible, value); }

    public ObservableCollection<Character> Characters { get; } = new();
    public ObservableCollection<Game> AvailableGames { get; } = new();
    public ObservableCollection<PlayerItem> Players { get; } = new();

    public ICommand ToggleReadyCommand { get; }
    public ICommand ToggleCharacterListCommand { get; }
    public ICommand SelectCharacterCommand { get; }

    public LobbyViewModel(GameService gameService, AccountService accountService, IMasterDataRepository masterRepo, ILobbyRepository lobbyRepo, IUserRepository userRepo, Connection connection)
    {
        _gameService = gameService;
        _accountService = accountService;
        _masterRepo = masterRepo;
        _lobbyRepo = lobbyRepo;
        _userRepo = userRepo;
        _connection = connection;

        ToggleReadyCommand = new Command(async () => await ExecuteToggleReady());
        ToggleCharacterListCommand = new Command(() => IsCharacterListVisible = !IsCharacterListVisible);
        SelectCharacterCommand = new Command<Character>(async (c) => await SelectCharacterAsync(c));
    }

    public async Task InitializeAsync()
    {
        await LoadCatalogAsync();
        UpdateUserData();
        
        // Prioridad 1: Conectar al servidor de Unity para que el jugador aparezca YA
        await ConnectToGameServerAsync();
        
        // Prioridad 2: Registrar en el historial (en paralelo para no bloquear la UI)
        _ = RegisterInHistoryAsync();

        // FIX: Reiniciar la escucha de Firestore al volver de un minijuego
        if (!string.IsNullOrEmpty(_roomCode))
        {
            StartLobbyListener();
        }
    }

    private async Task ConnectToGameServerAsync()
    {
        if (string.IsNullOrEmpty(_roomCode)) return;

        try
        {
            var party = await _lobbyRepo.GetPartyAsync(_roomCode);
            if (party != null && !string.IsNullOrEmpty(party.IpAddress))
            {
                Debug.WriteLine($"[Lobby] Intentando conectar a {party.IpAddress}:{party.Port}");
                
                // Limpiar conexión previa por si acaso
                await _connection.DisconnectAsync();
                
                await _connection.ConnectTcpAsync(party.IpAddress, party.Port);
                
                if (_connection.IsConnected)
                {
                    var user = _accountService.GetCurrentUser();
                    if (user != null)
                    {
                        // Identificarse ante el servidor de Unity nada más conectar
                        await _connection.SendMessageAsync($"SYNC_CHAR|{user.Id}|{user.SelectedCharacterId}|{user.Username}");
                        Debug.WriteLine("[Lobby] Conexión establecida e identificación enviada.");
                    }
                }
            }
        }
        catch (ChibitsLink.main.cs.exception.NetworkException ex)
        {
            Debug.WriteLine($"[Lobby] Error de conexión TCP: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.DisplayAlert("Error de Red", ex.Message, "Entendido");
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Lobby] Fallo inesperado en TCP: {ex.Message}");
        }
    }

    private async Task RegisterInHistoryAsync()
    {
        var user = _accountService.GetCurrentUser();
        if (user != null && !string.IsNullOrEmpty(_roomCode))
        {
            try
            {
                await _userRepo.AddToHistoryAsync(user.Id, _roomCode);
                Debug.WriteLine($"[Lobby] Sala {_roomCode} registrada en el historial del usuario {user.Id}");
            }
            catch (ChibitsLink.main.cs.exception.DatabaseException ex)
            {
                Debug.WriteLine($"[Lobby] Error al registrar historial en BBDD: {ex.Message}");
                // No lanzamos alerta para no interrumpir el flujo, pero se registra concretamente
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lobby] Error inesperado al registrar historial: {ex.Message}");
            }
        }
    }

    public async Task LeaveLobbyAsync()
    {
        var user = _accountService.GetCurrentUser();
        if (user != null && _connection.IsConnected)
        {
            await _connection.SendMessageAsync($"LEAVE|{user.Id}");
        }
        await _connection.DisconnectAsync();
        Cleanup();
    }

    private void UpdateUserData()
    {
        var user = _accountService.GetCurrentUser();
        if (user == null) return;

        Username = user.Username.ToUpper();
        LevelDisplay = $"LVL. {user.Level}";

        var selected = Characters.FirstOrDefault(c => c.Id == user.SelectedCharacterId);
        if (selected != null)
        {
            SelectedCharacterName = selected.Name;
            SelectedCharacterImage = selected.ImageUrl;
        }
    }

    private async Task LoadCatalogAsync()
    {
        var chars = await _masterRepo.GetCharactersAsync();
        Characters.Clear();
        foreach (var c in chars) Characters.Add(c);

        var games = await _masterRepo.GetGamesAsync();
        AvailableGames.Clear();
        foreach (var g in games) AvailableGames.Add(g);
    }

    private void StartLobbyListener()
    {
        _lobbyListener?.Dispose();
        _lobbyListener = _lobbyRepo.ListenToParty(_roomCode, async (party) =>
        {
            if (party == null) return;
            
            // 1. Detectar si entramos en fase de votación
            bool nowVoting = (party.GameState == "VOTING");
            
            // Si acabamos de entrar en votación y la lista está vacía, reintentar carga de catálogo
            if (nowVoting && !IsVoting && AvailableGames.Count == 0)
            {
                await LoadCatalogAsync();
            }
            
            IsVoting = nowVoting;

            // Asegurarnos de que el catálogo esté cargado antes de procesar la lista
            if (Characters.Count == 0)
            {
                await LoadCatalogAsync();
            }

            // 2. Actualizar lista de jugadores
            UpdatePlayerList(party);

            // 3. Detectar si el juego ha empezado
            if (party.GameState == "IN_GAME")
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.GoToAsync($"//ControllerPage?code={_roomCode}");
                });
            }
        });
    }

    private void UpdatePlayerList(Party party)
    {
        if (party == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var currentIds = party.PlayerIds ?? new List<string>();
            Debug.WriteLine($"[Lobby] Actualizando lista. Jugadores en Firestore: {currentIds.Count}");
            
            // Reconstruir la lista para asegurar sincronización total
            Players.Clear();

            foreach (var userId in currentIds)
            {
                Debug.WriteLine($"[Lobby] Procesando jugador: {userId}");
                string name = "JUGADOR";
                if (party.ParticipantNames != null && party.ParticipantNames.TryGetValue(userId, out var n))
                    name = n;

                string charId = "";
                if (party.ParticipantCharacters != null && party.ParticipantCharacters.TryGetValue(userId, out var c))
                    charId = c;

                string charImg = "char_placeholder";
                var character = Characters.FirstOrDefault(ch => ch.Id == charId);
                if (character != null) charImg = character.ImageUrl;

                bool ready = party.ReadyPlayerIds != null && party.ReadyPlayerIds.Contains(userId);
                
                int level = 1;
                if (party.ParticipantLevels != null && party.ParticipantLevels.TryGetValue(userId, out var lvl))
                    level = lvl;

                Players.Add(new PlayerItem
                {
                    Name = name.ToUpper(),
                    CharacterImage = charImg,
                    IsReady = ready,
                    Level = level
                });
            }
        });
    }

    private async Task ExecuteToggleReady()
    {
        // No permitir cambiar el estado de listo si ya estamos votando o en juego
        if (IsVoting) return;

        var user = _accountService.GetCurrentUser();
        if (user == null) return;

        IsReady = !IsReady;
        await _gameService.ToggleReadyAsync(_roomCode, user.Id, IsReady);
        
        if (_connection.IsConnected)
        {
            await _connection.SendMessageAsync($"READY|{user.Id}|{IsReady}");
        }
    }

    private async Task SelectCharacterAsync(Character character)
    {
        if (character == null) return;

        IsCharacterListVisible = false;
        SelectedCharacterName = character.Name;
        SelectedCharacterImage = character.ImageUrl;

        var user = _accountService.GetCurrentUser();
        if (user != null)
        {
            user.SelectedCharacterId = character.Id;
            await _accountService.UpdateUser(user);

            if (_connection.IsConnected)
            {
                await _connection.SendMessageAsync($"SYNC_CHAR|{user.Id}|{character.Id}|{user.Username}");
            }
        }
    }

    public void Cleanup()
    {
        _lobbyListener?.Dispose();
    }
}
