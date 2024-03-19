using CheckListMaker.Messengers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CheckListMaker.ViewModels;

/// <summary> SettingsViewのViewModel </summary>
internal partial class SettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _requiresSave;

    /// <summary> Constructor </summary>
    public SettingsViewModel() => RequiresSave = App.RequiresSave;

    [RelayCommand]
    private void ChangeRequiresSave()
    {
        App.RequiresSave = RequiresSave;

        Preferences.Default.Set("requires_save", RequiresSave);

        // Trueなら現在のCheckListアイテムを保存
        if (RequiresSave)
        {
            WeakReferenceMessenger.Default.Send(new SaveSettingsChangedMessage(null));
        }
    }
}
