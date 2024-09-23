using CheckListMaker.Messengers;
using CheckListMaker.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Plugin.MauiMTAdmob;

namespace CheckListMaker;

/// <summary> App </summary>
public partial class App : Application
{
    private static bool _isDark;
    private static bool _requiresSave;

    /// <summary> Constructor </summary>
    public App(AppShellViewModel viewModel, IConfiguration config)
    {
        InitializeComponent();

        // AdMob global preferences
        CrossMauiMTAdmob.Current.ComplyWithFamilyPolicies = true;
        CrossMauiMTAdmob.Current.UseRestrictedDataProcessing = true;

        _requiresSave = Preferences.Default.Get("requires_save", false);

        _isDark = Preferences.Default.Get("is_dark", false);
        SetTheme(_isDark);

        MainPage = new AppShell(viewModel);
    }

    /// <summary> 状態保存をするかのフラグ </summary>
    public static bool RequiresSave
    {
        get => _requiresSave;
        set
        {
            if (_requiresSave == value)
            {
                return;
            }

            _requiresSave = value;
            Preferences.Default.Set("requires_save", _requiresSave);

            // Trueなら現在のCheckListアイテムを保存
            if (_requiresSave)
            {
                WeakReferenceMessenger.Default.Send(new SaveSettingsChangedMessage(null));
            }
        }
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

    private static void SetTheme(bool isDark) => Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
}
