using CommunityToolkit.Mvvm.ComponentModel;

namespace CheckListMaker.ViewModels;

/// <summary>
/// Serves as the base class for all ViewModel classes in the application.
/// Inherits from <see cref="ObservableObject"/> to provide property change notification functionality.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
}
