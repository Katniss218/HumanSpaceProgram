using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_Vector2
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this Vector2 v, IReverseReferenceMap s = null )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y };
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetData( this ref Vector2 v, SerializedData data, IForwardReferenceMap l = null )
		{
			v.x = (float)data[0];
			v.y = (float)data[1];
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2 ToVector2( this SerializedData data, IForwardReferenceMap l = null ) 
		{
            return new Vector2( (float)data[0], (float)data[1] );
		}
	}
}