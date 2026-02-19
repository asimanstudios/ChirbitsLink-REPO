using System.Collections.ObjectModel;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.repository;
using ChibitsLink.main.cs.net;

namespace ChibitsLink.main.cs.view;

public partial class MainMenuPage : ContentPage
{
    private readonly AccountService _accountService;
    private readonly Database _db;
    private readonly Connection _connection;
    public ObservableCollection<Character> Characters { get; set; } = new();

    public MainMenuPage(AccountService accountService, Database db, Connection connection)
    {
        InitializeComponent();
        _accountService = accountService;
        _db = db;
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
            UsernameLabel.Text = user.Username.ToUpper();
            UpdateProfileImage(user.SelectedCharacterId);

            // Sync Shell UI
            if (Shell.Current is AppShell shell)
            {
                shell.UpdateHeader(user.Username);
            }
        }
        else
        {
            // Security: Redirect to login if no session
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

    private void UpdateProfileImage(string characterId)
    {
        var character = Characters.FirstOrDefault(c => c.Name == characterId) 
                        ?? Characters.FirstOrDefault(); // Default to first if not found
        
        if (character != null)
        {
            ProfileImage.Source = character.ImageUrl;
        }
    }

    private async void LoadCharacters()
    {
        try
        {
            var dbCharacters = await _db.GetCharacters(); 
            
            Characters.Clear();
            if (dbCharacters != null && dbCharacters.Any())
            {
                foreach (var c in dbCharacters) Characters.Add(c);
            }
            else
            {
                AddMockCharacters();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadCharacters Error: {ex.Message}");
            
            // Production improvement: Notify user but keep app running with mocks
            await DisplayAlert("Error de Conexión", "No hemos podido contactar con el reino. Usando héroes de reserva por ahora.", "Vale");
            
            Characters.Clear();
            AddMockCharacters();
        }
        
        CharactersCollection.ItemsSource = Characters;
    }

    private void AddMockCharacters()
    {
        Characters.Add(new Character { Id = "1", Name = "VALIENTE", ImageUrl = "dotnet_bot.png", Level = 5 });
        Characters.Add(new Character { Id = "2", Name = "MAGA", ImageUrl = "dotnet_bot.png", Level = 3 });
        Characters.Add(new Character { Id = "3", Name = "PICARO", ImageUrl = "dotnet_bot.png", Level = 10 });
    }

    private async void OnCharacterSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Character character)
        {
            // Close the carousel
            CharacterSelectionLayout.IsVisible = false;
            
            // Update UI
            ProfileImage.Source = character.ImageUrl;
            LevelLabel.Text = $"Nivel {character.Level}";
            
            // Update User Service & DB
            var currentUser = _accountService.GetCurrentUser();
            if (currentUser != null)
            {
                currentUser.SelectedCharacterId = character.Name; 
                await _accountService.UpdateUser(currentUser);

                // Real-time Sync via TCP
                if (_connection.IsConnected)
                {
                    string syncMessage = $"SYNC_CHAR|{currentUser.Id}|{character.Name}";
                    await _connection.SendMessageAsync(syncMessage);
                }
            }

            // Visual feedback
            await DisplayAlert("¡Héroe Elegido!", $"Tu destino ahora está unido a {character.Name}", "Vale");
            
            // Deselect to allow re-selection
            CharactersCollection.SelectedItem = null;
        }
    }

    private async void OnControllerClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ControllerPage");
    }

    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("HistoryPage");
    }

    private void OnProfileTapped(object sender, EventArgs e)
    {
        CharacterSelectionLayout.IsVisible = !CharacterSelectionLayout.IsVisible;
    }

    private async void OnJoinRoomClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("JoinRoomPage");
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
            await _accountService.Logout();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
