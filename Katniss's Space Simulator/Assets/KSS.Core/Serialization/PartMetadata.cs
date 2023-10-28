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
    public class PartMetadata : IPersistent
    {
        /// <summary>
        /// The unique ID of this part.
        /// </summary>
        public readonly string ID;

        /// <summary>
        /// The name of the author of the part.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The display name shown in the GUI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description shown in the GUI.
        /// </summary>
        public string Description { get; set; }

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
            HashSet<string> cats = new HashSet<string>();
            foreach( var part in parts )
            {
                foreach( var cat in part.Categories )
                {
                    cats.Add( cat );
                }
            }
            return cats;
        }

        public SerializedData GetData( ISaver s )
        {
            throw new NotImplementedException();
        }

        public void SetData( ILoader l, SerializedData data )
        {
            throw new NotImplementedException();
        }
    }
}