using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace CheckListMaker.Helpers;

/// <summary> SnackBarを表示するHelperクラス </summary>
internal static class SnackbarViewer
{
    private static readonly SnackbarOptions _snackbarOptions = new()
    {
        CornerRadius = new CornerRadius(10),
        Font = Microsoft.Maui.Font.SystemFontOfSize(14),
        ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(14),
    };

    /// <summary> Snackbarを表示する </summary>
    internal static async Task Show(string text)
    {
        var duration = TimeSpan.FromSeconds(3);

        var snackbar = Snackbar.Make(
            message: text,
            duration: duration,
            visualOptions: _snackbarOptions);

        await snackbar.Show();
    }
}
