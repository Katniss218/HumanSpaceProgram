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
        public VersionConstraint? Versions { get; set; }

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
            this.Versions = versions;
        }

        public ModDependency( string id, VersionConstraint? versions, bool isOptional )
        {
            this.ID = id;
            this.Versions = versions;
            this.IsOptional = isOptional;
        }

        /// <summary>
        /// Checks if the given mod version satisfies this dependency.
        /// </summary>
        public bool IsSatisfiedBy( Version version )
        {
            if( Versions == null )
                return true;

            return Versions.Value.IsSatisfiedBy( version );
        }

        public override string ToString()
        {
            string result = ID;
            if( Versions.HasValue )
            {
                result += $" {Versions.Value}";
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
                .WithMember( "versions", o => o.Versions )
                .WithMember( "optional", o => o.IsOptional );
        }
    }
}