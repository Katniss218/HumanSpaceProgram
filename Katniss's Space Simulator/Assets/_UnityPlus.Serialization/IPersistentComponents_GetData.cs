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
        public static SerializedData GetData( this Component c, ISaver s )
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

                    // particle system

                    // navmesh stuff

                    // colliders

                    // rigidbody


            }
            return null;
        }
    }
}