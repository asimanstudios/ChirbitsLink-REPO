using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.viewmodel;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository.interfaces;

namespace ChibitsLink.main.cs.view;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;
    private readonly AccountService _accountService;
    private readonly IMasterDataRepository _masterRepo;
    private readonly IUserRepository _userRepo;

    public HistoryPage(HistoryViewModel viewModel, AccountService accountService, IMasterDataRepository masterRepo, IUserRepository userRepo)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _accountService = accountService;
        _masterRepo = masterRepo;
        _userRepo = userRepo;

        BindingContext = _viewModel;

        // La vista abre el modal: el ViewModel dispara el evento, la Vista lo gestiona
        _viewModel.PartySelected += OnPartySelectedAsync;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var user = _accountService.GetCurrentUser();
        if (user == null)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
        else
        {
            await _viewModel.LoadHistoryAsync();
        }
    }

    private async Task OnPartySelectedAsync(HistoryItem item)
    {
        await Navigation.PushModalAsync(new HistoryDetailPage(item.OriginalParty!, _masterRepo, _userRepo));
    }
}
