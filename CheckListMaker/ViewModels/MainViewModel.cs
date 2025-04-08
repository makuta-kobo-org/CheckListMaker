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
using LiteDB;

namespace CheckListMaker.ViewModels;

/// <summary>
/// MainViewModel class handles the main operations and data binding for the checklist application.
/// </summary>
[QueryProperty(nameof(CurrentCheckList), "SelectedCheckList")]
internal partial class MainViewModel : BaseViewModel
{
    private readonly IMediaService _mediaService;
    private readonly IComputerVisionService _computerVisionService;
    private readonly ILiteDbService _liteDbService;
    private readonly ICustomPopupService _popupService;
    private readonly IAlertService _alertService;
    private bool _isFirstLaunch = true;
    private CheckItem _draggedItem;

    [ObservableProperty]
    private int _numberOfColumns = 2;

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isToggled = true;

    [ObservableProperty]
    private string _bannerId;

    [ObservableProperty]
    private CheckList _currentCheckList;

    /// <summary>
    /// Initializes a new instance of the MainViewModel class.
    /// </summary>
    /// <param name="mediaService">Service for media operations.</param>
    /// <param name="computerVisionService">Service for computer vision operations.</param>
    /// <param name="liteDbService">Service for LiteDB operations.</param>
    /// <param name="popupService">Service for popup operations.</param>
    /// <param name="alertService">Service for alert operations.</param>
    /// <param name="adMobConstants">Constants for AdMob configuration.</param>
    public MainViewModel(
        IMediaService mediaService,
        IComputerVisionService computerVisionService,
        ILiteDbService liteDbService,
        ICustomPopupService popupService,
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

    /// <summary>
    /// Requests the specified permission.
    /// </summary>
    /// <typeparam name="TPermission">The type of permission to request.</typeparam>
    /// <returns>The status of the permission.</returns>
    protected virtual async Task<PermissionStatus> RequestPermissionsAsync<TPermission>()
        where TPermission : Permissions.BasePermission, new()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<TPermission>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<TPermission>();
        }

        return status;
    }

    /// <summary>
    /// Checks if the specified permission is granted.
    /// </summary>
    /// <param name="status">The status of the permission to check.</param>
    /// <returns>True if the permission is granted, otherwise false.</returns>
    private static bool IsGranted(PermissionStatus status)
        => status == PermissionStatus.Granted || status == PermissionStatus.Limited;

    /// <summary>
    /// Called when the drag operation on the specified item ends.
    /// </summary>
    /// <param name="item">The item that was dragged.</param>
    [RelayCommand]
    private static void OnItemDragLeave(CheckItem item)
    {
#if DEBUG
        Trace.WriteLine($"OnItemDragLeave : {item?.ItemText}");
#endif

        item.IsBeingDraggedOver = false;
    }

    /// <summary>
    /// Called when the specified item is tapped.
    /// </summary>
    /// <param name="item">The item that was tapped.</param>
    [RelayCommand]
    private async Task OnItemTapped(CheckItem item)
    {
        item.IsChecked = !item.IsChecked;

        await UpdateCheckListInDbAsync();
    }

    /// <summary>
    /// Called when the page appears.
    /// Loads the checklist on the first launch.
    /// </summary>
    [RelayCommand]
    private async Task OnAppearingAsync()
    {
        if (_isFirstLaunch)
        {
            _isFirstLaunch = false;
            await LoadCheckListAsync();
            return;
        }

        if (IsCheckListExists())
        {
            return;
        }

        await LoadCheckListAsync();
    }

    /// <summary>
    /// Checks if the checklist exists.
    /// </summary>
    /// <returns>True if the checklist exists, otherwise false.</returns>
    private bool IsCheckListExists() => _liteDbService.FindAll().Any(x => x.Id == CurrentCheckList.Id);

    /// <summary>
    /// Adds the current checklist to the database.
    /// </summary>
    private async Task AddCheckListToDbAsync()
    {
        try
        {
            _liteDbService.Insert(CurrentCheckList);
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
    }

    /// <summary>
    /// Updates the current checklist in the database.
    /// </summary>
    private async Task UpdateCheckListInDbAsync()
    {
        try
        {
            _liteDbService.Upsert(CurrentCheckList);
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
    }

    /// <summary>
    /// Toggles the number of columns.
    /// </summary>
    [RelayCommand]
    private void ToggleNumberOfColumns()
        => NumberOfColumns = IsToggled ? 2 : 1;

    /// <summary>
    /// Loads the checklist from the local JSON file.
    /// </summary>
    private async Task LoadCheckListAsync()
    {
        try
        {
            var itemsList = _liteDbService.FindAll();

            CurrentCheckList = itemsList.Count > 0
                ? itemsList.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault()
                : new();
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
    }

    /// <summary>
    /// Creates a checklist with a captured image.
    /// </summary>
    [RelayCommand]
    private async Task CreateCheckListWithCapturedImageAsync()
    {
        var popup = new LoadingPopup();

        try
        {
            _popupService.ShowPopup(popup);

            var cameraStatus = await RequestPermissionsAsync<Permissions.Camera>();

            if (!IsGranted(cameraStatus))
            {
                throw new NoPermissionsException(AppResource.Exception_NoPermissions_Camera);
            }

            var imagePath = await _mediaService.DoCapturePhoto();

            if (imagePath == null)
            {
                return;
            }

            await GenerateCheckListAsync(imagePath);

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

    /// <summary>
    /// Creates a checklist with a selected image.
    /// </summary>
    [RelayCommand]
    private async Task CreateCheckListWithSelectedImageAsync()
    {
        var popup = new LoadingPopup();

        try
        {
            _popupService.ShowPopup(popup);

            var mediaStatus = await RequestPermissionsAsync<Permissions.Media>();

            if (!IsGranted(mediaStatus))
            {
                throw new NoPermissionsException(AppResource.Exception_NoPermissions_Media);
            }

            var imagePath = await _mediaService.DoPickPhoto();

            if (imagePath == null)
            {
                return;
            }

            await GenerateCheckListAsync(imagePath);

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

    /// <summary>
    /// Adds a new check item to the current checklist.
    /// </summary>
    [RelayCommand]
    private async Task AddCheckItemAsync()
    {
        try
        {
            var result = await _alertService.ShowPromptAlert(AppResource.Main_Label_AddTitle, string.Empty, string.Empty);

            if (!string.IsNullOrEmpty(result))
            {
                CurrentCheckList.Items.Add(new CheckItem() { ItemText = result });

                await UpdateCheckListInDbAsync();
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
    }

    /// <summary>
    /// Removes the specified check item from the current checklist.
    /// </summary>
    /// <param name="item">The check item to remove.</param>
    [RelayCommand]
    private async Task RemoveCheckItemAsync(CheckItem item)
    {
        var popup = new LoadingPopup();

        try
        {
            _popupService.ShowPopup(popup);

            CurrentCheckList.Items.Remove(item);

            await UpdateCheckListInDbAsync();

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

    /// <summary>
    /// Navigates to the history view.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToHistoryViewAsync()
    {
        Shell.Current.FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"///{nameof(HistoryView)}", false);
    }

    /// <summary>
    /// Called when the specified item is being dragged.
    /// </summary>
    /// <param name="item">The item being dragged.</param>
    [RelayCommand]
    private void OnItemDragged(CheckItem item)
    {
#if DEBUG
        Trace.WriteLine($"OnItemDragged : {item}");
#endif
        item.IsBeingDragged = true;
        _draggedItem = item;
    }

    /// <summary>
    /// Called when the specified item is being dragged over another item.
    /// </summary>
    /// <param name="item">The item being dragged over.</param>
    [RelayCommand]
    private void OnItemDraggedOver(CheckItem item)
    {
#if DEBUG
        Trace.WriteLine($"OnItemDraggedOver : {item?.ItemText}");
#endif

        if (item == _draggedItem)
        {
            item.IsBeingDragged = false;
        }

        item.IsBeingDraggedOver = item != _draggedItem;
    }

    /// <summary>
    /// Called when the specified item is dropped.
    /// </summary>
    /// <param name="item">The item that was dropped.</param>
    [RelayCommand]
    private async Task OnItemDroppedAsync(CheckItem item)
    {
        try
        {
            var itemToMove = _draggedItem;
            var itemToInsertBefore = item;

            if (itemToMove == null || itemToInsertBefore == null || itemToMove == itemToInsertBefore)
            {
                return;
            }

            int insertAtIndex = CurrentCheckList.Items.IndexOf(itemToInsertBefore);

            if (insertAtIndex >= 0 && insertAtIndex < CurrentCheckList.Items.Count)
            {
                CurrentCheckList.Items.Remove(itemToMove);
                CurrentCheckList.Items.Insert(insertAtIndex, itemToMove);
                itemToMove.IsBeingDragged = false;
                itemToInsertBefore.IsBeingDraggedOver = false;
            }

            await UpdateCheckListInDbAsync();

#if DEBUG
            Trace.WriteLine($"OnItemDroppedAsync: [{itemToMove?.ItemText}] => [{itemToInsertBefore?.ItemText}], target index = [{insertAtIndex}]");
#endif
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
    }

    /// <summary>
    /// Generates a checklist from the specified image path.
    /// </summary>
    /// <param name="imagePath">The path of the image to process.</param>
    private async Task GenerateCheckListAsync(string imagePath)
    {
        var results = await _computerVisionService.GetCheckItems(imagePath);

        if (results == null || results.Items.Count < 1)
        {
            throw new NoCheckItemsException();
        }

        CurrentCheckList = results;

        await AddCheckListToDbAsync();
    }
}
