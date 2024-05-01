using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
	public static class Persistent_String
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this string value, IReverseReferenceMap s = null )
		{
			return (SerializedPrimitive)value;
		}
				
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static string ToString( this SerializedData data, IForwardReferenceMap l = null ) 
		{
            return (string)data;
		}
	}
}