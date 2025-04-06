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

/// <summary> MainページのViewModel </summary>
[QueryProperty(nameof(CurrentCheckList), "SelectedCheckList")]
internal partial class MainViewModel : BaseViewModel
{
    private readonly IMediaService _mediaService;
    private readonly IComputerVisionService _computerVisionService;
    private readonly ILiteDbService _liteDbService;
    private readonly IMyPopupService _popupService;
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

    [RelayCommand]
    private static void OnItemDragLeave(CheckItem item)
    {
#if DEBUG
        Trace.WriteLine($"OnItemDragLeave : {item?.ItemText}");
#endif

        item.IsBeingDraggedOver = false;
    }

    [RelayCommand]
    private static void OnItemTapped(CheckItem item) => item.IsChecked = !item.IsChecked;

    private static bool IsGranted(PermissionStatus status)
        => status == PermissionStatus.Granted || status == PermissionStatus.Limited;

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

    private bool IsCheckListExists() => _liteDbService.FindAll().Any(x => x.Id == CurrentCheckList.Id);

    /// <summary> Add New CheckList to DB </summary>
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

    /// <summary> Update CheckList to DB </summary>
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

    [RelayCommand]
    private void ToggleNumberOfColumns()
        => NumberOfColumns = IsToggled ? 2 : 1;

    /// <summary> ローカルに保存していたjson fileから前回の状態を復帰する </summary>
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

    [RelayCommand]
    private async Task NavigateToHistoryViewAsync()
    {
        Shell.Current.FlyoutIsPresented = false;
        await Shell.Current.GoToAsync($"///{nameof(HistoryView)}", false);
    }

    [RelayCommand]
    private void OnItemDragged(CheckItem item)
    {
#if DEBUG
        Trace.WriteLine($"OnItemDragged : {item}");
#endif
        item.IsBeingDragged = true;
        _draggedItem = item;
    }

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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "<保留中>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "<保留中>")]
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
}
