using CheckListMaker.Models;
using Xunit.Abstractions;

namespace CheckListMakerTest.Tests.MauiApps.Models;

public class CheckItemsTests(ITestOutputHelper output)
{
    [Fact]
    public void CreateNew_IsDefaultValue_Correct()
    {
        // Act
        var actual = new CheckList();

        // Assert
        actual.CreatedDateTime.IsNotNull();

        output.WriteLine(actual.CreatedDateTime.ToString());
    }

    [Fact]
    public void AddCheckItem_Success()
    {
        // Arrange
        var actual = new CheckList();

        // Act
        actual.Items.Add(new CheckItem() { ItemText = "Test" });

        // Assert
        actual.Items[0].ItemText.Is("Test");

        foreach (var item in actual.Items)
        {
            output.WriteLine(item.ItemText);
        }
    }

    [Fact]
    public void ItemTextsOneLine_ShouldReturnsOneLineSuccessfully()
    {
        // Arrange
        var _text1 = "TEST-1";
        var _text2 = "TEST-2";
        var _text3 = "TEST-3";

        var expect = $"{_text1} {_text2} {_text3}";

        var items = new CheckList();
        items.Items.Add(new CheckItem() { ItemText = _text1 });
        items.Items.Add(new CheckItem() { ItemText = _text2 });
        items.Items.Add(new CheckItem() { ItemText = _text3 });

        // Act
        var actual = items.ItemTextsOneLine;

        // Assert
        actual.Is(expect);
    }
}
