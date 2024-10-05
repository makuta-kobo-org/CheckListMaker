using CheckListMaker.ViewModels;
using Microsoft.Extensions.Configuration;
using Plugin.MauiMTAdmob;

namespace CheckListMaker;

/// <summary> App </summary>
public partial class App : Application
{
    private static bool _isDark;

    /// <summary> Constructor </summary>
    public App(AppShellViewModel viewModel)
    {
        InitializeComponent();

        // AdMob global preferences
        CrossMauiMTAdmob.Current.ComplyWithFamilyPolicies = true;
        CrossMauiMTAdmob.Current.UseRestrictedDataProcessing = true;

        _isDark = Preferences.Default.Get("is_dark", false);
        SetTheme(_isDark);

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

    private static void SetTheme(bool isDark) =>
        Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
}
