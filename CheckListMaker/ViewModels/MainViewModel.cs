using System.Collections.ObjectModel;
using System.Text.Json;
using CheckListMaker.Controls;
using CheckListMaker.Helpers;
using CheckListMaker.Messengers;
using CheckListMaker.Models;
using CheckListMaker.Resources;
using CheckListMaker.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CheckListMaker.ViewModels;

/// <summary> MainページのViewModel </summary>
internal partial class MainViewModel : BaseViewModel
{
    private const string JsonFileName = "checkItems.json";

    private readonly IMediaService _mediaService;
    private readonly IComputerVisionService _computerVisionService;
    private readonly IMyPopupService _popupService;
    private CheckItem _itemBeingDragged;

    [ObservableProperty]
    private int _numberOfColumns = 2;

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isToggled = true;

    [ObservableProperty]
    private string _bannerId = App.AdMobConstants.bannerId;

    /// <summary> Constructor </summary>
    public MainViewModel(
        IMediaService mediaService,
        IComputerVisionService computerVisionService,
        IMyPopupService popupService)
    {
        _mediaService = mediaService;
        _computerVisionService = computerVisionService;
        _popupService = popupService;

        // 前回状態の復元
        if (App.RequiresSave)
        {
            ReadItems();
        }

        WeakReferenceMessenger.Default.Register<SaveSettingsChangedMessage>(
            this, async (r, m) => await SaveItems());
    }

    /// <summary> CheckListItem のコレクション  </summary>
    public CheckItems CurrentItems { get; set; } = new CheckItems();

    [RelayCommand]
    private static void ItemDragLeave(CheckItem item)
    {
        Console.WriteLine($"ItemDragLeave : {item?.ItemText}");

        item.IsBeingDraggedOver = false;
    }

    [RelayCommand]
    private static void ItemTapped(CheckItem item) => item.IsChecked = !item.IsChecked;

    /// <summary> CheckList CurrentItems をローカルのjson fileに保存する </summary>
    private async Task SaveItems()
    {
        if (!App.RequiresSave)
        {
            return;
        }

        try
        {
            string json = JsonSerializer.Serialize<CheckItems>(CurrentItems);

            var jsonPath = Path.Combine(FileSystem.Current.AppDataDirectory, JsonFileName);

            Console.WriteLine("SaveItems Executing");
            Console.WriteLine(json);
            File.WriteAllText(jsonPath, json);
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", ex.Message);
        }
    }

    [RelayCommand]
    private void ChangeNumberOfColumns()
        => NumberOfColumns = IsToggled ? 2 : 1;

    /// <summary> ローカルに保存していたjson fileから前回の状態を復帰する </summary>
    private void ReadItems()
    {
        try
        {
            var jsonPath = Path.Combine(FileSystem.Current.AppDataDirectory, JsonFileName);

            if (!File.Exists(jsonPath))
            {
                return;
            }

            var json = File.ReadAllText(jsonPath);

            CurrentItems = JsonSerializer.Deserialize<CheckItems>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR : {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CreateListWithCapturedImage()
    {
        var popup = new LoadingPopup();

        try
        {
            _popupService.ShowPopup(popup);

            var imagePath = await _mediaService.DoCapturePhoto();

            if (imagePath == null)
            {
                return;
            }

            await CreateCheckItems(imagePath);

            await SnackbarViewer.Show(AppResource.Main_Snackbar_Done);
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", ex.Message);
        }
        finally
        {
            _popupService.ClosePopup(popup);
        }
    }

    [RelayCommand]
    private async Task CreateListWithSelectedImage()
    {
        var popup = new LoadingPopup();

        try
        {
            _popupService.ShowPopup(popup);

            var imagePath = await _mediaService.DoPickPhoto();

            if (imagePath == null)
            {
                return;
            }

            await CreateCheckItems(imagePath);

            await SnackbarViewer.Show(AppResource.Main_Snackbar_Done);
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", ex.Message);
        }
        finally
        {
            _popupService.ClosePopup(popup);
        }
    }

    private async Task CreateCheckItems(string imagePath)
    {
        CurrentItems = await _computerVisionService.GetCheckItems(imagePath);

        OnPropertyChanged(nameof(CurrentItems));

        await SaveItems();
    }

    [RelayCommand]
    private async Task AddItem()
    {
        var popup = new LoadingPopup();

        try
        {
            _popupService.ShowPopup(popup);

            if (!string.IsNullOrEmpty(InputText))
            {
                CurrentItems.Items.Add(new CheckItem() { ItemText = InputText });
                InputText = string.Empty;
            }

            await SaveItems();
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", ex.Message);
        }
        finally
        {
            _popupService.ClosePopup(popup);
        }
    }

    [RelayCommand]
    private async Task DeleteItem(CheckItem item)
    {
        var popup = new LoadingPopup();

        try
        {
            CurrentItems.Items.Remove(item);

            await SaveItems();

            await SnackbarViewer.Show(AppResource.Main_String_DeleteMessage);
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", ex.Message);
        }
        finally
        {
            _popupService.ClosePopup(popup);
        }
    }

    [RelayCommand]
    private void ItemDragged(CheckItem item)
    {
        Console.WriteLine($"ItemDragged : {item}");
        item.IsBeingDragged = true;
        _itemBeingDragged = item;
    }

    [RelayCommand]
    private void ItemDraggedOver(CheckItem item)
    {
        Console.WriteLine($"ItemDraggedOver : {item?.ItemText}");

        if (item == _itemBeingDragged)
        {
            item.IsBeingDragged = false;
        }

        item.IsBeingDraggedOver = item != _itemBeingDragged;
    }

    [RelayCommand]
    private async Task ItemDropped(CheckItem item)
    {
        try
        {
            var itemToMove = _itemBeingDragged;
            var itemToInsertBefore = item;

            if (itemToMove == null || itemToInsertBefore == null || itemToMove == itemToInsertBefore)
            {
                return;
            }

            int insertAtIndex = CurrentItems.Items.IndexOf(itemToInsertBefore);

            if (insertAtIndex >= 0 && insertAtIndex < CurrentItems.Items.Count)
            {
                CurrentItems.Items.Remove(itemToMove);
                CurrentItems.Items.Insert(insertAtIndex, itemToMove);
                itemToMove.IsBeingDragged = false;
                itemToInsertBefore.IsBeingDraggedOver = false;
            }

            await SaveItems();

            Console.WriteLine($"ItemDropped: [{itemToMove?.ItemText}] => [{itemToInsertBefore?.ItemText}], target index = [{insertAtIndex}]");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
