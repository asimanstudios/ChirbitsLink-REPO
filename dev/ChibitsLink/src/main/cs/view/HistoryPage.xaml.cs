using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.viewmodel;
using ChibitsLink.main.repository.interfaces;

namespace ChibitsLink.main.cs.view;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;
    private readonly AccountService _accountService;

    public HistoryPage(HistoryViewModel viewModel, AccountService accountService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _accountService = accountService;
        
        BindingContext = _viewModel;
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

        await _viewModel.LoadHistoryAsync();
    }
}
