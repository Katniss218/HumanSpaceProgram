using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;
using Version = HSP.Content.Version;

namespace HSP.Content
{
    /// <summary>
    /// Represents metadata for a mod, including version, dependencies, and other information.
    /// </summary>
    [Serializable]
    public class ModMetadata
    {
        /// <summary>
        /// The name of the file that stores the mod metadata.
        /// </summary>
        public const string MOD_MANIFEST_FILENAME = "_mod.json";

        /// <summary>
        /// The unique ID of this mod. Must match the directory name in GameData.
        /// </summary>
        public string ModID { get; set; }

        /// <summary>
        /// The current version of this mod.
        /// </summary>
        public Version ModVersion { get; set; }

        /// <summary>
        /// The display name of this mod.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The author(s) of this mod.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// A description of this mod.
        /// </summary>
        public string Description { get; set; }

        public bool ExcludeFromSaves { get; set; } = false;

        /// <summary>
        /// The dependencies of this mod.
        /// </summary>
        public List<ModDependency> Dependencies { get; set; } = new List<ModDependency>();

        public ModMetadata()
        {

        }

        /// <summary>
        /// Loads mod metadata from a mod directory.
        /// </summary>
        /// <param name="modDirectory">The path to the mod directory containing _mod.json</param>
        public static ModMetadata LoadFromDisk( string modDirectory )
        {
            string filePath = Path.Combine( modDirectory, MOD_MANIFEST_FILENAME );

            if( !File.Exists( filePath ) )
            {
                throw new FileNotFoundException( $"Mod metadata file not found: {filePath}" );
            }

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( filePath );
            var data = handler.Read();
            ModMetadata metadata = SerializationUnit.Deserialize<ModMetadata>( data );

            // Validate that ModID matches directory name.
            string expectedModId = Path.GetFileName( modDirectory );
            if( metadata.ModID != expectedModId )
            {
                throw new InvalidOperationException( $"Mod ID '{metadata.ModID}' does not match directory name '{expectedModId}'" );
            }

            return metadata;
        }

        /// <summary>
        /// Saves mod metadata to a mod directory.
        /// </summary>
        /// <param name="modDirectory">The path to the mod directory where _mod.json should be written</param>
        public void SaveToDisk( string modDirectory )
        {
            // Validate that ModID matches directory name.
            string expectedModId = Path.GetFileName( modDirectory );
            if( this.ModID != expectedModId )
            {
                throw new InvalidOperationException( $"Mod ID '{this.ModID}' does not match directory name '{expectedModId}'" );
            }

            string filePath = Path.Combine( modDirectory, MOD_MANIFEST_FILENAME );

            var data = SerializationUnit.Serialize( this );

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( filePath );
            handler.Write( data );
        }

        /// <summary>
        /// Checks if all dependencies of this mod are satisfied by the given loaded mods.
        /// </summary>
        /// <param name="loadedMods">Dictionary of loaded mods keyed by mod ID</param>
        /// <returns>True if all dependencies are satisfied</returns>
        public bool AreDependenciesSatisfied( Dictionary<string, ModMetadata> loadedMods )
        {
            foreach( var dependency in Dependencies )
            {
                if( dependency.IsOptional )
                    continue;

                if( !loadedMods.TryGetValue( dependency.ID, out ModMetadata requiredMod ) )
                {
                    return false;
                }

                if( !dependency.IsSatisfiedBy( requiredMod.ModVersion ) )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets all unsatisfied dependencies for this mod.
        /// </summary>
        /// <param name="loadedMods">Dictionary of loaded mods keyed by mod ID</param>
        /// <returns>List of unsatisfied dependencies</returns>
        public IEnumerable<ModDependency> GetUnsatisfiedDependencies( Dictionary<string, ModMetadata> loadedMods )
        {
            if( Dependencies == null || Dependencies.Count == 0 )
                return Enumerable.Empty<ModDependency>();

            List<ModDependency> unsatisfied = new List<ModDependency>();

            foreach( var dependency in Dependencies )
            {
                if( dependency.IsOptional )
                    continue;

                if( !loadedMods.TryGetValue( dependency.ID, out ModMetadata requiredMod ) )
                {
                    unsatisfied.Add( dependency );
                    continue;
                }

                if( !dependency.IsSatisfiedBy( requiredMod.ModVersion ) )
                {
                    unsatisfied.Add( dependency );
                }
            }

            return unsatisfied;
        }

        [MapsInheritingFrom( typeof( ModMetadata ) )]
        public static SerializationMapping ModMetadataMapping()
        {
            return new MemberwiseSerializationMapping<ModMetadata>()
                .WithMember( "mod_id", o => o.ModID )
                .WithMember( "version", o => o.ModVersion )
                .WithMember( "name", o => o.Name )
                .WithMember( "author", o => o.Author )
                .WithMember( "description", o => o.Description )
                .WithMember( "exclude_from_saves", o => o.ExcludeFromSaves )
                .WithMember( "dependencies", o => o.Dependencies );
        }
    }
}
