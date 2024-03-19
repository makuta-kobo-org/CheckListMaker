using CheckListMaker.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CheckListMaker.ViewModels;

/// <summary> AppShell </summary>
public partial class AppShellViewModel : ObservableObject
{
    [RelayCommand]
    private async Task GoSettingsView()
    {
        Shell.Current.FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"///{nameof(SettingsView)}", false);
    }

    [RelayCommand]
    private async Task GoAboutView()
    {
        Shell.Current.FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"///{nameof(AboutView)}", false);
    }
}
