using System.Windows.Input;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.utils;

namespace ChibitsLink.main.cs.viewmodel;

public class RegisterPageViewModel : BaseViewModel
{
    private readonly AccountService _accountService;
    private string _realName = string.Empty;
    private string _username = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;

    public string RealName { get => _realName; set => SetProperty(ref _realName, value); }
    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

    public ICommand RegisterCommand { get; }

    public RegisterPageViewModel(AccountService accountService)
    {
        _accountService = accountService;
        RegisterCommand = new Command(async () => await ExecuteRegister());
    }

    private async Task ExecuteRegister()
    {
        bool hasMandatoryFields = !string.IsNullOrWhiteSpace(Username)
            && !string.IsNullOrWhiteSpace(Password)
            && !string.IsNullOrWhiteSpace(Email);

        if (!hasMandatoryFields)
        {
            await Shell.Current.DisplayAlert("Campos Vacíos", "Por favor rellena todos los campos mágicos.", "Entendido");
        }
        else if (Password != ConfirmPassword)
        {
            await Shell.Current.DisplayAlert("Error", "Las contraseñas no coinciden, joven héroe.", "Reintentar");
        }
        else
        {
            var (isValid, message) = PasswordValidator.Validate(Password);
            if (!isValid)
            {
                await Shell.Current.DisplayAlert("Contraseña Débil", message, "Mejorar");
            }
            else
            {
                IsBusy = true;
                try
                {
                    await _accountService.RegisterAsync(RealName, Username, Email, Password);
                    await Shell.Current.GoToAsync("//LoginPage");
                }
                catch (ChibitsLink.main.cs.exception.AuthException ex)
                {
                    await Shell.Current.DisplayAlert("Error en Registro", ex.Message, "Entendido");
                }
                catch (ChibitsLink.main.cs.exception.DatabaseException ex)
                {
                    await Shell.Current.DisplayAlert("Problema de Base de Datos", ex.Message, "OK");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}
