using Android.App;
using Android.Runtime;
using Microsoft.Maui.Handlers;

namespace CheckListMaker;

/// <summary>
/// Represents the main application class for the Android platform in the .NET MAUI project.
/// </summary>
[Application]
public class MainApplication : MauiApplication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainApplication"/> class.
    /// </summary>
    /// <param name="handle">A pointer to the native Android application handle.</param>
    /// <param name="ownership">Specifies how the handle is owned.</param>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
        => EntryHandler.Mapper.AppendToMapping(
            "MyCustomization",
            (handler, view)
                => handler.PlatformView.Background = null);

    /// <summary>
    /// Creates and returns the <see cref="MauiApp"/> instance for the application.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/> instance.</returns>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
