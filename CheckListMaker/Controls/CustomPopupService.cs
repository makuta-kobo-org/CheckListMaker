using CommunityToolkit.Maui.Views;

namespace CheckListMaker.Controls;

/// <summary>
/// Provides functionality to display and manage custom popups in a .NET MAUI application.
/// </summary>
public sealed class CustomPopupService : ICustomPopupService
{
    /// <summary>
    /// Gets or sets the current page where the popup will be displayed.
    /// </summary>
    private Page Page { get; set; }

    /// <summary>
    /// Displays a popup asynchronously on the current page.
    /// </summary>
    /// <param name="popup">The popup to be displayed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="NullReferenceException">Thrown if the current application or main page is null.</exception>
    public async Task ShowPopupAsync(Popup popup)
    {
        Page ??= Application.Current?.MainPage ?? throw new NullReferenceException();
        await Page.ShowPopupAsync(popup);
    }

    /// <summary>
    /// Displays a popup on the current page.
    /// </summary>
    /// <param name="popup">The popup to be displayed.</param>
    /// <exception cref="NullReferenceException">Thrown if the current application or main page is null.</exception>
    public void ShowPopup(Popup popup)
    {
        Page ??= Application.Current?.MainPage ?? throw new NullReferenceException();
        Page.ShowPopup(popup);
    }

    /// <summary>
    /// Closes the specified popup.
    /// </summary>
    /// <param name="popup">The popup to be closed.</param>
    public void ClosePopup(Popup popup) => popup.Close();
}
