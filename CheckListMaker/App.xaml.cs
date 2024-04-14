using CheckListMaker.ViewModels;
using Microsoft.Extensions.Configuration;

namespace CheckListMaker;

/// <summary> App </summary>
public partial class App : Application
{
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

        MainPage = new AppShell(viewModel);
    }

    /// <summary> 状態保存をするかのフラグ </summary>
    public static bool RequiresSave { get; set; }

    /// <summary> Adomobの広告IDを格納するConstants </summary>
    public static AdMobConstantsRecord AdMobConstants { get; private set; }

    /// <summary> Adomobの広告IDを格納するレコード </summary>
    public record AdMobConstantsRecord(string bannerId, string interstitialId);
}
