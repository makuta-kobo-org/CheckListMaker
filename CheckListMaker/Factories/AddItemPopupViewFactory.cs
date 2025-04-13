using CheckListMaker.Controls;
using CheckListMaker.ViewModels;

namespace CheckListMaker.Factories;

/// <summary>
/// Factory class responsible for creating instances of <see cref="AddItemPopupView"/>.
/// </summary>
internal class AddItemPopupViewFactory : IAddItemPopupViewFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddItemPopupViewFactory"/> class.
    /// </summary>
    public AddItemPopupViewFactory()
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AddItemPopupView"/> class.
    /// </summary>
    /// <returns>A new <see cref="AddItemPopupView"/> instance with its BindingContext set to a new <see cref="AddItemPopupViewModel"/>.</returns>
    public AddItemPopupView Create() => new AddItemPopupView(new AddItemPopupViewModel());
}
