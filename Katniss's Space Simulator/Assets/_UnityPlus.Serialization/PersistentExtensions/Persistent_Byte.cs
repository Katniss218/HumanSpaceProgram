using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
    public static class Persistent_Byte
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this byte value, IReverseReferenceMap s = null )
        {
            return (SerializedPrimitive)value;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static byte ToByte( this SerializedData data, IForwardReferenceMap l = null )
        {
            return (byte)data;
        }
    }
}