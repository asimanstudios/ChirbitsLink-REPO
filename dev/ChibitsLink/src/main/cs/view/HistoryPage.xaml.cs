using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;
using System.Collections.ObjectModel;

namespace ChibitsLink.main.cs.view;

public partial class HistoryPage : ContentPage
{
    private readonly AccountService _accountService;
    public ObservableCollection<LobbyHistory> History { get; set; } = new();

    public HistoryPage(AccountService accountService)
    {
        InitializeComponent();
        _accountService = accountService;
        BindingContext = this;
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

        await LoadHistory();
    }

    private async Task LoadHistory()
    {
        var user = _accountService.GetCurrentUser();
        if (user == null) return;

        // In a real app, fetch from Database repository
        // var historyData = await _db.GetUserHistory(user.Id);
        
        // Demo data
        History.Clear();
        History.Add(new LobbyHistory { RoomCode = "123456", Date = DateTime.Now.AddDays(-1), Won = true });
        History.Add(new LobbyHistory { RoomCode = "789012", Date = DateTime.Now.AddDays(-2), Won = false });
        History.Add(new LobbyHistory { RoomCode = "456789", Date = DateTime.Now.AddDays(-5), Won = true });

        HistoryCollection.ItemsSource = History;
    }
}
