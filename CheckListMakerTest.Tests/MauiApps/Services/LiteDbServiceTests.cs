using System.Reflection;
using CheckListMaker.Models;
using CheckListMaker.Services;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace CheckListMakerTest.Tests.MauiApps.Services;

public class LiteDbServiceTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly LiteDbService _service;
    private readonly string _dbFilePath;
    private readonly int _upperLimit;

    /// <summary> Set up </summary>
    public LiteDbServiceTests(ITestOutputHelper output)
    {
        _output = output;

        using var appsettings = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("CheckListMakerTest.Tests.MauiApps.appsettings.Test.json");

        var _config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonStream(appsettings)
            .Build();

        _dbFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            _config["LiteDb:FileName"]);

        _upperLimit = int.TryParse(_config["LiteDb:UpperLimit"], out var parsedValue) ? parsedValue : 10;

        _service = new LiteDbService(_dbFilePath, _upperLimit);
    }

    /// <summary> デストラクタ </summary>
    public void Dispose()
    {
        if (File.Exists(_dbFilePath))
        {
            File.Delete(_dbFilePath);
        }
    }

    [Fact]
    public void FindAll_ReturnsAllCheckLists()
    {
        // Arrange
        using (var db = new LiteDatabase(_dbFilePath))
        {
            var col = db.GetCollection<CheckList>();
            col.Insert(new CheckList { Items = [new CheckItem { ItemText = "Test Checklist" }] });
        }

        // Act
        var result = _service.FindAll();

        // Assert
        result.Count.Is(1);
        result[0].Items[0].ItemText.Is("Test Checklist");
    }

    [Fact]
    public void Insert_AddsNewCheckList_AndDeletesAboveLimit()
    {
        // Arrange
        for (int i = 1; i <= 12; i++)
        {
            _service.Insert(new CheckList { Items = [new CheckItem { ItemText = $"CheckItem {i}" }] });
        }

        // Act
        var result = _service.FindAll();

        // Assert
        result.Count.Is(_upperLimit);
    }

    [Fact]
    public void Update_UpdatesExistingCheckList()
    {
        // Arrange
        var checklist = new CheckList { Items = [new CheckItem { ItemText = "Old Name" }] };
        _service.Insert(checklist);
        checklist.Items[0].ItemText = "New Name";

        // Act
        _service.Update(checklist);
        var result = _service.FindAll();

        // Assert
        result[0].Items[0].ItemText.Is("New Name");
    }

    [Fact]
    public void Delete_RemovesCheckList()
    {
        // Arrange
        var checklist = new CheckList { Items = [new CheckItem { ItemText = "To Be Deleted" }] };
        _service.Insert(checklist);

        // Act
        _service.Delete(checklist);
        var result = _service.FindAll();

        // Assert
        result.Count.Is(0);
    }
}
