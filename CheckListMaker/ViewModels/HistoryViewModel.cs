using System.Collections.ObjectModel;
using System.Text;
using CheckListMaker.Controls;
using CheckListMaker.Helpers;
using CheckListMaker.Models;
using CheckListMaker.Resources;
using CheckListMaker.Services;
using CheckListMaker.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CheckListMaker.ViewModels;

/// <summary>
/// ViewModel for the History page.
/// </summary>
/// <remarks>
/// This class provides functionality for managing and interacting with the history of checklists.
/// </remarks>
internal partial class HistoryViewModel : BaseViewModel
{
    private readonly ICustomPopupService _popupService;
    private readonly ILiteDbService _liteDbService;
    private readonly IAlertService _alertService;

    [ObservableProperty]
    private string _bannerId;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryViewModel"/> class.
    /// </summary>
    /// <param name="popupService">Service for managing popups.</param>
    /// <param name="liteDbService">Service for interacting with the LiteDB database.</param>
    /// <param name="alertService">Service for displaying alerts.</param>
    /// <param name="adMobConstants">Constants for AdMob configuration.</param>
    public HistoryViewModel(
        ICustomPopupService popupService,
        ILiteDbService liteDbService,
        IAlertService alertService,
        AdMobConstants adMobConstants)
    {
        _popupService = popupService;
        _liteDbService = liteDbService;
        _alertService = alertService;

        BannerId = adMobConstants.BannerId;
    }

    /// <summary>
    /// Gets the collection of checklist items.
    /// </summary>
    public ObservableCollection<CheckList> CheckLists { get; private set; } = [];

    /// <summary>
    /// Command executed when the page appears.
    /// </summary>
    [RelayCommand]
    private void OnAppearing()
    {
        var itemsList = _liteDbService.FindAll();

        CheckLists = [.. itemsList.OrderByDescending(x => x.CreatedDateTime)];

        OnPropertyChanged(nameof(CheckLists));
    }

    /// <summary>
    /// Navigates to the main view with the selected checklist.
    /// </summary>
    /// <param name="selectedCheckList">The selected checklist to navigate with.</param>
    [RelayCommand]
    private async Task NavigateToMainViewAsync(CheckList selectedCheckList)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "SelectedCheckList", selectedCheckList },
        };

        await Shell.Current.GoToAsync($"//{nameof(MainView)}", navigationParameter);
    }

    /// <summary>
    /// Edits the title of the specified checklist.
    /// </summary>
    /// <param name="checklist">The checklist to edit.</param>
    [RelayCommand]
    private async Task EditTitleAsync(CheckList checklist)
    {
        var popup = new LoadingPopup();

        try
        {
            var newTitle = await _alertService.ShowPromptAlert(
                title: AppResource.Alert_Text_EditTitle,
                message: AppResource.Alert_Text_EditMessage,
                initialValue: checklist.Title);

            if (string.IsNullOrWhiteSpace(newTitle) || newTitle == checklist.Title)
            {
                return;
            }

            _popupService.ShowPopup(popup);

            checklist.Title = newTitle;
            _liteDbService.Upsert(checklist);

            await SnackbarViewer.Show(AppResource.Alert_EditResultMessage);
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
    /// Removes the specified checklist.
    /// </summary>
    /// <param name="checklist">The checklist to remove.</param>
    [RelayCommand]
    private async Task RemoveCheckListAsync(CheckList checklist)
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

            CheckLists.Remove(checklist);
            _liteDbService.Delete(checklist);

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
    /// Displays a help message when the help icon is tapped.
    /// </summary>
    [RelayCommand]
    private async Task HelpIconTapped()
    {
        var message = new StringBuilder()
            .AppendLine(AppResource.Alert_Text_HelpMessage1)
            .AppendLine(AppResource.Alert_Text_HelpMessage2)
            .ToString();

        await _alertService.ShowAlert(AppResource.Alert_Text_HelpTitle, message);
    }
}
