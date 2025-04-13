using CheckListMaker.Models;
using CheckListMaker.Resources;
using LiteDB;

namespace CheckListMaker.Services;

/// <summary>
/// A service class for performing CRUD operations and migrations using LiteDB.
/// </summary>
internal sealed class LiteDbService : ILiteDbService, IDisposable
{
    private const int CURRENT_DB_VERSION = 1;

    private readonly int _upperLimit;
    private readonly string _dbFilePath;
#nullable enable
    private LiteDatabase? _database;
#nullable disable

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteDbService"/> class.
    /// </summary>
    /// <param name="dbFilePath">The file path of the LiteDB database.</param>
    /// <param name="upperLimit">The maximum number of records allowed in the database.</param>
    public LiteDbService(string dbFilePath, int upperLimit)
    {
        _dbFilePath = dbFilePath;
        _upperLimit = upperLimit;
        _database = new LiteDatabase(_dbFilePath);
        MigrateIfNeeded();
    }

    /// <summary>
    /// Retrieves all CheckList records from the database.
    /// </summary>
    /// <returns>A list of all CheckList records.</returns>
    public List<CheckList> FindAll()
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        return col.FindAll().ToList();
    }

    /// <summary>
    /// Inserts a new CheckList record into the database.
    /// </summary>
    /// <param name="checkList">The CheckList record to insert.</param>
    public void Insert(CheckList checkList)
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        col.Insert(checkList);
        DeleteAboveTheUpperLimit(col);
    }

    /// <summary>
    /// Updates an existing CheckList record or inserts it if it does not exist.
    /// </summary>
    /// <param name="checkList">The CheckList record to update or insert.</param>
    public void Upsert(CheckList checkList)
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        col.Upsert(checkList);
    }

    /// <summary>
    /// Deletes a CheckList record from the database.
    /// </summary>
    /// <param name="checkList">The CheckList record to delete.</param>
    public void Delete(CheckList checkList)
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        col.Delete(checkList.Id);
    }

    /// <summary>
    /// Releases the resources used by the LiteDbService.
    /// </summary>
    public void Dispose()
    {
        if (_database == null)
        {
            return;
        }

        _database?.Dispose();
        _database = null;
    }

    /// <summary>
    /// Applies migrations between the specified versions.
    /// </summary>
    /// <param name="fromVersion">The current schema version.</param>
    /// <param name="toVersion">The target schema version.</param>
    private void ApplyMigration(int fromVersion, int toVersion)
    {
        var col = _database!.GetCollection<CheckList>();

        if (fromVersion < 1)
        {
            // Migration from version 0 to 1
            foreach (var item in col.FindAll())
            {
                item.Title = AppResource.CheckList_Text_Title; // Set default value for the new field
                col.Update(item);
            }
        }

        if (fromVersion < 2)
        {
            // Migration from version 1 to 2
            // Add or modify fields as needed
        }

        // Add further migration steps as needed
    }

    /// <summary>
    /// Deletes records exceeding the upper limit, keeping only the most recent ones.
    /// </summary>
    /// <param name="col">The collection of CheckList records.</param>
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

    /// <summary>
    /// Executes migrations if needed based on the current database schema version.
    /// </summary>
    private void MigrateIfNeeded()
    {
        var metadataCol = _database!.GetCollection<MigrationMetadata>("migration_metadata");
        var metadata = metadataCol.FindById("schemaVersion");

        if (metadata == null)
        {
            // Initial migration (default version is 0)
            int previousVersion = 0;
            if (previousVersion < CURRENT_DB_VERSION)
            {
                ApplyMigration(previousVersion, CURRENT_DB_VERSION);
            }

            // Register schema version after migration
            metadataCol.Upsert(new MigrationMetadata { Id = "schemaVersion", Version = CURRENT_DB_VERSION });
        }
        else if (metadata.Version < CURRENT_DB_VERSION)
        {
            // Migrate from the existing version to the current version
            ApplyMigration(metadata.Version, CURRENT_DB_VERSION);

            // Update schema version after migration
            metadata.Version = CURRENT_DB_VERSION;
            metadataCol.Update(metadata);
        }
    }

    /// <summary>
    /// Ensures the LiteDatabase instance is initialized, reinitializing it if necessary.
    /// </summary>
    private void EnsureDatabase() => _database ??= new LiteDatabase(_dbFilePath);
}
