using KSS.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
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
        /// Returns the path to the (root) directory of the timeline.
        /// </summary>
        public string GetRootDirectory()
        {
            return this.Filepath;
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

        public void WriteToDisk()
        {
            string savePath = GetRootDirectory();
            string saveFilePath = Path.Combine( savePath, PART_METADATA_FILENAME );

            StringBuilder sb = new StringBuilder();
            new JsonStringWriter( this.GetData(), sb ).Write();

            File.WriteAllText( saveFilePath, sb.ToString(), Encoding.UTF8 );
        }

        public void ReadDataFromDisk()
        {
            string savePath = GetRootDirectory();
            string saveFilePath = Path.Combine( savePath, PART_METADATA_FILENAME );

            string saveJson = File.ReadAllText( saveFilePath, Encoding.UTF8 );

            SerializedData data = new JsonStringReader( saveJson ).Read();

            this.SetData( data );
        }

        public SerializedData GetData()
        {
            return new SerializedObject()
            {
                { "name", this.Name.GetData() },
                { "description", this.Description.GetData() },
                { "author", this.Author.GetData() },
                { "categories", new SerializedArray( this.Categories.Select( c=>c.GetData()) ) },
                { "filter", this.Filter.GetData() },
                { "group", this.Group.GetData() }
            };
        }

        public void SetData( SerializedData data )
        {
            if( data.TryGetValue( "name", out var name ) )
                this.Name = name.AsString();

            if( data.TryGetValue( "description", out var description ) )
                this.Description = description.AsString();

            if( data.TryGetValue( "author", out var author ) )
                this.Author = author.AsString();

            if( data.TryGetValue<SerializedArray>( "categories", out var categories ) )
            {
                this.Categories = new string[categories.Count];
                int i = 0;
                foreach( var elemKvp in categories )
                {
                    this.Categories[i] = elemKvp.AsString();
                    i++;
                }
            }

            if( data.TryGetValue( "filter", out var filter ) )
                this.Filter = filter.AsString();

            if( data.TryGetValue( "group", out var group ) )
                this.Group = group.AsString();
        }
    }
}