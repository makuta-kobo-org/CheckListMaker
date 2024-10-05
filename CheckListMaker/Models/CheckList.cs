using System.Collections.ObjectModel;
using System.Text;
using CheckListMaker.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;

namespace CheckListMaker.Models;

/// <summary> CheckList Item Collectionのモデル </summary>
internal partial class CheckList : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CheckItem> _items = [];

    /// <summary> Primary Id </summary>
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    /// <summary> 作成日 UTC </summary>
    public DateTimeOffset CreatedDateTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary> CreatedDateTimeを、local timeかつfomatして返す </summary>
    public string CreatedDateTimeDisplay =>
        CreatedDateTime.ToLocalTime().ToString(AppResource.Format_Date);

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
