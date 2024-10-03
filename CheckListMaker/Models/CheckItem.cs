using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;

namespace CheckListMaker.Models;

/// <summary> CheckList Item のモデル </summary>
internal partial class CheckItem : ObservableObject
{
    [ObservableProperty]
    private string _itemText;

    [ObservableProperty]
    private bool _isChecked = false;

    [ObservableProperty]
    private bool _isBeingDragged;

    [ObservableProperty]
    private bool _isBeingDraggedOver;

    /// <summary> Primary Id </summary>
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
}
