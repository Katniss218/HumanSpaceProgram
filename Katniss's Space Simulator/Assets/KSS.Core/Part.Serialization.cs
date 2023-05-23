using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    public sealed partial class Part : MonoBehaviour
    {
        // Serialization / Persistence logic of parts.


        // parts (mostly their functionalities) consist of 2 components
        // - definition component (unchanging)
        // - persistent component (changing)
        // the components aren't strictly defined, but whatever you decide to save I call persistent.

        /// <summary>
        /// Saves the persistent data of this part.
        /// </summary>
        public JToken Save()
        {
            // part needs to later know how to spawn itself. the part might've come from many different sources, in theory, right now.
            // - so we need to keep info about what was used to spawn the part.

            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads the persistent data of this part.
        /// </summary>
        public void Load( JToken data )
        {
            // functionalities / modules on the part must be the same as in the json.
            throw new NotImplementedException();


            // parts should be spawned before their persistent data is applied, because that data might reference other parts.
            // non-persistent data is set up in isolation, and must only reference itself.
        }
    }
}