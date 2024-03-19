using CheckListMaker.ViewModels;
using CheckListMaker.Views;

namespace CheckListMaker;

/// <summary> AppShell </summary>
public partial class AppShell : Shell
{
    /// <summary> Constructor </summary>
    public AppShell(AppShellViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        Routing.RegisterRoute($"///{nameof(SettingsView)}", typeof(SettingsView));
        Routing.RegisterRoute($"///{nameof(AboutView)}", typeof(AboutView));
    }
}
