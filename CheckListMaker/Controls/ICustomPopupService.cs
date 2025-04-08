using CommunityToolkit.Maui.Views;

namespace CheckListMaker.Controls;

/// <summary> interface ICustomPopupService </summary>
public interface ICustomPopupService
{
    /// <summary> ShowPopup </summary>
    void ShowPopup(Popup popup);

    /// <summary> ClosePopup </summary>
    void ClosePopup(Popup popup);
}
