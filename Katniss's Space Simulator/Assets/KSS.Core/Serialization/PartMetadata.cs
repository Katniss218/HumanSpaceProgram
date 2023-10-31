using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace KSS.Core.Serialization
{
    /// <summary>
    /// Represents additional metadata of a saved part of a vessel/building.
    /// </summary>
    public class PartMetadata
    {
        /// <summary>
        /// The unique ID of this part.
        /// </summary>
        public readonly string ID;

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

        /// <summary>
        /// The (filter) categories that this part belongs to.
        /// </summary>
        public string[] Categories { get; set; }

        /// <summary>
        /// The filter string for searching this part.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Parts with the same group will be grouped under one entry.
        /// </summary>
        public string Group { get; set; } = null;

        public PartMetadata( string id )
        {
            this.ID = id;
        }

        public static IEnumerable<PartMetadata> Filtered( IEnumerable<PartMetadata> parts, string filter )
        {
            return parts.Where(
                p => (p.Name?.Contains( filter, StringComparison.InvariantCultureIgnoreCase ) ?? false)
                  || (p.Author?.Contains( filter, StringComparison.InvariantCultureIgnoreCase ) ?? false)
                  || (p.Filter?.Contains( filter, StringComparison.InvariantCultureIgnoreCase ) ?? false) );
        }

        public static HashSet<string> GetUniqueCategories( IEnumerable<PartMetadata> parts )
        {
            HashSet<string> uniqueCategories = new HashSet<string>();
            foreach( var part in parts )
            {
                foreach( var category in part.Categories )
                {
                    uniqueCategories.Add( category );
                }
            }
            return uniqueCategories;
        }

        public SerializedData GetData()
        {
            SerializedArray categories = new SerializedArray();
            foreach( var category in this.Categories )
            {
                categories.Add( category );
            }
            return new SerializedObject()
            {
                { "name", this.Name },
                { "description", this.Description },
                { "author", this.Author },
                { "categories", categories },
                { "filter", this.Filter },
                { "group", this.Group }
            };
        }

        public void SetData( SerializedData data )
        {
            if( data.TryGetValue( "name", out var name ) )
            {
                this.Name = (string)name;
            }
            if( data.TryGetValue( "description", out var description ) )
            {
                this.Description = (string)description;
            }
            if( data.TryGetValue( "author", out var author ) )
            {
                this.Author = (string)author;
            }

            if( data.TryGetValue( "categories", out var cat ) )
            {
                SerializedArray categories = (SerializedArray)cat;
                this.Categories = new string[categories.Count];
                int i = 0;
                foreach( var elemKvp in (SerializedArray)categories )
                {
                    this.Categories[i] = (string)categories;
                    i++;
                }
            }
            if( data.TryGetValue( "filter", out var filter ) )
            {
                this.Filter = (string)filter;
            }
            if( data.TryGetValue( "group", out var group ) )
            {
                this.Group = (string)group;
            }
        }
    }
}