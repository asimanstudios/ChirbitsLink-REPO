using System.Threading.Tasks;
using System.Windows.Input;
using ChibitsLink.main.cs.service;

namespace ChibitsLink.main.cs.viewmodel;

public class LoginPageViewModel : BaseViewModel
{
    private readonly AccountService _accountService;
    private string _username = string.Empty;
    private string _password = string.Empty;

    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    public ICommand LoginCommand { get; }

    public LoginPageViewModel(AccountService accountService)
    {
        _accountService = accountService;
        LoginCommand = new Command(async () => await ExecuteLogin());
    }

    private async Task ExecuteLogin()
    {
        bool hasInput = !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

        if (hasInput)
        {
            IsBusy = true;
            try
            {
                await _accountService.Login(Username, Password);
                await Shell.Current.GoToAsync("//MainMenuPage");
            }
            catch (ChibitsLink.main.cs.exception.AuthException ex)
            {
                await Shell.Current.DisplayAlert("Credenciales Inválidas", ex.Message, "Reintentar");
            }
            catch (ChibitsLink.main.cs.exception.DatabaseException ex)
            {
                await Shell.Current.DisplayAlert("Error de Perfil", $"No pudimos cargar tus datos de jugador: {ex.Message}", "Entendido");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
