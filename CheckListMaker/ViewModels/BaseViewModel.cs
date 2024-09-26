using CommunityToolkit.Mvvm.ComponentModel;

namespace CheckListMaker.ViewModels;

/// <summary> ViewModel 基本クラス </summary>
internal abstract partial class BaseViewModel : ObservableObject
{
    /// <summary> Alertを表示する </summary>
    public static async Task ShowAlert(string title, string message) =>
        await App.Current.MainPage.DisplayAlert(title, message, "OK");

    /// <summary> 2ボタンのAlertを表示する </summary>
    public static async Task<bool> ShowOkCancelAlert(string title, string message) =>
        await App.Current.MainPage.DisplayAlert(title, message, "OK", "Cancel");

    /// <summary> PromptAlertを表示する </summary>
    public static async Task<string> ShowPromptAlert(string title, string message, string initialValue) =>
        await App.Current.MainPage.DisplayPromptAsync(title: title, message: message, initialValue: initialValue);
}
