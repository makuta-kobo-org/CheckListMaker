using CommunityToolkit.Maui.Views;

namespace CheckListMaker.Controls;

/// <summary> MyPopupService </summary>
public sealed class MyPopupService : IMyPopupService
{
    /// <summary> Page </summary>
    private Page Page { get; set; }

    /// <summary> ShowPopup </summary>
    public void ShowPopup(Popup popup)
    {
        Page ??= Application.Current?.MainPage ?? throw new NullReferenceException();
        Page.ShowPopup(popup);
    }

    /// <summary> ClosePopup </summary>
    public void ClosePopup(Popup popup) => popup.Close();
}
