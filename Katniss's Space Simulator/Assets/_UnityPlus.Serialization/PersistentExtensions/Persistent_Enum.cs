using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class Persistent_Enum
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this Enum value, IReverseReferenceMap s = null )
        {
            return (SerializedPrimitive)value.ToString( "G" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T AsEnum<T>( this SerializedData data, IForwardReferenceMap l = null ) where T : struct
        {
            return Enum.Parse<T>( data.AsString() );
        }
    }
}