namespace ChibitsLink;

using ChibitsLink.main.cs.view;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
	}

    public void UpdateHeader(string username)
    {
        // La cabecera visual del menú lateral fue eliminada.
        // Se mantiene el método vacío por si otras clases lo llaman (ej: tras hacer Login).
    }
}
