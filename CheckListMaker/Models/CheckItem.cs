using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;

namespace CheckListMaker.Models;

/// <summary>
/// Represents a checklist item with properties for text, status, and drag-and-drop functionality.
/// </summary>
public partial class CheckItem : ObservableObject
{
    /// <summary>
    /// The text content of the checklist item.
    /// </summary>
    [ObservableProperty]
    private string _itemText;

    /// <summary>
    /// Indicates whether the checklist item is checked.
    /// </summary>
    [ObservableProperty]
    private bool _isChecked = false;

    /// <summary>
    /// Indicates whether the checklist item is currently being dragged.
    /// </summary>
    [ObservableProperty]
    private bool _isBeingDragged;

    /// <summary>
    /// Indicates whether the checklist item is being dragged over by another item.
    /// </summary>
    [ObservableProperty]
    private bool _isBeingDraggedOver;

    /// <summary>
    /// The unique identifier for the checklist item.
    /// </summary>
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
}
