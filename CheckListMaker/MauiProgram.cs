using System.ComponentModel;
using System.Reflection;
using CheckListMaker.Controls;
using CheckListMaker.Models;
using CheckListMaker.Services;
using CheckListMaker.ViewModels;
using CheckListMaker.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plugin.MauiMTAdmob;

namespace CheckListMaker;

/// <summary> MauiProgram起点 </summary>
public static class MauiProgram
{
    /// <summary> CreateMauiApp </summary>
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

        // appsettings.json
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

        // DI
        RegisterServices(builder.Services, builder.Configuration);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    /// <summary> ServiceProvider の設定</summary>
    private static void RegisterServices(
        IServiceCollection services,
        IConfiguration config)
    {
        // Constants
        services.AddSingleton<AdMobConstants>(
            options => config.GetRequiredSection("AdMob").Get<AdMobConstants>());

        // Services
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

        // Controls
        services.AddTransient<IMyPopupService, MyPopupService>();

        // Views and ViewModels
        services.AddTransient<AppShell, AppShellViewModel>();
        services.AddTransientViewAndViewModel<MainView, MainViewModel>();
        services.AddTransientViewAndViewModel<SettingsView, SettingsViewModel>();
        services.AddTransientViewAndViewModel<AboutView, AboutViewModel>();
        services.AddTransientViewAndViewModel<HistoryView, HistoryViewModel>();
    }

    /// <summary> ViewとViewModelのService登録およびBindincContextへの設定 </summary>
    private static IServiceCollection AddTransientViewAndViewModel<TView, TViewModel>(this IServiceCollection services)
        where TView : BindableObject, new()
        where TViewModel : class, INotifyPropertyChanged =>
            services
            .AddTransient<TViewModel>()
            .AddTransient(serviceProvider => new TView() { BindingContext = serviceProvider.GetService(typeof(TViewModel)) });
}
