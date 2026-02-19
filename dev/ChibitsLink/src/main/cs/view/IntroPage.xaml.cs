using ChibitsLink.main.cs.service;

namespace ChibitsLink.main.cs.view;

public partial class IntroPage : ContentPage
{
    private readonly AccountService _accountService;

    public IntroPage(AccountService accountService)
    {
        InitializeComponent();
        _accountService = accountService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Animation Sequence
        await Task.WhenAll(
            LogoImage.FadeTo(1, 1000),
            LogoImage.ScaleTo(1, 1000, Easing.SpringOut)
        );

        await TitleLabel.FadeTo(1, 500);
        await SubtitleLabel.FadeTo(1, 500);
        
        await Task.Delay(1000); // Pause to appreciate the logo

        // Session Check
        bool hasValidSession = await _accountService.IsSessionActiveAsync();

        if (hasValidSession)
        {
            await Shell.Current.GoToAsync("//MainMenuPage");
        }
        else
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
