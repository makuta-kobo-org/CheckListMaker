using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CheckListMaker.Models;

/// <summary> CheckList Item Collectionのモデル </summary>
internal partial class CheckItems : ObservableObject
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    [ObservableProperty]
    private DateTimeOffset _createdDateTime = DateTimeOffset.Now;

    [ObservableProperty]
    private ObservableCollection<CheckItem> _items = [];

    /// <summary> ItemTextをスペース区切りで一行にまとめて返す </summary>
    public string ItemTextsOneLine
    {
        get
        {
            var sb = new StringBuilder();

            foreach (var item in Items)
            {
                sb.Append($"{item.ItemText} ");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
