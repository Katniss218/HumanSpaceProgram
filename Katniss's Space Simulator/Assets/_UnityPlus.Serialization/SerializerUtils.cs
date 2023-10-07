using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
    public static class SerializerUtils
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void TryWriteData( ISaver s, object obj, SerializedData data, ref SerializedArray objects )
        {
            if( data != null )
            {
                objects.Add( new SerializedObject()
                {
                    { "$ref", s.WriteGuid( s.GetReferenceID( obj ) ) },
                    { "data", data }
                } );
            }
        }
    }
}