using CheckListMaker.Models;
using Xunit.Abstractions;

namespace CheckListMakerTest.Tests.MauiApps.Models;

public class CheckItemsTests(ITestOutputHelper output)
{
    [Fact]
    public void CreateNew_IsDefaultValue_Correct()
    {
        // Act
        var actual = new CheckItems();

        // Assert
        actual.CreatedDateTime.IsNotNull();

        output.WriteLine(actual.CreatedDateTime.ToString());
    }

    [Fact]
    public void AddCheckItem_Success()
    {
        // Arrange
        var actual = new CheckItems();

        // Act
        actual.Items.Add(new CheckItem() { ItemText = "Test" });

        // Assert
        actual.Items[0].ItemText.Is("Test");

        foreach (var item in actual.Items)
        {
            output.WriteLine(item.ItemText);
        }
    }
}
