using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_Vector3Int
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this Vector3Int v, IReverseReferenceMap s = null )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y, (SerializedPrimitive)v.z };
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3Int Vector3Int( this SerializedData data, IForwardReferenceMap l = null ) 
		{
            return new Vector3Int( (int)data[0], (int)data[1], (int)data[2] );
		}
	}
}