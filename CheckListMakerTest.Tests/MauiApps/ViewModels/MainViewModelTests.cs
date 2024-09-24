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
    private readonly Mock<IMyPopupService> _popupServiceMock;
    private readonly Mock<AdMobConstants> _adMobConstans;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _mediaServiceMock = new Mock<IMediaService>();
        _computerVisionServiceMock = new Mock<IComputerVisionService>();
        _popupServiceMock = new Mock<IMyPopupService>();
        _adMobConstans = new Mock<AdMobConstants>();

        _viewModel = new MainViewModel(
            _mediaServiceMock.Object,
            _computerVisionServiceMock.Object,
            _popupServiceMock.Object,
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
    public async Task AddItem_WithInputText_AddsNewCheckItem()
    {
        // Arrange
        _viewModel.InputText = "New Item";
        _viewModel.CurrentItems = new CheckItems { Items = [] };

        // Act
        await _viewModel.AddItemCommand.ExecuteAsync(null);

        // Assert
        _viewModel.CurrentItems.Items.Any(i => i.ItemText == "New Item").IsTrue();
        _viewModel.InputText.Is(string.Empty);
    }

    [Fact]
    public async Task AddItem_WithEmptyInputText_DoesNotAddCheckItem()
    {
        // Arrange
        _viewModel.InputText = string.Empty;
        _viewModel.CurrentItems = new CheckItems { Items = [] };

        // Act
        await _viewModel.AddItemCommand.ExecuteAsync(null);

        // Assert
        _viewModel.CurrentItems.Items.Any().IsFalse();
    }

    [Fact]
    public async Task DeleteItem_RemovesCheckItemFromList()
    {
        // Arrange
        var itemToDelete = new CheckItem { ItemText = "Item to delete" };
        _viewModel.CurrentItems = new CheckItems { Items = new ObservableCollection<CheckItem> { itemToDelete } };

        // Act
        await _viewModel.DeleteItemCommand.ExecuteAsync(itemToDelete);

        // Assert
        _viewModel.CurrentItems.Items.Contains(itemToDelete).IsFalse();
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
