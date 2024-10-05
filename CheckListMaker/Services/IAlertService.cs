namespace CheckListMaker.Services;

/// <summary> AlertServiceのインターフェイス </summary>
internal interface IAlertService
{
    /// <summary> Alertを表示する </summary>
    Task ShowAlert(string title, string message);

    /// <summary> 2ボタンのAlertを表示する </summary>
    Task<bool> ShowOkCancelAlert(string title, string message);

    /// <summary> PromptAlertを表示する </summary>
    Task<string> ShowPromptAlert(string title, string message, string initialValue);
}
