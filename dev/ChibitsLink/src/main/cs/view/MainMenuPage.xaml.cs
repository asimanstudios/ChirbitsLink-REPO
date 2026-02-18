using System.Collections.ObjectModel;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;

namespace ChibitsLink.main.cs.view;

public partial class MainMenuPage : ContentPage
{
    private readonly AccountService _accountService;
    public ObservableCollection<Character> Characters { get; set; } = new();

    public MainMenuPage(AccountService accountService)
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
            UsernameLabel.Text = user.Username.ToUpper();
        }
    }

    private void LoadCharacters()
    {
        // Mock data for Chimparty style
        Characters.Clear();
        Characters.Add(new Character { Name = "VALIENTE", ImageUrl = "dotnet_bot.png" });
        Characters.Add(new Character { Name = "MAGA", ImageUrl = "dotnet_bot.png" });
        Characters.Add(new Character { Name = "PICARO", ImageUrl = "dotnet_bot.png" });
        Characters.Add(new Character { Name = "BRUJO", ImageUrl = "dotnet_bot.png" });
        
        CharactersCollection.ItemsSource = Characters;
    }

    private async void OnCharacterSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Character character)
        {
            // Visual feedback could be added here
            await DisplayAlert("Personaje", $"Has elegido a {character.Name}", "OK");
        }
    }

    private async void OnJoinClicked(object sender, EventArgs e)
    {
        string code = RoomCodeEntry.Text;
        if (string.IsNullOrEmpty(code) || code.Length != 6)
        {
            await DisplayAlert("Ups", "El código debe ser de 6 dígitos mágicos.", "Vale");
            return;
        }

        // Logic to join party
        await DisplayAlert("¡Genial!", $"Uniéndose a la sala {code}...", "¡VAMOS!");
        await Shell.Current.GoToAsync("//ControllerPage");
    }

    private async void OnControllerClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ControllerPage");
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SettingsPage");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cerrar Sesión", "¿Seguro que quieres abandonar el reino?", "Sí, salir", "Me quedo");
        if (confirm)
        {
            _accountService.Logout();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
