using System.Runtime.Serialization;

namespace HSP.Content.Mods
{
    /// <summary>
    /// Exception thrown when a mod dependency fails to load or can't be resolved.
    /// </summary>
    public class ModDependencyException : ModLoadException
    {
        /// <summary>
        /// The ID of the dependency mod that caused the issue.
        /// </summary>
        public string DependencyModID;
        /// <summary>
        /// The type of dependency issue encountered.
        /// </summary>
        public ModDependencyIssueType Type { get; }
        /// <summary>
        /// The required version of the dependency mod.
        /// </summary>
        public VersionConstraint RequiredVersion { get; }
        /// <summary>
        /// The actual version of the dependency mod, if available.
        /// </summary>
        public Version? ActualVersion { get; }

        public ModDependencyException( string modId, string dependencyModId, ModDependencyIssueType type, VersionConstraint requiredVersion, Version? actualVersion ) : base( modId, $"Failed to load the dependency '{dependencyModId}' of mod '{modId}'." )
        {
            this.DependencyModID = dependencyModId;
            this.Type = type;
            this.RequiredVersion = requiredVersion;
            this.ActualVersion = actualVersion;
        }

        public ModDependencyException( string modId, string dependencyModId, ModDependencyIssueType type, VersionConstraint requiredVersion, Version? actualVersion, string message ) : base( modId, message )
        {
            this.DependencyModID = dependencyModId;
            this.Type = type;
            this.RequiredVersion = requiredVersion;
            this.ActualVersion = actualVersion;
        }

        public ModDependencyException( string modId, string dependencyModId, ModDependencyIssueType type, VersionConstraint requiredVersion, Version? actualVersion, System.Exception inner ) : base( modId, $"Failed to load the dependency '{dependencyModId}' of mod '{modId}'.", inner )
        {
            this.DependencyModID = dependencyModId;
            this.Type = type;
            this.RequiredVersion = requiredVersion;
            this.ActualVersion = actualVersion;
        }

        public ModDependencyException( string modId, string dependencyModId, ModDependencyIssueType type, VersionConstraint requiredVersion, Version? actualVersion, string message, System.Exception inner ) : base( modId, message, inner )
        {
            this.DependencyModID = dependencyModId;
            this.Type = type;
            this.RequiredVersion = requiredVersion;
            this.ActualVersion = actualVersion;
        }

        protected ModDependencyException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }

    }
}