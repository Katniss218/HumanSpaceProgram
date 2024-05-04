using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
	public static class Persistent_Boolean
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this bool value, IReverseReferenceMap l = null )
		{
			return (SerializedPrimitive)value;
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool AsBoolean( this SerializedData data, IForwardReferenceMap l = null ) 
		{
            return (bool)(SerializedPrimitive)data;
		}
	}
}