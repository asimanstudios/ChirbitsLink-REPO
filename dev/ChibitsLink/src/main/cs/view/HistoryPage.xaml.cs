using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;
using System.Collections.ObjectModel;

namespace ChibitsLink.main.cs.view;

public partial class HistoryPage : ContentPage
{
    private readonly AccountService _accountService;
    private readonly ChibitsLink.main.repository.Database _db;
    public ObservableCollection<PartyHistory> History { get; set; } = new();

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
            SetLoading(true);
            var historyData = await _db.GetUserHistory(user.Id);
            
            History.Clear();
            foreach (var item in historyData)
            {
                // Asegurar que si Date es el valor por defecto, usamos el Timestamp
                if (item.Date == default) item.Date = item.Timestamp;
                History.Add(item);
            }

            HistoryCollection.ItemsSource = History;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading history: {ex.Message}");
            await DisplayAlert("Error", "No se pudo cargar el historial de batallas.", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
        HistoryCollection.IsVisible = !isLoading;
    }
}
