using Android.App;
using Android.Runtime;

namespace CheckListMaker;

/// <summary> MainApplication </summary>
[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
    /// <summary> override CreateMauiApp </summary>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
