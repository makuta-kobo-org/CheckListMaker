using System.Reflection;
using CheckListMaker.Models;
using CheckListMaker.Services;
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

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonStream(appsettings)
            .Build();

        // ユニークなファイル名を生成（タイムスタンプ＋GUID）
        string baseFileName = config["LiteDb:FileName"];
        string uniqueFileName = $"{Path.GetFileNameWithoutExtension(baseFileName)}_{Guid.NewGuid()}{Path.GetExtension(baseFileName)}";
        _dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, uniqueFileName);

        _upperLimit = int.TryParse(config["LiteDb:UpperLimit"], out var parsedValue) ? parsedValue : 10;

        _service = new LiteDbService(_dbFilePath, _upperLimit);
    }

    /// <summary> テスト終了時にDBファイルを削除 </summary>
    public void Dispose()
    {
        // サービスがまだ破棄されていない場合は明示的に破棄する
        _service.Dispose();

        // ガベージコレクションと最終化を実施して、ファイルハンドルが確実に解放されるように待つ
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // DBファイルが存在すれば削除
        if (File.Exists(_dbFilePath))
        {
            File.Delete(_dbFilePath);
        }
    }

    [Fact]
    public void FindAll_ShouldReturnAllCheckLists()
    {
        // Arrange: 外部インスタンスではなく、service の Insert 経由でシードする
        _service.Insert(new CheckList { Title = "Test Checklist 1" });
        _service.Insert(new CheckList { Title = "Test Checklist 2" });

        // Act
        var result = _service.FindAll();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Title == "Test Checklist 1");
        Assert.Contains(result, x => x.Title == "Test Checklist 2");
    }

    [Fact]
    public void Insert_ShouldAddNewCheckList()
    {
        // Arrange
        var newCheckList = new CheckList { Title = "New Checklist" };

        // Act
        _service.Insert(newCheckList);
        var result = _service.FindAll();

        // Assert
        Assert.Single(result);
        Assert.Equal("New Checklist", result[0].Title);
    }

    [Fact]
    public void Insert_ShouldDeleteOldestWhenAboveLimit()
    {
        // Arrange
        // 各チェックリストの CreatedDateTime に微小な差分を与えて、古い順に並ぶようにする
        for (int i = 1; i <= _upperLimit + 2; i++)
        {
            var checklist = new CheckList
            {
                Title = $"Checklist {i}",
                CreatedDateTime = DateTimeOffset.Now.AddMilliseconds(i)
            };
            _service.Insert(checklist);
        }

        // Act
        var result = _service.FindAll();

        // Assert
        Assert.Equal(_upperLimit, result.Count);
        // 古いエントリ（Checklist 1、Checklist 2）が削除されていることを確認
        Assert.DoesNotContain(result, x => x.Title == "Checklist 1");
        Assert.DoesNotContain(result, x => x.Title == "Checklist 2");
        // 最新のエントリが残っていることを確認
        Assert.Contains(result, x => x.Title == $"Checklist {_upperLimit + 2}");
    }

    [Fact]
    public void Upsert_ShouldUpdateExistingCheckList()
    {
        // Arrange
        var checklist = new CheckList { Title = "Original Title" };
        _service.Insert(checklist);

        // Act
        checklist.Title = "Updated Title";
        _service.Upsert(checklist);
        var result = _service.FindAll();

        // Assert
        Assert.Single(result);
        Assert.Equal("Updated Title", result[0].Title);
    }

    [Fact]
    public void Delete_ShouldRemoveCheckList()
    {
        // Arrange
        var checklist = new CheckList { Title = "To Be Deleted" };
        _service.Insert(checklist);

        // Act
        _service.Delete(checklist);
        var result = _service.FindAll();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void EnsureDatabase_ShouldReinitializeDatabaseAfterDispose()
    {
        // Arrange
        var checklist = new CheckList { Title = "Test Checklist" };
        _service.Insert(checklist);

        // Act
        _service.Dispose(); // Dispose で LiteDatabase を閉じる
        var result = _service.FindAll(); // EnsureDatabase により再初期化される

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Checklist", result[0].Title);
    }

    [Fact]
    public void Insert_ShouldHandleNullDatabaseGracefully()
    {
        // Arrange
        // 同一インスタンスの Dispose 後に再利用すると他のテストに影響するため、新規インスタンスを生成する
        _service.Dispose();
        var serviceAfterDispose = new LiteDbService(_dbFilePath, _upperLimit);

        // Act
        var checklist = new CheckList { Title = "New Checklist After Dispose" };
        serviceAfterDispose.Insert(checklist);
        var result = serviceAfterDispose.FindAll();

        // Assert
        Assert.Single(result);
        Assert.Equal("New Checklist After Dispose", result[0].Title);

        serviceAfterDispose.Dispose();
    }
}
