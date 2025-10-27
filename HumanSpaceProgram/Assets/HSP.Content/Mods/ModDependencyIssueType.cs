namespace HSP.Content.Mods
{
    public enum ModDependencyIssueType
    {
        /// <summary>
        /// A required mod is missing.
        /// </summary>
        Missing,
        /// <summary>
        /// The loaded mod version does not match the required version.
        /// </summary>
        VersionMismatch
    }
}