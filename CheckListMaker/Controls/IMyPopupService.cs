using CommunityToolkit.Maui.Views;

namespace CheckListMaker.Controls;

/// <summary> interface IMyPopupService </summary>
public interface IMyPopupService
{
    /// <summary> ShowPopup </summary>
    void ShowPopup(Popup popup);

    /// <summary> ClosePopup </summary>
    void ClosePopup(Popup popup);
}
