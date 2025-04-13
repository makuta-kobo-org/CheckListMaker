using Foundation;

namespace CheckListMaker;

/// <summary>
/// Represents the application delegate for the iOS platform.
/// </summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <summary>
    /// Creates the .NET MAUI application instance.
    /// </summary>
    /// <returns>A new instance of <see cref="MauiApp"/>.</returns>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
