namespace CheckListMaker.Services;

/// <summary>
/// Interface for an alert service that provides methods to display various types of alerts.
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Displays a simple alert with a title and message.
    /// </summary>
    /// <param name="title">The title of the alert.</param>
    /// <param name="message">The message content of the alert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowAlert(string title, string message);

    /// <summary>
    /// Displays an alert with two buttons: "OK" and "Cancel".
    /// </summary>
    /// <param name="title">The title of the alert.</param>
    /// <param name="message">The message content of the alert.</param>
    /// <returns>A task that returns true if "OK" is pressed, otherwise false.</returns>
    Task<bool> ShowOkCancelAlert(string title, string message);

    /// <summary>
    /// Displays a prompt alert that allows the user to input a value.
    /// </summary>
    /// <param name="title">The title of the alert.</param>
    /// <param name="message">The message content of the alert.</param>
    /// <param name="initialValue">The initial value to display in the input field.</param>
    /// <returns>A task that returns the user input as a string.</returns>
    Task<string> ShowPromptAlert(string title, string message, string initialValue);
}
