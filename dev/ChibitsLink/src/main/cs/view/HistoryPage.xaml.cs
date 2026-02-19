using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;
using System.Collections.ObjectModel;

namespace ChibitsLink.main.cs.view;

public partial class HistoryPage : ContentPage
{
    private readonly AccountService _accountService;
    private readonly ChibitsLink.main.repository.Database _db;
    public ObservableCollection<LobbyHistory> History { get; set; } = new();

    public HistoryPage(AccountService accountService, ChibitsLink.main.repository.Database db)
    {
        InitializeComponent();
        _accountService = accountService;
        _db = db;
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

        try
        {
            var historyData = await _db.GetUserHistory(user.Id);
            
            History.Clear();
            foreach (var item in historyData)
            {
                History.Add(item);
            }

            HistoryCollection.ItemsSource = History;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading history: {ex.Message}");
        }
    }
}
