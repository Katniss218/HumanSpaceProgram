using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace KSS.Core.Serialization
{
    /// <summary>
    /// Represents additional metadata of a saved vessel.
    /// </summary>
    public class VesselMetadata : IPersistent
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