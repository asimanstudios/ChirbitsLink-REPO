using ChibitsLink.main.cs.viewmodel;

namespace ChibitsLink.main.cs.view;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterPageViewModel _viewModel;

    public RegisterPage(RegisterPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
