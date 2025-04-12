using CheckListMaker.Models;
using CheckListMaker.Resources;
using LiteDB;

namespace CheckListMaker.Services;

/// <summary>
/// LiteDB の CRUD 操作およびマイグレーションを行うクラス
/// </summary>
internal sealed class LiteDbService : ILiteDbService, IDisposable
{
    private const int CURRENT_DB_VERSION = 1;

    private readonly int _upperLimit;
    private readonly string _dbFilePath;
    private LiteDatabase? _database;

    /// <summary> Constructor </summary>
    public LiteDbService(string dbFilePath, int upperLimit)
    {
        _dbFilePath = dbFilePath;
        _upperLimit = upperLimit;
        _database = new LiteDatabase(_dbFilePath);
        MigrateIfNeeded();
    }

    /// <summary> 必要に応じてマイグレーションを実行する </summary>
    private void MigrateIfNeeded()
    {
        // "migration_metadata" コレクションを利用してスキーマバージョンを管理
        var metadataCol = _database!.GetCollection<MigrationMetadata>("migration_metadata");
        var metadata = metadataCol.FindById("schemaVersion");

        if (metadata == null)
        {
            // 初回マイグレーション (初期状態はバージョン0)
            int previousVersion = 0;
            if (previousVersion < CURRENT_DB_VERSION)
            {
                ApplyMigration(previousVersion, CURRENT_DB_VERSION);
            }

            // マイグレーション終了後、バージョン情報を登録
            metadataCol.Upsert(new MigrationMetadata { Id = "schemaVersion", Version = CURRENT_DB_VERSION });
        }
        else if (metadata.Version < CURRENT_DB_VERSION)
        {
            // 既存バージョンから CURRENT_DB_VERSION へのマイグレーション処理
            ApplyMigration(metadata.Version, CURRENT_DB_VERSION);

            // マイグレーション終了後、バージョン情報を更新
            metadata.Version = CURRENT_DB_VERSION;
            metadataCol.Update(metadata);
        }
    }

    /// <summary>
    /// 指定されたバージョン間のマイグレーションを適用する
    /// </summary>
    /// <param name="fromVersion">現在のバージョン</param>
    /// <param name="toVersion">目標バージョン</param>
    private void ApplyMigration(int fromVersion, int toVersion)
    {
        var col = _database!.GetCollection<CheckList>();

        // バージョンごとのマイグレーション処理を記述
        if (fromVersion < 1)
        {
            // バージョン 0 -> 1 のマイグレーション処理
            // 例: CheckList に新しいフィールドを追加し、初期値を設定
            foreach (var item in col.FindAll())
            {
                item.Title = AppResource.CheckList_Text_Title; // 新しいフィールドに初期値を設定
                col.Update(item);
            }
        }

        if (fromVersion < 2)
        {
            // バージョン 1 -> 2 のマイグレーション処理
            // 例: 別のフィールドを追加または変更
            // ※ 必要に応じて実装
        }

        // 必要に応じてさらにバージョン間の処理を追加
    }

    /// <summary> 全ての CheckList を取得する </summary>
    public List<CheckList> FindAll()
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        return col.FindAll().ToList();
    }

    /// <summary> CheckList を追加する </summary>
    public void Insert(CheckList checkList)
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        col.Insert(checkList);
        DeleteAboveTheUpperLimit(col);
    }

    /// <summary> CheckList を更新または追加する </summary>
    public void Upsert(CheckList checkList)
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        col.Upsert(checkList);
    }

    /// <summary> CheckList を削除する </summary>
    public void Delete(CheckList checkList)
    {
        EnsureDatabase();
        var col = _database!.GetCollection<CheckList>();
        col.Delete(checkList.Id);
    }

    /// <summary> リソースを解放する </summary>
    public void Dispose()
    {
        if (_database == null)
        {
            return;
        }

        _database?.Dispose();
        _database = null;
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

    /// <summary> LiteDatabase インスタンスを確認し、必要に応じて再初期化する </summary>
    private void EnsureDatabase() => _database ??= new LiteDatabase(_dbFilePath);
}
