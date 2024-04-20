using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CheckListMaker.ViewModels;

/// <summary> SettingsViewのViewModel </summary>
internal partial class SettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _requiresSave;

    [ObservableProperty]
    private bool _isDark;

    /// <summary> Constructor </summary>
    public SettingsViewModel()
    {
        RequiresSave = App.RequiresSave;
        IsDark = App.IsDark;
    }

    [RelayCommand]
    private void ChangedRequiresSave() => App.RequiresSave = RequiresSave;

    [RelayCommand]
    private void ChangedIsDark() => App.IsDark = IsDark;
}
