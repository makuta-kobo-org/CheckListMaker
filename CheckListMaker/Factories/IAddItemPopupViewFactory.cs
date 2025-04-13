using CheckListMaker.Controls;

namespace CheckListMaker.Factories;

/// <summary>
/// Defines a factory interface for creating instances of <see cref="AddItemPopupView"/>.
/// </summary>
public interface IAddItemPopupViewFactory
{
    /// <summary>
    /// Creates a new instance of the <see cref="AddItemPopupView"/> class.
    /// </summary>
    /// <returns>A new <see cref="AddItemPopupView"/> instance.</returns>
    AddItemPopupView Create();
}
