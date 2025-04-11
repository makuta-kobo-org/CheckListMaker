using CheckListMaker.Controls;
using CheckListMaker.Models;
using CheckListMaker.Services;
using CheckListMaker.ViewModels;
using CommunityToolkit.Maui.Views;
using LiteDB;
using Moq;

namespace CheckListMakerTest.Tests.MauiApps.ViewModels;

public class HistoryViewModelTests
{
    private readonly Mock<ICustomPopupService> _popupServiceMock;
    private readonly Mock<ILiteDbService> _liteDbServiceMock;
    private readonly Mock<IAlertService> _alertServiceMock;
    private readonly HistoryViewModel _viewModel;

    public HistoryViewModelTests()
    {
        _popupServiceMock = new Mock<ICustomPopupService>();
        _liteDbServiceMock = new Mock<ILiteDbService>();
        _alertServiceMock = new Mock<IAlertService>();

        _viewModel = new HistoryViewModel(
            _popupServiceMock.Object,
            _liteDbServiceMock.Object,
            _alertServiceMock.Object
        );
    }

    [Fact]
    public void Appearing_ShouldPopulateCheckLists()
    {
        // Arrange
        var checkLists = new List<CheckList>
        {
            new CheckList { CreatedDateTime = DateTimeOffset.Now },
            new CheckList { CreatedDateTime = DateTimeOffset.Now.AddDays(-1) }
        };
        _liteDbServiceMock.Setup(x => x.FindAll()).Returns(checkLists);

        // Act
        _viewModel.AppearingCommand.Execute(null);

        // Assert
        _viewModel.CheckLists.Count.Is(2);
        _viewModel.CheckLists[0].CreatedDateTime.Is(checkLists[0].CreatedDateTime);
        _viewModel.CheckLists[1].CreatedDateTime.Is(checkLists[1].CreatedDateTime);
    }

    [Fact]
    public async Task RemoveCheckList_ShouldRemoveItemFromCollectionAndDeleteFromDb()
    {
        // Arrange
        var checkList = new CheckList { Id = new ObjectId() };
        _viewModel.CheckLists.Add(checkList);
        _alertServiceMock.Setup(x => x.ShowOkCancelAlert(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        // Act
        await _viewModel.RemoveCheckListCommand.ExecuteAsync(checkList);

        // Assert
        _viewModel.CheckLists.Count.Is(0);
        _liteDbServiceMock.Verify(x => x.Delete(checkList), Times.Once);
    }

    [Fact]
    public async Task EditTitle_WhenInputNullOrUnchanged_ShouldNotUpdate()
    {
        // Arrange
        var checkList = new CheckList { Id = new ObjectId(), Title = "Original Title" };
        _alertServiceMock.Setup(x => x.ShowPromptAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync("Original Title");

        // Act
        await _viewModel.EditTitleCommand.ExecuteAsync(checkList);

        // Assert
        _liteDbServiceMock.Verify(x => x.Upsert(It.IsAny<CheckList>()), Times.Never);
        _popupServiceMock.Verify(x => x.ShowPopup(It.IsAny<Popup>()), Times.Never);
    }

    [Fact]
    public async Task EditTitle_WhenInputNewTitle_ShouldUpdateTitle()
    {
        // Arrange
        var checkList = new CheckList { Id = new ObjectId(), Title = "Old Title" };
        string newTitle = "New Title";
        _alertServiceMock.Setup(x => x.ShowPromptAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(newTitle);

        // Act
        await _viewModel.EditTitleCommand.ExecuteAsync(checkList);

        // Assert
        checkList.Title.Is(newTitle);
        _liteDbServiceMock.Verify(x => x.Upsert(checkList), Times.Once);
        _popupServiceMock.Verify(x => x.ShowPopup(It.IsAny<Popup>()), Times.Once);
        _popupServiceMock.Verify(x => x.ClosePopup(It.IsAny<Popup>()), Times.Once);
    }

    [Fact]
    public async Task EditTitle_WhenExceptionThrown_ShowsAlertAndClosesPopup()
    {
        // Arrange
        var checkList = new CheckList { Id = new ObjectId(), Title = "Old Title" };
        string newTitle = "New Title";
        _alertServiceMock.Setup(x => x.ShowPromptAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(newTitle);
        _liteDbServiceMock.Setup(x => x.Upsert(checkList)).Throws(new Exception("Upsert error"));

        // Act
        await _viewModel.EditTitleCommand.ExecuteAsync(checkList);

        // Assert
        _alertServiceMock.Verify(x => x.ShowAlert("Error", "Upsert error"), Times.Once);
        _popupServiceMock.Verify(x => x.ClosePopup(It.IsAny<Popup>()), Times.Once);
    }

    [Fact]
    public async Task RemoveCheckList_WhenCancelled_ShouldNotRemoveOrDelete()
    {
        // Arrange
        var checkList = new CheckList { Id = new ObjectId() };
        _viewModel.CheckLists.Add(checkList);
        _alertServiceMock.Setup(x => x.ShowOkCancelAlert(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        // Act
        await _viewModel.RemoveCheckListCommand.ExecuteAsync(checkList);

        // Assert
        _viewModel.CheckLists.Contains(checkList).IsTrue();
        _liteDbServiceMock.Verify(x => x.Delete(It.IsAny<CheckList>()), Times.Never);
    }
}
