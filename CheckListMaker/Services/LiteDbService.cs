using CheckListMaker.Models;
using LiteDB;
using Microsoft.Extensions.Configuration;

namespace CheckListMaker.Services;

/// <summary> LiteDBのCRUDを行う </summary>
internal sealed class LiteDbService : ILiteDbService, IDisposable
{
    private readonly int _upperLimit;
    private readonly string _dbFilePath;
    private LiteDatabase? _database;

    /// <summary> Constructor </summary>
    public LiteDbService(string dbFilePath, int upperLimit)
    {
        _dbFilePath = dbFilePath;
        _upperLimit = upperLimit;
        _database = new LiteDatabase(_dbFilePath);
    }

    /// <summary> 全てのCheckListを取得する </summary>
    public List<CheckList> FindAll()
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        return col.FindAll().ToList();
    }

    /// <summary> CheckListを追加する </summary>
    public void Insert(CheckList checkList)
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        col.Insert(checkList);
        DeleteAboveTheUpperLimit(col);
    }

    /// <summary> CheckListを更新または追加する </summary>
    public void Upsert(CheckList checkList)
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        col.Upsert(checkList);
    }

    /// <summary> CheckListを削除する </summary>
    public void Delete(CheckList checkList)
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        col.Delete(checkList.Id);
    }

    /// <summary> 上限数以上の古いデータを削除する </summary>
    private void DeleteAboveTheUpperLimit(ILiteCollection<CheckList> col)
    {
        var checkLists = col.FindAll().OrderByDescending(x => x.CreatedDateTime).ToList();

        if (checkLists.Count > _upperLimit)
        {
            var deletionTargets = checkLists.Skip(_upperLimit).ToList();
            foreach (var deletionTarget in deletionTargets)
            {
                col.Delete(deletionTarget.Id);
            }
        }
    }

    /// <summary> LiteDatabaseインスタンスを確認し、必要に応じて再初期化する </summary>
    private void EnsureDatabase()
    {
        if (_database == null)
        {
            _database = new LiteDatabase(_dbFilePath);
        }
    }

    /// <summary> リソースを解放する </summary>
    public void Dispose()
    {
        _database?.Dispose();
        _database = null;
    }
}
