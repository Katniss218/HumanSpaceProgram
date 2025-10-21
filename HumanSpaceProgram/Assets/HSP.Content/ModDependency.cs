using System;
using UnityPlus.Serialization;

namespace HSP.Content
{
    /// <summary>
    /// Represents a dependency on another mod with optional version constraints.
    /// </summary>
    [Serializable]
    public class ModDependency
    {
        /// <summary>
        /// The ID of the required mod.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The version constraint for the required mod. Null means any version is acceptable.
        /// </summary>
        public VersionConstraint? Version { get; set; }

        /// <summary>
        /// Whether this dependency is optional. If a dependency is missing, the mod will not load, unless the missing dependency is optional. <br/>
        /// Use to mark a mod as "supported but not required".
        /// </summary>
        public bool IsOptional { get; set; } = false;

        public ModDependency()
        {
        }

        public ModDependency( string id, VersionConstraint? versions = null )
        {
            this.ID = id;
            this.Version = versions;
        }

        public ModDependency( string id, VersionConstraint? versions, bool isOptional )
        {
            this.ID = id;
            this.Version = versions;
            this.IsOptional = isOptional;
        }

        /// <summary>
        /// Checks if the given mod version satisfies this dependency.
        /// </summary>
        public bool IsSatisfiedBy( Version version )
        {
            if( Version == null )
                return true;

            return Version.Value.IsSatisfiedBy( version );
        }

        public override string ToString()
        {
            string result = ID;
            if( Version.HasValue )
            {
                result += $" {Version.Value}";
            }
            if( IsOptional )
            {
                result += " (optional)";
            }
            return result;
        }

        [MapsInheritingFrom( typeof( ModDependency ) )]
        public static SerializationMapping ModDependencyMapping()
        {
            return new MemberwiseSerializationMapping<ModDependency>()
                .WithMember( "id", o => o.ID )
                .WithMember( "version", o => o.Version )
                .WithMember( "optional", o => o.IsOptional );
        }
    }
}