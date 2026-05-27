using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.repository.interfaces;
using ChibitsLink.main.cs.net;

namespace ChibitsLink.main.cs.viewmodel;

public class MainMenuViewModel : BaseViewModel
{
    private readonly AccountService _accountService;
    private readonly IMasterDataRepository _masterRepo;
    private readonly ILobbyRepository _lobbyRepo;
    private readonly Connection _connection;

    private string _username = string.Empty;
    private string _levelDisplay = "LVL. 1";
    private double _xpProgress = 0;
    private string _xpStatus = "0 / 5000 XP";
    private string _selectedCharacterName = "SIN PERSONAJE";
    private string _selectedCharacterImage = "char_placeholder";
    private bool _isCharacterListVisible = false;

    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string LevelDisplay { get => _levelDisplay; set => SetProperty(ref _levelDisplay, value); }
    public double XpProgress { get => _xpProgress; set => SetProperty(ref _xpProgress, value); }
    public string XpStatus { get => _xpStatus; set => SetProperty(ref _xpStatus, value); }
    public string SelectedCharacterName { get => _selectedCharacterName; set => SetProperty(ref _selectedCharacterName, value); }
    public string SelectedCharacterImage { get => _selectedCharacterImage; set => SetProperty(ref _selectedCharacterImage, value); }
    public bool IsCharacterListVisible { get => _isCharacterListVisible; set => SetProperty(ref _isCharacterListVisible, value); }

    public ObservableCollection<Character> Characters { get; } = new();

    public ICommand ToggleCharacterListCommand { get; }
    public ICommand SelectCharacterCommand { get; }

    public MainMenuViewModel(AccountService accountService, IMasterDataRepository masterRepo, ILobbyRepository lobbyRepo, Connection connection)
    {
        _accountService = accountService;
        _masterRepo = masterRepo;
        _lobbyRepo = lobbyRepo;
        _connection = connection;

        ToggleCharacterListCommand = new Command(() => IsCharacterListVisible = !IsCharacterListVisible);
        SelectCharacterCommand = new Command<Character>(async (c) => await SelectCharacterAsync(c));
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            // Ejecutamos las tareas de red en paralelo para reducir el tiempo de espera a la mitad
            var xpTask = _accountService.CheckAndClaimPendingExperienceAsync(_lobbyRepo);
            var charTask = LoadCharactersAsync();

            await Task.WhenAll(xpTask, charTask);
            
            // Actualizamos la UI una vez que ambas tareas han terminado
            UpdateUserData(); 
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateUserData()
    {
        var user = _accountService.GetCurrentUser();
        if (user != null)
        {
            Username = user.Username.ToUpper();
            LevelDisplay = $"LVL. {user.Level}";

            // Lógica de progreso de XP (5k por nivel con escala incremental)
            int currentLevel = user.Level;
            int totalXp = user.Experience;
            int xpForCurrentLevel = 0;
            for (int i = 1; i < currentLevel; i++) xpForCurrentLevel += i * 5000;

            int xpIntoLevel = Math.Max(0, totalXp - xpForCurrentLevel);
            int xpNeeded = currentLevel * 5000;

            XpProgress = (double)xpIntoLevel / xpNeeded;
            XpStatus = $"{xpIntoLevel} / {xpNeeded} XP";
        }
    }

    private async Task LoadCharactersAsync()
    {
        var dbChars = await _masterRepo.GetCharactersAsync();
        Characters.Clear();
        foreach (var c in dbChars) Characters.Add(c);

        var user = _accountService.GetCurrentUser();
        if (user != null)
        {
            var selected = Characters.FirstOrDefault(charac => charac.Id == user.SelectedCharacterId);
            if (selected != null)
            {
                SelectedCharacterName = selected.Name;
                SelectedCharacterImage = selected.ImageUrl;
            }
        }
    }

    private async Task SelectCharacterAsync(Character character)
    {
        if (character != null)
        {
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
    }
}
