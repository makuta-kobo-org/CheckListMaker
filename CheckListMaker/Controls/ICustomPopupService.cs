using CommunityToolkit.Maui.Views;

namespace CheckListMaker.Controls;

/// <summary>
/// Provides methods to display and manage popups in the application.
/// </summary>
public interface ICustomPopupService
{
    /// <summary>
    /// Displays a popup asynchronously.
    /// </summary>
    /// <param name="popup">The popup to be displayed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ShowPopupAsync(Popup popup);

    /// <summary>
    /// Displays a popup synchronously.
    /// </summary>
    /// <param name="popup">The popup to be displayed.</param>
    void ShowPopup(Popup popup);

    /// <summary>
    /// Closes the specified popup.
    /// </summary>
    /// <param name="popup">The popup to be closed.</param>
    void ClosePopup(Popup popup);
}
