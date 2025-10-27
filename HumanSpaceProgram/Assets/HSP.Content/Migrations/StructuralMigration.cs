using Version = HSP.Content.Version;

namespace HSP.Content.Migrations
{
    public delegate void StructuralMigrationFunc( IMigrationContext context );

    /// <summary>
    /// Represents a *structural* migration from a specific version to another specific version.
    /// </summary>
    /// <remarks>
    /// Structural migrations are intended to give the user full control over the files inside the migrated directory, instead of just modifying data inside individual files.
    /// </remarks>
    public struct StructuralMigration
    {
        public Version FromVersion { get; }
        public Version ToVersion { get; }
        public StructuralMigrationFunc MigrationFunc { get; }
        public string Description { get; }

        public StructuralMigration( Version fromVersion, Version toVersion, StructuralMigrationFunc migrationFunc, string description = null )
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