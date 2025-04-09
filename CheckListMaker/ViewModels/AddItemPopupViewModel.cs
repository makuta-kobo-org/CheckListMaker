using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CheckListMaker.ViewModels;

/// <summary>
/// ViewModel for the Add Item Popup.
/// Handles user input and communication with other components.
/// </summary>
public partial class AddItemPopupViewModel : BaseViewModel
{
    /// <summary>
    /// Stores the text input from the user.
    /// </summary>
    [ObservableProperty]
    private string _inputText;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddItemPopupViewModel"/> class.
    /// </summary>
    public AddItemPopupViewModel() => _inputText = string.Empty;

    /// <summary>
    /// Adds a new checklist item if the input text is valid.
    /// Sends a message with the new item and clears the input field.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [RelayCommand]
    private async Task AddCheckItemAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            return;
        }

        // Send a message with the new checklist item
        WeakReferenceMessenger.Default.Send(new NewCheckItemMessage(InputText));

        // Clear the input field
        InputText = string.Empty;

        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles the loaded event for the popup.
    /// Focuses the Entry control if the soft input keyboard is not already showing.
    /// </summary>
    /// <param name="parameter">The parameter passed to the command, expected to be an Entry control.</param>
    [RelayCommand]
    private void OnLoaded(object parameter)
    {
        if (parameter is Entry entry)
        {
            if (!entry.IsSoftInputShowing())
            {
                entry.Focus();
            }
        }
    }

    /// <summary>
    /// Closes the popup.
    /// </summary>
    /// <param name="parameter">The parameter passed to the command, expected to be a Popup instance.</param>
    [RelayCommand]
    private void ClosePopup(object parameter) => ((Popup)parameter).Close();
}
