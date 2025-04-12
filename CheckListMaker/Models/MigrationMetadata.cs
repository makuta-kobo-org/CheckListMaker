namespace CheckListMaker.Models;

/// <summary>
/// マイグレーションメタデータを保持するクラス
/// </summary>
public class MigrationMetadata
{
    // 固定のIDを利用して、複数回の検索を容易にする
    public string Id { get; set; } = "schemaVersion";
    public int Version { get; set; }
}
