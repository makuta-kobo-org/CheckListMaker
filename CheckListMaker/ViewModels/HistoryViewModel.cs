using System.Collections.ObjectModel;
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
internal partial class HistoryViewModel : BaseViewModel
{
    private readonly IMyPopupService _popupService;
    private readonly ILiteDbService _liteDbService;
    private readonly IAlertService _alertService;

    [ObservableProperty]
    private CheckList _selectedCheckItems = null;

    /// <summary> Constructor </summary>
    public HistoryViewModel(
        IMyPopupService myPopupService,
        ILiteDbService liteDbService,
        IAlertService alertService)
    {
        _popupService = myPopupService;
        _liteDbService = liteDbService;
        _alertService = alertService;
    }

    /// <summary> CheckListItem のコレクション  </summary>
    public ObservableCollection<CheckList> CheckListCollection { get; private set; } = [];

    [RelayCommand]
    private void Appearing()
    {
        var itemsList = _liteDbService.FindAll();

        CheckListCollection = new ObservableCollection<CheckList>(itemsList.OrderByDescending(x => x.CreatedDateTime));

        OnPropertyChanged(nameof(CheckListCollection));
    }

    [RelayCommand]
    private async Task GoToMainView(CheckList selectedCheckList) =>
        await Shell.Current.GoToAsync(
            state: $"//{nameof(MainView)}",
            parameters: new Dictionary<string, object>
            {
                { "SelectedCheckList", selectedCheckList },
                { "NavigationType", "command" },
            });

    /// <summary> CheckListItem 削除コマンド  </summary>
    [RelayCommand]
    private async Task DeleteCheckList(CheckList items)
    {
        var popup = new LoadingPopup();

        try
        {
            var isOk = await _alertService.ShowOkCancelAlert(
                AppResource.History_Label_AlertTitle,
                AppResource.History_Label_AlertMessage);

            if (!isOk)
            {
                return;
            }

            _popupService.ShowPopup(popup);

            CheckListCollection.Remove(items);
            _liteDbService.Delete(items);

            // TODO save to json
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
