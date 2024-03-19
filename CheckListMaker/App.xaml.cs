using CheckListMaker.ViewModels;

namespace CheckListMaker;

/// <summary> App </summary>
public partial class App : Application
{
    /// <summary> Constructor </summary>
    public App(AppShellViewModel viewModel)
    {
        InitializeComponent();

        RequiresSave = Preferences.Default.Get("requires_save", false);

        MainPage = new AppShell(viewModel);
    }

    /// <summary> 状態保存をするかのフラグ </summary>
    public static bool RequiresSave { get; set; }
}
