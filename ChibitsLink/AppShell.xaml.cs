namespace ChibitsLink;

using ChibitsLink.main.cs.view;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
	}
}
