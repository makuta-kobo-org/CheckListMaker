using System.Collections.ObjectModel;
using CheckListMaker.Controls;
using CheckListMaker.Factories;
using CheckListMaker.Models;
using CheckListMaker.Services;
using CheckListMaker.ViewModels;
using Moq;

namespace CheckListMakerTest.Tests.MauiApps.ViewModels;

/// <summary>
/// Unit tests for the <see cref="MainViewModel"/> class.
/// </summary>
public class MainViewModelTests
{
    private readonly Mock<IMediaService> _mediaServiceMock;
    private readonly Mock<IComputerVisionService> _computerVisionServiceMock;
    private readonly Mock<ILiteDbService> _liteDbServiceMock;
    private readonly Mock<ICustomPopupService> _popupServiceMock;
    private readonly Mock<IAlertService> _alertServiceMock;
    private readonly Mock<AdMobConstants> _adMobConstans;
    private readonly MainViewModel _viewModel;
    private readonly IAddItemPopupViewFactory _addItemPopupViewFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModelTests"/> class.
    /// Sets up mocks and initializes the <see cref="MainViewModel"/> instance.
    /// </summary>
    public MainViewModelTests()
    {
        _mediaServiceMock = new Mock<IMediaService>();
        _computerVisionServiceMock = new Mock<IComputerVisionService>();
        _liteDbServiceMock = new Mock<ILiteDbService>();
        _popupServiceMock = new Mock<ICustomPopupService>();
        _alertServiceMock = new Mock<IAlertService>();
        _adMobConstans = new Mock<AdMobConstants>();
        _addItemPopupViewFactory = new AddItemPopupViewFactory();

        _viewModel = new MainViewModel(
            _mediaServiceMock.Object,
            _computerVisionServiceMock.Object,
            _liteDbServiceMock.Object,
            _popupServiceMock.Object,
            _alertServiceMock.Object,
            _adMobConstans.Object,
            _addItemPopupViewFactory
        );
    }

    /// <summary>
    /// A test-specific implementation of <see cref="MainViewModel"/> that overrides permission requests.
    /// </summary>
    private class TestMainViewModel : MainViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMainViewModel"/> class.
        /// </summary>
        public TestMainViewModel(
            IMediaService mediaService,
            IComputerVisionService computerVisionService,
            ILiteDbService liteDbService,
            ICustomPopupService popupService,
            IAlertService alertService,
            AdMobConstants adMobConstants,
            IAddItemPopupViewFactory addItemPopupViewFactory)
            : base(mediaService, computerVisionService, liteDbService, popupService, alertService, adMobConstants, addItemPopupViewFactory)
        {
        }

        /// <summary>
        /// Overrides the permission request to always return <see cref="PermissionStatus.Granted"/>.
        /// </summary>
        protected override Task<PermissionStatus> RequestPermissionsAsync<TPermission>()
            => Task.FromResult(PermissionStatus.Granted);
    }

    /// <summary>
    /// Tests that toggling the number of columns to true sets the column count to 2.
    /// </summary>
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

    /// <summary>
    /// Tests that toggling the number of columns to false sets the column count to 1.
    /// </summary>
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

    /// <summary>
    /// Tests that deleting an item removes it from the checklist.
    /// </summary>
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

    /// <summary>
    /// Tests that tapping an item toggles its checked state.
    /// </summary>
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

    /// <summary>
    /// Tests that tapping an item updates the checklist in the database.
    /// </summary>
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

    /// <summary>
    /// Tests that dragging an item away sets its "IsBeingDraggedOver" property to false.
    /// </summary>
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

    /// <summary>
    /// Tests that the appearing command reads the checklist on the first launch.
    /// </summary>
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

    /// <summary>
    /// Tests that dragging an item sets its "IsBeingDragged" property to true.
    /// </summary>
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

    /// <summary>
    /// Tests that dragging over an item sets its "IsBeingDraggedOver" property to true.
    /// </summary>
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

    /// <summary>
    /// Tests that dropping an item moves it and updates the database.
    /// </summary>
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

    /// <summary>
    /// Tests that creating a checklist with a selected image inserts it into the database.
    /// </summary>
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
            _adMobConstans.Object,
            _addItemPopupViewFactory
        );

        // Act
        await viewModel.CreateCheckListWithSelectedImageCommand.ExecuteAsync(null);

        // Assert
        _liteDbServiceMock.Verify(service => service.Insert(checkList), Times.Once);
    }

    /// <summary>
    /// Tests that creating a checklist with a captured image inserts it into the database.
    /// </summary>
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
            _adMobConstans.Object,
            _addItemPopupViewFactory
        );

        // Act
        await viewModel.CreateCheckListWithCapturedImageCommand.ExecuteAsync(null);

        // Assert
        _liteDbServiceMock.Verify(service => service.Insert(checkList), Times.Once);
    }
}
