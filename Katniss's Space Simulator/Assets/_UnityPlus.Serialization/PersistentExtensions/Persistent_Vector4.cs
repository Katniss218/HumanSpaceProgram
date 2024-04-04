using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_Vector4
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this Vector4 v, IReverseReferenceMap s = null )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y, (SerializedPrimitive)v.z, (SerializedPrimitive)v.w };
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetData( this ref Vector4 v, SerializedData data, IForwardReferenceMap l = null )
		{
			v.x = (float)data[0];
			v.y = (float)data[1];
			v.z = (float)data[2];
			v.w = (float)data[3];
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector4 ToVector4( this SerializedData data, IForwardReferenceMap l = null ) 
		{
            return new Vector4( (float)data[0], (float)data[1], (float)data[2], (float)data[3] );
		}
	}
}