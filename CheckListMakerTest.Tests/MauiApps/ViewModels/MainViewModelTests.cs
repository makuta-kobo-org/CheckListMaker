using System.Collections.ObjectModel;
using CheckListMaker.Controls;
using CheckListMaker.Models;
using CheckListMaker.Services;
using CheckListMaker.ViewModels;
using Moq;

namespace CheckListMakerTest.Tests.MauiApps.ViewModels;

public class MainViewModelTests
{
    private readonly Mock<IMediaService> _mediaServiceMock;
    private readonly Mock<IComputerVisionService> _computerVisionServiceMock;
    private readonly Mock<ILiteDbService> _liteDbServiceMock;
    private readonly Mock<IMyPopupService> _popupServiceMock;
    private readonly Mock<IAlertService> _alertServiceMock;
    private readonly Mock<AdMobConstants> _adMobConstans;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _mediaServiceMock = new Mock<IMediaService>();
        _computerVisionServiceMock = new Mock<IComputerVisionService>();
        _liteDbServiceMock = new Mock<ILiteDbService>();
        _popupServiceMock = new Mock<IMyPopupService>();
        _alertServiceMock = new Mock<IAlertService>();
        _adMobConstans = new Mock<AdMobConstants>();

        _viewModel = new MainViewModel(
            _mediaServiceMock.Object,
            _computerVisionServiceMock.Object,
            _liteDbServiceMock.Object,
            _popupServiceMock.Object,
            _alertServiceMock.Object,
            _adMobConstans.Object
        );
    }

    [Fact]
    public void ChangeNumberOfColumns_ToggledTrue_ChangesColumnsTo2()
    {
        // Arrange
        _viewModel.IsToggled = true;

        // Act
        _viewModel.ChangeNumberOfColumnsCommand.Execute(null);

        // Assert
        _viewModel.NumberOfColumns.Is(2);
    }

    [Fact]
    public void ChangeNumberOfColumns_ToggledFalse_ChangesColumnsTo1()
    {
        // Arrange
        _viewModel.IsToggled = false;

        // Act
        _viewModel.ChangeNumberOfColumnsCommand.Execute(null);

        // Assert
        _viewModel.NumberOfColumns.Is(1);
    }

    [Fact]
    public async Task AddItem_WhenCalled_AddsNewItemToCheckListAndUpdatesDb()
    {
        // Arrange
        string newItemText = "New Item";
        _viewModel.CurrentCehckList = new CheckList { Items = [] };

        _alertServiceMock
            .Setup(service => service.ShowPromptAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(newItemText);

        // Act
        await _viewModel.AddItemCommand.ExecuteAsync(null);

        // Assert
        _viewModel.CurrentCehckList.Items.Count.Is(1);
        _viewModel.CurrentCehckList.Items[0].ItemText.Is(newItemText);
        _liteDbServiceMock.Verify(service => service.Update(It.IsAny<CheckList>()), Times.Once);
    }

    [Fact]
    public async Task AddItem_WhenPromptReturnsNull_DoesNotAddItem()
    {
        // Arrange
        _viewModel.CurrentCehckList = new CheckList { Items = [] };

        _alertServiceMock
            .Setup(service => service.ShowPromptAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string)null);

        // Act
        await _viewModel.AddItemCommand.ExecuteAsync(null);

        // Assert
        _viewModel.CurrentCehckList.Items.Any().IsFalse();
        _liteDbServiceMock.Verify(service => service.Update(It.IsAny<CheckList>()), Times.Never);
    }

    [Fact]
    public async Task DeleteItem_RemovesCheckItemFromList()
    {
        // Arrange
        var itemToDelete = new CheckItem { ItemText = "Item to delete" };
        _viewModel.CurrentCehckList = new CheckList { Items = new ObservableCollection<CheckItem> { itemToDelete } };

        // Act
        await _viewModel.DeleteItemCommand.ExecuteAsync(itemToDelete);

        // Assert
        _viewModel.CurrentCehckList.Items.Contains(itemToDelete).IsFalse();
    }

    [Fact]
    public void ItemTapped_TogglesCheckItemIsChecked()
    {
        // Arrange
        var item = new CheckItem { IsChecked = false };

        // Act
        _viewModel.ItemTappedCommand.Execute(item);

        // Assert
        item.IsChecked.IsTrue();
    }

    [Fact]
    public void ItemDragLeave_SetsItemIsBeingDraggedOverToFalse()
    {
        // Arrange
        var item = new CheckItem { IsBeingDraggedOver = true };

        // Act
        _viewModel.ItemDragLeaveCommand.Execute(item);

        // Assert
        item.IsBeingDraggedOver.IsFalse();
    }
}
