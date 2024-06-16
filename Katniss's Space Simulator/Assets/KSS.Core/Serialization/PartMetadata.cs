using KSS.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization.Json;

namespace KSS.Core.Serialization
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
        public readonly NamespacedIdentifier ID;

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

        public PartMetadata( string path )
        {
            if( path.EndsWith( PART_METADATA_FILENAME ) )
            {
                path = path[..PART_METADATA_FILENAME.Length];
            }
            this.Filepath = path;

            string partId = Path.GetFileName( path );
            string modId = Path.GetFileName( Path.GetDirectoryName( Path.GetDirectoryName( path ) ) );
            this.ID = new NamespacedIdentifier( modId, partId );
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

        [SerializationMappingProvider( typeof( PartMetadata ) )]
        public static SerializationMapping PartMetadataMapping()
        {
            return new MemberwiseSerializationMapping<PartMetadata>()
            {
                ("name", new Member<PartMetadata, string>( o => o.Name )),
                ("description", new Member<PartMetadata, string>( o => o.Description )),
                ("author", new Member<PartMetadata, string>( o => o.Author )),
                ("categories", new Member<PartMetadata, string[]>( o => o.Categories )),
                ("filter", new Member<PartMetadata, string>( o => o.Filter )),
                ("group", new Member<PartMetadata, string>( o => o.Group )),
               // ("file_version", new Member<PartMetadata, Version>( o => o.FileVersion )),
               // ("mod_versions", new Member<PartMetadata, Dictionary<string, Version>>( o => o.ModVersions ))
            };
        }
    }
}