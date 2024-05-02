using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class Persistent_KeyCode
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this KeyCode value, IReverseReferenceMap s = null )
        {
            return (SerializedPrimitive)value.ToString( "G" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static KeyCode ToKeyCode( this SerializedData data, IForwardReferenceMap l = null )
        {
            return Enum.Parse<KeyCode>( (string)data );
        }
    }
}