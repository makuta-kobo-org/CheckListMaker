using UIKit;

namespace CheckListMaker;

/// <summary>
/// The main entry point of the application.
/// </summary>
public class Program
{
    /// <summary>
    /// The main method of the application.
    /// </summary>
    /// <param name="args">An array of command-line arguments.</param>
    private static void Main(string[] args) =>

        // If you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
}
