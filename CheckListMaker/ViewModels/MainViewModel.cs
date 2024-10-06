using System.Diagnostics;
using CheckListMaker.Controls;
using CheckListMaker.Exceptions;
using CheckListMaker.Helpers;
using CheckListMaker.Models;
using CheckListMaker.Resources;
using CheckListMaker.Services;
using CheckListMaker.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CheckListMaker.ViewModels;

/// <summary> MainページのViewModel </summary>
[QueryProperty(nameof(CurrentCehckList), "SelectedCheckList")]
[QueryProperty(nameof(NavigationType), "NavigationType")]
internal partial class MainViewModel : BaseViewModel
{
    private readonly IMediaService _mediaService;
    private readonly IComputerVisionService _computerVisionService;
    private readonly ILiteDbService _liteDbService;
    private readonly IMyPopupService _popupService;
    private readonly IAlertService _alertService;
    private CheckItem _itemBeingDragged;

    [ObservableProperty]
    private int _numberOfColumns = 2;

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isToggled = true;

    [ObservableProperty]
    private string _bannerId;

    [ObservableProperty]
    private CheckList _currentCehckList;

    /// <summary> Constructor </summary>
    public MainViewModel(
        IMediaService mediaService,
        IComputerVisionService computerVisionService,
        ILiteDbService liteDbService,
        IMyPopupService popupService,
        IAlertService alertService,
        AdMobConstants adMobConstants)
    {
        _mediaService = mediaService;
        _computerVisionService = computerVisionService;
        _liteDbService = liteDbService;
        _popupService = popupService;
        _alertService = alertService;

        BannerId = adMobConstants.BannerId;
    }

    /// <summary> 画面遷移の種別判定フラグ  </summary>
    public string NavigationType { get; set; } = string.Empty;

    [RelayCommand]
    private static void ItemDragLeave(CheckItem item)
    {
#if DEBUG
        Trace.WriteLine($"ItemDragLeave : {item?.ItemText}");
#endif

        item.IsBeingDraggedOver = false;
    }

    [RelayCommand]
    private static void ItemTapped(CheckItem item) => item.IsChecked = !item.IsChecked;

    private static bool IsGranted(PermissionStatus status)
        => status == PermissionStatus.Granted || status == PermissionStatus.Limited;

    [RelayCommand]
    private async Task Appearing()
    {
        if (NavigationType != "command")
        {
            await ReadCheckList();
        }
        else
        {
            NavigationType = string.Empty;
        }
    }

    /// <summary> Add New CheckList to DB </summary>
    private async Task AddToDb()
    {
        try
        {
            _liteDbService.Insert(CurrentCehckList);
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
    }

    /// <summary> Update CheckList to DB </summary>
    private async Task UpdateDb()
    {
        try
        {
            _liteDbService.Upsert(CurrentCehckList);
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
    }

    [RelayCommand]
    private void ChangeNumberOfColumns()
        => NumberOfColumns = IsToggled ? 2 : 1;

    /// <summary> ローカルに保存していたjson fileから前回の状態を復帰する </summary>
    private async Task ReadCheckList()
    {
        try
        {
            var itemsList = _liteDbService.FindAll();

            CurrentCehckList = itemsList.Count > 0
                ? itemsList.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault()
                : new();
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task CreateListWithCapturedImage()
    {
        var popup = new LoadingPopup();

        try
        {
            _popupService.ShowPopup(popup);

            var cameraStatus = await CheckPermissions<Permissions.Camera>();

            if (!IsGranted(cameraStatus))
            {
                throw new NoPermissionsException(AppResource.Exception_NoPermissions_Camera);
            }

            var imagePath = await _mediaService.DoCapturePhoto();

            if (imagePath == null)
            {
                return;
            }

            await CreateCheckList(imagePath);

            await SnackbarViewer.Show(AppResource.Main_Snackbar_Done);
        }
        catch (NoPermissionsException ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
        catch (NoCheckItemsException ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
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

            var mediaStatus = await CheckPermissions<Permissions.Media>();

            if (!IsGranted(mediaStatus))
            {
                throw new NoPermissionsException(AppResource.Exception_NoPermissions_Media);
            }

            var imagePath = await _mediaService.DoPickPhoto();

            if (imagePath == null)
            {
                return;
            }

            await CreateCheckList(imagePath);

            await SnackbarViewer.Show(AppResource.Main_Snackbar_Done);
        }
        catch (NoPermissionsException ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
        catch (NoCheckItemsException ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
        finally
        {
            _popupService.ClosePopup(popup);
        }
    }

    [RelayCommand]
    private async Task AddItem()
    {
        try
        {
            var result = await _alertService.ShowPromptAlert(AppResource.Main_Label_AddTitle, string.Empty, string.Empty);

            if (!string.IsNullOrEmpty(result))
            {
                CurrentCehckList.Items.Add(new CheckItem() { ItemText = result });

                await UpdateDb();
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeleteItem(CheckItem item)
    {
        var popup = new LoadingPopup();

        try
        {
            _popupService.ShowPopup(popup);

            CurrentCehckList.Items.Remove(item);

            await UpdateDb();

            await SnackbarViewer.Show(AppResource.Alert_DeleteResultMessage);
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
        finally
        {
            _popupService.ClosePopup(popup);
        }
    }

    [RelayCommand]
    private async Task GoToHistoryView()
    {
        Shell.Current.FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"///{nameof(HistoryView)}", false);
    }

    [RelayCommand]
    private void ItemDragged(CheckItem item)
    {
#if DEBUG
        Trace.WriteLine($"ItemDragged : {item}");
#endif
        item.IsBeingDragged = true;
        _itemBeingDragged = item;
    }

    [RelayCommand]
    private void ItemDraggedOver(CheckItem item)
    {
#if DEBUG
        Trace.WriteLine($"ItemDraggedOver : {item?.ItemText}");
#endif

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

            int insertAtIndex = CurrentCehckList.Items.IndexOf(itemToInsertBefore);

            if (insertAtIndex >= 0 && insertAtIndex < CurrentCehckList.Items.Count)
            {
                CurrentCehckList.Items.Remove(itemToMove);
                CurrentCehckList.Items.Insert(insertAtIndex, itemToMove);
                itemToMove.IsBeingDragged = false;
                itemToInsertBefore.IsBeingDraggedOver = false;
            }

            await UpdateDb();

#if DEBUG
            Trace.WriteLine($"ItemDropped: [{itemToMove?.ItemText}] => [{itemToInsertBefore?.ItemText}], target index = [{insertAtIndex}]");
#endif
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
    }

    private async Task CreateCheckList(string imagePath)
    {
        var results = await _computerVisionService.GetCheckItems(imagePath);

        if (results == null || results.Items.Count < 1)
        {
            throw new NoCheckItemsException();
        }

        CurrentCehckList = results;

        await AddToDb();
    }

    private async Task<PermissionStatus> CheckPermissions<TPermission>()
        where TPermission : Permissions.BasePermission, new()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<TPermission>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<TPermission>();
        }

        return status;
    }
}
