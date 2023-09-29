using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public sealed partial class Vessel
    {
        // Serialization / Persistence logic of vessels.


        /// <summary>
        /// Saves the vessel's persistent data to JSON.
        /// </summary>
        public SerializedData Save()
        {
            // save a vessel to a json file.
            // - save the parts, and their persistent state.
            SerializedData data = new SerializedObject();

            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads the vessel's persistent data from JSON.
        /// </summary>
        public void Load( SerializedData data )
        {
            // load a vessel from a json file.
            // - create the vessel
            // - create the parts based on what parts are defined in the save (using appropriate part factories).
            // - load persistent data of parts.

            // json factory?
            // normal factory + json later?
            throw new NotImplementedException();
        }
    }
}