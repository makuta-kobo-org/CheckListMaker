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

    private class TestMainViewModel : MainViewModel
    {
        public TestMainViewModel(
            IMediaService mediaService,
            IComputerVisionService computerVisionService,
            ILiteDbService liteDbService,
            IMyPopupService popupService,
            IAlertService alertService,
            AdMobConstants adMobConstants)
            : base(mediaService, computerVisionService, liteDbService, popupService, alertService, adMobConstants)
        {
        }

        protected override Task<PermissionStatus> RequestPermissionsAsync<TPermission>()
            => Task.FromResult(PermissionStatus.Granted);
    }

    [Fact]
    public void ToggleNumberOfColumns_ToggledTrue_ChangesColumnsTo2()
    {
        // Arrange
        _viewModel.IsToggled = true;

        // Act
        _viewModel.ToggleNumberOfColumnsCommand.Execute(null);

        // Assert
        _viewModel.NumberOfColumns.Is(2);
    }

    [Fact]
    public void ToggleNumberOfColumns_ToggledFalse_ChangesColumnsTo1()
    {
        // Arrange
        _viewModel.IsToggled = false;

        // Act
        _viewModel.ToggleNumberOfColumnsCommand.Execute(null);

        // Assert
        _viewModel.NumberOfColumns.Is(1);
    }

    [Fact]
    public async Task AddItem_WhenCalled_AddsNewItemToCheckListAndUpdatesDb()
    {
        // Arrange
        string newItemText = "New Item";
        _viewModel.CurrentCheckList = new CheckList { Items = [] };

        _alertServiceMock
            .Setup(service => service.ShowPromptAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(newItemText);

        // Act
        await _viewModel.AddCheckItemCommand.ExecuteAsync(null);

        // Assert
        _viewModel.CurrentCheckList.Items.Count.Is(1);
        _viewModel.CurrentCheckList.Items[0].ItemText.Is(newItemText);
        _liteDbServiceMock.Verify(service => service.Upsert(It.IsAny<CheckList>()), Times.Once);
    }

    [Fact]
    public async Task AddItem_WhenPromptReturnsNull_DoesNotAddItem()
    {
        // Arrange
        _viewModel.CurrentCheckList = new CheckList { Items = [] };

        _alertServiceMock
            .Setup(service => service.ShowPromptAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string)null);

        // Act
        await _viewModel.AddCheckItemCommand.ExecuteAsync(null);

        // Assert
        _viewModel.CurrentCheckList.Items.Any().IsFalse();
        _liteDbServiceMock.Verify(service => service.Upsert(It.IsAny<CheckList>()), Times.Never);
    }

    [Fact]
    public async Task DeleteItem_RemovesCheckItemFromList()
    {
        // Arrange
        var itemToDelete = new CheckItem { ItemText = "Item to delete" };
        _viewModel.CurrentCheckList = new CheckList { Items = new ObservableCollection<CheckItem> { itemToDelete } };

        // Act
        await _viewModel.RemoveCheckItemCommand.ExecuteAsync(itemToDelete);

        // Assert
        _viewModel.CurrentCheckList.Items.Contains(itemToDelete).IsFalse();
    }

    [Fact]
    public async Task ItemTapped_TogglesCheckItemIsChecked()
    {
        // Arrange
        var item = new CheckItem { IsChecked = false };

        // Act
        await _viewModel.ItemTappedCommand.ExecuteAsync(item);

        // Assert
        item.IsChecked.IsTrue();
        _liteDbServiceMock.Verify(service => service.Upsert(It.IsAny<CheckList>()), Times.Once);
    }

    [Fact]
    public async Task ItemTapped_UpdatesCheckListInDb()
    {
        // Arrange
        var item = new CheckItem { IsChecked = false };
        _viewModel.CurrentCheckList = new CheckList { Items = new ObservableCollection<CheckItem> { item } };

        // Act
        await _viewModel.ItemTappedCommand.ExecuteAsync(item);

        // Assert
        _liteDbServiceMock.Verify(service => service.Upsert(_viewModel.CurrentCheckList), Times.Once);
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

    [Fact]
    public async Task AppearingCommand_InitialLaunch_ReadsCheckList()
    {
        // Arrange
        _viewModel.AsDynamic()._isFirstLaunch = true;

        // Act
        await _viewModel.AppearingCommand.ExecuteAsync(null);

        // Assert
        _liteDbServiceMock.Verify(service => service.FindAll(), Times.Once);
    }

    [Fact]
    public void ItemDraggedCommand_SetsItemIsBeingDragged()
    {
        // Arrange
        var item = new CheckItem { IsBeingDragged = false };

        // Act
        _viewModel.ItemDraggedCommand.Execute(item);

        // Assert
        item.IsBeingDragged.IsTrue();
    }

    [Fact]
    public void ItemDraggedOverCommand_SetsItemIsBeingDraggedOver()
    {
        // Arrange
        var item = new CheckItem { IsBeingDraggedOver = false };

        // Act
        _viewModel.ItemDraggedOverCommand.Execute(item);

        // Assert
        item.IsBeingDraggedOver.IsTrue();
    }

    [Fact]
    public async Task ItemDroppedCommand_MovesItemAndUpdatesDb()
    {
        // Arrange
        var itemToMove = new CheckItem { ItemText = "Move" };
        var itemToInsertBefore = new CheckItem { ItemText = "Before" };
        _viewModel.CurrentCheckList = new CheckList { Items = new ObservableCollection<CheckItem> { itemToMove, itemToInsertBefore } };
        _viewModel.ItemDraggedCommand.Execute(itemToMove);

        // Act
        await _viewModel.ItemDroppedCommand.ExecuteAsync(itemToInsertBefore);

        // Assert
        _viewModel.CurrentCheckList.Items[0].Is(itemToInsertBefore);
        _viewModel.CurrentCheckList.Items[1].Is(itemToMove);
        _liteDbServiceMock.Verify(service => service.Upsert(It.IsAny<CheckList>()), Times.Once);
    }

    [Fact]
    public async Task CreateListWithSelectedImageCommand_ValidImagePath_CreatesCheckList()
    {
        // Arrange
        var imagePath = "valid/path";
        var checkList = new CheckList { Items = new ObservableCollection<CheckItem> { new CheckItem { ItemText = "Item" } } };
        _mediaServiceMock.Setup(service => service.DoPickPhoto()).ReturnsAsync(imagePath);
        _computerVisionServiceMock.Setup(service => service.GetCheckItems(imagePath)).ReturnsAsync(checkList);

        var viewModel = new TestMainViewModel(
            _mediaServiceMock.Object,
            _computerVisionServiceMock.Object,
            _liteDbServiceMock.Object,
            _popupServiceMock.Object,
            _alertServiceMock.Object,
            _adMobConstans.Object
        );

        // Act
        await viewModel.CreateCheckListWithSelectedImageCommand.ExecuteAsync(null);

        // Assert
        _liteDbServiceMock.Verify(service => service.Insert(checkList), Times.Once);
    }

    [Fact]
    public async Task CreateListWithCapturedImageCommand_ValidImagePath_CreatesCheckList()
    {
        // Arrange
        var imagePath = "valid/path";
        var checkList = new CheckList { Items = new ObservableCollection<CheckItem> { new CheckItem { ItemText = "Item" } } };
        _mediaServiceMock.Setup(service => service.DoCapturePhoto()).ReturnsAsync(imagePath);
        _computerVisionServiceMock.Setup(service => service.GetCheckItems(imagePath)).ReturnsAsync(checkList);

        var viewModel = new TestMainViewModel(
            _mediaServiceMock.Object,
            _computerVisionServiceMock.Object,
            _liteDbServiceMock.Object,
            _popupServiceMock.Object,
            _alertServiceMock.Object,
            _adMobConstans.Object
        );

        // Act
        await viewModel.CreateCheckListWithCapturedImageCommand.ExecuteAsync(null);

        // Assert
        _liteDbServiceMock.Verify(service => service.Insert(checkList), Times.Once);
    }
}
