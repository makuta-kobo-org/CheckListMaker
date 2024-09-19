using CheckListMaker.Models;
using Xunit.Abstractions;

namespace CheckListMakerTest.Tests.MauiApps.Models;

public class CheckItemTest(ITestOutputHelper output)
{
    [Fact]
    public void CreateNew_IsDefaultValue_Correct()
    {
        var actual = new CheckItem();

        actual.IsChecked.IsFalse();
    }

    [Fact]
    public void CreateNewWithText_IsItemText_Correct()
    {
        var expect = "Item Text Value";
        var actual = new CheckItem() { ItemText = expect};
        actual.ItemText.Is(expect);

        output.WriteLine(actual.ItemText);
    }
}
