using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatnisssSpaceSimulator.Core
{
    public sealed partial class Vessel
    {
        // Serialization / Persistence logic of vessels.


        /// <summary>
        /// Saves the vessel's persistent data to JSON.
        /// </summary>
        public JToken Save()
        {
            if( this.RootPart == null )
            {
                throw new InvalidOperationException( "The vessel must contain at least 1 part in order to be serialized." );
            }

            // save a vessel to a json file.
            // - save the parts, and their persistent state.
            JObject data = new JObject();

            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads the vessel's persistent data from JSON.
        /// </summary>
        public void Load( JToken data )
        {
            if( this.RootPart != null )
            {
                throw new InvalidOperationException( "The vessel must be partless in order to use it for deserialization." );
            }

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