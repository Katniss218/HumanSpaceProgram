namespace HSP.Content.Mods
{
    /// <summary>
    /// Represents a compatibility issue in mod dependencies.
    /// </summary>
    public class ModDependencyIssue
    {
        public string ModID { get; }
        public ModDependencyIssueType Type { get; }
        public Version RequiredVersion { get; }
        public Version? LoadedVersion { get; }

        public ModDependencyIssue( string modId, ModDependencyIssueType type, Version requiredVersion, Version? loadedVersion )
        {
            this.ModID = modId;
            this.Type = type;
            this.RequiredVersion = requiredVersion;
            this.LoadedVersion = loadedVersion;
        }

        public override string ToString()
        {
            switch( Type )
            {
                case ModDependencyIssueType.Missing:
                    return $"Mod '{ModID}' is not loaded (required version {RequiredVersion})";
                case ModDependencyIssueType.VersionMismatch:
                    return $"Mod '{ModID}' version mismatch: required {RequiredVersion}, loaded {LoadedVersion}";
                default:
                    return $"Unknown issue with mod '{ModID}'";
            }
        }
    }
}
