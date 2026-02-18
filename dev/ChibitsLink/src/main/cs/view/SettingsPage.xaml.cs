using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.model;

namespace ChibitsLink.main.cs.view;

public partial class SettingsPage : ContentPage
{
    private readonly AccountService _accountService;

    public SettingsPage(AccountService accountService)
    {
        InitializeComponent();
        _accountService = accountService;
        
        LoadUserData();
    }

    private void LoadUserData()
    {
        var user = _accountService.GetCurrentUser();
        if (user != null)
        {
            UsernameEntry.Text = user.Username;
            // email is not in User model but we can show it from FirebaseAuth result if needed
            // For now just placeholders or generic
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // R01F02 - Logic to update user in Firestore
        await DisplayAlert("Éxito", "Tus ajustes han sido guardados en el pergamino real.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
