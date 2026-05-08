using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ChibitsLink.main.cs.exception;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.repository.interfaces;

namespace ChibitsLink.main.cs.viewmodel;

public class HistoryViewModel : BaseViewModel
{
    private readonly AccountService _accountService;
    private readonly IUserRepository _userRepo;
    private readonly IMasterDataRepository _masterRepo;
    
    /// <summary>
    /// Evento disparado cuando el usuario selecciona una partida del historial.
    /// La Vista (HistoryPage) es la responsable de abrir el detalle modal.
    /// </summary>
    public event Func<HistoryItem, Task>? PartySelected;

    public ObservableCollection<HistoryItem> History { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand SelectPartyCommand { get; }

    public HistoryViewModel(AccountService accountService, IUserRepository userRepo, IMasterDataRepository masterRepo)
    {
        _accountService = accountService;
        _userRepo = userRepo;
        _masterRepo = masterRepo;

        RefreshCommand = new Command(async () => await LoadHistoryAsync());
        SelectPartyCommand = new Command<HistoryItem>(async (item) => await OnPartySelectedAsync(item));
    }

    public async Task LoadHistoryAsync()
    {
        var user = _accountService.GetCurrentUser();
        if (user != null)
        {
            IsBusy = true;
            try
            {
                var historyData = await _userRepo.GetUserHistoryAsync(user.Id);
                History.Clear();

                foreach (var party in historyData.OrderByDescending(p => p.CreatedAt))
                {
                    History.Add(new HistoryItem
                    {
                        OriginalParty = party,
                        Code = $"SALA #{party.RoomCode}",
                        DateText = party.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                        PlayerCount = $"👥 {party.PlayerIds?.Count ?? 0} JUGADORES"
                    });
                }
            }
            catch (DatabaseException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HistoryViewModel] Firestore error: {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("[HistoryViewModel] Load cancelled.");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private async Task OnPartySelectedAsync(HistoryItem item)
    {
        if (item?.OriginalParty != null && PartySelected != null)
        {
            await PartySelected.Invoke(item);
        }
    }
}
