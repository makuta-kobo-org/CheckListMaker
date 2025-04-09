using CheckListMaker.Models;

namespace CheckListMaker.Services;

/// <summary>
/// Interface for LiteDbService, providing methods to interact with the LiteDB database.
/// </summary>
public interface ILiteDbService
{
    /// <summary>
    /// Retrieves all CheckLists from the database.
    /// </summary>
    /// <returns>A list of all CheckLists.</returns>
    List<CheckList> FindAll();

    /// <summary>
    /// Inserts a new CheckList into the database.
    /// </summary>
    /// <param name="checkList">The CheckList to insert.</param>
    void Insert(CheckList checkList);

    /// <summary>
    /// Updates an existing CheckList or inserts it if it does not exist.
    /// </summary>
    /// <param name="checkList">The CheckList to update or insert.</param>
    void Upsert(CheckList checkList);

    /// <summary>
    /// Deletes a CheckList from the database.
    /// </summary>
    /// <param name="checkList">The CheckList to delete.</param>
    void Delete(CheckList checkList);
}
