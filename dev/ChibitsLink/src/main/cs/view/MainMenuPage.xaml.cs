using System.Diagnostics;
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
            UpdateProfileDisplay(user.SelectedCharacterId);

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

    private void UpdateProfileDisplay(string characterId)
    {
        Debug.WriteLine($"[MainMenu] Buscando personaje con ID: {characterId}");
        var character = Characters.FirstOrDefault(c => c.Id == characterId) 
                        ?? Characters.FirstOrDefault();
        
        if (character != null)
        {
            // Fallback for missing/null ImageUrl
            var imageUrl = string.IsNullOrEmpty(character.ImageUrl) ? "char_placeholder.png" : character.ImageUrl;
            
            // Critical fix: Ensure we don't try to load known missing default assets
            if (imageUrl == "dotnet_bot.png" || imageUrl.Contains("char_knight.png") || imageUrl.Contains("char_valiente.png"))
            {
                imageUrl = "char_placeholder.png";
            }

            ProfileImage.Source = imageUrl;
            CharacterNameLabel.Text = character.Name;
            Debug.WriteLine($"[MainMenu] UI Actualizada: {character.Name} con imagen {imageUrl}");
        }

        var user = _accountService.GetCurrentUser();
        if (user != null)
        {
            UserLevelLabel.Text = $"LVL. {user.Level}";
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
                foreach (var c in dbCharacters) 
                {
                    // Sanitize ImageUrl
                    if (string.IsNullOrEmpty(c.ImageUrl) || c.ImageUrl == "dotnet_bot.png" || (c.ImageUrl.Contains("char_") && !c.ImageUrl.Contains("placeholder")))
                    {
                        c.ImageUrl = "char_placeholder.png";
                    }
                    Characters.Add(c);
                }
            }
            else
            {
                AddMockCharacters();
            }

            // Refresh profile display now that we have character data
            var user = _accountService.GetCurrentUser();
            if (user != null)
            {
                UpdateProfileDisplay(user.SelectedCharacterId);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadCharacters Error: {ex.Message}");
            
            // Production improvement: Notify user but keep app running with mocks
            await DisplayAlert("Error de Conexión", "No hemos podido contactar con el reino. Usando héroes de reserva por ahora.", "Vale");
            
            Characters.Clear();
            AddMockCharacters();

            var user = _accountService.GetCurrentUser();
            if (user != null) UpdateProfileDisplay(user.SelectedCharacterId);
        }
        
        CharactersCollection.ItemsSource = Characters;
    }

    private void AddMockCharacters()
    {
        Characters.Add(new Character { Id = "barbarian", Name = "Barbarian", ImageUrl = "char_placeholder.png" });
        Characters.Add(new Character { Id = "rogue", Name = "Rogue", ImageUrl = "char_placeholder.png" });
        Characters.Add(new Character { Id = "knight", Name = "Knight", ImageUrl = "char_placeholder.png" });
    }

    private async void OnCharacterSelected(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Character character)
        {
            Debug.WriteLine($"[MainMenu] Personaje seleccionado mediante Tap: {character.Id}");
            
            // Close the carousel
            CharacterSelectionLayout.IsVisible = false;
            
            // Update UI
            ProfileImage.Source = character.ImageUrl;
            CharacterNameLabel.Text = character.Name;
            
            // Update User Service & DB
            var currentUser = _accountService.GetCurrentUser();
            if (currentUser != null)
            {
                currentUser.SelectedCharacterId = character.Id; 
                await _accountService.UpdateUser(currentUser);

                // Real-time Sync via TCP
                if (_connection.IsConnected)
                {
                    string syncMessage = $"SYNC_CHAR|{currentUser.Id}|{character.Id}|{currentUser.Username}";
                    await _connection.SendMessageAsync(syncMessage);
                    Debug.WriteLine($"[MainMenu] Sync enviado: {syncMessage}");
                }
            }

            // Visual feedback
            await DisplayAlert("¡Héroe Elegido!", $"Tu destino ahora está unido a {character.Name}", "Vale");
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
        Debug.WriteLine("[MainMenu] Perfil tocado. Alternando selector...");
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
            if (_connection.IsConnected) await _connection.DisconnectAsync();
            await _accountService.Logout();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
