using System.Collections.ObjectModel;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;

namespace ChibitsLink.main.cs.view;

[QueryProperty(nameof(RoomCode), "code")]
public partial class LobbyPage : ContentPage
{
    private readonly AccountService _accountService;
    private string _roomCode = "";
    private bool _isReady = false;
    public ObservableCollection<Character> Characters { get; set; } = new();

    public string RoomCode
    {
        get => _roomCode;
        set
        {
            _roomCode = value;
            RoomTitleLabel.Text = $"SALA #{_roomCode}";
        }
    }

    public LobbyPage(AccountService accountService)
    {
        InitializeComponent();
        _accountService = accountService;
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
        Characters.Add(new Character { Id = "1", Name = "VALIENTE", ImageUrl = "dotnet_bot.png" });
        Characters.Add(new Character { Id = "2", Name = "MAGA", ImageUrl = "dotnet_bot.png" });
        Characters.Add(new Character { Id = "3", Name = "PICARO", ImageUrl = "dotnet_bot.png" });
        Characters.Add(new Character { Id = "4", Name = "BRUJO", ImageUrl = "dotnet_bot.png" });
        
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
            }
            
            CharactersCollection.SelectedItem = null;
        }
    }

    private async void OnReadyClicked(object sender, EventArgs e)
    {
        _isReady = !_isReady;
        
        if (_isReady)
        {
            ReadyButton.Text = "¡LISTO!";
            ReadyButton.BackgroundColor = Color.FromArgb("#00B894"); // Success color
            
            // Simular inicio de partida después de que otros estén listos
            await Task.Delay(2000);
            await Shell.Current.GoToAsync("//ControllerPage");
        }
        else
        {
            ReadyButton.Text = "MARCAR COMO LISTO";
            ReadyButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
        }
    }

    private async void OnLeaveClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
