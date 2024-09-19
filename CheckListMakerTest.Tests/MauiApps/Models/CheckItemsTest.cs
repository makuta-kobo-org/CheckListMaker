using CheckListMaker.Models;
using Xunit.Abstractions;

namespace CheckListMakerTest.Tests.MauiApps.Models;

public class CheckItemsTest(ITestOutputHelper output)
{
    [Fact]
    public void CreateNew_IsDefaultValue_Correct()
    {
        var actual = new CheckItems();

        actual.CreatedDateTime.IsNotNull();

        output.WriteLine(actual.CreatedDateTime.ToString());
    }

    [Fact]
    public void AddCheckItem_Success()
    {
        var actual = new CheckItems();

        actual.Items.Add(new CheckItem() { ItemText = "Test" });

        actual.Items[0].ItemText.Is("Test");

        foreach (var item in actual.Items)
        {
            output.WriteLine(item.ItemText);
        }
    }
}
