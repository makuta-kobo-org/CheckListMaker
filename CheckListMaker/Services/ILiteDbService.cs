using CheckListMaker.Models;

namespace CheckListMaker.Services;

/// <summary> LiteDbServiceのインターフェイス </summary>
internal interface ILiteDbService
{
    /// <summary> Select All of CheckLists </summary>
    List<CheckList> FindAll();

    /// <summary> Insert a new CheckList </summary>
    void Insert(CheckList checkList);

    /// <summary> Update a CheckList </summary>
    void Update(CheckList checkList);

    /// <summary> Delete a CheckList </summary>
    void Delete(CheckList checkList);
}
