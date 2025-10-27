using UnityPlus.Serialization;
using Version = HSP.Content.Version;

namespace HSP.Content.Migrations
{
    public delegate void DataMigrationFunc( ref SerializedData data );

    /// <summary>
    /// Represents a *data* migration from a specific version to another specific version.
    /// </summary>
    /// <remarks>
    /// Data migrations are intended to be used for "pure" (stateless) data transformations inside individual files, such as renaming fields, changing types, etc.
    /// </remarks>
    public struct DataMigration
    {
        public Version FromVersion { get; }
        public Version ToVersion { get; }
        public DataMigrationFunc MigrationFunc { get; }
        public string Description { get; }

        public DataMigration( Version fromVersion, Version toVersion, DataMigrationFunc migrationFunc, string description = null )
        {
            this.FromVersion = fromVersion;
            this.ToVersion = toVersion;
            this.MigrationFunc = migrationFunc;
            this.Description = description;
        }

        public override string ToString()
        {
            return $"{FromVersion} -> {ToVersion}" + (string.IsNullOrEmpty( Description ) ? "" : $" ({Description})");
        }
    }
}