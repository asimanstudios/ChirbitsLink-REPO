using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.model;

namespace ChibitsLink.main.cs.view;

public partial class SettingsPage : ContentPage
{
    private readonly AccountService _accountService;

    public SettingsPage(AccountService accountService)
    {
        InitializeComponent();
        _accountService = accountService;
        
        LoadUserData();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var user = _accountService.GetCurrentUser();
        if (user == null)
        {
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }
    }

    private void LoadUserData()
    {
        var user = _accountService.GetCurrentUser();
        if (user != null)
        {
            RealNameEntry.Text = user.RealName;
            UsernameEntry.Text = user.Username;
            EmailEntry.Text = user.Id; // Placeholder - ideally fetch actual email
        }

        // Load Networking Settings
        ServerIpEntry.Text = Preferences.Get("pref_server_ip", "127.0.0.1");
        ServerPortEntry.Text = Preferences.Get("pref_server_port", "11000").ToString();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var user = _accountService.GetCurrentUser();
        if (user != null)
        {
            user.RealName = RealNameEntry.Text;
            user.Username = UsernameEntry.Text;
            
            await _accountService.UpdateUser(user);

            // Sync Shell UI
            if (Shell.Current is AppShell shell)
            {
                shell.UpdateHeader(user.Username);
            }

            // Handle email update
            if (!string.IsNullOrEmpty(EmailEntry.Text) && EmailEntry.Text != user.Id)
            {
                var emailResult = await _accountService.UpdateEmail(EmailEntry.Text);
                if (!emailResult.Success)
                {
                    await DisplayAlert("Error Email", emailResult.ErrorMessage, "OK");
                }
            }

            // Handle password update
            if (!string.IsNullOrEmpty(NewPasswordEntry.Text))
            {
                var passResult = await _accountService.ChangePassword(NewPasswordEntry.Text);
                if (!passResult.Success)
                {
                    await DisplayAlert("Error Contraseña", passResult.ErrorMessage, "OK");
                }
            }

            // Save Networking Settings
            Preferences.Set("pref_server_ip", ServerIpEntry.Text ?? "127.0.0.1");
            if (int.TryParse(ServerPortEntry.Text, out int port))
            {
                Preferences.Set("pref_server_port", port);
            }

            await DisplayAlert("Éxito", "Tus ajustes han sido guardados en el pergamino real.", "OK");
            await Shell.Current.GoToAsync("//MainMenuPage"); // Direct navigation to ensure "Volver" feel
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainMenuPage");
    }
}
