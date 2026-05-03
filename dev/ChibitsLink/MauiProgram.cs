using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.net;
using ChibitsLink.main.cs.view;
using ChibitsLink.main.cs.controller;
using ChibitsLink.main.cs.viewmodel;
using ChibitsLink.main.repository;
using ChibitsLink.main.repository.interfaces;

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
				fonts.AddFont("Fredoka-Regular.ttf", "FredokaRegular");
				fonts.AddFont("Fredoka-Bold.ttf", "FredokaBold");
				fonts.AddFont("Fredoka-SemiBold.ttf", "FredokaSemiBold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // Repositories
        builder.Services.AddSingleton<FirebaseConnection>();
        builder.Services.AddSingleton<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<ILobbyRepository, LobbyRepository>();
        builder.Services.AddSingleton<IMasterDataRepository, MasterDataRepository>();
        builder.Services.AddSingleton<DatabaseSeeder>();

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

        // ViewModels
        builder.Services.AddTransient<MainMenuViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<LoginPageViewModel>();
        builder.Services.AddTransient<RegisterPageViewModel>();
        builder.Services.AddTransient<JoinRoomViewModel>();
        builder.Services.AddTransient<LobbyViewModel>();

        // Controllers (Services for the pages)
        builder.Services.AddSingleton<AccountController>();
        builder.Services.AddSingleton<ControllerController>();
        builder.Services.AddSingleton<ConexionController>();

        var app = builder.Build();
		return app;
	}
}
