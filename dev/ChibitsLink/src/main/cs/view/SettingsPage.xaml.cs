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
        }
    }

    private void LoadUserData()
    {
        var user = _accountService.GetCurrentUser();
        if (user != null)
        {
            RealNameEntry.Text = user.RealName;
            UsernameEntry.Text = user.Username;
            EmailEntry.Text = user.Email;
        }

        // Load Networking Settings
        ServerIpEntry.Text = Preferences.Get("pref_server_ip", "127.0.0.1");
        ServerPortEntry.Text = Preferences.Get("pref_server_port", 11000).ToString();
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

            bool wantsEmailChange = !string.IsNullOrEmpty(EmailEntry.Text) && EmailEntry.Text != user.Email;
            bool wantsPasswordChange = !string.IsNullOrEmpty(NewPasswordEntry.Text);
            bool passwordChangeBlocked = false;

            // Si quiere cambiar email o contraseña, necesita re-autenticarse primero
            if (wantsEmailChange || wantsPasswordChange)
            {
                string currentPassword = CurrentPasswordEntry.Text ?? string.Empty;

                if (string.IsNullOrEmpty(currentPassword))
                {
                    await DisplayAlert("Verificación Requerida", "Introduce tu contraseña actual para modificar el email o la contraseña.", "Entendido");
                    passwordChangeBlocked = true;
                }
                else
                {
                    var reauth = await _accountService.ReauthenticateAsync(currentPassword);
                    if (!reauth.Success)
                    {
                        await DisplayAlert("Verificación Fallida", reauth.ErrorMessage ?? "Contraseña actual incorrecta.", "Reintentar");
                        passwordChangeBlocked = true;
                    }
                }
            }

            // Handle email update (solo si la re-auth fue exitosa)
            if (!passwordChangeBlocked && wantsEmailChange)
            {
                var emailResult = await _accountService.UpdateEmail(EmailEntry.Text);
                if (emailResult.Success)
                {
                    user.Email = EmailEntry.Text;
                }
                else
                {
                    await DisplayAlert("Error Email", emailResult.ErrorMessage, "OK");
                    passwordChangeBlocked = true;
                }
            }

            // Handle password update (solo si la re-auth fue exitosa y no hay bloqueo previo)
            if (!passwordChangeBlocked && wantsPasswordChange)
            {
                if (NewPasswordEntry.Text != ConfirmNewPasswordEntry.Text)
                {
                    await DisplayAlert("Error", "Las contraseñas no coinciden.", "Reintentar");
                    passwordChangeBlocked = true;
                }
                else
                {
                    var (isValid, message) = ChibitsLink.main.cs.utils.PasswordValidator.Validate(NewPasswordEntry.Text);
                    if (!isValid)
                    {
                        await DisplayAlert("Contraseña Débil", message, "OK");
                        passwordChangeBlocked = true;
                    }
                    else
                    {
                        var passResult = await _accountService.ChangePassword(NewPasswordEntry.Text);
                        if (!passResult.Success)
                        {
                            await DisplayAlert("Error Contraseña", passResult.ErrorMessage, "OK");
                            passwordChangeBlocked = true;
                        }
                    }
                }
            }

            if (!passwordChangeBlocked)
            {
                // Save Networking Settings
                Preferences.Set("pref_server_ip", ServerIpEntry.Text ?? "127.0.0.1");
                if (int.TryParse(ServerPortEntry.Text, out int port))
                {
                    Preferences.Set("pref_server_port", port);
                }

                await DisplayAlert("Éxito", "Tus ajustes han sido guardados en el pergamino real.", "OK");
                await Shell.Current.GoToAsync("//MainMenuPage");
            }
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainMenuPage");
    }
}
