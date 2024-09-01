using CommunityToolkit.Mvvm.ComponentModel;

namespace CheckListMaker.ViewModels;

/// <summary> アプリについてページのViewModel </summary>
internal partial class AboutViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _appName = AppInfo.Name;

    [ObservableProperty]
    private string _appVersion = AppInfo.VersionString;
}
