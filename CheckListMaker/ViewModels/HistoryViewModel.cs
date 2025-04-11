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

/// <summary> HistoryページのViewModel </summary>
/// <remarks> Constructor </remarks>
internal partial class HistoryViewModel : BaseViewModel
{
    private readonly ICustomPopupService _popupService;
    private readonly ILiteDbService _liteDbService;
    private readonly IAlertService _alertService;

    [ObservableProperty]
    private string _bannerId;

    /// <summary>
    /// HistoryViewModelのコンストラクタ
    /// </summary>
    /// <param name="popupService">ポップアップサービス</param>
    /// <param name="liteDbService">LiteDbサービス</param>
    /// <param name="alertService">アラートサービス</param>
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

    /// <summary> CheckListItem のコレクション  </summary>
    public ObservableCollection<CheckList> CheckLists { get; private set; } = [];

    [RelayCommand]
    private void OnAppearing()
    {
        var itemsList = _liteDbService.FindAll();

        CheckLists = [.. itemsList.OrderByDescending(x => x.CreatedDateTime)];

        OnPropertyChanged(nameof(CheckLists));
    }

    [RelayCommand]
    private async Task NavigateToMainViewAsync(CheckList selectedCheckList)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "SelectedCheckList", selectedCheckList },
        };

        await Shell.Current.GoToAsync($"//{nameof(MainView)}", navigationParameter);
    }

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

    /// <summary> CheckListItem 削除コマンド  </summary>
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

    [RelayCommand]
    private async Task HelpIconTapped()
    {
        var message = new StringBuilder()
            .AppendLine(AppResource.Alert_Text_HelpMessage1)
            .AppendLine(AppResource.Alert_Text_HelpMessage2)
            .ToString();

        await _alertService.ShowAlert( AppResource.Alert_Text_HelpTitle, message);
    }
}
