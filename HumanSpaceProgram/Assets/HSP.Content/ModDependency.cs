using System;
using System.Collections.Generic;
using UnityPlus.Serialization;

namespace HSP.Content
{
    public enum ModDependencyType : byte
    {
        /// <summary>
        /// This mod is required for the mod to function properly.
        /// </summary>
        Required,
        /// <summary>
        /// This mod is optional, the mod may provide functionality or interface with it.
        /// </summary>
        Supported,
        /// <summary>
        /// This mod is incompatible with the mod.
        /// </summary>
        Incompatible
    }

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
        /// The version constraint for the required mod. Null means any version is acceptable. <br/>
        /// This member can be null if the <see cref="DependencyType"/> is <see cref="ModDependencyType.Incompatible"/>, or if the version doesn't matter.
        /// </summary>
        public VersionConstraint? Version { get; set; }

        public ModDependencyType DependencyType { get; set; } = ModDependencyType.Required;

        public ModDependency()
        {
        }

        public ModDependency( string id, VersionConstraint? versions = null )
        {
            this.ID = id;
            this.Version = versions;
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

        public bool IsSatisfiedBy( Dictionary<string, ModMetadata> loadedMods )
        {
            if( this.DependencyType == ModDependencyType.Incompatible )
            {
                return !loadedMods.ContainsKey( this.ID );
            }

            if( this.DependencyType == ModDependencyType.Required )
            {
                if( !loadedMods.TryGetValue( this.ID, out ModMetadata requiredMod ) )
                    return false;

                if( !this.IsSatisfiedBy( requiredMod.Version ) )
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            string result = ID;
            if( Version.HasValue )
            {
                result += $" {Version.Value}";
            }
            result += $" ({DependencyType})";
            return result;
        }

        [MapsInheritingFrom( typeof( ModDependency ) )]
        public static SerializationMapping ModDependencyMapping()
        {
            return new MemberwiseSerializationMapping<ModDependency>()
                .WithMember( "id", o => o.ID )
                .WithMember( "version", o => o.Version )
                .WithMember( "type", o => o.DependencyType );
        }
    }
}