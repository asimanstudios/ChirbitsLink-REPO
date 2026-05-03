using System.Collections.ObjectModel;
using System.Windows.Input;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.repository.interfaces;
using ChibitsLink.main.cs.view;

namespace ChibitsLink.main.cs.viewmodel;

/// <summary>
/// Modelo simple para la lista del historial. Evita problemas de binding con modelos complejos.
/// </summary>
public class HistoryItem
{
    public Party OriginalParty { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string DateText { get; set; } = string.Empty;
    public string PlayerCount { get; set; } = string.Empty;
}

public class HistoryViewModel : BaseViewModel
{
    private readonly AccountService _accountService;
    private readonly IUserRepository _userRepo;
    private readonly IMasterDataRepository _masterRepo;
    
    public ObservableCollection<HistoryItem> History { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand SelectPartyCommand { get; }

    public HistoryViewModel(AccountService accountService, IUserRepository userRepo, IMasterDataRepository masterRepo)
    {
        _accountService = accountService;
        _userRepo = userRepo;
        _masterRepo = masterRepo;
        
        RefreshCommand = new Command(async () => await LoadHistoryAsync());
        SelectPartyCommand = new Command<HistoryItem>(async (item) => await OpenDetailAsync(item));
    }

    public async Task LoadHistoryAsync()
    {
        var user = _accountService.GetCurrentUser();
        if (user == null) return;

        IsBusy = true;
        try
        {
            var historyData = await _userRepo.GetUserHistoryAsync(user.Id);
            History.Clear();
            
            // Mapeo manual y seguro
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryViewModel] Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenDetailAsync(HistoryItem item)
    {
        if (item?.OriginalParty == null) return;
        await Shell.Current.Navigation.PushModalAsync(new HistoryDetailPage(item.OriginalParty, _masterRepo, _userRepo));
    }
}
