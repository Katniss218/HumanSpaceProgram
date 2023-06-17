using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static partial class IPersistentComponents_GetData
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this Component c, Saver s )
        {
            switch( c )
            {
                case IPersistent comp:
                    return comp.GetData( s );
                case Transform comp:
                    return comp.GetData( s );
                case MeshFilter comp:
                    return comp.GetData( s );
                case MeshRenderer comp:
                    return comp.GetData( s );
            }
            return null;
        }
    }
}