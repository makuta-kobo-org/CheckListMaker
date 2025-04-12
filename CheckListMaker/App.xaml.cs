using CheckListMaker.Services;
using CheckListMaker.ViewModels;
using Plugin.MauiMTAdmob;

namespace CheckListMaker;

/// <summary>
/// Represents the main application class for the CheckListMaker app.
/// </summary>
public partial class App : Application
{
    private static bool _isDark;
    private readonly ILiteDbService _liteDbService;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="viewModel">The view model for the application shell.</param>
    /// <param name="liteDbService">The LiteDB service for database operations.</param>
    public App(AppShellViewModel viewModel, ILiteDbService liteDbService)
    {
        InitializeComponent();

        // Configure AdMob global preferences
        CrossMauiMTAdmob.Current.ComplyWithFamilyPolicies = true;
        CrossMauiMTAdmob.Current.UseRestrictedDataProcessing = true;

        _isDark = Preferences.Default.Get("is_dark", false);
        SetTheme(_isDark);

        _liteDbService = liteDbService;

        MainPage = new AppShell(viewModel);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the application is in dark mode.
    /// </summary>
    public static bool IsDark
    {
        get => _isDark;
        set
        {
            if (_isDark == value)
            {
                return;
            }

            _isDark = value;
            SetTheme(_isDark);
            Preferences.Default.Set("is_dark", _isDark);
        }
    }

    /// <summary>
    /// Called when the application starts.
    /// </summary>
    protected override void OnStart() => base.OnStart();

    /// <summary>
    /// Called when the application goes to sleep (background).
    /// </summary>
    protected override void OnSleep()
    {
        base.OnSleep();

        // Release resources when the application moves to the background
        if (_liteDbService is IDisposable disposableService)
        {
            disposableService.Dispose();
        }
    }

    /// <summary>
    /// Called when the application resumes from sleep (foreground).
    /// </summary>
    protected override void OnResume() => base.OnResume();

    /// <summary>
    /// Sets the application theme to dark or light mode.
    /// </summary>
    /// <param name="isDark">A value indicating whether to set the theme to dark mode.</param>
    private static void SetTheme(bool isDark) =>
        Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
}
