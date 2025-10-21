using UnityPlus.Serialization;
using Version = HSP.Content.Version;

namespace HSP.Timelines
{
    public delegate void SaveMigrationFunc( ref SerializedData data );

    /// <summary>
    /// Represents a migration from a specific version to another specific version.
    /// </summary>
    public struct SaveMigration
    {
        public Version FromVersion { get; }
        public Version ToVersion { get; }
        public SaveMigrationFunc MigrationFunc { get; }
        public string Description { get; }

        public SaveMigration( Version fromVersion, Version toVersion, SaveMigrationFunc migrationFunc, string description = null )
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