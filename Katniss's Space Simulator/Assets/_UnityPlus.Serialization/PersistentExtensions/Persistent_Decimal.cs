using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
    public static class Persistent_Decimal
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this decimal value, IReverseReferenceMap s = null )
        {
            return (SerializedPrimitive)value;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static decimal ToDecimal( this SerializedData data, IForwardReferenceMap l = null )
        {
            return (decimal)data;
        }
    }
}