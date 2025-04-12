namespace CheckListMaker.Models;

/// <summary>
/// Represents metadata for a database migration, including an identifier and version number.
/// </summary>
public class MigrationMetadata
{
    /// <summary>
    /// Gets or sets the identifier for the migration metadata.
    /// Default value is "schemaVersion".
    /// </summary>
    public string Id { get; set; } = "schemaVersion";

    /// <summary>
    /// Gets or sets the version number of the migration.
    /// </summary>
    public int Version { get; set; }
}
