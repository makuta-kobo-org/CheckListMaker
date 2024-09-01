using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.MauiMTAdmob;

namespace CheckListMaker;

/// <summary> MainActivity </summary>
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    /// <summary> override OnCreate </summary>
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        try
        {
            // AdMob
            CrossMauiMTAdmob.Current.Init(this, "ca-app-pub-2051194976533200~4730043267");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
