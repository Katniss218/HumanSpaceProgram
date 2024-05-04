using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_Vector2Int
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this Vector2Int v, IReverseReferenceMap s = null )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y };
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2Int AsVector2Int( this SerializedData data, IForwardReferenceMap l = null ) 
		{
            return new Vector2Int( (int)(SerializedPrimitive)data[0], (int)(SerializedPrimitive)data[1] );
		}
	}
}