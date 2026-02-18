using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.net;
using ChibitsLink.main.cs.view;
using ChibitsLink.main.cs.controller;

namespace ChibitsLink;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // Repositories
        builder.Services.AddSingleton<ChibitsLink.main.repository.FirebaseConnection>();
        builder.Services.AddSingleton<ChibitsLink.main.repository.Database>();

        // Services
        builder.Services.AddSingleton<ChibitsLink.main.cs.net.Connection>();
        builder.Services.AddSingleton<AccountService>();
        builder.Services.AddSingleton<BluetoothService>();
        builder.Services.AddSingleton<ControllerService>();
        builder.Services.AddSingleton<GameService>();
#if ANDROID
        builder.Services.AddSingleton<IOrientationService, ChibitsLink.Platforms.Android.OrientationService>();
#endif

        // Pages
        builder.Services.AddTransient<IntroPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ControllerPage>();
        builder.Services.AddTransient<SelectionPage>();
        builder.Services.AddTransient<MainMenuPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<JoinRoomPage>();
        builder.Services.AddTransient<LobbyPage>();
        builder.Services.AddTransient<HistoryPage>();

        // Controllers (Services for the pages)
        builder.Services.AddSingleton<AccountController>();
        builder.Services.AddSingleton<ControllerController>();
        builder.Services.AddSingleton<ConexionController>();

		return builder.Build();
	}
}
