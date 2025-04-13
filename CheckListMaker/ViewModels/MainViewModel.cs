using CheckListMaker.Controls;
using CheckListMaker.Exceptions;
using CheckListMaker.Factories;
using CheckListMaker.Helpers;
using CheckListMaker.Models;
using CheckListMaker.Resources;
using CheckListMaker.Services;
using CheckListMaker.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiteDB;

namespace CheckListMaker.ViewModels;

/// <summary>
/// ViewModel for managing the main operations and data binding of the checklist application.
/// </summary>
[QueryProperty(nameof(CurrentCheckList), "SelectedCheckList")]
public partial class MainViewModel : BaseViewModel
{
    private readonly IMediaService _mediaService;
    private readonly IComputerVisionService _computerVisionService;
    private readonly ILiteDbService _liteDbService;
    private readonly ICustomPopupService _popupService;
    private readonly IAlertService _alertService;
    private readonly IAddItemPopupViewFactory _addItemPopupViewFactory;
    private bool _isFirstLaunch = true;

    [ObservableProperty]
    private int _numberOfColumns = 2;

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private string _bannerId;

    [ObservableProperty]
    private CheckList _currentCheckList;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="mediaService">Service for media operations.</param>
    /// <param name="computerVisionService">Service for computer vision operations.</param>
    /// <param name="liteDbService">Service for LiteDB operations.</param>
    /// <param name="popupService">Service for popup operations.</param>
    /// <param name="alertService">Service for alert operations.</param>
    /// <param name="adMobConstants">Constants for AdMob configuration.</param>
    /// <param name="addItemPopupViewFactory">Factory for creating add item popup views.</param>
    public MainViewModel(
        IMediaService mediaService,
        IComputerVisionService computerVisionService,
        ILiteDbService liteDbService,
        ICustomPopupService popupService,
        IAlertService alertService,
        AdMobConstants adMobConstants,
        IAddItemPopupViewFactory addItemPopupViewFactory)
    {
        _mediaService = mediaService;
        _computerVisionService = computerVisionService;
        _liteDbService = liteDbService;
        _popupService = popupService;
        _alertService = alertService;
        _addItemPopupViewFactory = addItemPopupViewFactory;

        BannerId = adMobConstants.BannerId;

        // Register to receive messages for adding new checklist items.
        WeakReferenceMessenger.Default.Register<NewCheckItemMessage>(this, (r, m) =>
        {
            CurrentCheckList ??= new CheckList();

            CurrentCheckList.Items.Add(new CheckItem { ItemText = m.inputText });
            _liteDbService.Upsert(CurrentCheckList);
        });
    }

    /// <summary>
    /// Requests the specified permission.
    /// </summary>
    /// <typeparam name="TPermission">The type of permission to request.</typeparam>
    /// <returns>The status of the requested permission.</returns>
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
    /// Determines if the specified permission is granted.
    /// </summary>
    /// <param name="status">The permission status to check.</param>
    /// <returns>True if the permission is granted, otherwise false.</returns>
    private static bool IsGranted(PermissionStatus status)
        => status == PermissionStatus.Granted || status == PermissionStatus.Limited;

    /// <summary>
    /// Toggles the checked state of a checklist item when tapped.
    /// </summary>
    /// <param name="item">The item that was tapped.</param>
    [RelayCommand]
    private async Task OnItemTapped(CheckItem item)
    {
        item.IsChecked = !item.IsChecked;
        await UpdateCheckListInDbAsync();
    }

    /// <summary>
    /// Handles the page appearing event. Loads the checklist on the first launch.
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
    /// Checks if the current checklist exists in the database.
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
    /// Toggles the number of columns in the checklist view.
    /// </summary>
    [RelayCommand]
    private void ToggleNumberOfColumns()
        => NumberOfColumns = NumberOfColumns == 1 ? 2 : 1;

    /// <summary>
    /// Loads the checklist from the database.
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
    /// Creates a checklist using a captured image.
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
    /// Creates a checklist using a selected image.
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
    /// Displays a popup to add a new checklist item.
    /// </summary>
    [RelayCommand]
    private async Task AddCheckItemAsync()
    {
        try
        {
            var popup = _addItemPopupViewFactory.Create();
            await _popupService.ShowPopupAsync(popup);
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlert("Error", ex.Message);
        }
    }

    /// <summary>
    /// Removes a specified checklist item from the current checklist.
    /// </summary>
    /// <param name="item">The item to remove.</param>
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

    [RelayCommand]
    private void CreateNewCheckList()
    {
        var popup = new LoadingPopup();

        try
        {
            _popupService.ShowPopup(popup);

            CurrentCheckList = new CheckList();

            _liteDbService.Insert(CurrentCheckList);
        }
        catch (Exception ex)
        {
            _alertService.ShowAlert("Error", ex.Message);
        }
        finally
        {
            _popupService.ClosePopup(popup);
        }
    }

    [RelayCommand]
    private async Task RemoveCheckListAsync()
    {
        var popup = new LoadingPopup();

        try
        {
            var isConfirmed = await _alertService.ShowOkCancelAlert(
                AppResource.Alert_Label_ConfirmTitle,
                AppResource.Alert_Label_DeleteMessage);

            if (!isConfirmed)
            {
                return;
            }

            _popupService.ShowPopup(popup);

            _liteDbService.Delete(CurrentCheckList);

            await LoadCheckListAsync();

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
    /// Handles the completion of a reorder operation for checklist items.
    /// </summary>
    /// <remarks>
    /// This method is triggered when the user completes a drag-and-drop reorder operation
    /// on the checklist items. It updates the current checklist in the database to reflect
    /// the new order of items.
    /// </remarks>
    [RelayCommand]
    private void ReorderCompleted()
    {
        if (CurrentCheckList == null)
        {
            return;
        }

        try
        {
            _liteDbService.Upsert(CurrentCheckList);
        }
        catch (Exception ex)
        {
            _alertService.ShowAlert("Error", ex.Message);
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

/// <summary>
/// Represents a message for adding a new checklist item.
/// </summary>
/// <param name="inputText"> The text of the new checklist item.</param>
public record NewCheckItemMessage(string inputText);
