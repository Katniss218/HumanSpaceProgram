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
        public string ID { get; set; }

        /// <summary>
        /// The user-friendly display name of this mod.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A user-friendly description of this mod.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The current version of this mod.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// The author(s) of this mod.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The license information for this mod.
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// The URL to the website about this mod, if applicable.
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// Whether to exclude this mod from being saved in the save mods. <br/>
        /// Setting this to true will make it so that saves created with this mod loaded will not warn after this mod has been uninstalled.
        /// </summary>
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
            if( metadata.ID != expectedModId )
            {
                throw new InvalidOperationException( $"Mod ID '{metadata.ID}' does not match directory name '{expectedModId}'" );
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
            if( this.ID != expectedModId )
            {
                throw new InvalidOperationException( $"Mod ID '{this.ID}' does not match directory name '{expectedModId}'" );
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

                if( !dependency.IsSatisfiedBy( requiredMod.Version ) )
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

                if( !dependency.IsSatisfiedBy( requiredMod.Version ) )
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
                .WithMember( "mod_id", o => o.ID )
                .WithMember( "name", o => o.Name )
                .WithMember( "description", o => o.Description )
                .WithMember( "version", o => o.Version )
                .WithMember( "author", o => o.Author )
                .WithMember( "license", o => o.License )
                .WithMember( "website", o => o.Website )
                .WithMember( "exclude_from_saves", o => o.ExcludeFromSaves )
                .WithMember( "dependencies", o => o.Dependencies );
        }
    }
}
