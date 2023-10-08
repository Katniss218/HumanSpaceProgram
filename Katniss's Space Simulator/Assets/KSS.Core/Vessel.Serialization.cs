using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public sealed partial class Vessel : IPersistent
    {
        public SerializedData GetData( ISaver s )
        {
            Debug.LogWarning( "saving vessel" );
            return new SerializedObject()
            {

            };
            // save vessel data itself.
        }

        public void SetData( ILoader l, SerializedData data )
        {
            Debug.LogWarning( "loading vessel" );
        }
    }
}