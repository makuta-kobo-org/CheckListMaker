using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CheckListMaker.ViewModels;

/// <summary> SettingsViewのViewModel </summary>
internal partial class SettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _isDark;

    /// <summary> Constructor </summary>
    public SettingsViewModel() => IsDark = App.IsDark;

    [RelayCommand]
    private void ChangedIsDark() => App.IsDark = IsDark;
}
