using CheckListMaker.Messengers;
using CheckListMaker.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;

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
        // CrossMauiMTAdmob.Current.ComplyWithFamilyPolicies = true;
        // CrossMauiMTAdmob.Current.UseRestrictedDataProcessing = true;

        // AdMobConstants =
        //     config.GetRequiredSection("AdMob").Get<AdMobConstantsRecord>();

        RequiresSave = Preferences.Default.Get("requires_save", false);

        IsDark = Preferences.Default.Get("is_dark", false);

        MainPage = new AppShell(viewModel);
    }

    /// <summary> 状態保存をするかのフラグ </summary>
    public static bool RequiresSave
    {
        get => _requiresSave;
        set
        {
            if (_requiresSave != value)
            {
                _requiresSave = value;
                Preferences.Default.Set("requires_save", RequiresSave);

                // Trueなら現在のCheckListアイテムを保存
                if (RequiresSave)
                {
                    WeakReferenceMessenger.Default.Send(new SaveSettingsChangedMessage(null));
                }
            }
        }
    }

    /// <summary> ダークモード判定のフラグ </summary>
    public static bool IsDark
    {
        get => _isDark;
        set
        {
            if (_isDark != value)
            {
                _isDark = value;
                Current.UserAppTheme = _isDark ? AppTheme.Dark : AppTheme.Light;
                Preferences.Default.Set("is_dark", IsDark);
            }
        }
    }

    /// <summary> Adomobの広告IDを格納するConstants </summary>
    public static AdMobConstantsRecord AdMobConstants { get; private set; }

    /// <summary> Adomobの広告IDを格納するレコード </summary>
    public record AdMobConstantsRecord(string bannerId, string interstitialId);
}
