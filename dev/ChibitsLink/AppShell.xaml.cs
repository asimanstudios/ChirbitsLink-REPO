namespace ChibitsLink;

using ChibitsLink.main.cs.view;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("JoinRoomPage", typeof(JoinRoomPage));
        Routing.RegisterRoute("LobbyPage", typeof(LobbyPage));
        Routing.RegisterRoute("HistoryPage", typeof(HistoryPage));
	}

    public void UpdateHeader(string username)
    {
        if (FlyoutUsernameLabel != null)
        {
            FlyoutUsernameLabel.Text = username.ToUpper();
        }
    }
}
