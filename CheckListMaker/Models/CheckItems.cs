using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CheckListMaker.Models;

/// <summary> CheckList Item Collectionのモデル </summary>
internal partial class CheckItems : ObservableObject
{
    [ObservableProperty]
    private DateTimeOffset _createdDateTime = DateTimeOffset.Now;

    [ObservableProperty]
    private ObservableCollection<CheckItem> _items;
}
