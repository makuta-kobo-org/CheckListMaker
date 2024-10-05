namespace CheckListMaker.Services;

/// <summary> Alert関連のサービスクラス </summary>
internal class AlertService : IAlertService
{
    /// <summary> Alertを表示する </summary>
    public async Task ShowAlert(string title, string message) =>
        await App.Current.MainPage.DisplayAlert(title, message, "OK");

    /// <summary> 2ボタンのAlertを表示する </summary>
    public async Task<bool> ShowOkCancelAlert(string title, string message) =>
        await App.Current.MainPage.DisplayAlert(title, message, "OK", "Cancel");

    /// <summary> PromptAlertを表示する </summary>
    public async Task<string> ShowPromptAlert(string title, string message, string initialValue) =>
        await App.Current.MainPage.DisplayPromptAsync(title: title, message: message, initialValue: initialValue);
}
