using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Content.Vessels.Serialization
{
    /// <summary>
    /// Represents additional metadata of a saved part of a vessel/building.
    /// </summary>
    public class PartMetadata
    {
        /// <summary>
        /// The name of the file that stores the part metadata.
        /// </summary>
        public const string PART_METADATA_FILENAME = "_part.json";

        /// <summary>
        /// The filepath of this metadata.
        /// </summary>
        public readonly string Filepath;

        /// <summary>
        /// The unique ID of this part.
        /// </summary>
        public readonly NamespacedID ID;

        /// <summary>
        /// The display name shown in the GUI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description shown in the GUI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The name of the author of the part.
        /// </summary>
        public string Author { get; set; }

        //public Sprite Icon { get; set; }

        /// <summary>
        /// The (filter) categories that this part belongs to.
        /// </summary>
        public string[] Categories { get; set; } = new string[] { };

        /// <summary>
        /// The filter string for searching this part.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Parts with the same group will be grouped under one entry.
        /// </summary>
        public string Group { get; set; } = null;

        /// <summary>
        /// The version of the part file.
        /// </summary>
        public Version FileVersion { get; set; }

        /// <summary>
        /// The versions of all the mods used when the part file was created.
        /// </summary>
        public Dictionary<string, Version> ModVersions { get; set; } = new Dictionary<string, Version>();

        public PartMetadata( string path )
        {
            if( path.EndsWith( PART_METADATA_FILENAME ) )
            {
                path = path[..PART_METADATA_FILENAME.Length];
            }
            this.Filepath = path;

            string partId = Path.GetFileName( path );
            string modId = Path.GetFileName( Path.GetDirectoryName( Path.GetDirectoryName( path ) ) );
            this.ID = new NamespacedID( modId, partId );
        }

        /// <summary>
        /// Root directory is the directory that contains the _part.json file.
        /// </summary>
        public static string GetRootDirectory( string path )
        {
            return path;
        }

        /// <summary>
        /// Root directory is the directory that contains the _part.json file.
        /// </summary>
        public string GetRootDirectory()
        {
            return GetRootDirectory( this.Filepath );
        }

        public static IEnumerable<PartMetadata> Filtered( IEnumerable<PartMetadata> parts, string category, string filter )
        {
            return parts.Where(
                p =>
                {
                    bool isCategory = category == null
                        ? true
                        : (p.Categories?.Contains( category ) ?? true);

                    bool isFilter = filter == null
                        ? true
                        : (p.Name?.Contains( filter, StringComparison.InvariantCultureIgnoreCase ) ?? false)
                       || (p.Author?.Contains( filter, StringComparison.InvariantCultureIgnoreCase ) ?? false)
                       || (p.Filter?.Contains( filter, StringComparison.InvariantCultureIgnoreCase ) ?? false);

                    return
                        isCategory && isFilter;
                } );
        }

        public static string[] GetUniqueCategories( IEnumerable<PartMetadata> parts )
        {
            HashSet<string> uniqueCategories = new HashSet<string>();
            foreach( var part in parts )
            {
                foreach( var category in part.Categories )
                {
                    uniqueCategories.Add( category );
                }
            }
            return uniqueCategories.ToArray();
        }

        public static PartMetadata LoadFromDisk( string path )
        {
            string saveFilePath = Path.Combine( GetRootDirectory( path ), PART_METADATA_FILENAME );

            PartMetadata partMetadata = new PartMetadata( path );

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( saveFilePath );
            var data = handler.Read();
            SerializationUnit.Populate( partMetadata, data );
            return partMetadata;
        }

        public void SaveToDisk()
        {
            string saveFilePath = Path.Combine( GetRootDirectory(), PART_METADATA_FILENAME );

            var data = SerializationUnit.Serialize( this );

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( saveFilePath );
            handler.Write( data );
        }

        [MapsInheritingFrom( typeof( PartMetadata ) )]
        public static SerializationMapping PartMetadataMapping()
        {
            return new MemberwiseSerializationMapping<PartMetadata>()
                .WithMember( "name", o => o.Name )
                .WithMember( "description", o => o.Description )
                .WithMember( "author", o => o.Author )
                .WithMember( "categories", o => o.Categories )
                .WithMember( "filter", o => o.Filter )
                .WithMember( "group", o => o.Group )
                .WithMember( "file_version", o => o.FileVersion )
                .WithMember( "mod_versions", o => o.ModVersions );
        }
    }
}