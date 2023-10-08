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
        /// <summary>
        /// The special token name for a reference ID (part of Object).
        /// </summary>
        public const string ID = "$id";

        /// <summary>
        /// The special token name for a reference (part of Reference).
        /// </summary>
        public const string REF = "$ref";

        /// <summary>
        /// The special token name for an asset reference.
        /// </summary>
        public const string ASSETREF = "$assetref";

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void TryWriteData( ISaver s, object obj, SerializedData data, ref SerializedArray objects )
        {
            if( data != null )
            {
                objects.Add( new SerializedObject()
                {
                    { $"{SerializerUtils.REF}", s.WriteGuid( s.GetReferenceID( obj ) ) },
                    { "data", data }
                } );
            }
        }
    }
}