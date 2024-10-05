using CheckListMaker.Models;
using Xunit.Abstractions;

namespace CheckListMakerTest.Tests.MauiApps.Models;

public class CheckItemTests(ITestOutputHelper output)
{
    [Fact]
    public void CreateNew_IsDefaultValue_Correct()
    {
        // Act
        var actual = new CheckItem();

        // Assert
        actual.IsChecked.IsFalse();
    }

    [Fact]
    public void CreateNewWithText_IsItemText_Correct()
    {
        // Arrange
        var expect = "Item Text Value";

        // Act
        var actual = new CheckItem() { ItemText = expect };

        // Assert
        actual.ItemText.Is(expect);

        output.WriteLine(actual.ItemText);
    }
}
