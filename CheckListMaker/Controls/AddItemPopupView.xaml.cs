using CheckListMaker.ViewModels;
using CommunityToolkit.Maui.Views;

namespace CheckListMaker.Controls;

/// <summary>
/// Represents a popup control for adding a new item to the checklist.
/// </summary>
public partial class AddItemPopupView : Popup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddItemPopupView"/> class.
    /// </summary>
    /// <param name="addItemPopupViewModel">
    /// The <see cref="AddItemPopupViewModel"/> instance to be used as the BindingContext for this popup.
    /// </param>
    public AddItemPopupView(AddItemPopupViewModel addItemPopupViewModel)
    {
        InitializeComponent();
        BindingContext = addItemPopupViewModel;
    }
}
