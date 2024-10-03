using CheckListMaker.Models;
using LiteDB;
using Microsoft.Extensions.Configuration;

namespace CheckListMaker.Services;

/// <summary> Lite DBのCRUDを行う </summary>
internal sealed class LiteDbService : ILiteDbService
{
    private readonly int _upperLimit = 10;
    private readonly string _dbFilePath;

    /// <summary> Constructor </summary>
    public LiteDbService(string dbFilePath, int upperLimit)
    {
        _upperLimit = upperLimit;
        _dbFilePath = dbFilePath;
    }

    /// <summary> Add CheckList </summary>
    public List<CheckList> FindAll()
    {
        using var db = new LiteDatabase(_dbFilePath);

        var col = db.GetCollection<CheckList>();

        return col.FindAll().ToList();
    }

    /// <summary> Add a CheckList </summary>
    public void Insert(CheckList checkList)
    {
        using var db = new LiteDatabase(_dbFilePath);

        var col = db.GetCollection<CheckList>();

        col.Insert(checkList);

        DeleteAboveTheUpperLimit(col);
    }

    /// <summary> Update CheckList </summary>
    public void Update(CheckList checkList)
    {
        using var db = new LiteDatabase(_dbFilePath);

        var col = db.GetCollection<CheckList>();

        col.Update(checkList);
    }

    /// <summary> Delete CheckList </summary>
    public void Delete(CheckList checkList)
    {
        using var db = new LiteDatabase(_dbFilePath);

        var col = db.GetCollection<CheckList>();

        col.Delete(checkList.Id);
    }

    /// <summary> 上限数以上の古いデータを削除する </summary>
    private void DeleteAboveTheUpperLimit(ILiteCollection<CheckList> col)
    {
        var checkLists = col.FindAll();

        if (checkLists.Count() > 10)
        {
            var deletionTargets = checkLists.Skip(10).ToList();

            foreach (var deletionTarget in deletionTargets)
            {
                col.Delete(deletionTarget.Id);
            }
        }
    }
}
