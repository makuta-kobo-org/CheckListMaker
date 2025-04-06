using System.Collections.ObjectModel;
using CheckListMaker.Controls;
using CheckListMaker.Helpers;
using CheckListMaker.Models;
using CheckListMaker.Resources;
using CheckListMaker.Services;
using CheckListMaker.Views;
using CommunityToolkit.Mvvm.Input;

namespace CheckListMaker.ViewModels;

/// <summary> HistoryページのViewModel </summary>
/// <remarks> Constructor </remarks>
internal partial class HistoryViewModel : BaseViewModel
{
    private readonly IMyPopupService _popupService;
    private readonly ILiteDbService _liteDbService;
    private readonly IAlertService _alertService;

    /// <summary>
    /// HistoryViewModelのコンストラクタ
    /// </summary>
    /// <param name="popupService">ポップアップサービス</param>
    /// <param name="liteDbService">LiteDbサービス</param>
    /// <param name="alertService">アラートサービス</param>
    public HistoryViewModel(
        IMyPopupService popupService,
        ILiteDbService liteDbService,
        IAlertService alertService)
    {
        _popupService = popupService;
        _liteDbService = liteDbService;
        _alertService = alertService;
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

    /// <summary> CheckListItem 削除コマンド  </summary>
    [RelayCommand]
    private async Task RemoveCheckListAsync(CheckList items)
    {
        var popup = new LoadingPopup();

        try
        {
            var isConfirmed = await _alertService.ShowOkCancelAlert(
                AppResource.History_Label_AlertTitle,
                AppResource.History_Label_AlertMessage);

            if (!isConfirmed)
            {
                return;
            }

            _popupService.ShowPopup(popup);

            CheckLists.Remove(items);
            _liteDbService.Delete(items);

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
}
