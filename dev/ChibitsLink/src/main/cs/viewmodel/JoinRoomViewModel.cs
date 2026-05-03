using System.Threading.Tasks;
using System.Windows.Input;
using ChibitsLink.main.cs.service;

namespace ChibitsLink.main.cs.viewmodel;

public class JoinRoomViewModel : BaseViewModel
{
    private readonly GameService _gameService;
    private string _roomCode = string.Empty;

    public string RoomCode { get => _roomCode; set => SetProperty(ref _roomCode, value); }

    public ICommand JoinCommand { get; }

    public JoinRoomViewModel(GameService gameService)
    {
        _gameService = gameService;
        JoinCommand = new Command(async () => await ExecuteJoin());
    }

    private async Task ExecuteJoin()
    {
        if (RoomCode != null)
        {
            RoomCode = RoomCode.Trim().ToUpperInvariant();
        }

        if (string.IsNullOrEmpty(RoomCode) || RoomCode.Length != 6)
        {
            await Shell.Current.DisplayAlert("Código Inválido", "El código de la sala debe tener exactamente 6 caracteres.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            bool isValid = await _gameService.ValidateLobbyAsync(RoomCode);
            if (isValid)
            {
                await Shell.Current.GoToAsync($"//LobbyPage?code={RoomCode}");
            }
            else
            {
                await Shell.Current.DisplayAlert("Sala Inexistente", "No hemos encontrado ninguna sala abierta con ese código.", "Entendido");
            }
        }
        catch (ChibitsLink.main.cs.exception.DatabaseException ex)
        {
            string details = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            await Shell.Current.DisplayAlert("Error de Conexión", $"No se pudo verificar la sala: {details}", "OK");
        }
        catch (System.Exception ex)
        {
            await Shell.Current.DisplayAlert("Error Inesperado", $"Ocurrió un error al buscar la sala: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
