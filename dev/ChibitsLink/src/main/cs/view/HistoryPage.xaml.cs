using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ChibitsLink.main.cs.view;

public class PartyHistoryItem
{
    public Party Party { get; set; } = null!;
    public bool Won { get; set; }
    public int Score { get; set; }
    public DateTime Date => Party.CreatedAt;
    public string RoomCode => Party.RoomCode;
}

public partial class HistoryPage : ContentPage
{
    private readonly AccountService _accountService;
    private readonly ChibitsLink.main.repository.Database _db;
    public ObservableCollection<PartyHistoryItem> History { get; set; } = new();
    public ICommand RefreshCommand { get; }

    public HistoryPage(AccountService accountService, ChibitsLink.main.repository.Database db)
    {
        InitializeComponent();
        _accountService = accountService;
        _db = db;
        RefreshCommand = new Command(async () => await LoadHistory());
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
            foreach (var party in historyData)
            {
                bool won = false;
                int myScore = 0;
                if (party.PlayerScores != null && party.PlayerScores.ContainsKey(user.Id))
                {
                    myScore = party.PlayerScores[user.Id];
                    if (party.PlayerScores.Count > 0)
                    {
                        int maxScore = party.PlayerScores.Values.Max();
                        won = myScore == maxScore && myScore > 0;
                    }
                }

                History.Add(new PartyHistoryItem 
                { 
                    Party = party, 
                    Won = won, 
                    Score = myScore 
                });
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
            RefreshControl.IsRefreshing = false;
        }
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
        HistoryCollection.IsVisible = !isLoading;
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as PartyHistoryItem;
        if (item == null) return;

        // Deseleccionar para permitir volver a pulsar
        ((CollectionView)sender).SelectedItem = null;

        var party = item.Party;
        // Navegar a la pantalla de detalle épica
        await Navigation.PushModalAsync(new HistoryDetailPage(party, _db));
    }
}
