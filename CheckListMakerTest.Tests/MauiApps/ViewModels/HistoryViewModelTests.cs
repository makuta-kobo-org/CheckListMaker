using CheckListMaker.Controls;
using CheckListMaker.Models;
using CheckListMaker.Services;
using CheckListMaker.ViewModels;
using LiteDB;
using Moq;

namespace CheckListMakerTest.Tests.MauiApps.ViewModels;

public class HistoryViewModelTests
{
    private readonly Mock<IMyPopupService> _popupServiceMock;
    private readonly Mock<ILiteDbService> _liteDbServiceMock;
    private readonly Mock<IAlertService> _alertServiceMock;
    private readonly HistoryViewModel _viewModel;

    public HistoryViewModelTests()
    {
        _popupServiceMock = new Mock<IMyPopupService>();
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
}
