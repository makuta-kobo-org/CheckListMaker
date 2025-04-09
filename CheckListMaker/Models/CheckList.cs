using System.Collections.ObjectModel;
using System.Text;
using CheckListMaker.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;

namespace CheckListMaker.Models;

/// <summary>
/// Represents a collection of checklist items.
/// </summary>
public partial class CheckList : ObservableObject
{
    /// <summary>
    /// Gets or sets the observable collection of check items.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<CheckItem> _items = new();

    /// <summary>
    /// Gets or sets the primary identifier for the checklist.
    /// </summary>
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    /// <summary>
    /// Gets or sets the creation date and time in UTC.
    /// </summary>
    public DateTimeOffset CreatedDateTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the creation date and time converted to local time and formatted using the predefined date format.
    /// </summary>
    public string CreatedDateTimeDisplay =>
        CreatedDateTime.ToLocalTime().ToString(AppResource.Format_Date);

    /// <summary>
    /// Gets a single line string representation of all item texts, separated by spaces.
    /// </summary>
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
