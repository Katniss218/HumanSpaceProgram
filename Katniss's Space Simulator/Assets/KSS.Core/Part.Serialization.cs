using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public sealed partial class Part : MonoBehaviour, IPersistent
    {
        // Serialization / Persistence logic of parts.


        // parts (mostly their functionalities) consist of 2 components
        // - definition component (unchanging)
        // - persistent component (changing)
        // the components aren't strictly defined, but whatever you decide to save I call persistent.


        public void SetData( Loader l, SerializedData data )
        {
            // functionalities / modules on the part must be the same as in the json.
            throw new NotImplementedException();

            // parts should be spawned before their persistent data is applied, because that data might reference other parts.
        }

        public SerializedData GetData( Saver s )
        {
            throw new NotImplementedException();
        }
    }
}