using System.ComponentModel;
using System.Reflection;
using CheckListMaker.Controls;
using CheckListMaker.Factories;
using CheckListMaker.Models;
using CheckListMaker.Services;
using CheckListMaker.ViewModels;
using CheckListMaker.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plugin.MauiMTAdmob;

namespace CheckListMaker;

/// <summary>
/// Entry point for the Maui application.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the Maui application.
    /// </summary>
    /// <returns>A configured <see cref="MauiApp"/> instance.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiMTAdmob()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("MPLUS1p-Regular.ttf", "MPLUS1p-Regular");
                fonts.AddFont("fontello.ttf", "fontello");
            });

        // Load appsettings.json based on the environment
#if DEBUG
        var env = "Development";
#else
        var env = "Production";
#endif
        using var appsettings = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream($"CheckListMaker.appsettings.{env}.json");

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonStream(appsettings)
            .Build();

        builder.Configuration.AddConfiguration(configBuilder);

        // Register services for dependency injection
        RegisterServices(builder.Services, builder.Configuration);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    /// <summary>
    /// Registers services and dependencies into the service provider.
    /// </summary>
    /// <param name="services">The service collection to register dependencies.</param>
    /// <param name="config">The application configuration.</param>
    private static void RegisterServices(
        IServiceCollection services,
        IConfiguration config)
    {
        // Register constants
        services.AddSingleton<AdMobConstants>(
            options => config.GetRequiredSection("AdMob").Get<AdMobConstants>());

        // Register services
        services.AddTransient<IMediaService, MediaService>();
        services.AddTransient<IAlertService, AlertService>();
        services.AddSingleton<IComputerVisionService>(
            options => ComputerVisionService.GetInstance(config));
        services.AddSingleton<ILiteDbService, LiteDbService>(options =>
            {
                var dbFilePath = Path.Combine(
                    FileSystem.Current.AppDataDirectory,
                    config["LiteDb:FileName"]);

                var upperLimit = int.TryParse(config["LiteDb:UpperLimit"], out var parsedValue) ? parsedValue : 10;

                return new LiteDbService(dbFilePath, upperLimit);
            });

        // Register controls
        services.AddTransient<ICustomPopupService, CustomPopupService>();

        // Register views and view models
        services.AddTransient<AppShell, AppShellViewModel>();
        services.AddTransientViewAndViewModel<MainView, MainViewModel>();
        services.AddTransientViewAndViewModel<SettingsView, SettingsViewModel>();
        services.AddTransientViewAndViewModel<AboutView, AboutViewModel>();
        services.AddTransientViewAndViewModel<HistoryView, HistoryViewModel>();

        // Register factories
        services.AddSingleton<IAddItemPopupViewFactory, AddItemPopupViewFactory>();
    }

    /// <summary>
    /// Registers a view and its corresponding view model into the service provider.
    /// Also sets the view model as the binding context for the view.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <param name="services">The service collection to register dependencies.</param>
    /// <returns>The updated service collection.</returns>
    private static IServiceCollection AddTransientViewAndViewModel<TView, TViewModel>(this IServiceCollection services)
        where TView : BindableObject, new()
        where TViewModel : class, INotifyPropertyChanged =>
            services
            .AddTransient<TViewModel>()
            .AddTransient(serviceProvider => new TView() { BindingContext = serviceProvider.GetService(typeof(TViewModel)) });
}
