using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CheckListMaker.Models;

/// <summary> CheckList Item のモデル </summary>
internal partial class CheckItem : ObservableObject
{
    [ObservableProperty]
    private string _itemText;

    [ObservableProperty]
    private bool _isTapped;

    [ObservableProperty]
    private bool _isBeingDragged;

    [ObservableProperty]
    private bool _isBeingDraggedOver;
}
