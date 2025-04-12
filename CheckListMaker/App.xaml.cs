using CheckListMaker.Services;
using CheckListMaker.ViewModels;
using Plugin.MauiMTAdmob;

namespace CheckListMaker;

/// <summary> App </summary>
public partial class App : Application
{
    private static bool _isDark;
    private readonly ILiteDbService _liteDbService;

    /// <summary> Constructor </summary>
    public App(AppShellViewModel viewModel, ILiteDbService liteDbService)
    {
        InitializeComponent();

        // AdMob global preferences
        CrossMauiMTAdmob.Current.ComplyWithFamilyPolicies = true;
        CrossMauiMTAdmob.Current.UseRestrictedDataProcessing = true;

        _isDark = Preferences.Default.Get("is_dark", false);
        SetTheme(_isDark);

        _liteDbService = liteDbService;

        MainPage = new AppShell(viewModel);
    }

    /// <summary> ダークモード判定のフラグ </summary>
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

    protected override void OnStart()
    {
        base.OnStart();
        // アプリケーション開始時の処理（必要に応じて追加）
    }

    protected override void OnSleep()
    {
        base.OnSleep();

        // アプリケーションがバックグラウンドに移行する際にリソースを解放
        if (_liteDbService is IDisposable disposableService)
        {
            disposableService.Dispose();
        }
    }

    protected override void OnResume()
    {
        base.OnResume();
        // アプリケーションがフォアグラウンドに復帰した際の処理（必要に応じて追加）
    }

    private static void SetTheme(bool isDark) =>
        Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
}
