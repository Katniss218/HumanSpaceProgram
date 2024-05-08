using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
    public static class Persistent_Float
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData AsSerialized( this float value, IReverseReferenceMap s = null )
        {
            return (SerializedPrimitive)value;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float AsFloat( this SerializedData data, IForwardReferenceMap l = null )
        {
            return (float)(SerializedPrimitive)data;
        }
    }
}